using System;
using System.Linq;
using CareSphere.Models;

namespace CareSphere.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Seed Drug Formulary if empty
            if (!context.DrugFormulary.Any())
            {
                var drugs = new[]
                {
                    new DrugFormulary
                    {
                        Id = Guid.NewGuid(),
                        DrugCode = "DF-PARA-500",
                        GenericName = "Paracetamol",
                        BrandName = "Calpol",
                        Form = "Tablet",
                        Strength = "500",
                        Unit = "mg",
                        IsControlled = false,
                        IsActive = true,
                        TenantId = Guid.Empty
                    },
                    new DrugFormulary
                    {
                        Id = Guid.NewGuid(),
                        DrugCode = "DF-AMOX-500",
                        GenericName = "Amoxicillin",
                        BrandName = "Amoxil",
                        Form = "Capsule",
                        Strength = "500",
                        Unit = "mg",
                        IsControlled = false,
                        IsActive = true,
                        TenantId = Guid.Empty
                    },
                    new DrugFormulary
                    {
                        Id = Guid.NewGuid(),
                        DrugCode = "DF-IBU-400",
                        GenericName = "Ibuprofen",
                        BrandName = "Advil",
                        Form = "Tablet",
                        Strength = "400",
                        Unit = "mg",
                        IsControlled = false,
                        IsActive = true,
                        TenantId = Guid.Empty
                    },
                    new DrugFormulary
                    {
                        Id = Guid.NewGuid(),
                        DrugCode = "DF-MET-500",
                        GenericName = "Metformin",
                        BrandName = "Glucophage",
                        Form = "Tablet",
                        Strength = "500",
                        Unit = "mg",
                        IsControlled = false,
                        IsActive = true,
                        TenantId = Guid.Empty
                    },
                    new DrugFormulary
                    {
                        Id = Guid.NewGuid(),
                        DrugCode = "DF-ATO-10",
                        GenericName = "Atorvastatin",
                        BrandName = "Lipitor",
                        Form = "Tablet",
                        Strength = "10",
                        Unit = "mg",
                        IsControlled = false,
                        IsActive = true,
                        TenantId = Guid.Empty
                    },
                    new DrugFormulary
                    {
                        Id = Guid.NewGuid(),
                        DrugCode = "DF-CLOP-75",
                        GenericName = "Clopidogrel",
                        BrandName = "Plavix",
                        Form = "Tablet",
                        Strength = "75",
                        Unit = "mg",
                        IsControlled = false,
                        IsActive = true,
                        TenantId = Guid.Empty
                    },
                    new DrugFormulary
                    {
                        Id = Guid.NewGuid(),
                        DrugCode = "DF-ASP-81",
                        GenericName = "Aspirin",
                        BrandName = "Ecotrin",
                        Form = "Tablet",
                        Strength = "81",
                        Unit = "mg",
                        IsControlled = false,
                        IsActive = true,
                        TenantId = Guid.Empty
                    },
                    new DrugFormulary
                    {
                        Id = Guid.NewGuid(),
                        DrugCode = "DF-WARF-5",
                        GenericName = "Warfarin",
                        BrandName = "Coumadin",
                        Form = "Tablet",
                        Strength = "5",
                        Unit = "mg",
                        IsControlled = false,
                        IsActive = true,
                        TenantId = Guid.Empty
                    },
                    new DrugFormulary
                    {
                        Id = Guid.NewGuid(),
                        DrugCode = "DF-TRAM-50",
                        GenericName = "Tramadol",
                        BrandName = "Ultram",
                        Form = "Tablet",
                        Strength = "50",
                        Unit = "mg",
                        IsControlled = true,
                        IsActive = true,
                        TenantId = Guid.Empty
                    }
                };

                context.DrugFormulary.AddRange(drugs);
                context.SaveChanges();
            }

            // Seed Drug Interactions if empty
            if (!context.DrugInteractions.Any())
            {
                var interactions = new[]
                {
                    new DrugInteraction
                    {
                        Id = Guid.NewGuid(),
                        DrugCodeA = "DF-CLOP-75",
                        DrugCodeB = "DF-ASP-81",
                        Severity = "Warning",
                        Description = "Moderate interaction: Increased risk of bleeding.",
                        TenantId = Guid.Empty
                    },
                    new DrugInteraction
                    {
                        Id = Guid.NewGuid(),
                        DrugCodeA = "DF-WARF-5",
                        DrugCodeB = "DF-ASP-81",
                        Severity = "Contraindicated",
                        Description = "High risk of severe bleeding. Do not co-prescribe without strict monitoring.",
                        TenantId = Guid.Empty
                    },
                    new DrugInteraction
                    {
                        Id = Guid.NewGuid(),
                        DrugCodeA = "DF-IBU-400",
                        DrugCodeB = "DF-ASP-81",
                        Severity = "Warning",
                        Description = "Ibuprofen may decrease the cardioprotective effect of low-dose aspirin.",
                        TenantId = Guid.Empty
                    }
                };

                context.DrugInteractions.AddRange(interactions);
                context.SaveChanges();
            }
        }
    }
}
