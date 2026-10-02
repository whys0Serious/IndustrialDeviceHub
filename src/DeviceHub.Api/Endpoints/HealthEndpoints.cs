using DeviceHub.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace DeviceHub.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", async (DeviceHubDbContext db) =>
        {
            var databaseOk = await db.Database.CanConnectAsync();

            var result = new
            {
                status = databaseOk ? "Healthy" : "Unhealthy",
                timestamp = DateTime.Now,
                checks = new
                {
                    database = databaseOk ? "OK" : "Failed"
                }
            };

            return databaseOk
                ? Results.Ok(result)
                : Results.Json(result, statusCode: 503);
        })
        .WithName("Health")
        .WithTags("Health")
        .WithSummary("健康检查")
        .WithDescription("返回数据库连接状态");

        return app;
    }
}