using DeviceHub.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 复用 Core / Infrastructure 的 DI 扩展方法
var conn = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddInfrastructure(conn);
builder.Services.AddCoreServices();

var app = builder.Build();