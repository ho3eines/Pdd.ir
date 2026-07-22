using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Pdd.ir.Client.Authorization
{
    /// <summary>
    /// Attribute for role-based page authorization.
    /// Usage: @attribute [AuthorizeRole("admin")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        public AuthorizeRoleAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
            Policy = null;
        }
    }
}
