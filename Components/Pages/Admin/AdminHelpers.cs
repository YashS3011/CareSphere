namespace CareSphere.Components.Pages.Admin
{
    /// <summary>
    /// Centralized helper for admin module pages — eliminates duplicated role lists,
    /// language dictionaries, and badge styling scattered across multiple Razor pages.
    /// </summary>
    public static class AdminHelpers
    {
        /// <summary>
        /// Returns all CareSphere roles in display order. Single source of truth used
        /// by UserList, UserCreate, UserEdit, and RolePermissions pages.
        /// </summary>
        public static IReadOnlyList<string> GetAllRoles() => new[]
        {
            CareSphereRoles.SuperAdmin,
            CareSphereRoles.HospitalAdmin,
            CareSphereRoles.Doctor,
            CareSphereRoles.Nurse,
            CareSphereRoles.Pharmacist,
            CareSphereRoles.LabTechnician,
            CareSphereRoles.FrontDesk,
            CareSphereRoles.Finance,
            CareSphereRoles.NabhAuditor,
            CareSphereRoles.Patient,
            CareSphereRoles.Receptionist,
            CareSphereRoles.BillingStaff,
        };

        /// <summary>
        /// Supported UI languages with BCP-47 codes. Replaces hardcoded dropdown
        /// options across UserEdit and future preference pages.
        /// </summary>
        public static IReadOnlyDictionary<string, string> GetSupportedLanguages() => new Dictionary<string, string>
        {
            ["en"] = "English",
            ["hi"] = "Hindi",
            ["ta"] = "Tamil",
            ["te"] = "Telugu",
            ["mr"] = "Marathi",
            ["bn"] = "Bengali",
            ["gu"] = "Gujarati",
            ["kn"] = "Kannada",
            ["ml"] = "Malayalam",
            ["pa"] = "Punjabi",
        };

        /// <summary>
        /// Returns a Bootstrap badge CSS class for the given role.
        /// </summary>
        public static string GetRoleBadgeClass(string role) => role switch
        {
            CareSphereRoles.SuperAdmin => "bg-danger",
            CareSphereRoles.HospitalAdmin => "bg-warning text-dark",
            CareSphereRoles.Doctor => "bg-primary",
            CareSphereRoles.Nurse => "bg-info text-dark",
            CareSphereRoles.Pharmacist => "bg-success",
            CareSphereRoles.LabTechnician => "bg-secondary",
            CareSphereRoles.FrontDesk => "bg-dark",
            CareSphereRoles.Finance => "bg-dark",
            CareSphereRoles.NabhAuditor => "bg-secondary",
            CareSphereRoles.Patient => "bg-light text-dark",
            CareSphereRoles.Receptionist => "bg-primary text-white",
            CareSphereRoles.BillingStaff => "bg-warning text-dark",
            _ => "bg-light text-dark",
        };

        /// <summary>
        /// Returns a Bootstrap icon class for the given role.
        /// </summary>
        public static string GetRoleIcon(string role) => role switch
        {
            CareSphereRoles.SuperAdmin => "bi-stars",
            CareSphereRoles.HospitalAdmin => "bi-building",
            CareSphereRoles.Doctor => "bi-person-badge",
            CareSphereRoles.Nurse => "bi-heart-fill",
            CareSphereRoles.Pharmacist => "bi-capsule",
            CareSphereRoles.LabTechnician => "bi-flask",
            CareSphereRoles.FrontDesk => "bi-reception-4",
            CareSphereRoles.Finance => "bi-wallet2",
            CareSphereRoles.NabhAuditor => "bi-clipboard-check",
            CareSphereRoles.Patient => "bi-person",
            CareSphereRoles.Receptionist => "bi-telephone-fill",
            CareSphereRoles.BillingStaff => "bi-receipt-cutoff",
            _ => "bi-shield",
        };

        /// <summary>
        /// Returns a background CSS class for role cards.
        /// </summary>
        public static string GetRoleBgClass(string role) => role switch
        {
            CareSphereRoles.SuperAdmin => "bg-danger",
            CareSphereRoles.HospitalAdmin => "bg-warning",
            CareSphereRoles.Doctor => "bg-primary",
            CareSphereRoles.Nurse => "bg-info",
            CareSphereRoles.Pharmacist => "bg-success",
            CareSphereRoles.Receptionist => "bg-primary",
            CareSphereRoles.BillingStaff => "bg-warning",
            _ => "bg-secondary",
        };

        /// <summary>
        /// Returns a short description for the given role.
        /// </summary>
        public static string GetRoleDescription(string role) => role switch
        {
            CareSphereRoles.SuperAdmin => "Full system access",
            CareSphereRoles.HospitalAdmin => "Admin minus clinical writes",
            CareSphereRoles.Doctor => "Clinical workflows",
            CareSphereRoles.Nurse => "Care and bedside",
            CareSphereRoles.Pharmacist => "Pharmacy operations",
            CareSphereRoles.LabTechnician => "Lab samples and results",
            CareSphereRoles.FrontDesk => "Reception and billing",
            CareSphereRoles.Finance => "Billing and payments",
            CareSphereRoles.NabhAuditor => "Read-only compliance",
            CareSphereRoles.Patient => "Own record only",
            CareSphereRoles.Receptionist => "Reception and front desk",
            CareSphereRoles.BillingStaff => "Billing and claim processing",
            _ => "",
        };
    }
}
