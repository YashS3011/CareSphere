using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using CareSphere.Modules.Admin.Services;
using CareSphere.Data;
using Microsoft.EntityFrameworkCore;
using CareSphere.Models;
using CareSphere.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CareSphere.Authorization
{
    public class TabIsolatedAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _js;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITenantContext _tenantContext;
        private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
        private ClaimsPrincipal? _cachedUser;
        private DateTime _lastCacheTime;

        public TabIsolatedAuthenticationStateProvider(
            IJSRuntime js,
            IServiceScopeFactory scopeFactory,
            ITenantContext tenantContext)
        {
            _js = js;
            _scopeFactory = scopeFactory;
            _tenantContext = tenantContext;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_cachedUser != null && (DateTime.UtcNow - _lastCacheTime).TotalSeconds < 30)
            {
                return new AuthenticationState(_cachedUser);
            }

            try
            {
                // Read the session token from window.name (tab-isolated and survives F5 refresh)
                var sessionToken = await _js.InvokeAsync<string>("eval", "window.name");
                if (string.IsNullOrEmpty(sessionToken))
                {
                    return new AuthenticationState(_anonymous);
                }

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
                    var scopedTenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

                    // Query database for matching active session, bypassing query filters since user is not authenticated yet.
                    var session = await dbContext.UserSessions
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow);

                    if (session == null)
                    {
                        return new AuthenticationState(_anonymous);
                    }

                    var user = await dbContext.Users
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(u => u.Id == session.UserId);

                    if (user == null || !user.IsActive)
                    {
                        return new AuthenticationState(_anonymous);
                    }

                    // Inject tenant ID into the scoped TenantContext so all EF query filters work correctly.
                    // This is required because the HTTP cookie is not written in Blazor Interactive Server mode.
                    _tenantContext.SetTenantId(user.TenantId);

                    // Propagate to the scoped context of this query as well
                    scopedTenantContext.SetTenantId(user.TenantId);

                    // Check if the tenant is active
                    bool isPlatformAdmin = user.Role == "platform_super_admin" || user.TenantId == Guid.Empty;
                    if (!isPlatformAdmin)
                    {
                        var tenantSettings = await dbContext.TenantSettings
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(t => t.TenantId == user.TenantId);
                        if (tenantSettings == null || !tenantSettings.IsActive)
                        {
                            return new AuthenticationState(_anonymous);
                        }
                    }

                    // Build permission claims
                    var permissions = await permissionService.GetUserPermissionsAsync(user.TenantId, user.Id);
                    var claims = new List<System.Security.Claims.Claim>
                    {
                        new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id),
                        new System.Security.Claims.Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? ""),
                        new System.Security.Claims.Claim(CareSphereClaimTypes.TenantId, user.TenantId.ToString()),
                        new System.Security.Claims.Claim(CareSphereClaimTypes.FullName, user.FullName),
                        new System.Security.Claims.Claim(ClaimTypes.Role, user.Role),
                        new System.Security.Claims.Claim("role", user.Role),
                        new System.Security.Claims.Claim("session_token", sessionToken)
                    };

                    if (user.DoctorId.HasValue)
                        claims.Add(new System.Security.Claims.Claim(CareSphereClaimTypes.DoctorId, user.DoctorId.Value.ToString()));

                    foreach (var perm in permissions)
                        claims.Add(new System.Security.Claims.Claim(CareSphereClaimTypes.Permission, perm));

                    var identity = new ClaimsIdentity(claims, "TabIsolatedAuth");
                    _cachedUser = new ClaimsPrincipal(identity);
                    _lastCacheTime = DateTime.UtcNow;
                    return new AuthenticationState(_cachedUser);
                }
            }
            catch
            {
                // During prerendering or early circuit setup, window.name is not available. Return anonymous.
                return new AuthenticationState(_anonymous);
            }
        }

        public void NotifyUserLogin(ClaimsPrincipal user)
        {
            _cachedUser = user;
            _lastCacheTime = DateTime.UtcNow;
            var tenantIdClaim = user.FindFirst(CareSphereClaimTypes.TenantId)?.Value 
                                ?? user.FindFirst("tenant_id")?.Value;
            if (Guid.TryParse(tenantIdClaim, out var tenantId) && tenantId != Guid.Empty)
            {
                _tenantContext.SetTenantId(tenantId);
            }
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
        }

        public void NotifyUserLogout()
        {
            _cachedUser = null;
            _lastCacheTime = DateTime.MinValue;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }
    }
}
