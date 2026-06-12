// Regression guard: these tests ensure permission boundaries between roles
// are never accidentally widened. Must pass on every PR.
using System;
using System.Linq;
using CareSphere.Models;
using Xunit;

namespace CareSphere.Tests
{
    public class RolePermissionTests
    {
        [Fact]
        public void Receptionist_ShouldNot_Have_BillingEdit()
        {
            var permissions = RolePermissionDefaults.DefaultPermissions["Receptionist"];
            Assert.DoesNotContain("Billing.Edit", permissions);
        }

        [Fact]
        public void Receptionist_ShouldNot_Have_EMRAccess()
        {
            var permissions = RolePermissionDefaults.DefaultPermissions["Receptionist"];
            Assert.DoesNotContain("Encounter.View", permissions);
            Assert.DoesNotContain("SoapNote.View", permissions);
            Assert.DoesNotContain("Prescription.View", permissions);
        }

        [Fact]
        public void Nurse_ShouldNot_Have_PrescriptionCreate()
        {
            var permissions = RolePermissionDefaults.DefaultPermissions["Nurse"];
            Assert.DoesNotContain("Prescription.Create", permissions);
        }

        [Fact]
        public void Nurse_ShouldNot_Have_BillingAccess()
        {
            var permissions = RolePermissionDefaults.DefaultPermissions["Nurse"];
            Assert.DoesNotContain("Billing.View", permissions);
            Assert.DoesNotContain("Billing.Edit", permissions);
        }

        [Fact]
        public void BillingStaff_ShouldNot_Have_RoleManagement()
        {
            var permissions = RolePermissionDefaults.DefaultPermissions["BillingStaff"];
            Assert.DoesNotContain("Roles.Manage", permissions);
            Assert.DoesNotContain("Users.Manage", permissions);
        }

        [Fact]
        public void BillingStaff_ShouldNot_Have_EMRAccess()
        {
            var permissions = RolePermissionDefaults.DefaultPermissions["BillingStaff"];
            Assert.DoesNotContain("Encounter.View", permissions);
            Assert.DoesNotContain("SoapNote.View", permissions);
        }

        [Fact]
        public void AllNewRoles_ShouldExist_InDefaults()
        {
            var keys = RolePermissionDefaults.DefaultPermissions.Keys;
            Assert.Contains("Receptionist", keys);
            Assert.Contains("Nurse", keys);
            Assert.Contains("BillingStaff", keys);
            Assert.Contains("Patient", keys);
        }
    }
}
