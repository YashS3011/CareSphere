using CareSphere.Modules.Clinical.Services;
using CareSphere.Modules.Laboratory.Services;
using CareSphere.Modules.Pharmacy.Services;
using CareSphere.Modules.Billing.Services;
using CareSphere.Modules.Patients.Services;
using CareSphere.Modules.Ward.Services;
using CareSphere.Modules.Notifications.Services;
using CareSphere.Modules.Admin.Services;
using CareSphere.Modules.Shared.Services;
using CareSphere.Modules.Shared.Events;
using CareSphere.Models;

namespace CareSphere.Modules.Admin.Services
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public ApplicationUser? User { get; set; }
        public static AuthResult Ok(ApplicationUser user) => new() { Success = true, User = user };
        public static AuthResult Fail(string error) => new() { Success = false, Error = error };
    }

    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user with email/password. On success, creates a session, signs in with claims,
        /// and writes a USER_LOGIN audit event.
        /// </summary>
        Task<AuthResult> LoginAsync(string email, string password, Guid tenantId, string? ipAddress, string? userAgent);

        /// <summary>
        /// Signs out the user, revokes the session token, and writes a USER_LOGOUT audit event.
        /// </summary>
        Task LogoutAsync(string userId, string sessionToken);

        /// <summary>
        /// Updates the LastActivityAt on the session token if it is still valid.
        /// </summary>
        Task<bool> RefreshSessionAsync(string sessionToken);

        /// <summary>
        /// Returns true if the session token is valid, not expired, and not revoked.
        /// </summary>
        Task<bool> ValidateSessionAsync(string sessionToken);

        /// <summary>
        /// Revokes all active sessions for a user (called when deactivating the account).
        /// </summary>
        Task RevokeAllSessionsAsync(string userId);
    }
}
