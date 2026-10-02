using DeviceHub.Api.Endpoints;
using DeviceHub.Api.Middleware;
using DeviceHub.Infrastructure.Extensions;
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
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthEndpoints();

app.Run();