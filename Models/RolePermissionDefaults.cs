namespace CareSphere.Models
{
    /// <summary>
    /// Default permission sets for each CareSphere role.
    /// These serve as the baseline; per-tenant overrides are stored in RolePermissions table.
    /// </summary>
    public static class RolePermissionDefaults
    {
        public static readonly Dictionary<string, List<string>> DefaultPermissions = new()
        {
            [CareSphereRoles.SuperAdmin] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Patients_Create,
                CareSpherePermissions.Patients_Edit,
                CareSpherePermissions.Patients_Delete,
                CareSpherePermissions.Appointments_Create,
                CareSpherePermissions.Appointments_View,
                CareSpherePermissions.Appointments_Book,
                CareSpherePermissions.OwnRecords_View,
                CareSpherePermissions.OwnInvoices_Download,
                CareSpherePermissions.Beds_View,
                CareSpherePermissions.Beds_Manage,
                CareSpherePermissions.Beds_Allocate,
                CareSpherePermissions.BedAllotment_View,
                CareSpherePermissions.Encounters_View,
                CareSpherePermissions.Encounters_Create,
                CareSpherePermissions.SoapNotes_Write,
                CareSpherePermissions.SoapNotes_Finalize,
                CareSpherePermissions.Prescriptions_Write,
                CareSpherePermissions.Prescriptions_Cancel,
                CareSpherePermissions.TeleConsult_Start,
                CareSpherePermissions.Queue_View,
                CareSpherePermissions.Queue_Manage,
                CareSpherePermissions.Vitals_Create,
                CareSpherePermissions.Vitals_View,
                CareSpherePermissions.NursingNotes_Create,
                CareSpherePermissions.NursingNotes_View,
                CareSpherePermissions.MedicationAdmin_Create,
                CareSpherePermissions.MedicationAdmin_View,
                CareSpherePermissions.DoctorSchedule_Manage,
                CareSpherePermissions.Pharmacy_ViewStock,
                CareSpherePermissions.Pharmacy_Dispense,
                CareSpherePermissions.Pharmacy_ManageInventory,
                CareSpherePermissions.Pharmacy_OtcSale,
                CareSpherePermissions.Pharmacy_ManagePO,
                CareSpherePermissions.Lab_OrderTests,
                CareSpherePermissions.Lab_CollectSample,
                CareSpherePermissions.Lab_EnterResults,
                CareSpherePermissions.Lab_VerifyResults,
                CareSpherePermissions.Lab_ViewReports,
                CareSpherePermissions.Billing_ViewInvoices,
                CareSpherePermissions.Billing_CreateInvoices,
                CareSpherePermissions.Billing_RecordPayments,
                CareSpherePermissions.Billing_ManageClaims,
                CareSpherePermissions.Billing_View,
                CareSpherePermissions.Billing_Create,
                CareSpherePermissions.Billing_Edit,
                CareSpherePermissions.Payments_Manage,
                CareSpherePermissions.InsuranceClaims_Manage,
                CareSpherePermissions.InsuranceClaims_View,
                CareSpherePermissions.Admin_ManageUsers,
                CareSpherePermissions.Admin_ManageRoles,
                CareSpherePermissions.Admin_ViewAuditLog,
                CareSpherePermissions.Admin_ManageTenant,
                CareSpherePermissions.Analytics_View,
            },

            [CareSphereRoles.HospitalAdmin] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Patients_Create,
                CareSpherePermissions.Patients_Edit,
                CareSpherePermissions.Patients_Delete,
                CareSpherePermissions.Appointments_Create,
                CareSpherePermissions.Appointments_View,
                CareSpherePermissions.Beds_View,
                CareSpherePermissions.Beds_Manage,
                CareSpherePermissions.Beds_Allocate,
                CareSpherePermissions.BedAllotment_View,
                CareSpherePermissions.Encounters_View,
                CareSpherePermissions.Queue_View,
                CareSpherePermissions.Pharmacy_ViewStock,
                CareSpherePermissions.Lab_ViewReports,
                CareSpherePermissions.Billing_ViewInvoices,
                CareSpherePermissions.Billing_CreateInvoices,
                CareSpherePermissions.Billing_RecordPayments,
                CareSpherePermissions.Billing_ManageClaims,
                CareSpherePermissions.Billing_View,
                CareSpherePermissions.Billing_Create,
                CareSpherePermissions.Billing_Edit,
                CareSpherePermissions.Payments_Manage,
                CareSpherePermissions.InsuranceClaims_Manage,
                CareSpherePermissions.InsuranceClaims_View,
                CareSpherePermissions.Admin_ManageUsers,
                CareSpherePermissions.Admin_ManageRoles,
                CareSpherePermissions.Admin_ViewAuditLog,
            },

            [CareSphereRoles.Doctor] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Encounters_View,
                CareSpherePermissions.Encounters_Create,
                CareSpherePermissions.SoapNotes_Write,
                CareSpherePermissions.SoapNotes_Finalize,
                CareSpherePermissions.Prescriptions_Write,
                CareSpherePermissions.Prescriptions_Cancel,
                CareSpherePermissions.TeleConsult_Start,
                CareSpherePermissions.Queue_View,
                CareSpherePermissions.Vitals_View,
                CareSpherePermissions.NursingNotes_View,
                CareSpherePermissions.MedicationAdmin_View,
                CareSpherePermissions.Lab_OrderTests,
                CareSpherePermissions.Lab_ViewReports,
                CareSpherePermissions.Billing_ViewInvoices,
            },

            [CareSphereRoles.Nurse] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Beds_View,
                CareSpherePermissions.Beds_Allocate,
                CareSpherePermissions.BedAllotment_View,
                CareSpherePermissions.Queue_View,
                CareSpherePermissions.Vitals_Create,
                CareSpherePermissions.Vitals_View,
                CareSpherePermissions.NursingNotes_Create,
                CareSpherePermissions.NursingNotes_View,
                CareSpherePermissions.MedicationAdmin_Create,
                CareSpherePermissions.MedicationAdmin_View,
                CareSpherePermissions.Lab_CollectSample,
            },

            [CareSphereRoles.Pharmacist] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Pharmacy_ViewStock,
                CareSpherePermissions.Pharmacy_Dispense,
                CareSpherePermissions.Pharmacy_ManageInventory,
                CareSpherePermissions.Pharmacy_OtcSale,
                CareSpherePermissions.Pharmacy_ManagePO,
                CareSpherePermissions.Billing_ViewInvoices,
            },

            [CareSphereRoles.LabTechnician] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Lab_CollectSample,
                CareSpherePermissions.Lab_EnterResults,
                CareSpherePermissions.Lab_ViewReports,
                CareSpherePermissions.Lab_VerifyResults,
            },

            [CareSphereRoles.FrontDesk] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Patients_Create,
                CareSpherePermissions.Patients_Edit,
                CareSpherePermissions.Appointments_Create,
                CareSpherePermissions.Appointments_View,
                CareSpherePermissions.Beds_View,
                CareSpherePermissions.BedAllotment_View,
                CareSpherePermissions.Billing_ViewInvoices,
            },

            [CareSphereRoles.Receptionist] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Patients_Create,
                CareSpherePermissions.Appointments_Book,
                CareSpherePermissions.Appointments_Create,
                CareSpherePermissions.Appointments_View,
                CareSpherePermissions.Beds_View,
                CareSpherePermissions.Billing_ViewInvoices,
            },

            [CareSphereRoles.Finance] = new List<string>
            {
                CareSpherePermissions.Billing_ViewInvoices,
                CareSpherePermissions.Billing_CreateInvoices,
                CareSpherePermissions.Billing_RecordPayments,
                CareSpherePermissions.Billing_ManageClaims,
                CareSpherePermissions.Billing_View,
                CareSpherePermissions.Billing_Create,
                CareSpherePermissions.Billing_Edit,
                CareSpherePermissions.Payments_Manage,
                CareSpherePermissions.InsuranceClaims_Manage,
                CareSpherePermissions.InsuranceClaims_View,
            },

            [CareSphereRoles.BillingStaff] = new List<string>
            {
                CareSpherePermissions.Billing_ViewInvoices,
                CareSpherePermissions.Billing_CreateInvoices,
                CareSpherePermissions.Billing_RecordPayments,
                CareSpherePermissions.Billing_View,
                CareSpherePermissions.Billing_Create,
            },

            [CareSphereRoles.NabhAuditor] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Beds_View,
                CareSpherePermissions.BedAllotment_View,
                CareSpherePermissions.Encounters_View,
                CareSpherePermissions.Vitals_View,
                CareSpherePermissions.NursingNotes_View,
                CareSpherePermissions.MedicationAdmin_View,
                CareSpherePermissions.Lab_ViewReports,
                CareSpherePermissions.Billing_ViewInvoices,
                CareSpherePermissions.Admin_ViewAuditLog,
            },

            [CareSphereRoles.Patient] = new List<string>
            {
                CareSpherePermissions.OwnRecords_View,
                CareSpherePermissions.OwnInvoices_Download,
                CareSpherePermissions.Appointments_Book,
                CareSpherePermissions.Lab_ViewReports,
                CareSpherePermissions.Billing_ViewInvoices,
            },
        };
    }
}
