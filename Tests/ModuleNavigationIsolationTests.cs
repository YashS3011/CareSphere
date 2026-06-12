using System;
using System.IO;
using Xunit;

namespace CareSphere.Tests
{
    public class ModuleNavigationIsolationTests
    {
        private static string GetWorkspaceRoot()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "CareSphere.sln")))
            {
                dir = dir.Parent;
            }
            if (dir == null)
            {
                throw new InvalidOperationException("Could not find workspace root containing CareSphere.sln");
            }
            return dir.FullName;
        }

        private static string ReadLayoutContent(string moduleName, string layoutName)
        {
            var root = GetWorkspaceRoot();
            var path = Path.Combine(root, "Modules", moduleName, "Layout", layoutName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Layout file not found: {path}");
            }
            return File.ReadAllText(path);
        }

        [Fact]
        public void PatientsLayout_ShouldNot_Contain_BillingLinks()
        {
            var content = ReadLayoutContent("Patients", "PatientsLayout.razor");

            // Should not leak links to billing, lab, pharmacy, clinical, encounter, wards, beds, or admin.
            var forbiddenHrefs = new[]
            {
                "href=\"billing", "href=\"/billing",
                "href=\"lab", "href=\"/lab", "href=\"laboratory", "href=\"/laboratory",
                "href=\"pharmacy", "href=\"/pharmacy",
                "href=\"clinical", "href=\"/clinical", "href=\"doctor", "href=\"/doctor",
                "href=\"encounter", "href=\"/encounter",
                "href=\"wards", "href=\"/wards",
                "href=\"beds", "href=\"/beds",
                "href=\"admin", "href=\"/admin"
            };

            foreach (var href in forbiddenHrefs)
            {
                Assert.True(!content.Contains(href, StringComparison.OrdinalIgnoreCase), 
                    $"PatientsLayout.razor should not contain forbidden link: {href}");
            }
        }

        [Fact]
        public void PharmacyLayout_ShouldNot_Contain_ClinicalLinks()
        {
            var content = ReadLayoutContent("Pharmacy", "PharmacyLayout.razor");

            // Should not leak links to clinical or billing/lab
            var forbiddenHrefs = new[]
            {
                "href=\"clinical", "href=\"/clinical", "href=\"doctor", "href=\"/doctor",
                "href=\"encounter", "href=\"/encounter",
                "href=\"billing", "href=\"/billing",
                "href=\"lab", "href=\"/lab", "href=\"laboratory", "href=\"/laboratory"
            };

            foreach (var href in forbiddenHrefs)
            {
                Assert.True(!content.Contains(href, StringComparison.OrdinalIgnoreCase), 
                    $"PharmacyLayout.razor should not contain forbidden link: {href}");
            }
        }

        [Fact]
        public void BillingLayout_ShouldNot_Contain_ClinicalOrLabLinks()
        {
            var content = ReadLayoutContent("Billing", "BillingLayout.razor");

            // Should not leak links to clinical, lab, pharmacy, wards, or beds.
            var forbiddenHrefs = new[]
            {
                "href=\"clinical", "href=\"/clinical", "href=\"doctor", "href=\"/doctor",
                "href=\"encounter", "href=\"/encounter",
                "href=\"lab", "href=\"/lab", "href=\"laboratory", "href=\"/laboratory",
                "href=\"pharmacy", "href=\"/pharmacy",
                "href=\"wards", "href=\"/wards",
                "href=\"beds", "href=\"/beds"
            };

            foreach (var href in forbiddenHrefs)
            {
                Assert.True(!content.Contains(href, StringComparison.OrdinalIgnoreCase), 
                    $"BillingLayout.razor should not contain forbidden link: {href}");
            }
        }

        [Fact]
        public void ClinicalLayout_ShouldNot_Contain_BillingOrLabLinks()
        {
            var content = ReadLayoutContent("Clinical", "ClinicalLayout.razor");

            // Should not leak links to billing, lab, pharmacy, wards, or admin.
            var forbiddenHrefs = new[]
            {
                "href=\"billing", "href=\"/billing",
                "href=\"lab", "href=\"/lab", "href=\"laboratory", "href=\"/laboratory",
                "href=\"pharmacy", "href=\"/pharmacy",
                "href=\"wards", "href=\"/wards",
                "href=\"beds", "href=\"/beds",
                "href=\"admin", "href=\"/admin"
            };

            foreach (var href in forbiddenHrefs)
            {
                Assert.True(!content.Contains(href, StringComparison.OrdinalIgnoreCase), 
                    $"ClinicalLayout.razor should not contain forbidden link: {href}");
            }
        }

        [Fact]
        public void WardLayout_ShouldNot_Contain_ClinicalOrBillingLinks()
        {
            var content = ReadLayoutContent("Ward", "WardLayout.razor");

            // Should not leak links to clinical, billing, lab, pharmacy, or admin.
            var forbiddenHrefs = new[]
            {
                "href=\"clinical", "href=\"/clinical", "href=\"doctor", "href=\"/doctor",
                "href=\"encounter", "href=\"/encounter",
                "href=\"billing", "href=\"/billing",
                "href=\"lab", "href=\"/lab", "href=\"laboratory", "href=\"/laboratory",
                "href=\"pharmacy", "href=\"/pharmacy",
                "href=\"admin", "href=\"/admin"
            };

            foreach (var href in forbiddenHrefs)
            {
                Assert.True(!content.Contains(href, StringComparison.OrdinalIgnoreCase), 
                    $"WardLayout.razor should not contain forbidden link: {href}");
            }
        }
    }
}
