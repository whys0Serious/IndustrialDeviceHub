using System.Net;
using System.Text.Json;
using DeviceHub.Core.Exceptions;

namespace DeviceHub.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "业务异常：{Message}", ex.Message);
            await WriteErrorAsync(context, 400, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "系统异常");
            await WriteErrorAsync(context, 500, "服务器内部错误");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var result = JsonSerializer.Serialize(new { error = message });
        await context.Response.WriteAsync(result);
    }
}