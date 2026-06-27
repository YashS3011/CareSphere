namespace CareSphere
{
    /// <summary>
    /// System role name constants for CareSphere.
    /// </summary>
    public static class CareSphereRoles
    {
        public const string PlatformAdmin  = "PlatformAdmin";   // Vendor-level: manages hospitals as clients
        public const string HospitalAdmin  = "HospitalAdmin";
        public const string Doctor         = "Doctor";
        public const string Nurse          = "Nurse";
        public const string Pharmacist     = "Pharmacist";
        public const string LabTechnician  = "LabTechnician";
        public const string WardManager    = "WardManager";     // Replaces FrontDesk; manages wards & beds
        public const string Finance        = "Finance";
        public const string NabhAuditor    = "NabhAuditor";
        public const string Patient        = "Patient";
        public const string Receptionist   = "Receptionist";
        public const string BillingStaff   = "BillingStaff";
    }

    /// <summary>
    /// Permission string constants for CareSphere, organized by module.
    /// </summary>
    public static class CareSpherePermissions
    {
        // --- Platform (Vendor) ---
        public const string Platform_ManageHospitals = "Platform.ManageHospitals";

        // --- Patient Module ---
        public const string Patients_View        = "Patients.View";
        public const string Patients_Create      = "Patients.Create";
        public const string Patients_Edit        = "Patients.Edit";
        public const string Patients_Delete      = "Patients.Delete";
        public const string OwnRecords_View      = "OwnRecords.View";
        public const string OwnInvoices_Download = "OwnInvoices.Download";
        public const string Appointments_Book    = "Appointments.Book";
        public const string Appointments_Create  = "Appointments.Create";
        public const string Appointments_View    = "Appointments.View";

        // --- Bed / Ward Module ---
        public const string Beds_View         = "Beds.View";
        public const string Beds_Manage       = "Beds.Manage";
        public const string Beds_Allocate     = "Beds.Allocate";
        public const string BedAllotment_View = "BedAllotment.View";
        public const string Wards_Manage      = "Wards.Manage";

        // --- Doctor / EMR Module ---
        public const string Encounters_View         = "Encounters.View";
        public const string Encounters_Create       = "Encounters.Create";
        public const string SoapNotes_Write         = "SoapNotes.Write";
        public const string SoapNotes_Finalize      = "SoapNotes.Finalize";
        public const string Prescriptions_Write     = "Prescriptions.Write";
        public const string Prescriptions_Cancel    = "Prescriptions.Cancel";
        public const string TeleConsult_Start       = "TeleConsult.Start";
        public const string Queue_View              = "Queue.View";
        public const string Queue_Manage            = "Queue.Manage";
        public const string Vitals_Create           = "Vitals.Create";
        public const string Vitals_View             = "Vitals.View";
        public const string NursingNotes_Create     = "NursingNotes.Create";
        public const string NursingNotes_View       = "NursingNotes.View";
        public const string MedicationAdmin_Create  = "MedicationAdmin.Create";
        public const string MedicationAdmin_View    = "MedicationAdmin.View";
        public const string DoctorSchedule_Manage   = "DoctorSchedule.Manage";

        // --- Pharmacy Module ---
        public const string Pharmacy_ViewStock       = "Pharmacy.ViewStock";
        public const string Pharmacy_Dispense        = "Pharmacy.Dispense";
        public const string Pharmacy_ManageInventory = "Pharmacy.ManageInventory";
        public const string Pharmacy_OtcSale         = "Pharmacy.OtcSale";
        public const string Pharmacy_ManagePO        = "Pharmacy.ManagePO";

        // --- Lab Module ---
        public const string Lab_OrderTests    = "Lab.OrderTests";
        public const string Lab_CollectSample = "Lab.CollectSample";
        public const string Lab_EnterResults  = "Lab.EnterResults";
        public const string Lab_VerifyResults = "Lab.VerifyResults";
        public const string Lab_ViewReports   = "Lab.ViewReports";

        // --- Billing Module ---
        public const string Billing_ViewInvoices   = "Billing.ViewInvoices";
        public const string Billing_CreateInvoices = "Billing.CreateInvoices";
        public const string Billing_RecordPayments = "Billing.RecordPayments";
        public const string Billing_ManageClaims   = "Billing.ManageClaims";
        public const string Billing_View           = "Billing.View";
        public const string Billing_Create         = "Billing.Create";
        public const string Billing_Edit           = "Billing.Edit";
        public const string Payments_Manage        = "Payments.Manage";
        public const string InsuranceClaims_Manage = "InsuranceClaims.Manage";
        public const string InsuranceClaims_View   = "InsuranceClaims.View";

        // --- Admin Module ---
        public const string Admin_ManageUsers  = "Admin.ManageUsers";
        public const string Admin_ManageRoles  = "Admin.ManageRoles";
        public const string Admin_ViewAuditLog = "Admin.ViewAuditLog";
        public const string Admin_ManageTenant = "Admin.ManageTenant";

        // --- Analytics Module ---
        public const string Analytics_View = "Analytics.View";
    }

    /// <summary>
    /// Authorization policy name constants. One policy per permission.
    /// Register all policies in Program.cs using AddAuthorization.
    /// </summary>
    public static class PolicyNames
    {
        // Platform
        public const string Permission_Platform_ManageHospitals = "Permission_Platform_ManageHospitals";

        // Patient
        public const string Permission_Patients_View         = "Permission_Patients_View";
        public const string Permission_Patients_Create       = "Permission_Patients_Create";
        public const string Permission_Patients_Edit         = "Permission_Patients_Edit";
        public const string Permission_Patients_Delete       = "Permission_Patients_Delete";
        public const string Permission_OwnRecords_View       = "Permission_OwnRecords_View";
        public const string Permission_OwnInvoices_Download  = "Permission_OwnInvoices_Download";
        public const string Permission_Appointments_Book     = "Permission_Appointments_Book";
        public const string Permission_Appointments_Create   = "Permission_Appointments_Create";
        public const string Permission_Appointments_View     = "Permission_Appointments_View";

        // Beds / Wards
        public const string Permission_Beds_View         = "Permission_Beds_View";
        public const string Permission_Beds_Manage       = "Permission_Beds_Manage";
        public const string Permission_Beds_Allocate     = "Permission_Beds_Allocate";
        public const string Permission_BedAllotment_View = "Permission_BedAllotment_View";
        public const string Permission_Wards_Manage      = "Permission_Wards_Manage";

        // Doctor / EMR
        public const string Permission_Encounters_View          = "Permission_Encounters_View";
        public const string Permission_Encounters_Create        = "Permission_Encounters_Create";
        public const string Permission_SoapNotes_Write          = "Permission_SoapNotes_Write";
        public const string Permission_SoapNotes_Finalize       = "Permission_SoapNotes_Finalize";
        public const string Permission_Prescriptions_Write      = "Permission_Prescriptions_Write";
        public const string Permission_Prescriptions_Cancel     = "Permission_Prescriptions_Cancel";
        public const string Permission_TeleConsult_Start        = "Permission_TeleConsult_Start";
        public const string Permission_Queue_View               = "Permission_Queue_View";
        public const string Permission_Queue_Manage             = "Permission_Queue_Manage";
        public const string Permission_Vitals_Create            = "Permission_Vitals_Create";
        public const string Permission_Vitals_View              = "Permission_Vitals_View";
        public const string Permission_NursingNotes_Create      = "Permission_NursingNotes_Create";
        public const string Permission_NursingNotes_View        = "Permission_NursingNotes_View";
        public const string Permission_MedicationAdmin_Create   = "Permission_MedicationAdmin_Create";
        public const string Permission_MedicationAdmin_View     = "Permission_MedicationAdmin_View";
        public const string Permission_DoctorSchedule_Manage    = "Permission_DoctorSchedule_Manage";

        // Pharmacy
        public const string Permission_Pharmacy_ViewStock        = "Permission_Pharmacy_ViewStock";
        public const string Permission_Pharmacy_Dispense         = "Permission_Pharmacy_Dispense";
        public const string Permission_Pharmacy_ManageInventory  = "Permission_Pharmacy_ManageInventory";
        public const string Permission_Pharmacy_OtcSale          = "Permission_Pharmacy_OtcSale";
        public const string Permission_Pharmacy_ManagePO         = "Permission_Pharmacy_ManagePO";

        // Lab
        public const string Permission_Lab_OrderTests    = "Permission_Lab_OrderTests";
        public const string Permission_Lab_CollectSample = "Permission_Lab_CollectSample";
        public const string Permission_Lab_EnterResults  = "Permission_Lab_EnterResults";
        public const string Permission_Lab_VerifyResults = "Permission_Lab_VerifyResults";
        public const string Permission_Lab_ViewReports   = "Permission_Lab_ViewReports";

        // Billing
        public const string Permission_Billing_ViewInvoices    = "Permission_Billing_ViewInvoices";
        public const string Permission_Billing_CreateInvoices  = "Permission_Billing_CreateInvoices";
        public const string Permission_Billing_RecordPayments  = "Permission_Billing_RecordPayments";
        public const string Permission_Billing_ManageClaims    = "Permission_Billing_ManageClaims";
        public const string Permission_Billing_View            = "Permission_Billing_View";
        public const string Permission_Billing_Create          = "Permission_Billing_Create";
        public const string Permission_Billing_Edit            = "Permission_Billing_Edit";
        public const string Permission_Payments_Manage         = "Permission_Payments_Manage";
        public const string Permission_InsuranceClaims_Manage  = "Permission_InsuranceClaims_Manage";
        public const string Permission_InsuranceClaims_View    = "Permission_InsuranceClaims_View";

        // Admin
        public const string Permission_Admin_ManageUsers  = "Permission_Admin_ManageUsers";
        public const string Permission_Admin_ManageRoles  = "Permission_Admin_ManageRoles";
        public const string Permission_Admin_ViewAuditLog = "Permission_Admin_ViewAuditLog";
        public const string Permission_Admin_ManageTenant = "Permission_Admin_ManageTenant";

        // Analytics
        public const string Permission_Analytics_View = "Permission_Analytics_View";
    }

    /// <summary>
    /// Custom claim type URI constants used in CareSphere JWT / cookie claims.
    /// </summary>
    public static class CareSphereClaimTypes
    {
        public const string TenantId  = "caresphere/tenant_id";
        public const string Permission = "caresphere/permission";
        public const string FullName  = "caresphere/full_name";
        public const string DoctorId  = "caresphere/doctor_id";
        public const string PatientId = "caresphere/patient_id";
    }
}
