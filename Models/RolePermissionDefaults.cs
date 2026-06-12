namespace CareSphere.Models
{
    /// <summary>
    /// Default permission sets for each CareSphere role.
    /// These serve as the baseline; per-tenant overrides are stored in RolePermissions table.
    /// The AuditLog-only note for NabhAuditor is enforced at the service level.
    /// </summary>
    public static class RolePermissionDefaults
    {
        public static readonly Dictionary<string, List<string>> DefaultPermissions = new()
        {
            // SuperAdmin gets every permission
            [CareSphereRoles.SuperAdmin] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Patients_Create,
                CareSpherePermissions.Patients_Edit,
                CareSpherePermissions.Patients_Delete,
                CareSpherePermissions.Beds_View,
                CareSpherePermissions.Beds_Manage,
                CareSpherePermissions.Beds_Allocate,
                CareSpherePermissions.Encounters_View,
                CareSpherePermissions.Encounters_Create,
                CareSpherePermissions.SoapNotes_Write,
                CareSpherePermissions.SoapNotes_Finalize,
                CareSpherePermissions.Prescriptions_Write,
                CareSpherePermissions.Prescriptions_Cancel,
                CareSpherePermissions.TeleConsult_Start,
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
                CareSpherePermissions.Admin_ManageUsers,
                CareSpherePermissions.Admin_ManageRoles,
                CareSpherePermissions.Admin_ViewAuditLog,
                CareSpherePermissions.Admin_ManageTenant,
            },

            // HospitalAdmin — all except clinical write permissions
            [CareSphereRoles.HospitalAdmin] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Patients_Create,
                CareSpherePermissions.Patients_Edit,
                CareSpherePermissions.Patients_Delete,
                CareSpherePermissions.Beds_View,
                CareSpherePermissions.Beds_Manage,
                CareSpherePermissions.Beds_Allocate,
                CareSpherePermissions.Encounters_View,
                CareSpherePermissions.Encounters_Create,
                CareSpherePermissions.TeleConsult_Start,
                CareSpherePermissions.Pharmacy_ViewStock,
                CareSpherePermissions.Pharmacy_Dispense,
                CareSpherePermissions.Pharmacy_ManageInventory,
                CareSpherePermissions.Pharmacy_OtcSale,
                CareSpherePermissions.Pharmacy_ManagePO,
                CareSpherePermissions.Lab_OrderTests,
                CareSpherePermissions.Lab_CollectSample,
                CareSpherePermissions.Lab_ViewReports,
                CareSpherePermissions.Billing_ViewInvoices,
                CareSpherePermissions.Billing_CreateInvoices,
                CareSpherePermissions.Billing_RecordPayments,
                CareSpherePermissions.Billing_ManageClaims,
                CareSpherePermissions.Admin_ManageUsers,
                CareSpherePermissions.Admin_ManageRoles,
                CareSpherePermissions.Admin_ViewAuditLog,
                CareSpherePermissions.Admin_ManageTenant,
                // Excluded: SoapNotes_Write, SoapNotes_Finalize, Prescriptions_Write,
                //           Prescriptions_Cancel, Lab_EnterResults, Lab_VerifyResults
            },

            // Doctor — clinical workflow permissions
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
                CareSpherePermissions.Lab_OrderTests,
                CareSpherePermissions.Lab_ViewReports,
                CareSpherePermissions.Billing_ViewInvoices,
            },

            // Nurse — ward nursing staff
            // Can record vitals, view allotments, administer medications, write nursing notes
            // Must NOT have prescribing, billing, or admin access
            [CareSphereRoles.Nurse] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.BedAllotment_View,
                CareSpherePermissions.Vitals_Create,
                CareSpherePermissions.Vitals_View,
                CareSpherePermissions.NursingNotes_Create,
                CareSpherePermissions.NursingNotes_View,
                CareSpherePermissions.MedicationAdmin_Create,
                CareSpherePermissions.MedicationAdmin_View,
                CareSpherePermissions.Queue_View
            },

            // Pharmacist — pharmacy operations
            [CareSphereRoles.Pharmacist] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Pharmacy_ViewStock,
                CareSpherePermissions.Pharmacy_Dispense,
                CareSpherePermissions.Pharmacy_ManageInventory,
                CareSpherePermissions.Pharmacy_OtcSale,
                CareSpherePermissions.Pharmacy_ManagePO,
                CareSpherePermissions.Prescriptions_Cancel,
            },

            // Lab Technician — lab sample and result operations
            [CareSphereRoles.LabTechnician] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Lab_CollectSample,
                CareSpherePermissions.Lab_EnterResults,
                CareSpherePermissions.Lab_VerifyResults,
                CareSpherePermissions.Lab_ViewReports,
            },

            // Front Desk — reception and basic patient ops
            [CareSphereRoles.FrontDesk] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Patients_Create,
                CareSpherePermissions.Patients_Edit,
                CareSpherePermissions.Beds_View,
                CareSpherePermissions.Beds_Allocate,
                CareSpherePermissions.Billing_ViewInvoices,
            },

            // Finance — billing and payment management
            [CareSphereRoles.Finance] = new List<string>
            {
                CareSpherePermissions.Billing_ViewInvoices,
                CareSpherePermissions.Billing_CreateInvoices,
                CareSpherePermissions.Billing_RecordPayments,
                CareSpherePermissions.Billing_ManageClaims,
                CareSpherePermissions.Admin_ViewAuditLog,
            },

            // NABH Auditor — read-only compliance access
            [CareSphereRoles.NabhAuditor] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Admin_ViewAuditLog,
                CareSpherePermissions.Billing_ViewInvoices,
            },

            // Patient (portal) — future patient-facing portal role
            // Read-only access to their own records only
            [CareSphereRoles.Patient] = new List<string>
            {
                CareSpherePermissions.OwnRecords_View,
                CareSpherePermissions.OwnInvoices_Download,
                CareSpherePermissions.Appointments_Book
            },

            // Receptionist — front desk staff
            // Can manage patients and queues but must NOT have Billing.Edit or any EMR access
            [CareSphereRoles.Receptionist] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Patients_Create,
                CareSpherePermissions.Patients_Edit,
                CareSpherePermissions.Queue_Manage,
                CareSpherePermissions.Appointments_Create,
                CareSpherePermissions.Appointments_View,
                CareSpherePermissions.BedAllotment_View,
                CareSpherePermissions.Billing_View
            },

            // BillingStaff — dedicated billing and insurance clerks
            // Can fully manage billing and claims but must NOT change user roles or access EMR
            [CareSphereRoles.BillingStaff] = new List<string>
            {
                CareSpherePermissions.Patients_View,
                CareSpherePermissions.Billing_View,
                CareSpherePermissions.Billing_Create,
                CareSpherePermissions.Billing_Edit,
                CareSpherePermissions.Payments_Manage,
                CareSpherePermissions.InsuranceClaims_Manage,
                CareSpherePermissions.InsuranceClaims_View
            },
        };
    }
}
