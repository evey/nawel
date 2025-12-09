using Microsoft.AspNetCore.Authorization;

namespace Nawel.Api.Authorization;

/// <summary>
/// Authorization requirement for admin-only operations
/// </summary>
public class AdminRequirement : IAuthorizationRequirement
{
    // This is a marker class - no additional properties needed
}
