using DeviceHub.Core.DTOs;
using DeviceHub.Core.Entities;
using DeviceHub.Core.Exceptions;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DeviceHub.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository repo, ILogger<AuthService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                throw new BusinessException("请输入用户名和密码");

            var user = await _repo.GetByUsernameAsync(request.Username.Trim(), ct)
                ?? throw new BusinessException("用户名或密码错误");

            if (!user.IsActive)
                throw new BusinessException("账号已禁用");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new BusinessException("用户名或密码错误");

            user.LastLoginAt = DateTime.Now;
            await _repo.UpdateAsync(user, ct);

            _logger.LogInformation("用户登录成功：{Username}", user.Username);

            return new LoginResponse
            {
                UserId = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Role = user.Role
            };
        }

        public async Task<int> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new BusinessException("用户名不能为空");
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw new BusinessException("密码至少 6 位");
            if (string.IsNullOrWhiteSpace(request.DisplayName))
                throw new BusinessException("显示名不能为空");

            if (await _repo.UsernameExistsAsync(request.Username, null, ct))
                throw new BusinessException($"用户名 {request.Username} 已存在");

            var user = new User
            {
                Username = request.Username.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                DisplayName = request.DisplayName.Trim(),
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var saved = await _repo.AddAsync(user, ct);
            _logger.LogInformation("创建用户：{Username}", saved.Username);
            return saved.Id;
        }

        public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken ct = default)
        {
            var users = await _repo.GetAllAsync(ct);
            return users.Select(ToDto).ToList();
        }

        public async Task SetActiveAsync(int userId, bool isActive, CancellationToken ct = default)
        {
            var user = await _repo.GetByIdAsync(userId, ct)
                ?? throw new BusinessException($"用户 {userId} 不存在");

            // 不允许禁用自己
            // 这个校验放 ViewModel 里更合适（因为 AuthService 不知道"当前用户"）

            user.IsActive = isActive;
            await _repo.UpdateAsync(user, ct);
            _logger.LogInformation("用户 {Username} 已{Status}", user.Username, isActive ? "启用" : "禁用");
        }

        public async Task ChangePasswordAsync(int userId, string oldPassword, string newPassword, CancellationToken ct = default)
        {
            var user = await _repo.GetByIdAsync(userId, ct)
                ?? throw new BusinessException($"用户 {userId} 不存在");

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                throw new BusinessException("原密码错误");

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                throw new BusinessException("新密码至少 6 位");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _repo.UpdateAsync(user, ct);
        }

        public async Task EnsureDefaultAdminAsync(CancellationToken ct = default)
        {
            // 如果没有任何用户，创建一个默认管理员
            var users = await _repo.GetAllAsync(ct);
            if (users.Count > 0) return;

            var admin = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                DisplayName = "系统管理员",
                Role = Core.Enums.UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _repo.AddAsync(admin, ct);
            _logger.LogWarning("已创建默认管理员：admin / admin123（请尽快修改密码）");
        }

        private static UserDto ToDto(User u) => new()
        {
            Id = u.Id,
            Username = u.Username,
            DisplayName = u.DisplayName,
            Role = u.Role,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            LastLoginAt = u.LastLoginAt
        };
    }
}
