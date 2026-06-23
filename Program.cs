using CareSphere;
using CareSphere.Authorization;
using CareSphere.Components;
using CareSphere.Data;
using CareSphere.Infrastructure;
using CareSphere.Models;
using CareSphere.Modules.Clinical.Services;
using CareSphere.Modules.Laboratory.Services;
using CareSphere.Modules.Pharmacy.Services;
using CareSphere.Modules.Billing.Services;
using CareSphere.Modules.Patients.Services;
using CareSphere.Modules.Ward.Services;
using CareSphere.Modules.Notifications.Services;
using CareSphere.Modules.Admin.Services;
using CareSphere.Modules.Shared.Services;
using CareSphere.Modules.Appointments.Services;
using CareSphere.Modules.Analytics.Services;
using CareSphere.Modules.Nursing.Services;
using CareSphere.Modules.Shared.ReadModels;
using ApexCharts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

//AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers();

// HttpContext accessor (needed for user ID in services)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();

// In-memory cache (used by PermissionService)
builder.Services.AddMemoryCache();

// Configure EF Core with PostgreSQL or InMemory fallback
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connStr) || connStr.Contains("<host>") || connStr.Contains("placeholder"))
    {
        options.UseInMemoryDatabase("CareSphereDb");
    }
    else
    {
        options.UseNpgsql(connStr);
    }
},
ServiceLifetime.Transient, ServiceLifetime.Transient);

