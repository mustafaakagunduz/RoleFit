using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace RoleFit.Api.Services;

public class OpenAiLlmClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly LlmOptions _options;

    public OpenAiLlmClient(HttpClient httpClient, IOptions<LlmOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
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
            throw new LlmAnalysisException("OpenAI'a bağlanılamadı.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new LlmAnalysisException($"OpenAI çağrısı başarısız oldu ({(int)response.StatusCode}): {body}");
        }

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
}
