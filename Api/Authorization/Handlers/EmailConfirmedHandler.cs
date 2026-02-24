using Api.Authorization.Requirements;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Api.Authorization.Handlers;

public class EmailConfirmedHandler(UserManager<User> userManager) : AuthorizationHandler<EmailConfirmedRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EmailConfirmedRequirement requirement)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user is null) return;

        if(user.EmailConfirmed) 
            context.Succeed(requirement);
    }
}
