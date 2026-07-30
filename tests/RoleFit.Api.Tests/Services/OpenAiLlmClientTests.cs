using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RoleFit.Api.Services;
using Xunit;

namespace RoleFit.Api.Tests.Services;

public class OpenAiLlmClientTests
{
    private class StubHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses;

        public int CallCount { get; private set; }

        public StubHandler(params HttpResponseMessage[] responses)
        {
            _responses = new Queue<HttpResponseMessage>(responses);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(_responses.Dequeue());
        }
    }

    private static HttpResponseMessage SuccessResponse(string content)
    {
        var escaped = content.Replace("\"", "\\\"");
        var body = "{\"choices\":[{\"message\":{\"content\":\"" + escaped + "\"}}]}";
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body),
        };
    }

    private static OpenAiLlmClient CreateClient(StubHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.openai.com/") };
        var options = Options.Create(new LlmOptions { Model = "gpt-4o-mini", ApiKey = "test-key" });
        return new OpenAiLlmClient(httpClient, options, NullLogger<OpenAiLlmClient>.Instance);
    }

    [Fact]
    public async Task GetStructuredCompletionAsync_OnFirstSuccess_ReturnsContentWithoutRetrying()
    {
        var handler = new StubHandler(SuccessResponse("{\"ok\":true}"));
        var client = CreateClient(handler);

        var result = await client.GetStructuredCompletionAsync("system", "user", "schema", "{}");

        Assert.Equal("{\"ok\":true}", result);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetStructuredCompletionAsync_RetriesOnTransientServerError_ThenSucceeds()
    {
        var handler = new StubHandler(
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            SuccessResponse("{\"ok\":true}"));
        var client = CreateClient(handler);

        var result = await client.GetStructuredCompletionAsync("system", "user", "schema", "{}");

        Assert.Equal("{\"ok\":true}", result);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetStructuredCompletionAsync_WhenAlwaysTransientError_ThrowsAfterExhaustingRetries()
    {
        var handler = new StubHandler(
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<LlmAnalysisException>(
            () => client.GetStructuredCompletionAsync("system", "user", "schema", "{}"));
        Assert.Equal(3, handler.CallCount);
    }

    [Fact]
    public async Task GetStructuredCompletionAsync_OnNonTransientError_ThrowsWithoutRetrying()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("{\"error\":\"invalid_api_key\"}"),
        });
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<LlmAnalysisException>(
            () => client.GetStructuredCompletionAsync("system", "user", "schema", "{}"));
        Assert.Equal(1, handler.CallCount);
    }
}
