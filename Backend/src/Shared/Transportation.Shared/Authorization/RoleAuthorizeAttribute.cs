using Microsoft.AspNetCore.Authorization;

namespace Transportation.Shared.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RoleAuthorizeAttribute : AuthorizeAttribute
{
    public RoleAuthorizeAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }
}