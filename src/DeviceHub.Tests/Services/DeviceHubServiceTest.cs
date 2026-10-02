using DeviceHub.Core.DTOs;
using DeviceHub.Core.Entities;
using DeviceHub.Core.Enums;
using DeviceHub.Core.Exceptions;
using DeviceHub.Core.Interfaces;
using DeviceHub.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class DeviceServiceTests
{
    private readonly Mock<IDeviceRepository> _repoMock;
    private readonly Mock<IDeviceCategoryRepository> _categoryRepoMock;
    private readonly Mock<ILogger<DeviceService>> _loggerMock;
    private readonly DeviceService _service;

    public DeviceServiceTests()
    {
        _repoMock = new Mock<IDeviceRepository>();
        _categoryRepoMock = new Mock<IDeviceCategoryRepository>();
        _loggerMock = new Mock<ILogger<DeviceService>>();
        _service = new DeviceService(
            _repoMock.Object,
            _loggerMock.Object,
            _categoryRepoMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Code_Exists()
    {
        // Arrange
        _repoMock
            .Setup(r => r.CodeExistsAsync("TEMP-001", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new CreateDeviceDto
        {
            Name = "温度传感器",
            Code = "TEMP-001",
            Status = DeviceStatus.Running,
            CategoryId = 1
        };

        // Act
        var act = async () => await _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*TEMP-001*已存在*");
    }

    [Fact]
    public async Task CreateAsync_Should_Return_New_Id_When_Valid()
    {
        _repoMock
            .Setup(r => r.CodeExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        //Mock分类校验
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeviceCategory { Id = 1, Name = "温度传感器" });

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Device>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Device d, CancellationToken _) =>
            {
                d.Id = 123;
                return d;
            });

        var request = new CreateDeviceDto
        {
            Name = "温度传感器",
            Code = "TEMP-001",
            Status = DeviceStatus.Running,
            CategoryId = 1
        };

        var id = await _service.CreateAsync(request);

        id.Should().Be(123);
    }
}