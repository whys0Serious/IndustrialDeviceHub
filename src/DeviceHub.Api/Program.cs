using DeviceHub.Api.Endpoints;
using DeviceHub.Api.Middleware;
using DeviceHub.Core.Interfaces;
using DeviceHub.Infrastructure.Data;
using DeviceHub.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//Serilog日志
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

//Core + Infrastructure（与 WPF 共用）
builder.Services.AddCoreServices();
builder.Services.AddInfrastructure(builder.Configuration);

//Api专属
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "IndustrialDeviceHub API",
        Version = "v1",
        Description = "工业设备监控与运维管理平台 - 后端接口"
    });
});

//健康检查
builder.Services.AddHealthChecks();

var app = builder.Build();

//启动时自动迁移 + 初始化默认数据
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    var db = services.GetRequiredService<DeviceHubDbContext>();

    // SQLServer容器刚启动时可能未就绪，重试几次
    var retries = 10;
    while (true)
    {
        try
        {
            logger.LogInformation("正在执行数据库迁移...");
            db.Database.Migrate();
            logger.LogInformation("数据库迁移完成");
            break;
        }
        catch (Exception ex) when (retries-- > 0)
        {
            logger.LogWarning(ex, "数据库未就绪，3 秒后重试（剩余 {Retries} 次）", retries);
            await Task.Delay(3000);
        }
    }

    // 初始化默认管理员
    var authService = services.GetRequiredService<IAuthService>();
    await authService.EnsureDefaultAdminAsync();
    logger.LogInformation("默认管理员已就绪");
}

app.UseMiddleware<GlobalExceptionMiddleware>();

//中间件
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IndustrialDeviceHub API v1");
    });
}

app.UseSerilogRequestLogging();
//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthEndpoints();

app.Run();