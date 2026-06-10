namespace CareSphere.Infrastructure
{
    /// <summary>
    /// Middleware that intercepts requests with a Supabase Bearer token (no existing auth cookie)
    /// and exchanges it for a local ASP.NET Core Identity session via SupabaseAuthService.
    /// Register in Program.cs BEFORE app.UseAuthentication().
    /// </summary>
    public class SupabaseJwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SupabaseJwtMiddleware> _logger;

        public SupabaseJwtMiddleware(RequestDelegate next, ILogger<SupabaseJwtMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, SupabaseAuthService supabaseAuthService, IConfiguration configuration)
        {
            // Only process if there is a Bearer token and no existing auth cookie
            var authHeader = context.Request.Headers.Authorization.ToString();
            var hasExistingCookie = context.Request.Cookies.ContainsKey(".AspNetCore.Identity.Application");

            if (!hasExistingCookie &&
                authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader["Bearer ".Length..].Trim();

                var principal = supabaseAuthService.ValidateSupabaseJwtAsync(token);
                if (principal != null)
                {
                    var defaultTenantIdStr = configuration["App:DefaultTenantId"] ?? Guid.Empty.ToString();
                    var defaultTenantId = Guid.TryParse(defaultTenantIdStr, out var tid) ? tid : Guid.Empty;

                    var user = await supabaseAuthService.ExchangeSupabaseTokenAsync(principal, defaultTenantId);
                    if (user != null)
                        _logger.LogInformation("Supabase JWT authenticated user: {Email}", user.Email);
                }
            }

            await _next(context);
        }
    }
}
