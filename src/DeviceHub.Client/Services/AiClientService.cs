using DeviceHub.Core.DTOs;
using DeviceHub.Core.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;

namespace DeviceHub.Client.Services;

public class AiClientService : IAiClientService
{
    private readonly HttpClient _http;
    private readonly ILogger<AiClientService> _logger;

    public AiClientService(HttpClient http, ILogger<AiClientService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<AiChatResponse> DiagnoseAsync(string faultDescription, CancellationToken ct = default)
    {
        var request = new AiChatRequest { Message = faultDescription };

        try
        {
            var response = await _http.PostAsJsonAsync("/api/ai/diagnose", request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("AI 诊断失败：{StatusCode} {Error}", response.StatusCode, error);
                //按 HTTP 状态码分类，给用户可操作的提示
                var userMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized
                        => "登录已过期，请重新登录",
                    System.Net.HttpStatusCode.PaymentRequired
                        => "AI 服务余额不足，请联系管理员",
                    System.Net.HttpStatusCode.RequestTimeout
                        => "请求超时，请稍后重试",
                    System.Net.HttpStatusCode.TooManyRequests
                        => "请求过于频繁，请稍后重试",
                    System.Net.HttpStatusCode.InternalServerError
                        => "AI 服务暂时不可用，请稍后重试",
                    System.Net.HttpStatusCode.BadGateway
                        => "AI 服务暂时不可用，请稍后重试",
                    System.Net.HttpStatusCode.ServiceUnavailable
                        => "AI 服务暂时不可用，请稍后重试",
                    System.Net.HttpStatusCode.GatewayTimeout
                        => "AI 服务响应超时，请稍后重试",
                    _ => "AI 诊断失败，请稍后重试"
                };

                throw new BusinessException(userMessage);
            }

            var result = await response.Content.ReadFromJsonAsync<AiChatResponse>(ct);
            return result ?? new AiChatResponse { Suggestion = "（无响应）" };
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            //超时（HttpClient.Timeout 触发，不是外部取消）
            _logger.LogWarning("AI 诊断请求超时");
            throw new BusinessException("AI 服务响应超时，请稍后重试");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == null)
        {
            // 网络层失败（连接被拒、DNS 失败）——StatusCode 是 null
            _logger.LogWarning(ex, "AI 服务网络连接失败");
            throw new BusinessException("网络连接失败，请检查网络后重试");
        }
        catch (HttpRequestException ex)
        {
            //其他 HttpRequestException（少见）
            _logger.LogError(ex, "AI 服务请求失败：{StatusCode}", ex.StatusCode);
            throw new BusinessException("AI 服务请求失败，请稍后重试");
        }
    }
}