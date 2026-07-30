namespace RoleFit.Api.Services;

public interface ILlmClient
{
    /// <summary>
    /// Sends a system/user prompt pair and forces the response into the given JSON schema.
    /// Returns the raw JSON text produced by the model.
    /// </summary>
    Task<string> GetStructuredCompletionAsync(
        string systemPrompt,
        string userPrompt,
        string schemaName,
        string jsonSchema,
        CancellationToken cancellationToken = default);
}
