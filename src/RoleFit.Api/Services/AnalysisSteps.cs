namespace RoleFit.Api.Services;

internal record CandidateSkill(string Skill, string Evidence);

internal record CandidateProfile(List<CandidateSkill> Skills);

internal record RoleRequirement(string Requirement, string Importance);

internal record RoleRequirements(List<RoleRequirement> Requirements);
