using DeviceHub.Core.DTOs;
using DeviceHub.Core.Entities;
using DeviceHub.Core.Enums;
using DeviceHub.Core.Interfaces;
using DeviceHub.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DeviceHub.Tests.Services
{
    public class DeviceServiceTests
    {
        private readonly Mock<IDeviceRepository> _repoMock;
        private readonly Mock<ILogger<DeviceService>> _loggerMock;
        private readonly DeviceService _service;

        public DeviceServiceTests()
        {
            _repoMock = new Mock<IDeviceRepository>();
            _loggerMock = new Mock<ILogger<DeviceService>>();
            _service = new DeviceService(_repoMock.Object, _loggerMock.Object);
        }
        /// <summary>
        /// 新增设备信息，返回新ID
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateAsync_ShouldReturnNewId_WhenValid()
        {
            var request = new CreateDeviceDto
            {
                Name = "温度传感器1号",
                Code = "TEMP-001",
                Status = DeviceStatus.Stopped,
                CategoryId = 1,
                Remark = "测试"
            };

            _repoMock.Setup(r => r.CodeExistsAsync("TEMP-001", null, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

            _repoMock.Setup(r => r.AddAsync(It.IsAny<Device>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Device d, CancellationToken _) =>
                     {
                         d.Id = 100;
                         return d;
                     });

   
            var id = await _service.CreateAsync(request);


            id.Should().Be(100);
            _repoMock.Verify(r => r.AddAsync(
                It.Is<Device>(d => d.Name == "温度传感器1号" && d.Code == "TEMP-001"),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        /// <summary>
        /// 新增分类名称重复
        /// 返回异常信息
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenCodeExists()
        {
            
            var request = new CreateDeviceDto
            {
                Name = "重复设备",
                Code = "TEMP-001",
                Status = DeviceStatus.Stopped,
                CategoryId = 1
            };

            _repoMock.Setup(r => r.CodeExistsAsync("TEMP-001", null, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);

           
            var act = () => _service.CreateAsync(request);

          
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("*TEMP-001*已存在*");

            _repoMock.Verify(r => r.AddAsync(It.IsAny<Device>(), It.IsAny<CancellationToken>()), Times.Never);
        }


        /// <summary>
        /// 更新设备时查询ID
        /// 不存在返回异常提示
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenDeviceNotFound()
        {
            
            var request = new UpdateDeviceDto
            {
                Name = "新名字",
                Code = "TEMP-001",
                Status = DeviceStatus.Running,
                CategoryId = 1
            };

            _repoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Device?)null);

            
            var act = () => _service.UpdateAsync(999, request);

            
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("*999*不存在*");
        }
        /// <summary>
        /// 更新设备
        /// 名称重名返回异常信息
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenCodeConflicts()
        {
            
            var existing = new Device
            {
                Id = 1,
                Name = "设备1",
                Code = "TEMP-001",
                Status = DeviceStatus.Stopped,
                CategoryId = 1,
                CreatedAt = DateTime.Now
            };

            var request = new UpdateDeviceDto
            {
                Name = "设备1改名",
                Code = "TEMP-002",   //和其他设备冲突
                Status = DeviceStatus.Running,
                CategoryId = 1
            };

            _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(existing);
            _repoMock.Setup(r => r.CodeExistsAsync("TEMP-002", 1, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);

            
            var act = () => _service.UpdateAsync(1, request);

            
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("*TEMP-002*");
        }
        /// <summary>
        /// 更新成功
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task UpdateAsync_ShouldSucceed_WhenValid()
        {
           
            var existing = new Device
            {
                Id = 1,
                Name = "设备1",
                Code = "TEMP-001",
                Status = DeviceStatus.Stopped,
                CategoryId = 1,
                CreatedAt = DateTime.Now
            };

            var request = new UpdateDeviceDto
            {
                Name = "设备1改名",
                Code = "TEMP-001",
                Status = DeviceStatus.Running,
                CategoryId = 2,
                Remark = "新备注"
            };

            _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(existing);
            _repoMock.Setup(r => r.CodeExistsAsync("TEMP-001", 1, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

           
            await _service.UpdateAsync(1, request);

            
            existing.Name.Should().Be("设备1改名");
            existing.Status.Should().Be(DeviceStatus.Running);
            existing.CategoryId.Should().Be(2);
            existing.Remark.Should().Be("新备注");
            existing.UpdatedAt.Should().NotBeNull();

            _repoMock.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        }


        /// <summary>
        /// 删除时查询ID
        /// 不存在返回异常信息
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeleteAsync_ShouldThrow_WhenNotFound()
        {
           
            _repoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Device?)null);

            
            var act = () => _service.DeleteAsync(999);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("*999*不存在*");
        }
        /// <summary>
        /// 成功删除
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeleteAsync_ShouldSucceed_WhenValid()
        {
            
            var existing = new Device
            {
                Id = 1,
                Name = "设备1",
                Code = "TEMP-001",
                Status = DeviceStatus.Stopped,
                CategoryId = 1,
                CreatedAt = DateTime.Now
            };

            _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(existing);

            
            await _service.DeleteAsync(1);

            
            _repoMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

       /// <summary>
       /// 按ID查询分类详情
       /// 未查询到返回null
       /// </summary>
       /// <returns></returns>

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Device?)null);

            var result = await _service.GetByIdAsync(999);

            result.Should().BeNull();
        }
        /// <summary>
        /// 按ID查询成功返回实体
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetByIdAsync_ShouldMapToDto_WhenFound()
        {
            
            var device = new Device
            {
                Id = 1,
                Name = "设备1",
                Code = "TEMP-001",
                Status = DeviceStatus.Running,
                CategoryId = 5,
                Category = new DeviceCategory { Id = 5, Name = "温度传感器" },
                Remark = "备注",
                CreatedAt = new DateTime(2026, 1, 1)
            };

            _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(device);

            
            var dto = await _service.GetByIdAsync(1);

            
            dto.Should().NotBeNull();
            dto!.Name.Should().Be("设备1");
            dto.CategoryName.Should().Be("温度传感器");
            dto.StatusText.Should().Be("运行中");
        }
    }
}
