using CareSphere.Components;
using CareSphere.Data;
using CareSphere.Services;
using Microsoft.EntityFrameworkCore;

//AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure EF Core with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Transient, ServiceLifetime.Transient);

// Register Services
builder.Services.AddTransient<IPatientService, PatientService>();
builder.Services.AddTransient<IBedService, BedService>();

// Doctor Workflow & EMR Services
builder.Services.AddTransient<IAuditService, AuditService>();
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

// QuestPDF License registration
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var app = builder.Build();

// Seed Database
// using (var scope = app.Services.CreateScope())
// {
//     var services = scope.ServiceProvider;
//     try
//     {
//         var context = services.GetRequiredService<ApplicationDbContext>();
//         CareSphere.Data.DbInitializer.Initialize(context);
//     }
//     catch (Exception ex)
//     {
//         var logger = services.GetRequiredService<ILogger<Program>>();
//         logger.LogError(ex, "An error occurred seeding the database.");
//     }
// }

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();



app.Run();