// ─── ASP.NET Core Identity ──────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ─── Cookie Authentication ───────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
    options.AccessDeniedPath = "/auth/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ─── Authorization Policies ──────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HasPermission",
        p => p.RequireAuthenticatedUser());

    options.AddPolicy("PlatformSuperAdmin",
        p => p.RequireClaim("role", "platform_super_admin"));

    // Patient
    options.AddPolicy(PolicyNames.Permission_Patients_View,   p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Patients_View)));
    options.AddPolicy(PolicyNames.Permission_Patients_Create, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Patients_Create)));
    options.AddPolicy(PolicyNames.Permission_Patients_Edit,   p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Patients_Edit)));
    options.AddPolicy(PolicyNames.Permission_Patients_Delete, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Patients_Delete)));
    options.AddPolicy(PolicyNames.Permission_OwnRecords_View, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.OwnRecords_View)));
    options.AddPolicy(PolicyNames.Permission_OwnInvoices_Download, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.OwnInvoices_Download)));
    options.AddPolicy(PolicyNames.Permission_Appointments_Book, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Appointments_Book)));
    options.AddPolicy(PolicyNames.Permission_Appointments_Create, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Appointments_Create)));
    options.AddPolicy(PolicyNames.Permission_Appointments_View, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Appointments_View)));

    // Beds
    options.AddPolicy(PolicyNames.Permission_Beds_View,    p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Beds_View)));
    options.AddPolicy(PolicyNames.Permission_Beds_Manage,  p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Beds_Manage)));
    options.AddPolicy(PolicyNames.Permission_Beds_Allocate,p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Beds_Allocate)));
    options.AddPolicy(PolicyNames.Permission_BedAllotment_View, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.BedAllotment_View)));

    // Doctor / EMR
    options.AddPolicy(PolicyNames.Permission_Encounters_View,     p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Encounters_View)));
    options.AddPolicy(PolicyNames.Permission_Encounters_Create,   p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Encounters_Create)));
    options.AddPolicy(PolicyNames.Permission_SoapNotes_Write,     p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.SoapNotes_Write)));
    options.AddPolicy(PolicyNames.Permission_SoapNotes_Finalize,  p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.SoapNotes_Finalize)));
    options.AddPolicy(PolicyNames.Permission_Prescriptions_Write, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Prescriptions_Write)));
    options.AddPolicy(PolicyNames.Permission_Prescriptions_Cancel,p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Prescriptions_Cancel)));
    options.AddPolicy(PolicyNames.Permission_TeleConsult_Start,   p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.TeleConsult_Start)));
    options.AddPolicy(PolicyNames.Permission_Queue_View,          p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Queue_View)));
    options.AddPolicy(PolicyNames.Permission_Queue_Manage,        p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Queue_Manage)));
    options.AddPolicy(PolicyNames.Permission_Vitals_Create,       p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Vitals_Create)));
    options.AddPolicy(PolicyNames.Permission_Vitals_View,         p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Vitals_View)));
    options.AddPolicy(PolicyNames.Permission_NursingNotes_Create, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.NursingNotes_Create)));
    options.AddPolicy(PolicyNames.Permission_NursingNotes_View,   p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.NursingNotes_View)));
    options.AddPolicy(PolicyNames.Permission_MedicationAdmin_Create, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.MedicationAdmin_Create)));
    options.AddPolicy(PolicyNames.Permission_MedicationAdmin_View,   p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.MedicationAdmin_View)));

    // Pharmacy
    options.AddPolicy(PolicyNames.Permission_Pharmacy_ViewStock,      p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Pharmacy_ViewStock)));
    options.AddPolicy(PolicyNames.Permission_Pharmacy_Dispense,       p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Pharmacy_Dispense)));
    options.AddPolicy(PolicyNames.Permission_Pharmacy_ManageInventory,p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Pharmacy_ManageInventory)));
    options.AddPolicy(PolicyNames.Permission_Pharmacy_OtcSale,        p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Pharmacy_OtcSale)));
    options.AddPolicy(PolicyNames.Permission_Pharmacy_ManagePO,       p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Pharmacy_ManagePO)));

    // Lab
    options.AddPolicy(PolicyNames.Permission_Lab_OrderTests,    p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Lab_OrderTests)));
    options.AddPolicy(PolicyNames.Permission_Lab_CollectSample, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Lab_CollectSample)));
    options.AddPolicy(PolicyNames.Permission_Lab_EnterResults,  p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Lab_EnterResults)));
    options.AddPolicy(PolicyNames.Permission_Lab_VerifyResults, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Lab_VerifyResults)));
    options.AddPolicy(PolicyNames.Permission_Lab_ViewReports,   p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Lab_ViewReports)));

    // Billing
    options.AddPolicy(PolicyNames.Permission_Billing_ViewInvoices,   p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Billing_ViewInvoices)));
    options.AddPolicy(PolicyNames.Permission_Billing_CreateInvoices, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Billing_CreateInvoices)));
    options.AddPolicy(PolicyNames.Permission_Billing_RecordPayments, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Billing_RecordPayments)));
    options.AddPolicy(PolicyNames.Permission_Billing_ManageClaims,   p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Billing_ManageClaims)));
    options.AddPolicy(PolicyNames.Permission_Billing_View,           p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Billing_View)));
    options.AddPolicy(PolicyNames.Permission_Billing_Create,         p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Billing_Create)));
    options.AddPolicy(PolicyNames.Permission_Billing_Edit,           p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Billing_Edit)));
    options.AddPolicy(PolicyNames.Permission_Payments_Manage,        p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Payments_Manage)));
    options.AddPolicy(PolicyNames.Permission_InsuranceClaims_Manage, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.InsuranceClaims_Manage)));
    options.AddPolicy(PolicyNames.Permission_InsuranceClaims_View,   p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.InsuranceClaims_View)));

    // Admin
    options.AddPolicy(PolicyNames.Permission_Admin_ManageUsers,  p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Admin_ManageUsers)));
    options.AddPolicy(PolicyNames.Permission_Admin_ManageRoles,  p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Admin_ManageRoles)));
    options.AddPolicy(PolicyNames.Permission_Admin_ViewAuditLog, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Admin_ViewAuditLog)));
    options.AddPolicy(PolicyNames.Permission_Admin_ManageTenant, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Admin_ManageTenant)));
    options.AddPolicy(PolicyNames.Permission_DoctorSchedule_Manage, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.DoctorSchedule_Manage)));
    options.AddPolicy(PolicyNames.Permission_Analytics_View, p => p.Requirements.Add(new PermissionRequirement(CareSpherePermissions.Analytics_View)));
    options.AddPolicy("Queue_Access", p => p.Requirements.Add(new CareSphere.Authorization.QueueAccessRequirement()));
});

// Register PermissionAuthorizationHandler as scoped
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CareSphere.Authorization.QueueAccessAuthorizationHandler>();

// Register Shared Read Models
builder.Services.AddScoped<CareSphere.Modules.Shared.ReadModels.IPatientReadModel, CareSphere.Modules.Shared.ReadModels.PatientReadModel>();
builder.Services.AddScoped<CareSphere.Modules.Shared.ReadModels.IPrescriptionReadModel, CareSphere.Modules.Shared.ReadModels.PrescriptionReadModel>();
builder.Services.AddScoped<CareSphere.Modules.Shared.ReadModels.IBedReadModel, CareSphere.Modules.Shared.ReadModels.BedReadModel>();
builder.Services.AddScoped<CareSphere.Modules.Shared.ReadModels.IDoctorReadModel, CareSphere.Modules.Shared.ReadModels.DoctorReadModel>();

