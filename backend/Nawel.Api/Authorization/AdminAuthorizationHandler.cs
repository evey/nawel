using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Nawel.Api.Data;
using System.Security.Claims;

namespace Nawel.Api.Authorization;

/// <summary>
/// Authorization handler that checks if the user has admin privileges
/// </summary>
public class AdminAuthorizationHandler : AuthorizationHandler<AdminRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AdminAuthorizationHandler(
        IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory serviceScopeFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            context.Fail();
            return;
        }

        // Create a scope to access the DbContext
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NawelDbContext>();

        var user = await dbContext.Users.FindAsync(userId);

        if (user != null && user.IsAdmin)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
