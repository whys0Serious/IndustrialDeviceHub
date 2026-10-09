using DeviceHub.Core.Entities;
using DeviceHub.Core.Interfaces;
using DeviceHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceHub.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DeviceHubDbContext _db;

        public UserRepository(DeviceHubDbContext db) => _db = db;

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
            => await _db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

        public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _db.Users.FindAsync(new object[] { id }, ct);

        public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
            => await _db.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync(ct);

        public async Task<User> AddAsync(User user, CancellationToken ct = default)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
            return user;
        }

        public async Task UpdateAsync(User user, CancellationToken ct = default)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> UsernameExistsAsync(string username, int? excludeId = null, CancellationToken ct = default)
        {
            var query = _db.Users.Where(u => u.Username == username);
            if (excludeId.HasValue) query = query.Where(u => u.Id != excludeId.Value);
            return await query.AnyAsync(ct);
        }
    }
}
