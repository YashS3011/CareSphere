using CareSphere.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CareSphere.Modules.Admin.Services;
using CareSphere.Modules.Clinical.Services;
using CareSphere.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace CareSphere.Infrastructure
{
    /// <summary>
    /// Destructively truncates the database and seeds required roles, a set of login users for each role,
    /// and exactly one record per custom table.
    /// </summary>
    public class DatabaseSeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantService _tenantService;
        private readonly IPermissionService _permissionService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseSeeder> _logger;
        private readonly ApplicationDbContext _context;

        public DatabaseSeeder(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ITenantService tenantService,
            IPermissionService permissionService,
            IConfiguration configuration,
            ILogger<DatabaseSeeder> logger,
            ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _tenantService = tenantService;
            _permissionService = permissionService;
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        public async Task SeedAsync()
        {
            var defaultTenantIdStr = _configuration["App:DefaultTenantId"] ?? "00000000-0000-0000-0000-000000000001";
            var defaultTenantId = Guid.Parse(defaultTenantIdStr);
            var seedTenantId = defaultTenantId;
            var forceReset = string.Equals(_configuration["App:ForceResetDatabase"], "true", StringComparison.OrdinalIgnoreCase);

            async Task SeedNotificationTemplatesAsync()
            {
                if (!await _context.NotificationTemplates.AnyAsync(x => x.TemplateName == "LabReportReady" && x.Channel == "SMS"))
                {
                    _context.NotificationTemplates.Add(new NotificationTemplate
                    {
                        Id = Guid.NewGuid(),
                        TenantId = seedTenantId,
                        TemplateName = "LabReportReady",
                        NotificationType = "LabReportReady",
                        Channel = "SMS",
                        Language = "en",
                        IsActive = true,
                        TemplateBody = "Hi {{PatientName}}, your lab report (MRN: {{MRN}}) is ready. Please collect it from the lab desk."
                    });
                    await _context.SaveChangesAsync();
                }

                if (!await _context.NotificationTemplates.AnyAsync(x => x.TemplateName == "AppointmentConfirmation" && x.Channel == "SMS"))
                {
                    _context.NotificationTemplates.Add(new NotificationTemplate
                    {
                        Id = Guid.NewGuid(),
                        TenantId = seedTenantId,
                        TemplateName = "AppointmentConfirmation",
                        NotificationType = "AppointmentConfirmation",
                        Channel = "SMS",
                        Language = "en",
                        IsActive = true,
                        TemplateBody = "Hi {{PatientName}}, your appointment is confirmed for {{SlotTime}}. Please arrive 10 min early."
                    });
                    await _context.SaveChangesAsync();
                }

                if (!await _context.NotificationTemplates.AnyAsync(x => x.TemplateName == "DischargeNotification" && x.Channel == "SMS"))
                {
                    _context.NotificationTemplates.Add(new NotificationTemplate
                    {
                        Id = Guid.NewGuid(),
                        TenantId = seedTenantId,
                        TemplateName = "DischargeNotification",
                        NotificationType = "DischargeNotification",
                        Channel = "SMS",
                        Language = "en",
                        IsActive = true,
                        TemplateBody = "Dear {{PatientName}}, you have been discharged. Thank you for choosing our hospital. Get well soon!"
                    });
                    await _context.SaveChangesAsync();
                }

                if (!await _context.NotificationTemplates.AnyAsync(x => x.TemplateName == "AppointmentReminder" && x.Channel == "SMS"))
                {
                    _context.NotificationTemplates.Add(new NotificationTemplate
                    {
                        Id = Guid.NewGuid(),
                        TenantId = seedTenantId,
                        TemplateName = "AppointmentReminder",
                        NotificationType = "AppointmentReminder",
                        Channel = "SMS",
                        Language = "en",
                        IsActive = true,
                        TemplateBody = "Reminder: Hi {{PatientName}}, you have an appointment tomorrow at {{SlotTime}}. To reschedule please contact reception. - CareSphere HMS"
                    });
                    await _context.SaveChangesAsync();
                }

                if (!await _context.NotificationTemplates.AnyAsync(x => x.TemplateName == "CriticalVitalsAlert" && x.Channel == "InApp"))
                {
                    _context.NotificationTemplates.Add(new NotificationTemplate
                    {
                        Id = Guid.NewGuid(),
                        TenantId = seedTenantId,
                        TemplateName = "CriticalVitalsAlert",
                        NotificationType = "CriticalVitalsAlert",
                        Channel = "InApp",
                        Language = "en",
                        IsActive = true,
                        TemplateBody = "Patient {{PatientName}} (MRN: {{MRN}}) has critical vitals recorded at {{RecordedAt}}. Immediate review required."
                    });
                    await _context.SaveChangesAsync();
                }

                if (!await _context.NotificationTemplates.AnyAsync(x => x.TemplateName == "LabReportReady" && x.Channel == "WhatsApp"))
                {
                    _context.NotificationTemplates.Add(new NotificationTemplate
                    {
                        Id = Guid.NewGuid(),
                        TenantId = seedTenantId,
                        TemplateName = "LabReportReady",
                        NotificationType = "LabReportReady",
                        Channel = "WhatsApp",
                        Language = "en",
                        IsActive = true,
                        TemplateBody = "Hello {{PatientName}} 👋, your lab report (MRN: {{MRN}}) is now ready for collection. Please visit the lab desk or ask your doctor at your next appointment. - CareSphere HMS"
                    });
                    await _context.SaveChangesAsync();
                }

                if (!await _context.NotificationTemplates.AnyAsync(x => x.TemplateName == "AppointmentConfirmation" && x.Channel == "WhatsApp"))
                {
                    _context.NotificationTemplates.Add(new NotificationTemplate
                    {
                        Id = Guid.NewGuid(),
                        TenantId = seedTenantId,
                        TemplateName = "AppointmentConfirmation",
                        NotificationType = "AppointmentConfirmation",
                        Channel = "WhatsApp",
                        Language = "en",
                        IsActive = true,
                        TemplateBody = "Hello {{PatientName}} 👋, your appointment is confirmed for {{SlotTime}}. Please arrive 10 minutes early and carry a valid ID. See you soon! - CareSphere HMS"
                    });
                    await _context.SaveChangesAsync();
                }
            }

            // Check if already seeded (e.g. if any tenant settings exist)
            if (await _context.TenantSettings.IgnoreQueryFilters().AnyAsync() && !forceReset)
            {
                _logger.LogInformation("Database is already seeded. Running check-and-repair pass...");

                // Repair 1: Fix Guid.Empty tenant IDs left by old builds
                if (_context.Database.IsRelational())
                {
                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync($@"
                            DO $$
                            DECLARE
                                r RECORD;
                            BEGIN
                                FOR r IN (
                                    SELECT table_name 
                                    FROM information_schema.columns 
                                    WHERE table_schema = 'public' 
                                      AND column_name = 'tenant_id'
                                ) LOOP
                                    EXECUTE 'UPDATE ' || quote_ident(r.table_name) || ' SET tenant_id = ''{defaultTenantId}'' WHERE tenant_id = ''00000000-0000-0000-0000-000000000000''';
                                END LOOP;
                            END $$;");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error repairing Guid.Empty tenant IDs.");
                    }
                }

                TenantContext.BypassTenantId = defaultTenantId;
                try
                {
                    // Repair 2: Ensure nurse2@caresphere.dev exists
                    var nurse2 = await _userManager.Users.IgnoreQueryFilters()
                        .FirstOrDefaultAsync(u => u.NormalizedEmail == "NURSE2@CARESPHERE.DEV");
                    if (nurse2 == null)
                    {
                        _logger.LogInformation("Inserting missing nurse2@caresphere.dev account...");
                        var nurse2User = new ApplicationUser
                        {
                            UserName = "nurse2@caresphere.dev",
                            Email = "nurse2@caresphere.dev",
                            FullName = "Test Nurse (Nursing Module)",
                            TenantId = defaultTenantId,
                            Role = CareSphereRoles.Nurse,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            PreferredLanguage = "en",
                            EmailConfirmed = true
                        };
                        var nurse2Res = await _userManager.CreateAsync(nurse2User, "Nurse@123");
                        if (nurse2Res.Succeeded)
                            await _userManager.AddToRoleAsync(nurse2User, CareSphereRoles.Nurse);
                    }

                    // Repair 2.1: Ensure platform admin role and user exist
                    var platRoleExists = await _roleManager.RoleExistsAsync("platform_super_admin");
                    if (!platRoleExists)
                    {
                        await _roleManager.CreateAsync(new IdentityRole
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = "platform_super_admin",
                            NormalizedName = "PLATFORM_SUPER_ADMIN"
                        });
                    }
                    var platAdmin = await _userManager.Users.IgnoreQueryFilters()
                        .FirstOrDefaultAsync(u => u.NormalizedEmail == "PLATFORMADMIN@CARESPHERE.DEV");
                    if (platAdmin == null)
                    {
                        _logger.LogInformation("Inserting missing platformadmin@caresphere.dev account...");
                        var platUser = new ApplicationUser
                        {
                            UserName = "platformadmin@caresphere.dev",
                            Email = "platformadmin@caresphere.dev",
                            FullName = "Platform Administrator",
                            TenantId = Guid.Empty,
                            Role = "platform_super_admin",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            PreferredLanguage = "en",
                            EmailConfirmed = true
                        };
                        var platRes = await _userManager.CreateAsync(platUser, "PlatformAdmin@123");
                        if (platRes.Succeeded)
                            await _userManager.AddToRoleAsync(platUser, "platform_super_admin");
                    }

                    // Repair 3: Ensure Warfarin, Paracetamol, and Aspirin exist in DrugFormulary
                    var warExists = await _context.DrugFormulary.IgnoreQueryFilters()
                        .AnyAsync(d => d.DrugCode == "WAR-5");
                    if (!warExists)
                    {
                        _logger.LogInformation("Inserting missing Warfarin 5mg drug formulary entry...");
                        _context.DrugFormulary.Add(new DrugFormulary
                        {
                            Id = Guid.NewGuid(),
                            TenantId = defaultTenantId,
                            DrugCode = "WAR-5",
                            GenericName = "Warfarin",
                            BrandName = "Coumadin",
                            Form = "Tablet",
                            Strength = "5mg",
                            Unit = "Tablet",
                            IsControlled = false,
                            IsActive = true
                        });
                        await _context.SaveChangesAsync();
                    }

                    var parExists = await _context.DrugFormulary.IgnoreQueryFilters()
                        .AnyAsync(d => d.DrugCode == "PAR-500");
                    if (!parExists)
                    {
                        _logger.LogInformation("Inserting missing Paracetamol 500mg drug formulary entry...");
                        _context.DrugFormulary.Add(new DrugFormulary
                        {
                            Id = Guid.NewGuid(),
                            TenantId = defaultTenantId,
                            DrugCode = "PAR-500",
                            GenericName = "Paracetamol",
                            BrandName = "Crocin",
                            Form = "Tablet",
                            Strength = "500mg",
                            Unit = "Tablet",
                            IsControlled = false,
                            IsActive = true
                        });
                        await _context.SaveChangesAsync();
                    }

                    var aspExists = await _context.DrugFormulary.IgnoreQueryFilters()
                        .AnyAsync(d => d.DrugCode == "ASP-75");
                    if (!aspExists)
                    {
                        _logger.LogInformation("Inserting missing Aspirin 75mg drug formulary entry...");
                        _context.DrugFormulary.Add(new DrugFormulary
                        {
                            Id = Guid.NewGuid(),
                            TenantId = defaultTenantId,
                            DrugCode = "ASP-75",
                            GenericName = "Aspirin",
                            BrandName = "Ecotrin",
                            Form = "Tablet",
                            Strength = "75mg",
                            Unit = "Tablet",
                            IsControlled = false,
                            IsActive = true
                        });
                        await _context.SaveChangesAsync();
                    }

                    // Repair 4: Ensure Warfarin, Paracetamol, and Aspirin exist in PharmacyItems & PharmacyBatches
                    var warItemExists = await _context.PharmacyItems.IgnoreQueryFilters()
                        .AnyAsync(p => p.ItemCode == "WAR-5");
                    if (!warItemExists)
                    {
                        _logger.LogInformation("Inserting missing Warfarin 5mg pharmacy item...");
                        var warItemId = Guid.NewGuid();
                        _context.PharmacyItems.Add(new PharmacyItem
                        {
                            Id = warItemId,
                            TenantId = defaultTenantId,
                            ItemCode = "WAR-5",
                            ItemName = "Warfarin 5mg",
                            GenericName = "Warfarin",
                            Category = "Medicine",
                            Form = "Tablet",
                            Strength = "5mg",
                            Unit = "Strip",
                            IsControlled = false,
                            RequiresPrescription = true,
                            ReorderLevel = 50,
                            IsActive = true
                        });

                        // Find an existing supplier to link the batch to
                        var anySupplierId = await _context.Suppliers.IgnoreQueryFilters()
                            .Select(s => s.Id).FirstOrDefaultAsync();
                        if (anySupplierId != Guid.Empty)
                        {
                            _context.PharmacyBatches.Add(new PharmacyBatch
                            {
                                Id = Guid.NewGuid(),
                                TenantId = defaultTenantId,
                                ItemId = warItemId,
                                BatchNumber = "BAT-WAR-01",
                                SupplierId = anySupplierId,
                                ManufactureDate = DateTime.UtcNow.AddMonths(-3),
                                ExpiryDate = DateTime.UtcNow.AddYears(2),
                                PurchasePrice = 25.00m,
                                SellingPrice = 35.00m,
                                CurrentStock = 200,
                                ReservedStock = 0,
                                IsActive = true
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    var parItemExists = await _context.PharmacyItems.IgnoreQueryFilters()
                        .AnyAsync(p => p.ItemCode == "PAR-500");
                    if (!parItemExists)
                    {
                        _logger.LogInformation("Inserting missing Paracetamol 500mg pharmacy item...");
                        var parItemId = Guid.NewGuid();
                        _context.PharmacyItems.Add(new PharmacyItem
                        {
                            Id = parItemId,
                            TenantId = defaultTenantId,
                            ItemCode = "PAR-500",
                            ItemName = "Paracetamol 500mg",
                            GenericName = "Paracetamol",
                            Category = "Medicine",
                            Form = "Tablet",
                            Strength = "500mg",
                            Unit = "Strip",
                            Barcode = "8901234567890",
                            IsControlled = false,
                            RequiresPrescription = false,
                            ReorderLevel = 100,
                            IsActive = true
                        });

                        var anySupplierId = await _context.Suppliers.IgnoreQueryFilters()
                            .Select(s => s.Id).FirstOrDefaultAsync();
                        if (anySupplierId != Guid.Empty)
                        {
                            _context.PharmacyBatches.Add(new PharmacyBatch
                            {
                                Id = Guid.NewGuid(),
                                TenantId = defaultTenantId,
                                ItemId = parItemId,
                                BatchNumber = "BAT-PAR-99",
                                SupplierId = anySupplierId,
                                ManufactureDate = DateTime.UtcNow.AddMonths(-3),
                                ExpiryDate = DateTime.UtcNow.AddYears(2),
                                PurchasePrice = 10.00m,
                                SellingPrice = 15.00m,
                                CurrentStock = 500,
                                ReservedStock = 0,
                                IsActive = true
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    var aspItemExists = await _context.PharmacyItems.IgnoreQueryFilters()
                        .AnyAsync(p => p.ItemCode == "ASP-75");
                    if (!aspItemExists)
                    {
                        _logger.LogInformation("Inserting missing Aspirin 75mg pharmacy item...");
                        var aspItemId = Guid.NewGuid();
                        _context.PharmacyItems.Add(new PharmacyItem
                        {
                            Id = aspItemId,
                            TenantId = defaultTenantId,
                            ItemCode = "ASP-75",
                            ItemName = "Aspirin 75mg",
                            GenericName = "Aspirin",
                            Category = "Medicine",
                            Form = "Tablet",
                            Strength = "75mg",
                            Unit = "Strip",
                            Barcode = "501234567800",
                            IsControlled = false,
                            RequiresPrescription = true,
                            ReorderLevel = 50,
                            IsActive = true
                        });

                        var anySupplierId = await _context.Suppliers.IgnoreQueryFilters()
                            .Select(s => s.Id).FirstOrDefaultAsync();
                        if (anySupplierId != Guid.Empty)
                        {
                            _context.PharmacyBatches.Add(new PharmacyBatch
                            {
                                Id = Guid.NewGuid(),
                                TenantId = defaultTenantId,
                                ItemId = aspItemId,
                                BatchNumber = "BAT-ASP-75",
                                SupplierId = anySupplierId,
                                ManufactureDate = DateTime.UtcNow.AddMonths(-3),
                                ExpiryDate = DateTime.UtcNow.AddYears(2),
                                PurchasePrice = 8.00m,
                                SellingPrice = 10.00m,
                                CurrentStock = 300,
                                ReservedStock = 0,
                                IsActive = true
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    // Repair 5: Ensure Aspirin-Warfarin drug interaction exists
                    var interactionExists = await _context.DrugInteractions.IgnoreQueryFilters()
                        .AnyAsync(d => d.DrugCodeA == "ASP-75" && d.DrugCodeB == "WAR-5");
                    if (!interactionExists)
                    {
                        _logger.LogInformation("Inserting missing Aspirin-Warfarin drug interaction...");
                        _context.DrugInteractions.Add(new DrugInteraction
                        {
                            Id = Guid.NewGuid(),
                            TenantId = defaultTenantId,
                            DrugCodeA = "ASP-75",
                            DrugCodeB = "WAR-5",
                            Severity = "Warning",
                            Description = "Increased risk of bleeding when Aspirin is co-administered with Warfarin."
                        });
                        await _context.SaveChangesAsync();
                    }

                    // Repair 6: Ensure CBC lab test catalog and HB parameter exist
                    var cbcExists = await _context.LabTestCatalogs.IgnoreQueryFilters()
                        .AnyAsync(l => l.TestCode == "CBC");
                    if (!cbcExists)
                    {
                        _logger.LogInformation("Inserting missing CBC lab catalog entry...");
                        var cbcId = Guid.NewGuid();
                        _context.LabTestCatalogs.Add(new LabTestCatalog
                        {
                            Id = cbcId,
                            TenantId = defaultTenantId,
                            TestCode = "CBC",
                            TestName = "Complete Blood Count",
                            Category = "Hematology",
                            SampleType = "Blood",
                            TurnaroundHours = 12,
                            Price = 350.00m,
                            IsActive = true
                        });
                        _context.LabTestParameters.Add(new LabTestParameter
                        {
                            Id = Guid.NewGuid(),
                            TenantId = defaultTenantId,
                            TestId = cbcId,
                            ParameterName = "Hemoglobin",
                            ParameterCode = "HB",
                            Unit = "g/dL",
                            ReferenceRangeLow = 12.0m,
                            ReferenceRangeHigh = 16.0m,
                            DataType = "Numeric"
                        });
                        await _context.SaveChangesAsync();
                    }

                    // Repair 7: Re-sync all role permissions from defaults
                    _logger.LogInformation("Re-seeding role permissions from defaults...");
                    await _permissionService.SeedRolePermissionsAsync(defaultTenantId);

                    // Repair 8: Ensure notification templates exist
                    _logger.LogInformation("Repairing/Seeding notification templates...");
                    await SeedNotificationTemplatesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during check-and-repair pass.");
                }
                finally
                {
                    TenantContext.BypassTenantId = null;
                }

                return;
            }

            var bypassContext = new BypassTenantContext(defaultTenantId);
            TenantContext.BypassTenantId = bypassContext.TenantId;

            // Generate IDs beforehand to resolve inter-dependency ordering
            var doctorId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var wardId = Guid.NewGuid();
            var bedId = Guid.NewGuid();
            var bedAllotmentId = Guid.NewGuid();
            var encounterId = Guid.NewGuid();
            var prescriptionId = Guid.NewGuid();
            var invoiceId = Guid.NewGuid();
            var claimId = Guid.NewGuid();
            var supplierId = Guid.NewGuid();
            var pharmacyItemId = Guid.NewGuid();
            var batchId = Guid.NewGuid();
            var aspirinItemId = Guid.NewGuid();
            var aspirinBatchId = Guid.NewGuid();
            var warfarinItemId = Guid.NewGuid();
            var warfarinBatchId = Guid.NewGuid();
            var poId = Guid.NewGuid();
            var grnId = Guid.NewGuid();
            var catalogId = Guid.NewGuid();
            var paramId = Guid.NewGuid();
            var reqId = Guid.NewGuid();
            var reqItemId = Guid.NewGuid();
            var reportId = Guid.NewGuid();
            var logId = Guid.NewGuid();

            try
            {
                _logger.LogInformation("Truncating all public database tables (except migration history)...");

                // 1. Truncate all tables in the public schema cascade
                if (_context.Database.IsRelational())
                {
                    await _context.Database.ExecuteSqlRawAsync(@"
                        DO $$ 
                        DECLARE 
                            r RECORD;
                        BEGIN
                            FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public' AND tablename <> '__EFMigrationsHistory') LOOP
                                EXECUTE 'TRUNCATE TABLE ' || quote_ident(r.tablename) || ' CASCADE;';
                            END LOOP;
                        END $$;");
                }
                else
                {
                    await _context.Database.EnsureDeletedAsync();
                    await _context.Database.EnsureCreatedAsync();
                }

                _logger.LogInformation("Database truncation/reset completed. Seeding tenant settings and roles...");

                // 2. Seed TenantSettings
                var tenantSettingsId = Guid.NewGuid();
                var tenantSettings = new TenantSettings
                {
                    Id = tenantSettingsId,
                    TenantId = defaultTenantId,
                    HospitalName = "CareSphere General Hospital",
                    AddressLine1 = "123 Healthcare Ave",
                    City = "Mumbai",
                    State = "Maharashtra",
                    PinCode = "400001",
                    Phone = "9876543210",
                    Email = "contact@caresphere.in",
                    IsActive = true,
                    SubscriptionTier = "Pro",
                    MaxUsersAllowed = 100
                };
                _context.TenantSettings.Add(tenantSettings);
                await _context.SaveChangesAsync();

                // 3. Seed Roles
                var rolesList = new[]
                {
                    CareSphereRoles.SuperAdmin, CareSphereRoles.HospitalAdmin,
                    CareSphereRoles.Doctor, CareSphereRoles.Nurse,
                    CareSphereRoles.Pharmacist, CareSphereRoles.LabTechnician,
                    CareSphereRoles.FrontDesk, CareSphereRoles.Finance,
                    CareSphereRoles.NabhAuditor, CareSphereRoles.Patient,
                    CareSphereRoles.Receptionist, CareSphereRoles.BillingStaff,
                    "platform_super_admin"
                };

                var roleIds = new Dictionary<string, string>();
                foreach (var rName in rolesList)
                {
                    var rId = Guid.NewGuid().ToString();
                    var roleObj = new IdentityRole
                    {
                        Id = rId,
                        Name = rName,
                        NormalizedName = rName.ToUpperInvariant()
                    };
                    var rRes = await _roleManager.CreateAsync(roleObj);
                    if (!rRes.Succeeded)
                    {
                        throw new Exception($"Failed to create role {rName}: {string.Join(", ", rRes.Errors)}");
                    }
                    roleIds[rName] = rId;
                }

                _logger.LogInformation("Roles created. Seeding user accounts for all roles...");

                // Helper local function to create and link users
                async Task<string> CreateUserAsync(string email, string fullName, string roleName, string password, Guid? linkedDoctorId = null, Guid? customTenantId = null)
                {
                    var userId = Guid.NewGuid().ToString();
                    var user = new ApplicationUser
                    {
                        Id = userId,
                        UserName = email,
                        Email = email,
                        FullName = fullName,
                        TenantId = customTenantId ?? defaultTenantId,
                        Role = roleName,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        PreferredLanguage = "en",
                        EmailConfirmed = true,
                        DoctorId = linkedDoctorId
                    };
                    var uRes = await _userManager.CreateAsync(user, password);
                    if (!uRes.Succeeded)
                    {
                        throw new Exception($"Failed to create user {email}: {string.Join(", ", uRes.Errors)}");
                    }
                    var lRes = await _userManager.AddToRoleAsync(user, roleName);
                    if (!lRes.Succeeded)
                    {
                        throw new Exception($"Failed to add user {email} to role {roleName}: {string.Join(", ", lRes.Errors)}");
                    }
                    return userId;
                };

                // Seed the 7 main login users
                var adminUserId = await CreateUserAsync("admin@caresphere.in", "System Administrator", CareSphereRoles.SuperAdmin, "Admin@123456");
                var receptionistUserId = await CreateUserAsync("receptionist@caresphere.dev", "Test Receptionist", CareSphereRoles.Receptionist, "Receptionist@123");
                var nurseUserId = await CreateUserAsync("nurse@caresphere.dev", "Test Nurse", CareSphereRoles.Nurse, "Nurse@123");
                var billingUserId = await CreateUserAsync("billingstaff@caresphere.dev", "Test Billing Staff", CareSphereRoles.BillingStaff, "BillingStaff@123");
                var doctorUserId = await CreateUserAsync("doctor@caresphere.dev", "Test Doctor", CareSphereRoles.Doctor, "Doctor@123", doctorId);
                var pharmacistUserId = await CreateUserAsync("pharmacist@caresphere.dev", "Test Pharmacist", CareSphereRoles.Pharmacist, "Pharmacist@123");
                var labtechUserId = await CreateUserAsync("labtech@caresphere.dev", "Test Lab Technician", CareSphereRoles.LabTechnician, "LabTech@123");
                var nurse2UserId = await CreateUserAsync("nurse2@caresphere.dev", "Test Nurse (Nursing Module)", CareSphereRoles.Nurse, "Nurse@123");
                var platformAdminUserId = await CreateUserAsync("platformadmin@caresphere.dev", "Platform Administrator", "platform_super_admin", "PlatformAdmin@123", customTenantId: Guid.Empty);

                _logger.LogInformation("User accounts seeded. Seeding identity link tables...");

                // 4. Seed UserClaims (1 record)
                var userClaim = new IdentityUserClaim<string>
                {
                    UserId = adminUserId,
                    ClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
                    ClaimValue = "admin@caresphere.in"
                };
                _context.UserClaims.Add(userClaim);

                // 5. Seed UserLogins (1 record)
                var userLogin = new IdentityUserLogin<string>
                {
                    LoginProvider = "Local",
                    ProviderKey = "admin@caresphere.in",
                    ProviderDisplayName = "Local",
                    UserId = adminUserId
                };
                _context.UserLogins.Add(userLogin);

                // 6. Seed UserTokens (1 record)
                var userToken = new IdentityUserToken<string>
                {
                    UserId = adminUserId,
                    LoginProvider = "Local",
                    Name = "AuthenticatorKey",
                    Value = "DUMMYTOKENVALUE123"
                };
                _context.UserTokens.Add(userToken);

                // 7. Seed RoleClaims (1 record)
                var roleClaim = new IdentityRoleClaim<string>
                {
                    RoleId = roleIds[CareSphereRoles.SuperAdmin],
                    ClaimType = "permission",
                    ClaimValue = "admin.all"
                };
                _context.RoleClaims.Add(roleClaim);

                // 8. RolePermission (1 record)
                var rolePerm = new RolePermission
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    RoleName = "DummyRole",
                    Permission = "dummy.permission"
                };
                _context.RolePermissions.Add(rolePerm);

                // 9. UserPermission (1 record)
                var userPerm = new UserPermission
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    UserId = adminUserId,
                    Permission = "dummy.user.permission",
                    GrantedAt = DateTime.UtcNow,
                    GrantedByUserId = adminUserId,
                    IsRevoked = false
                };
                _context.UserPermissions.Add(userPerm);

                // 10. UserSession (1 record)
                var userSession = new UserSession
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    UserId = adminUserId,
                    SessionToken = Guid.NewGuid().ToString(),
                    IpAddress = "127.0.0.1",
                    DeviceInfo = "Chrome / Windows",
                    UserAgent = "Mozilla/5.0 ...",
                    LastActivityAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(8),
                    IsRevoked = false
                };
                _context.UserSessions.Add(userSession);

                _logger.LogInformation("Identity entities seeded. Seeding Core Clinical & Ward records...");

                // 11. Patient (1 record)
                var patient = new Patient
                {
                    Id = patientId,
                    TenantId = defaultTenantId,
                    Mrn = "MRN-000001",
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Gender = "Male",
                    Phone = "9999999999",
                    Email = "john.doe@example.com",
                    Address = "456 Patient Lane",
                    BloodGroup = "O+"
                };
                _context.Patients.Add(patient);

                // 12. PatientPreference (1 record)
                var patientPreference = new PatientPreference
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    PatientId = patientId,
                    PreferredLanguage = "en",
                    PreferredChannel = "SMS",
                    OptOutSms = false,
                    OptOutWhatsApp = false,
                    OptOutVoice = false,
                    AllowAppointmentReminders = true,
                    AllowFollowUpReminders = true,
                    AllowDischargeNotifications = true,
                    AllowLabNotifications = true
                };
                _context.PatientPreferences.Add(patientPreference);

                // 13. Ward (1 record)
                var ward = new Ward
                {
                    Id = wardId,
                    TenantId = defaultTenantId,
                    Name = "General Ward A",
                    WardType = "General",
                    Floor = "1st Floor",
                    Building = "Main Building"
                };
                _context.Wards.Add(ward);

                // 14. Bed (1 record)
                var bed = new Bed
                {
                    Id = bedId,
                    TenantId = defaultTenantId,
                    BedNumber = "B-101",
                    WardId = wardId,
                    BedType = "Standard",
                    Status = "Occupied",
                    IsActive = true
                };
                _context.Beds.Add(bed);

                // 15. BedAllotment (1 record)
                var bedAllotment = new BedAllotment
                {
                    Id = bedAllotmentId,
                    TenantId = defaultTenantId,
                    BedId = bedId,
                    PatientId = patientId,
                    AdmissionDate = DateTime.UtcNow.AddDays(-1),
                    AdmissionType = "IPD",
                    AdmittingDoctor = "Dr. Smith",
                    Notes = "Initial admission",
                    Status = "Active"
                };
                _context.BedAllotments.Add(bedAllotment);

                // 16. BedTransfer (1 record)
                var bedTransfer = new BedTransfer
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    AllotmentId = bedAllotmentId,
                    FromBedId = bedId,
                    ToBedId = bedId,
                    TransferReason = "Routine ward update",
                    TransferredAt = DateTime.UtcNow
                };
                _context.BedTransfers.Add(bedTransfer);

                // 17. Doctor (1 record - linked to Doctor User above)
                var doctor = new Doctor
                {
                    Id = doctorId,
                    TenantId = defaultTenantId,
                    FirstName = "Jane",
                    LastName = "Smith",
                    Specialization = "Cardiology",
                    RegistrationNumber = "REG-12345",
                    Phone = "8888888888",
                    Email = "doctor@caresphere.dev",
                    IsActive = true
                };
                _context.Doctors.Add(doctor);

                // 18. DoctorQueueEntry (1 record)
                var doctorQueueEntry = new DoctorQueueEntry
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    DoctorId = doctorId,
                    PatientId = patientId,
                    Status = "Waiting",
                    QueuePosition = 1,
                    EstimatedWaitMinutes = 15,
                    CheckedInAt = DateTime.UtcNow
                };
                _context.DoctorQueueEntries.Add(doctorQueueEntry);

                // 19. Encounter (1 record)
                var encounter = new Encounter
                {
                    Id = encounterId,
                    TenantId = defaultTenantId,
                    PatientId = patientId,
                    DoctorId = doctorId,
                    EncounterType = "OPD",
                    Status = "InProgress",
                    AdmissionDate = DateTime.UtcNow,
                    ChiefComplaint = "Mild chest discomfort"
                };
                _context.Encounters.Add(encounter);

                // 20. SoapNote (1 record)
                var soapNote = new SoapNote
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    EncounterId = encounterId,
                    Subjective = "Patient reports mild chest tightness.",
                    Objective = "BP 120/80, Pulse 72, clear lung sounds.",
                    Assessment = "Atypical chest pain.",
                    Plan = "Order ECG and basic lab tests.",
                    Status = "Draft",
                    CreatedByDoctorId = doctorId
                };
                _context.SoapNotes.Add(soapNote);

                // 21. Prescription (1 record)
                var prescription = new Prescription
                {
                    Id = prescriptionId,
                    TenantId = defaultTenantId,
                    EncounterId = encounterId,
                    PatientId = patientId,
                    DoctorId = doctorId,
                    DrugName = "Aspirin",
                    DrugCode = "ASP-75",
                    Form = "Tablet",
                    Strength = "75mg",
                    Frequency = "Once daily",
                    Duration = "7 days",
                    Route = "Oral",
                    Quantity = 7,
                    Status = "Active",
                    IssuedAt = DateTime.UtcNow
                };
                _context.Prescriptions.Add(prescription);

                // 22. DrugFormulary (1 record)
                var drugFormulary = new DrugFormulary
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    DrugCode = "ASP-75",
                    GenericName = "Aspirin",
                    BrandName = "Ecotrin",
                    Form = "Tablet",
                    Strength = "75mg",
                    Unit = "Tablet",
                    IsControlled = false,
                    IsActive = true
                };
                _context.DrugFormulary.Add(drugFormulary);

                var paracetamolFormulary = new DrugFormulary
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    DrugCode = "PAR-500",
                    GenericName = "Paracetamol",
                    BrandName = "Crocin",
                    Form = "Tablet",
                    Strength = "500mg",
                    Unit = "Tablet",
                    IsControlled = false,
                    IsActive = true
                };
                _context.DrugFormulary.Add(paracetamolFormulary);

                var warfarinFormulary = new DrugFormulary
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    DrugCode = "WAR-5",
                    GenericName = "Warfarin",
                    BrandName = "Coumadin",
                    Form = "Tablet",
                    Strength = "5mg",
                    Unit = "Tablet",
                    IsControlled = false,
                    IsActive = true
                };
                _context.DrugFormulary.Add(warfarinFormulary);

                // 23. TeleConsultSession (1 record)
                var teleConsultSession = new TeleConsultSession
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    EncounterId = encounterId,
                    DoctorId = doctorId,
                    PatientId = patientId,
                    SessionType = "Consultation",
                    Status = "Scheduled",
                    MeetingLink = "https://meet.caresphere.in/consult-123",
                    DurationMinutes = 15
                };
                _context.TeleConsultSessions.Add(teleConsultSession);

                // 24. DrugInteraction (1 record)
                var drugInteraction = new DrugInteraction
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    DrugCodeA = "ASP-75",
                    DrugCodeB = "WAR-5",
                    Severity = "Warning",
                    Description = "Increased risk of bleeding when Aspirin is co-administered with Warfarin."
                };
                _context.DrugInteractions.Add(drugInteraction);

                // 25. AuditEvent (1 record)
                var auditEvent = new AuditEvent
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    UserId = adminUserId,
                    Action = "UserLogin",
                    ResourceType = "User",
                    ResourceId = adminUserId,
                    IpAddress = "127.0.0.1",
                    Device = "Desktop Browser",
                    Timestamp = DateTime.UtcNow,
                    Details = "SuperAdmin logged in successfully."
                };
                _context.AuditEvents.Add(auditEvent);

                _logger.LogInformation("Clinical records seeded. Seeding Billing & Payments...");

                // 26. BillingInvoice (1 record)
                var billingInvoice = new BillingInvoice
                {
                    Id = invoiceId,
                    TenantId = defaultTenantId,
                    InvoiceNumber = "INV-2026-0001",
                    PatientId = patientId,
                    EncounterId = encounterId,
                    InvoiceDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(30),
                    SubtotalAmount = 500.00m,
                    DiscountAmount = 50.00m,
                    TaxAmount = 45.00m,
                    TotalAmount = 495.00m,
                    PaidAmount = 0.00m,
                    BalanceAmount = 495.00m,
                    Status = "Draft",
                    GeneratedByUserId = adminUserId
                };
                _context.BillingInvoices.Add(billingInvoice);

                // 27. BillingLineItem (1 record)
                var billingLineItem = new BillingLineItem
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    InvoiceId = invoiceId,
                    ItemType = "Consultation",
                    ItemDescription = "OPD Consultation Fee",
                    ItemCode = "OPD-CONS",
                    Quantity = 1m,
                    UnitPrice = 500.00m,
                    DiscountPercent = 10m,
                    TaxPercent = 9m,
                    LineTotal = 495.00m
                };
                _context.BillingLineItems.Add(billingLineItem);

                // 28. Payment (1 record)
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    InvoiceId = invoiceId,
                    PatientId = patientId,
                    PaymentDate = DateTime.UtcNow,
                    Amount = 495.00m,
                    PaymentMethod = "Cash",
                    Status = "Success",
                    RecordedByUserId = adminUserId
                };
                _context.Payments.Add(payment);

                // 29. InsuranceClaim (1 record)
                var insuranceClaim = new InsuranceClaim
                {
                    Id = claimId,
                    TenantId = defaultTenantId,
                    InvoiceId = invoiceId,
                    PatientId = patientId,
                    EncounterId = encounterId,
                    ClaimNumber = "CLM-2026-0001",
                    InsuranceProvider = "Star Health Insurance",
                    PolicyNumber = "POL-987654",
                    MemberName = "John Doe",
                    ClaimedAmount = 495.00m,
                    Status = "Draft"
                };
                _context.InsuranceClaims.Add(insuranceClaim);

                // 30. ClaimStatusHistory (1 record)
                var claimStatusHistory = new ClaimStatusHistory
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    ClaimId = claimId,
                    PreviousStatus = "None",
                    NewStatus = "Draft",
                    ChangedAt = DateTime.UtcNow,
                    ChangedByUserId = adminUserId,
                    Remarks = "Claim initialized in draft status."
                };
                _context.ClaimStatusHistories.Add(claimStatusHistory);

                // 31. InvoiceDocument (1 record)
                var invoiceDocument = new InvoiceDocument
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    InvoiceId = invoiceId,
                    DocumentType = "Invoice",
                    StoragePath = "documents/invoices/INV-2026-0001.pdf",
                    FileName = "INV-2026-0001.pdf",
                    GeneratedAt = DateTime.UtcNow,
                    FileSizeBytes = 102400,
                    IsLatest = true
                };
                _context.InvoiceDocuments.Add(invoiceDocument);

                _logger.LogInformation("Billing records seeded. Seeding Pharmacy & Inventory...");

                // 32. Supplier (1 record)
                var supplier = new Supplier
                {
                    Id = supplierId,
                    TenantId = defaultTenantId,
                    SupplierName = "MedLife Pharmaceuticals",
                    ContactPerson = "Mr. Amit Sharma",
                    Phone = "9876500001",
                    Email = "sales@medlife.com",
                    Address = "78 Pharma Zone, Andheri, Mumbai",
                    GstNumber = "27AAAAA0000A1Z1",
                    LicenseNumber = "DL-12345/2026",
                    IsActive = true
                };
                _context.Suppliers.Add(supplier);

                // 33. PharmacyItem (1 record)
                var pharmacyItem = new PharmacyItem
                {
                    Id = pharmacyItemId,
                    TenantId = defaultTenantId,
                    ItemCode = "PAR-500",
                    ItemName = "Paracetamol 500mg",
                    GenericName = "Paracetamol",
                    Category = "Medicine",
                    Form = "Tablet",
                    Strength = "500mg",
                    Unit = "Strip",
                    Barcode = "8901234567890",
                    IsControlled = false,
                    RequiresPrescription = false,
                    ReorderLevel = 100,
                    IsActive = true
                };
                _context.PharmacyItems.Add(pharmacyItem);

                var aspirinItem = new PharmacyItem
                {
                    Id = aspirinItemId,
                    TenantId = defaultTenantId,
                    ItemCode = "ASP-75",
                    ItemName = "Aspirin 75mg",
                    GenericName = "Aspirin",
                    Category = "Medicine",
                    Form = "Tablet",
                    Strength = "75mg",
                    Unit = "Strip",
                    Barcode = "501234567800",
                    IsControlled = false,
                    RequiresPrescription = true,
                    ReorderLevel = 50,
                    IsActive = true
                };
                _context.PharmacyItems.Add(aspirinItem);

                var warfarinItem = new PharmacyItem
                {
                    Id = warfarinItemId,
                    TenantId = defaultTenantId,
                    ItemCode = "WAR-5",
                    ItemName = "Warfarin 5mg",
                    GenericName = "Warfarin",
                    Category = "Medicine",
                    Form = "Tablet",
                    Strength = "5mg",
                    Unit = "Strip",
                    Barcode = "501234567801",
                    IsControlled = false,
                    RequiresPrescription = true,
                    ReorderLevel = 50,
                    IsActive = true
                };
                _context.PharmacyItems.Add(warfarinItem);

                // 34. PharmacyBatch (1 record)
                var pharmacyBatch = new PharmacyBatch
                {
                    Id = batchId,
                    TenantId = defaultTenantId,
                    ItemId = pharmacyItemId,
                    BatchNumber = "BAT-PAR-99",
                    SupplierId = supplierId,
                    ManufactureDate = DateTime.UtcNow.AddMonths(-3),
                    ExpiryDate = DateTime.UtcNow.AddYears(2),
                    PurchasePrice = 10.00m,
                    SellingPrice = 15.00m,
                    CurrentStock = 500,
                    ReservedStock = 0,
                    IsActive = true
                };
                _context.PharmacyBatches.Add(pharmacyBatch);

                var aspirinBatch = new PharmacyBatch
                {
                    Id = aspirinBatchId,
                    TenantId = defaultTenantId,
                    ItemId = aspirinItemId,
                    BatchNumber = "BAT-ASP-75",
                    SupplierId = supplierId,
                    ManufactureDate = DateTime.UtcNow.AddMonths(-3),
                    ExpiryDate = DateTime.UtcNow.AddYears(2),
                    PurchasePrice = 8.00m,
                    SellingPrice = 10.00m,
                    CurrentStock = 300,
                    ReservedStock = 0,
                    IsActive = true
                };
                _context.PharmacyBatches.Add(aspirinBatch);

                var warfarinBatch = new PharmacyBatch
                {
                    Id = warfarinBatchId,
                    TenantId = defaultTenantId,
                    ItemId = warfarinItemId,
                    BatchNumber = "BAT-WAR-01",
                    SupplierId = supplierId,
                    ManufactureDate = DateTime.UtcNow.AddMonths(-3),
                    ExpiryDate = DateTime.UtcNow.AddYears(2),
                    PurchasePrice = 25.00m,
                    SellingPrice = 35.00m,
                    CurrentStock = 200,
                    ReservedStock = 0,
                    IsActive = true
                };
                _context.PharmacyBatches.Add(warfarinBatch);

                // 35. PurchaseOrder (1 record)
                var purchaseOrder = new PurchaseOrder
                {
                    Id = poId,
                    TenantId = defaultTenantId,
                    PoNumber = "PO-20260612-001",
                    SupplierId = supplierId,
                    OrderDate = DateTime.UtcNow,
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(7),
                    Status = "Draft",
                    TotalAmount = 1000.00m,
                    CreatedByUserId = adminUserId
                };
                _context.PurchaseOrders.Add(purchaseOrder);

                // 36. PurchaseOrderItem (1 record)
                var purchaseOrderItem = new PurchaseOrderItem
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    PoId = poId,
                    ItemId = pharmacyItemId,
                    RequestedQuantity = 100,
                    ReceivedQuantity = 0,
                    UnitPrice = 10.00m,
                    TotalPrice = 1000.00m
                };
                _context.PurchaseOrderItems.Add(purchaseOrderItem);

                // 37. GoodsReceivedNote (1 record)
                var goodsReceivedNote = new GoodsReceivedNote
                {
                    Id = grnId,
                    TenantId = defaultTenantId,
                    GrnNumber = "GRN-20260612-001",
                    PoId = poId,
                    SupplierId = supplierId,
                    ReceivedDate = DateTime.UtcNow,
                    InvoiceNumber = "INV-SUP-987",
                    TotalAmount = 1000.00m,
                    Status = "Draft",
                    ReceivedByUserId = adminUserId
                };
                _context.GoodsReceivedNotes.Add(goodsReceivedNote);

                // 38. GrnItem (1 record)
                var grnItem = new GrnItem
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    GrnId = grnId,
                    ItemId = pharmacyItemId,
                    BatchNumber = "BAT-PAR-99",
                    ManufactureDate = DateTime.UtcNow.AddMonths(-3),
                    ExpiryDate = DateTime.UtcNow.AddYears(2),
                    ReceivedQuantity = 100,
                    FreeQuantity = 0,
                    PurchasePrice = 10.00m,
                    SellingPrice = 15.00m,
                    TotalAmount = 1000.00m,
                    BatchId = batchId
                };
                _context.GrnItems.Add(grnItem);

                // 39. StockLedgerEntry (1 record)
                var stockLedgerEntry = new StockLedgerEntry
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    ItemId = pharmacyItemId,
                    BatchId = batchId,
                    TransactionType = "GRN",
                    ReferenceId = grnId.ToString(),
                    ReferenceType = "GRN",
                    QuantityIn = 100,
                    QuantityOut = 0,
                    BalanceAfter = 500,
                    TransactionDate = DateTime.UtcNow,
                    CreatedByUserId = adminUserId
                };
                _context.StockLedgerEntries.Add(stockLedgerEntry);

                // 40. DispenseRecord (1 record)
                var dispenseRecord = new DispenseRecord
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    PrescriptionId = prescriptionId,
                    PatientId = patientId,
                    ItemId = pharmacyItemId,
                    BatchId = batchId,
                    DispensedQuantity = 7,
                    DispensedAt = DateTime.UtcNow,
                    DispensedByUserId = adminUserId,
                    BarcodeScanned = "8901234567890"
                };
                _context.DispenseRecords.Add(dispenseRecord);

                // 41. OtcSale (1 record)
                var otcSaleId = Guid.NewGuid();
                var otcSale = new OtcSale
                {
                    Id = otcSaleId,
                    TenantId = defaultTenantId,
                    SaleNumber = "OTC-20260612-001",
                    SaleDate = DateTime.UtcNow,
                    CustomerName = "Walk-in Patient",
                    CustomerPhone = "9999911111",
                    TotalAmount = 15.00m,
                    PaymentMethod = "UPI",
                    PaymentStatus = "Paid",
                    CreatedByUserId = adminUserId
                };
                _context.OtcSales.Add(otcSale);

                // 42. OtcSaleItem (1 record)
                var otcSaleItem = new OtcSaleItem
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    SaleId = otcSaleId,
                    ItemId = pharmacyItemId,
                    BatchId = batchId,
                    BarcodeScanned = "8901234567890",
                    Quantity = 1,
                    UnitPrice = 15.00m,
                    TotalPrice = 15.00m
                };
                _context.OtcSaleItems.Add(otcSaleItem);

                // 43. ExpiryAlert (1 record)
                var expiryAlert = new ExpiryAlert
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    BatchId = batchId,
                    ItemId = pharmacyItemId,
                    ExpiryDate = DateTime.UtcNow.AddYears(2),
                    DaysUntilExpiry = 730,
                    AlertType = "InApp",
                    SentAt = DateTime.UtcNow,
                    IsAcknowledged = false
                };
                _context.ExpiryAlerts.Add(expiryAlert);

                _logger.LogInformation("Pharmacy records seeded. Seeding Laboratory...");

                // 44. LabTestCatalog (1 record)
                var labTestCatalog = new LabTestCatalog
                {
                    Id = catalogId,
                    TenantId = defaultTenantId,
                    TestCode = "CBC",
                    TestName = "Complete Blood Count",
                    Category = "Hematology",
                    SampleType = "Blood",
                    TurnaroundHours = 12,
                    Price = 350.00m,
                    IsActive = true
                };
                _context.LabTestCatalogs.Add(labTestCatalog);

                // 45. LabTestParameter (1 record)
                var labTestParameter = new LabTestParameter
                {
                    Id = paramId,
                    TenantId = defaultTenantId,
                    TestId = catalogId,
                    ParameterName = "Hemoglobin",
                    ParameterCode = "HB",
                    Unit = "g/dL",
                    ReferenceRangeLow = 12.0m,
                    ReferenceRangeHigh = 16.0m,
                    DataType = "Numeric"
                };
                _context.LabTestParameters.Add(labTestParameter);

                // 46. LabRequisition (1 record)
                var labRequisition = new LabRequisition
                {
                    Id = reqId,
                    TenantId = defaultTenantId,
                    RequisitionNumber = "REQ-2026-0001",
                    PatientId = patientId,
                    EncounterId = encounterId,
                    OrderedByDoctorId = doctorId,
                    OrderedAt = DateTime.UtcNow,
                    Priority = "Routine",
                    Status = "Ordered"
                };
                _context.LabRequisitions.Add(labRequisition);

                // 47. LabRequisitionItem (1 record)
                var labRequisitionItem = new LabRequisitionItem
                {
                    Id = reqItemId,
                    TenantId = defaultTenantId,
                    RequisitionId = reqId,
                    TestId = catalogId,
                    Status = "Ordered"
                };
                _context.LabRequisitionItems.Add(labRequisitionItem);

                // 48. LabSample (1 record)
                var labSample = new LabSample
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    RequisitionId = reqId,
                    SampleType = "Blood",
                    CollectedAt = DateTime.UtcNow,
                    CollectedByUserId = adminUserId,
                    BarcodeLabel = "SMPL-CBC-0001",
                    IsReceived = true,
                    ReceivedAt = DateTime.UtcNow,
                    ReceivedByUserId = adminUserId
                };
                _context.LabSamples.Add(labSample);

                // 49. LabResult (1 record)
                var labResult = new LabResult
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    RequisitionItemId = reqItemId,
                    ParameterId = paramId,
                    ResultValue = "14.5",
                    ResultNumeric = 14.5m,
                    ResultUnit = "g/dL",
                    ReferenceRangeLow = 12.0m,
                    ReferenceRangeHigh = 16.0m,
                    IsAbnormal = false,
                    EnteredByUserId = adminUserId,
                    EnteredAt = DateTime.UtcNow,
                    VerifiedByUserId = adminUserId,
                    VerifiedAt = DateTime.UtcNow
                };
                _context.LabResults.Add(labResult);

                // 50. LabReport (1 record)
                var labReport = new LabReport
                {
                    Id = reportId,
                    TenantId = defaultTenantId,
                    RequisitionId = reqId,
                    GeneratedAt = DateTime.UtcNow,
                    GeneratedByUserId = adminUserId,
                    StoragePath = "documents/lab-reports/REQ-2026-0001.pdf",
                    FileName = "REQ-2026-0001.pdf",
                    IsLatest = true,
                    InAppNotificationSent = true
                };
                _context.LabReports.Add(labReport);

                // 51. InAppNotification (1 record)
                var inAppNotification = new InAppNotification
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    RecipientType = "Doctor",
                    RecipientId = doctorId.ToString(),
                    Title = "Lab Report Ready",
                    Message = "Complete Blood Count report is ready.",
                    ResourceType = "LabReport",
                    ResourceId = reportId.ToString(),
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.InAppNotifications.Add(inAppNotification);

                _logger.LogInformation("Lab records seeded. Seeding Notifications...");

                // 52. NotificationTemplate (1 record)
                var notificationTemplate = new NotificationTemplate
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    TemplateName = "General Alert Template",
                    NotificationType = "GeneralAlert",
                    Channel = "SMS",
                    Language = "en",
                    TemplateBody = "Hello {PatientName}, this is a general notification from CareSphere.",
                    IsActive = true
                };
                _context.NotificationTemplates.Add(notificationTemplate);

                // 53. NotificationLog (1 record)
                var notificationLog = new NotificationLog
                {
                    Id = logId,
                    TenantId = defaultTenantId,
                    PatientId = patientId,
                    RecipientPhone = "9999999999",
                    RecipientName = "John Doe",
                    Channel = "SMS",
                    NotificationType = "GeneralAlert",
                    Language = "en",
                    MessageBody = "Hello John Doe, this is a general notification from CareSphere.",
                    Status = "Sent",
                    SentAt = DateTime.UtcNow,
                    RetryCount = 0,
                    MaxRetries = 3
                };
                _context.NotificationLogs.Add(notificationLog);

                // 54. AppointmentReminder (1 record)
                var appointmentReminder = new AppointmentReminder
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    PatientId = patientId,
                    DoctorId = doctorId,
                    AppointmentDate = DateTime.UtcNow.AddDays(1),
                    ReminderType = "TwentyFourHour",
                    Channel = "SMS",
                    Language = "en",
                    Status = "Scheduled",
                    ScheduledAt = DateTime.UtcNow,
                    NotificationLogId = logId
                };
                _context.AppointmentReminders.Add(appointmentReminder);

                // 55. DischargeNotification (1 record)
                var dischargeNotification = new DischargeNotification
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    AllotmentId = bedAllotmentId,
                    PatientId = patientId,
                    DischargedAt = DateTime.UtcNow,
                    Channel = "SMS",
                    Language = "en",
                    Status = "Pending",
                    NotificationLogId = logId
                };
                _context.DischargeNotifications.Add(dischargeNotification);

                // 56. ServiceBusOutbox (1 record)
                var serviceBusOutbox = new ServiceBusOutbox
                {
                    Id = Guid.NewGuid(),
                    TenantId = defaultTenantId,
                    MessageType = "PatientCreated",
                    Payload = "{ \"PatientId\": \"" + patientId + "\" }",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                _context.ServiceBusOutboxes.Add(serviceBusOutbox);

                // Seed notification templates
                _logger.LogInformation("Seeding notification templates...");
                await SeedNotificationTemplatesAsync();

                _logger.LogInformation("Saving all seeded records to the database...");
                await _context.SaveChangesAsync();

                _logger.LogInformation("Database reset and role-based seeding completed successfully for tenant: {TenantId}", defaultTenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset and seed database.");
                throw;
            }
            finally
            {
                TenantContext.BypassTenantId = null;
            }
        }
    }
}
