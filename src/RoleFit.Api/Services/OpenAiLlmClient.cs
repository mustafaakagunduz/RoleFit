using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RoleFit.Api.Services;

public class OpenAiLlmClient : ILlmClient
{
    private static readonly TimeSpan[] RetryDelays =
    {
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromMilliseconds(1500),
    };

    private readonly HttpClient _httpClient;
    private readonly LlmOptions _options;
    private readonly ILogger<OpenAiLlmClient> _logger;

    public OpenAiLlmClient(HttpClient httpClient, IOptions<LlmOptions> options, ILogger<OpenAiLlmClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GetStructuredCompletionAsync(
        string systemPrompt,
        string userPrompt,
        string schemaName,
        string jsonSchema,
        CancellationToken cancellationToken = default)
    {
        var schema = JsonSerializer.Deserialize<JsonElement>(jsonSchema);

        var requestBody = new
        {
            model = _options.Model,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt },
            },
            response_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = schemaName,
                    strict = true,
                    schema,
                },
            },
        };

        var response = await SendWithRetryAsync(requestBody, schemaName, cancellationToken);

        try
        {
            using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
            var content = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new LlmAnalysisException("OpenAI boş bir yanıt döndü.");
            }

            return content;
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
        {
            throw new LlmAnalysisException("OpenAI yanıtı beklenen biçimde değil.", ex);
        }
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(object requestBody, string schemaName, CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
            {
                Content = JsonContent.Create(requestBody),
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request, cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                if (attempt >= RetryDelays.Length)
                {
                    throw new LlmAnalysisException("OpenAI'a bağlanılamadı.", ex);
                }

                _logger.LogWarning(
                    "OpenAI çağrısı ({SchemaName}) bağlantı hatasıyla başarısız oldu, deneme {Attempt}/{MaxAttempts}.",
                    schemaName,
                    attempt + 1,
                    RetryDelays.Length + 1);
                await Task.Delay(RetryDelays[attempt], cancellationToken);
                continue;
            }

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            var isTransient = response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500;
            if (isTransient && attempt < RetryDelays.Length)
            {
                _logger.LogWarning(
                    "OpenAI çağrısı ({SchemaName}) geçici hata ile başarısız oldu ({StatusCode}), deneme {Attempt}/{MaxAttempts}.",
                    schemaName,
                    (int)response.StatusCode,
                    attempt + 1,
                    RetryDelays.Length + 1);
                await Task.Delay(RetryDelays[attempt], cancellationToken);
                continue;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new LlmAnalysisException($"OpenAI çağrısı başarısız oldu ({(int)response.StatusCode}): {body}");
        }
    }
}
