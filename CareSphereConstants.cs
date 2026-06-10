namespace CareSphere
{
    /// <summary>
    /// System role name constants for CareSphere.
    /// </summary>
    public static class CareSphereRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string HospitalAdmin = "HospitalAdmin";
        public const string Doctor = "Doctor";
        public const string Nurse = "Nurse";
        public const string Pharmacist = "Pharmacist";
        public const string LabTechnician = "LabTechnician";
        public const string FrontDesk = "FrontDesk";
        public const string Finance = "Finance";
        public const string NabhAuditor = "NabhAuditor";
        public const string Patient = "Patient";
    }

    /// <summary>
    /// Permission string constants for CareSphere, organized by module.
    /// </summary>
    public static class CareSpherePermissions
    {
        // --- Patient Module ---
        public const string Patients_View = "Patients.View";
        public const string Patients_Create = "Patients.Create";
        public const string Patients_Edit = "Patients.Edit";
        public const string Patients_Delete = "Patients.Delete";

        // --- Bed Module ---
        public const string Beds_View = "Beds.View";
        public const string Beds_Manage = "Beds.Manage";
        public const string Beds_Allocate = "Beds.Allocate";

        // --- Doctor / EMR Module ---
        public const string Encounters_View = "Encounters.View";
        public const string Encounters_Create = "Encounters.Create";
        public const string SoapNotes_Write = "SoapNotes.Write";
        public const string SoapNotes_Finalize = "SoapNotes.Finalize";
        public const string Prescriptions_Write = "Prescriptions.Write";
        public const string Prescriptions_Cancel = "Prescriptions.Cancel";
        public const string TeleConsult_Start = "TeleConsult.Start";

        // --- Pharmacy Module ---
        public const string Pharmacy_ViewStock = "Pharmacy.ViewStock";
        public const string Pharmacy_Dispense = "Pharmacy.Dispense";
        public const string Pharmacy_ManageInventory = "Pharmacy.ManageInventory";
        public const string Pharmacy_OtcSale = "Pharmacy.OtcSale";
        public const string Pharmacy_ManagePO = "Pharmacy.ManagePO";

        // --- Lab Module ---
        public const string Lab_OrderTests = "Lab.OrderTests";
        public const string Lab_CollectSample = "Lab.CollectSample";
        public const string Lab_EnterResults = "Lab.EnterResults";
        public const string Lab_VerifyResults = "Lab.VerifyResults";
        public const string Lab_ViewReports = "Lab.ViewReports";

        // --- Billing Module ---
        public const string Billing_ViewInvoices = "Billing.ViewInvoices";
        public const string Billing_CreateInvoices = "Billing.CreateInvoices";
        public const string Billing_RecordPayments = "Billing.RecordPayments";
        public const string Billing_ManageClaims = "Billing.ManageClaims";

        // --- Admin Module ---
        public const string Admin_ManageUsers = "Admin.ManageUsers";
        public const string Admin_ManageRoles = "Admin.ManageRoles";
        public const string Admin_ViewAuditLog = "Admin.ViewAuditLog";
        public const string Admin_ManageTenant = "Admin.ManageTenant";
    }

    /// <summary>
    /// Authorization policy name constants. One policy per permission.
    /// Register all policies in Program.cs using AddAuthorization.
    /// </summary>
    public static class PolicyNames
    {
        // Patient
        public const string Permission_Patients_View = "Permission_Patients_View";
        public const string Permission_Patients_Create = "Permission_Patients_Create";
        public const string Permission_Patients_Edit = "Permission_Patients_Edit";
        public const string Permission_Patients_Delete = "Permission_Patients_Delete";

        // Beds
        public const string Permission_Beds_View = "Permission_Beds_View";
        public const string Permission_Beds_Manage = "Permission_Beds_Manage";
        public const string Permission_Beds_Allocate = "Permission_Beds_Allocate";

        // Doctor / EMR
        public const string Permission_Encounters_View = "Permission_Encounters_View";
        public const string Permission_Encounters_Create = "Permission_Encounters_Create";
        public const string Permission_SoapNotes_Write = "Permission_SoapNotes_Write";
        public const string Permission_SoapNotes_Finalize = "Permission_SoapNotes_Finalize";
        public const string Permission_Prescriptions_Write = "Permission_Prescriptions_Write";
        public const string Permission_Prescriptions_Cancel = "Permission_Prescriptions_Cancel";
        public const string Permission_TeleConsult_Start = "Permission_TeleConsult_Start";

        // Pharmacy
        public const string Permission_Pharmacy_ViewStock = "Permission_Pharmacy_ViewStock";
        public const string Permission_Pharmacy_Dispense = "Permission_Pharmacy_Dispense";
        public const string Permission_Pharmacy_ManageInventory = "Permission_Pharmacy_ManageInventory";
        public const string Permission_Pharmacy_OtcSale = "Permission_Pharmacy_OtcSale";
        public const string Permission_Pharmacy_ManagePO = "Permission_Pharmacy_ManagePO";

        // Lab
        public const string Permission_Lab_OrderTests = "Permission_Lab_OrderTests";
        public const string Permission_Lab_CollectSample = "Permission_Lab_CollectSample";
        public const string Permission_Lab_EnterResults = "Permission_Lab_EnterResults";
        public const string Permission_Lab_VerifyResults = "Permission_Lab_VerifyResults";
        public const string Permission_Lab_ViewReports = "Permission_Lab_ViewReports";

        // Billing
        public const string Permission_Billing_ViewInvoices = "Permission_Billing_ViewInvoices";
        public const string Permission_Billing_CreateInvoices = "Permission_Billing_CreateInvoices";
        public const string Permission_Billing_RecordPayments = "Permission_Billing_RecordPayments";
        public const string Permission_Billing_ManageClaims = "Permission_Billing_ManageClaims";

        // Admin
        public const string Permission_Admin_ManageUsers = "Permission_Admin_ManageUsers";
        public const string Permission_Admin_ManageRoles = "Permission_Admin_ManageRoles";
        public const string Permission_Admin_ViewAuditLog = "Permission_Admin_ViewAuditLog";
        public const string Permission_Admin_ManageTenant = "Permission_Admin_ManageTenant";
    }

    /// <summary>
    /// Custom claim type URI constants used in CareSphere JWT / cookie claims.
    /// </summary>
    public static class CareSphereClaimTypes
    {
        public const string TenantId = "caresphere/tenant_id";
        public const string Permission = "caresphere/permission";
        public const string FullName = "caresphere/full_name";
        public const string DoctorId = "caresphere/doctor_id";
    }
}
