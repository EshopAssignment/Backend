using Microsoft.AspNetCore.Authorization;

namespace Api.Authorization.Requirements;

public sealed class EmailConfirmedRequirement : IAuthorizationRequirement
{
}
