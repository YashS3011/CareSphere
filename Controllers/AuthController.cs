using CareSphere.Infrastructure;
using CareSphere.Data;
using CareSphere.Modules.Clinical.Services;
using CareSphere.Modules.Laboratory.Services;
using CareSphere.Modules.Pharmacy.Services;
using CareSphere.Modules.Billing.Services;
using CareSphere.Modules.Patients.Services;
using CareSphere.Modules.Ward.Services;
using CareSphere.Modules.Notifications.Services;
using CareSphere.Modules.Admin.Services;
using CareSphere.Modules.Shared.Services;
using CareSphere.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CareSphere.Controllers
{
    /// <summary>
    /// Handles SSO OAuth2/OIDC challenge and callback redirects.
    /// Routes:
    ///   GET /auth/challenge/{provider}?returnUrl=/  — initiates SSO handshake
    ///   GET /auth/callback                          — called by IdP after login
    /// </summary>
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly SupabaseAuthService _supabaseAuth;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IImpersonationService _impersonationService;
        private readonly ApplicationDbContext _context;

        public AuthController(
            SupabaseAuthService supabaseAuth,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IImpersonationService impersonationService,
            ApplicationDbContext context)
        {
            _supabaseAuth = supabaseAuth;
            _configuration = configuration;
            _logger = logger;
            _impersonationService = impersonationService;
            _context = context;
        }

        /// <summary>Initiates an SSO challenge with the given provider (Google, Microsoft, oidc).</summary>
        [HttpGet("challenge/{provider}")]
        public IActionResult Challenge(string provider, [FromQuery] string returnUrl = "/")
        {
            var allowedProviders = new[] { "Google", "Microsoft", "oidc" };
            if (!allowedProviders.Contains(provider, StringComparer.OrdinalIgnoreCase))
                return BadRequest("Unknown SSO provider.");

            var redirectUrl = Url.Action(nameof(Callback), "Auth", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return new ChallengeResult(provider, properties);
        }

        /// <summary>Handles the OAuth callback after the IdP redirects back.</summary>
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string returnUrl = "/")
        {
            // Read the external login info from the cookie set by the OAuth middleware
            var result = await HttpContext.AuthenticateAsync(
                Microsoft.AspNetCore.Identity.IdentityConstants.ExternalScheme);

            if (!result.Succeeded)
            {
                _logger.LogWarning("SSO callback authentication failed.");
                return Redirect("/auth/login?error=sso_failed");
            }

            var tenantIdStr = _configuration["App:DefaultTenantId"] ?? "00000000-0000-0000-0000-000000000001";
            var defaultTenantId = Guid.TryParse(tenantIdStr, out var tid) ? tid : Guid.Empty;

            // Exchange the external principal for a local Identity session
            var user = await _supabaseAuth.ExchangeSupabaseTokenAsync(result.Principal, defaultTenantId);
            if (user == null)
            {
                _logger.LogWarning("SSO callback: could not create/find local user.");
                return Redirect("/auth/login?error=sso_user_failed");
            }

            // Clear the external cookie
            await HttpContext.SignOutAsync(Microsoft.AspNetCore.Identity.IdentityConstants.ExternalScheme);

            // Redirect to the original return URL (validated to be local)
            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = "/";

            _logger.LogInformation("SSO login successful for {Email}", user.Email);
            return Redirect(returnUrl);
        }

        /// <summary>Performs local cookie sign-in for the impersonated user session.</summary>
        [HttpGet("impersonate-signin")]
        public async Task<IActionResult> ImpersonateSignIn([FromQuery] string token, [FromQuery] string returnUrl = "/")
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secret = _configuration["Supabase:JwtSecret"];
            if (string.IsNullOrEmpty(secret))
            {
                secret = "local_dev_secret_key_for_caresphere_1234567890";
            }
            var key = Encoding.UTF8.GetBytes(secret);
            if (key.Length < 32)
            {
                Array.Resize(ref key, 32);
            }

            ClaimsPrincipal principal;
            try
            {
                principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Impersonation JWT token validation failed.");
                return Redirect("/auth/login?error=impersonation_invalid");
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid token: userId claim is missing.");
            }

            // Load user to make sure they are active
            var user = await _context.Users
                .IgnoreQueryFilters()
                .Cast<ApplicationUser>()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || !user.IsActive)
            {
                return Redirect("/auth/login?error=user_inactive");
            }

            // Store short-lived token in the current session cookie
            Response.Cookies.Append("impersonation_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(15)
            });

            // Sign in locally via cookie authentication scheme
            var identity = new ClaimsIdentity(principal.Claims, Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme);
            await HttpContext.SignInAsync(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, new ClaimsPrincipal(identity));

            if (!Url.IsLocalUrl(returnUrl))
            {
                returnUrl = "/";
            }

            return Redirect(returnUrl);
        }

        /// <summary>Ends the impersonation session and signs the original SuperAdmin user back in.</summary>
        [HttpGet("impersonate-signout")]
        public async Task<IActionResult> ImpersonateSignOut()
        {
            var currentPrincipal = HttpContext.User;
            var superAdminUserId = currentPrincipal.FindFirstValue("impersonated_by");

            // Execute service logic to delete cookie and log ended event
            await _impersonationService.EndImpersonationAsync();

            // Clear impersonation token cookie
            Response.Cookies.Delete("impersonation_token");

            if (!string.IsNullOrEmpty(superAdminUserId))
            {
                var superAdminUser = await _context.Users
                    .IgnoreQueryFilters()
                    .Cast<ApplicationUser>()
                    .FirstOrDefaultAsync(u => u.Id == superAdminUserId);

                if (superAdminUser != null && superAdminUser.IsActive)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(CareSphereClaimTypes.TenantId, superAdminUser.TenantId.ToString()),
                        new Claim(CareSphereClaimTypes.FullName, superAdminUser.FullName),
                        new Claim(ClaimTypes.NameIdentifier, superAdminUser.Id),
                        new Claim(ClaimTypes.Name, superAdminUser.UserName ?? superAdminUser.Email ?? ""),
                        new Claim(ClaimTypes.Email, superAdminUser.Email ?? ""),
                        new Claim(ClaimTypes.Role, "platform_super_admin"),
                        new Claim("role", "platform_super_admin")
                    };

                    var identity = new ClaimsIdentity(claims, Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme);
                    await HttpContext.SignInAsync(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, new ClaimsPrincipal(identity));

                    return Redirect("/platform-admin/tenants");
                }
            }

            await HttpContext.SignOutAsync(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme);
            return Redirect("/auth/login");
        }
    }
}