// ─── Core Services ────────────────────────────────────────────────────────────
builder.Services.AddTransient<IPatientService, PatientService>();
builder.Services.AddTransient<IBedService, BedService>();

// Register New Modules Services
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IVitalSignsService, VitalSignsService>();
builder.Services.AddTransient<IVitalThresholdService, VitalThresholdService>();
builder.Services.AddScoped<INursingNoteService, NursingNoteService>();
builder.Services.AddScoped<IMedicationAdministrationService, MedicationAdministrationService>();
builder.Services.AddScoped<IShiftHandoverService, ShiftHandoverService>();

// Doctor Workflow & EMR Services
builder.Services.AddTransient<IAuditService, AuditService>();
builder.Services.AddTransient<IAuditLogService, AuditLogService>();
builder.Services.AddTransient<IDoctorService, DoctorService>();
builder.Services.AddTransient<IQueueService, QueueService>();
builder.Services.AddTransient<IEncounterService, EncounterService>();
builder.Services.AddTransient<ISoapNoteService, SoapNoteService>();
builder.Services.AddTransient<IPrescriptionService, PrescriptionService>();
builder.Services.AddTransient<ITeleConsultService, TeleConsultService>();
builder.Services.AddTransient<IClinicalDecisionSupportService, ClinicalDecisionSupportService>();

// Billing, Payments, and Insurance Claims Services
builder.Services.AddTransient<IInvoiceService, InvoiceService>();
builder.Services.AddTransient<IPaymentService, PaymentService>();
builder.Services.AddTransient<IClaimService, ClaimService>();
builder.Services.AddTransient<IDocumentService, DocumentService>();
builder.Services.AddSingleton<CareSphere.Infrastructure.RazorpayClientWrapper>();

// Pharmacy Management Services
builder.Services.AddTransient<ISupplierService, SupplierService>();
builder.Services.AddTransient<IPharmacyItemService, PharmacyItemService>();
builder.Services.AddTransient<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddTransient<IGrnService, GrnService>();
builder.Services.AddTransient<IStockLedgerService, StockLedgerService>();
builder.Services.AddTransient<IDispenseService, DispenseService>();
builder.Services.AddTransient<IOtcSaleService, OtcSaleService>();
builder.Services.AddTransient<IExpiryAlertService, ExpiryAlertService>();
builder.Services.AddHostedService<CareSphere.BackgroundServices.ExpiryAlertBackgroundService>();
builder.Services.AddHostedService<CareSphere.BackgroundServices.BedChargeBackgroundService>();

// Laboratory Management Services
builder.Services.AddTransient<ILabCatalogService, LabCatalogService>();
builder.Services.AddTransient<ILabRequisitionService, LabRequisitionService>();
builder.Services.AddTransient<ILabSampleService, LabSampleService>();
builder.Services.AddTransient<ILabResultService, LabResultService>();
builder.Services.AddTransient<ILabReportService, LabReportService>();
builder.Services.AddTransient<ILabNotificationService, LabNotificationService>();
builder.Services.AddHostedService<CareSphere.BackgroundServices.LabNotificationBackgroundService>();

// Notifications & Patient Engagement Services
builder.Services.AddTransient<IServiceBusService, ServiceBusService>();
builder.Services.AddTransient<INotificationTemplateService, NotificationTemplateService>();
builder.Services.AddTransient<INotificationSenderService, NotificationSenderService>();
builder.Services.AddTransient<IAppointmentReminderService, AppointmentReminderService>();
builder.Services.AddTransient<IDischargeNotificationService, DischargeNotificationService>();
builder.Services.AddTransient<IPatientPreferenceService, PatientPreferenceService>();
builder.Services.AddTransient<INotificationDashboardService, NotificationDashboardService>();

// Notifications Background Services
builder.Services.AddHostedService<CareSphere.BackgroundServices.AppointmentReminderBackgroundService>();
builder.Services.AddHostedService<CareSphere.BackgroundServices.ServiceBusOutboxBackgroundService>();
builder.Services.AddHostedService<CareSphere.BackgroundServices.NotificationRetryBackgroundService>();
builder.Services.AddHostedService<CareSphere.BackgroundServices.ServiceBusConsumerService>();

