namespace RoleFit.Api.Prompts;

/// <summary>Prompts/schemas for the two extraction steps that feed the final FitResult comparison.</summary>
public static class MultiStepPrompts
{
    public const string CandidateSkillsSchemaName = "candidate_skills";

    public const string CandidateSkillsJsonSchema = """
        {
          "type": "object",
          "properties": {
            "skills": {
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
            }
          },
          "required": ["skills"],
          "additionalProperties": false
        }
        """;

    public const string RoleRequirementsSchemaName = "role_requirements";

    public const string RoleRequirementsJsonSchema = """
        {
          "type": "object",
          "properties": {
            "requirements": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "requirement": { "type": "string" },
                  "importance": { "type": "string", "enum": ["critical", "important", "nice_to_have"] }
                },
                "required": ["requirement", "importance"],
                "additionalProperties": false
              }
            }
          },
          "required": ["requirements"],
          "additionalProperties": false
        }
        """;

    public static string BuildCandidateSkillsSystemPrompt() =>
        """
        You extract a candidate's skills and experience from a CV. For each skill or piece of
        experience, cite where in the CV it appears as evidence. Respond only with data matching
        the given JSON schema, in the same language as the CV.
        """;

    public static string BuildCandidateSkillsUserPrompt(string cvText) =>
        $"""
        CV:
        {cvText}
        """;

    public static string BuildRoleRequirementsSystemPrompt() =>
        """
        You extract the requirements a job description asks for, and rate how important each one
        is (critical, important, or nice_to_have). Respond only with data matching the given JSON
        schema, in the same language as the job description.
        """;

    public static string BuildRoleRequirementsUserPrompt(string jobDescription) =>
        $"""
        Job description:
        {jobDescription}
        """;

    public static string BuildComparisonSystemPrompt() => FitResultPrompts.BuildSystemPrompt();

    public static string BuildComparisonUserPrompt(
        string cvText,
        string jobDescription,
        string candidateSkillsJson,
        string roleRequirementsJson) =>
        $"""
        CV:
        {cvText}

        Job description:
        {jobDescription}

        Extracted candidate skills (JSON):
        {candidateSkillsJson}

        Extracted role requirements (JSON):
        {roleRequirementsJson}

        Using the extracted skills and requirements above as your primary basis, compare them and
        produce the final fit analysis matching the given JSON schema.
        """;
}
