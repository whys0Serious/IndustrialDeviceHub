using DeviceHub.Core.DTOs;
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
                throw new InvalidOperationException($"AI 诊断失败：{response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<AiChatResponse>(ct);
            return result ?? new AiChatResponse { Suggestion = "（无响应）" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AI 服务连接失败");
            throw new InvalidOperationException("无法连接 AI 服务，请确认 DeviceHub.Api 已启动", ex);
        }
    }
}