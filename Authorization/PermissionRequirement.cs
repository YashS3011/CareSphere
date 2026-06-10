using Microsoft.AspNetCore.Authorization;

namespace CareSphere.Authorization
{
    /// <summary>
    /// Encapsulates a single permission string as an authorization requirement.
    /// Used with PermissionAuthorizationHandler to enforce fine-grained access control.
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
