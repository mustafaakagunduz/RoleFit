namespace RoleFit.Api.Services;

public class LlmOptions
{
    public string Provider { get; set; } = "openai";
    public string Model { get; set; } = "gpt-4o-mini";
    public string ApiKey { get; set; } = string.Empty;
}
