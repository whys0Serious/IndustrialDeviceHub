using DeviceHub.Core.DTOs;
using DeviceHub.Core.Entities;
using DeviceHub.Core.Interfaces;
using DeviceHub.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DeviceHub.Tests;


public class DeviceCategoryServiceTests
{
    private readonly Mock<IDeviceCategoryRepository> _repoMock;
    private readonly Mock<ILogger<DeviceCategoryService>> _loggerMock;
    private readonly DeviceCategoryService _service;

    public DeviceCategoryServiceTests()
    {
        _repoMock = new Mock<IDeviceCategoryRepository>();
        _loggerMock = new Mock<ILogger<DeviceCategoryService>>();
        _service = new DeviceCategoryService(_repoMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// 新增分类
    /// 已存在返回异常信息
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNameExists()
    {
        var request = new CreateDeviceCategoryDto { Name = "温度传感器" };

        _repoMock.Setup(r => r.NameExistsAsync("温度传感器", null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

        var act = () => _service.CreateAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*温度传感器*已存在*");
    }
    /// <summary>
    /// 成功新增分类
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenValid()
    {
        var request = new CreateDeviceCategoryDto
        {
            Name = "压力传感器",
            Description = "测量压力"
        };

        _repoMock.Setup(r => r.NameExistsAsync("压力传感器", null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<DeviceCategory>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((DeviceCategory c, CancellationToken _) =>
                 {
                     c.Id = 10;
                     return c;
                 });

        var id = await _service.CreateAsync(request);

        id.Should().Be(10);
        _repoMock.Verify(r => r.AddAsync(
            It.Is<DeviceCategory>(c => c.Name == "压力传感器"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// 删除分类
    /// 有关联设备删除失败返回异常信息
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenCategoryHasDevices()
    {

        var category = new DeviceCategory { Id = 1, Name = "温度传感器" };

        _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(category);
        _repoMock.Setup(r => r.HasDevicesAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);   //分类下还有设备

        var act = () => _service.DeleteAsync(1);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*还有设备*");

        _repoMock.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    /// <summary>
    /// 成功删除分类
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task DeleteAsync_ShouldSucceed_WhenNoDevices()
    {
        var category = new DeviceCategory { Id = 1, Name = "空分类" };

        _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(category);
        _repoMock.Setup(r => r.HasDevicesAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

        await _service.DeleteAsync(1);

        _repoMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// 获取所有分类列表
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedDtos()
    {
        var categories = new List<DeviceCategory>
        {
            new() { Id = 1, Name = "温度传感器", Description = "温度" },
            new() { Id = 2, Name = "压力传感器", Description = "压力" }
        };

        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(categories);

        var dtos = await _service.GetAllAsync();

        dtos.Should().HaveCount(2);
        dtos[0].Name.Should().Be("温度传感器");
        dtos[1].Name.Should().Be("压力传感器");
    }
}
