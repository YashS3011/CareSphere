using CareSphere.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CareSphere.Infrastructure
{
    /// <summary>
    /// Validates incoming Supabase Auth JWTs and bridges them into ASP.NET Core Identity sessions.
    /// Use this for Supabase Auth SSO integration — not for generating JWT tokens internally
    /// (cookie auth is used for Blazor Server sessions).
    /// </summary>
    public class SupabaseAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<SupabaseAuthService> _logger;

        public SupabaseAuthService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<SupabaseAuthService> logger)
        {
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Validates a Supabase JWT using the project's JWT secret from appsettings Supabase:JwtSecret.
        /// Returns a ClaimsPrincipal with sub (user id), email, and caresphere/tenant_id claims, or null if invalid.
        /// </summary>
        public ClaimsPrincipal? ValidateSupabaseJwtAsync(string token)
        {
            var secret = _configuration["Supabase:JwtSecret"];
            if (string.IsNullOrEmpty(secret))
            {
                _logger.LogWarning("Supabase:JwtSecret is not configured. Supabase JWT validation is disabled.");
                return null;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

                var validationParams = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,   // Supabase issues its own issuer
                    ValidateAudience = false,  // Supabase uses "authenticated"
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };

                var principal = handler.ValidateToken(token, validationParams, out _);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Supabase JWT validation failed: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Exchanges a valid Supabase JWT for a local ASP.NET Core Identity session.
        /// Finds or creates the ApplicationUser matching the Supabase sub claim, then signs them in.
        /// Returns the ApplicationUser on success, null on failure.
        /// </summary>
        public async Task<ApplicationUser?> ExchangeSupabaseTokenAsync(ClaimsPrincipal supabasePrincipal, Guid defaultTenantId)
        {
            var email = supabasePrincipal.FindFirstValue(ClaimTypes.Email)
                        ?? supabasePrincipal.FindFirstValue("email");

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Supabase token has no email claim.");
                return null;
            }

            var tenantIdStr = supabasePrincipal.FindFirstValue(CareSphereClaimTypes.TenantId);
            var tenantId = Guid.TryParse(tenantIdStr, out var tid) ? tid : defaultTenantId;

            // Find or create local Identity user
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation("Creating local Identity user for Supabase email: {Email}", email);
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = email, // SSO users can update their profile later
                    TenantId = tenantId,
                    Role = CareSphereRoles.Patient, // Default role for SSO users
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PreferredLanguage = "en",
                };
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to create local user for Supabase SSO: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return null;
                }

                await _userManager.AddToRoleAsync(user, CareSphereRoles.Patient);
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Supabase SSO user is deactivated: {Email}", email);
                return null;
            }

            // Sign in locally via cookie
            await _signInManager.SignInAsync(user, isPersistent: false);
            return user;
        }
    }
}