// ─── Admin & Access Control Services ─────────────────────────────────────────
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<CareSphere.Authorization.TabIsolatedAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CareSphere.Authorization.TabIsolatedAuthenticationStateProvider>());
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<ITenantService, TenantService>();
builder.Services.AddScoped<IImpersonationService, ImpersonationService>();
builder.Services.AddScoped<UserTimeZoneService>();
builder.Services.AddScoped<SupabaseAuthService>();
builder.Services.AddScoped<DatabaseSeeder>();

// ─── SSO Configuration ────────────────────────────────────────────────────────
// Note: Dynamic SSO registration requires restarting the application after
// configuration changes in the current implementation.
// SAML support requires the Sustainsys.Saml2 NuGet package — add when SAML is needed.
ConfigureSsoProviders(builder);

// QuestPDF License registration
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Analytics Services
builder.Services.AddTransient<CareSphere.Modules.Analytics.Services.IAnalyticsService, CareSphere.Modules.Analytics.Services.AnalyticsService>();
builder.Services.AddApexCharts();

var app = builder.Build();

// ─── Seed database at startup ─────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var defaultTenantIdStr = builder.Configuration["App:DefaultTenantId"] ?? "00000000-0000-0000-0000-000000000001";
        var defaultTenantId = Guid.Parse(defaultTenantIdStr);
        TenantContext.BypassTenantId = defaultTenantId;

        // Apply any pending EF Core migrations
        var context = services.GetRequiredService<ApplicationDbContext>();
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        // Seed roles, SuperAdmin, tenant settings, and role permissions
        var seeder = services.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database seeding.");
    }
    finally
    {
        TenantContext.BypassTenantId = null;
    }
}

// ─── HTTP Pipeline ─────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

//app.UseHttpsRedirection();

// Supabase JWT middleware — must come before UseAuthentication
app.UseMiddleware<SupabaseJwtMiddleware>();

app.UseAuthentication();

// Global Tenant Status Lockout Middleware
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var tenantIdClaim = context.User.FindFirst("caresphere/tenant_id")?.Value;
        if (Guid.TryParse(tenantIdClaim, out var tenantId) && tenantId != Guid.Empty)
        {
            var isImpersonating = context.User.HasClaim("impersonation_session", "true");
            if (!isImpersonating)
            {
                var db = context.RequestServices.GetRequiredService<ApplicationDbContext>();
                var settings = await db.TenantSettings.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.TenantId == tenantId);
                if (settings != null && !settings.IsActive)
                {
                    var signInManager = context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
                    await signInManager.SignOutAsync();
                    context.Response.Cookies.Delete("impersonation_token");
                    context.Response.Redirect("/auth/login?error=tenant_frozen");
                    return;
                }
            }
        }
    }
    await next();
});

app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();


// ─── SSO Provider Registration ────────────────────────────────────────────────
static void ConfigureSsoProviders(WebApplicationBuilder builder)
{
    // Reads SSO configuration from all tenants in the database at startup.
    // Note: dynamic SSO registration requires app restart after config changes.
    // SAML support: add Sustainsys.Saml2 NuGet package when needed.

    var ssoSection = builder.Configuration.GetSection("Sso");

    // Google SSO
    var googleClientId = ssoSection["Google:ClientId"];
    var googleClientSecret = ssoSection["Google:ClientSecret"];
    if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
    {
        builder.Services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
            });
    }

    // Microsoft SSO
    var msClientId = ssoSection["Microsoft:ClientId"];
    var msClientSecret = ssoSection["Microsoft:ClientSecret"];
    if (!string.IsNullOrEmpty(msClientId) && !string.IsNullOrEmpty(msClientSecret))
    {
        builder.Services.AddAuthentication()
            .AddMicrosoftAccount(options =>
            {
                options.ClientId = msClientId;
                options.ClientSecret = msClientSecret;
            });
    }

    // Generic OIDC SSO
    var oidcAuthority = ssoSection["Oidc:Authority"];
    var oidcClientId = ssoSection["Oidc:ClientId"];
    var oidcClientSecret = ssoSection["Oidc:ClientSecret"];
    if (!string.IsNullOrEmpty(oidcAuthority) && !string.IsNullOrEmpty(oidcClientId))
    {
        builder.Services.AddAuthentication()
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = oidcAuthority;
                options.ClientId = oidcClientId;
                options.ClientSecret = oidcClientSecret;
                options.ResponseType = "code";
                options.SaveTokens = true;
            });
    }
}
