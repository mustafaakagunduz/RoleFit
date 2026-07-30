namespace RoleFit.Api.Prompts;

/// <summary>System/user prompt templates and the JSON schema used to force structured FitResult output.</summary>
public static class FitResultPrompts
{
    public const string SchemaName = "fit_result";

    public const string JsonSchema = """
        {
          "type": "object",
          "properties": {
            "overallScore": { "type": "integer", "description": "0-100 overall fit score" },
            "verdict": { "type": "string", "enum": ["strong", "moderate", "weak"] },
            "summary": { "type": "string", "description": "2-3 sentence role-specific positioning summary" },
            "matchedSkills": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "skill": { "type": "string" },
                  "evidence": { "type": "string", "description": "Where in the CV this shows up" }
                },
                "required": ["skill", "evidence"],
                "additionalProperties": false
              }
            },
            "gaps": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "requirement": { "type": "string" },
                  "severity": { "type": "string", "enum": ["critical", "important", "nice_to_have"] },
                  "suggestion": { "type": "string", "description": "Concrete suggestion to close or compensate for the gap" }
                },
                "required": ["requirement", "severity", "suggestion"],
                "additionalProperties": false
              }
            },
            "suggestedBullets": {
              "type": "array",
              "items": { "type": "string" },
              "description": "Optional CV bullet suggestions tailored to the role"
            }
          },
          "required": ["overallScore", "verdict", "summary", "matchedSkills", "gaps", "suggestedBullets"],
          "additionalProperties": false
        }
        """;

    public static string BuildSystemPrompt() =>
        """
        You are RoleFit, an assistant that analyzes how well a candidate's CV fits a job description.
        Compare the candidate's skills and experience against the role's requirements, then respond
        only with data matching the given JSON schema. Be honest and specific: cite where evidence for
        a matched skill appears in the CV, and give concrete, actionable suggestions for gaps. Write all
        text fields (summary, evidence, suggestions, bullets) in the same language as the CV and job
        description.
        """;

    public static string BuildUserPrompt(string cvText, string jobDescription) =>
        $"""
        CV:
        {cvText}

        Job description:
        {jobDescription}
        """;
}
