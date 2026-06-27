import re

with open('d:/CareSphere/Modules/Admin/Layout/AdminLayout.razor', 'r', encoding='utf-8') as f:
    content = f.read()

helpers = '''
    [CascadingParameter]
    private Task<AuthenticationState>? AuthState { get; set; }
    private System.Security.Claims.ClaimsPrincipal? CurrentUser => AuthState?.Result.User;

    private bool HasAdminAccess(System.Security.Claims.ClaimsPrincipal user) => user.HasClaim(c => c.Type == CareSphereClaimTypes.Permission && (c.Value == CareSpherePermissions.Admin_ManageUsers || c.Value == CareSpherePermissions.Admin_ManageRoles || c.Value == CareSpherePermissions.Admin_ViewAuditLog || c.Value == CareSpherePermissions.Admin_ManageTenant || c.Value == CareSpherePermissions.Analytics_View));
    private bool HasPatientAccess(System.Security.Claims.ClaimsPrincipal user) => user.HasClaim(c => c.Type == CareSphereClaimTypes.Permission && (c.Value == CareSpherePermissions.Patients_View || c.Value == CareSpherePermissions.Patients_Create || c.Value == CareSpherePermissions.Patients_Edit || c.Value == CareSpherePermissions.Appointments_View || c.Value == CareSpherePermissions.Appointments_Book || c.Value == CareSpherePermissions.DoctorSchedule_Manage));
    private bool HasWardAccess(System.Security.Claims.ClaimsPrincipal user) => user.HasClaim(c => c.Type == CareSphereClaimTypes.Permission && (c.Value == CareSpherePermissions.Beds_View || c.Value == CareSpherePermissions.Beds_Manage || c.Value == CareSpherePermissions.Beds_Allocate || c.Value == CareSpherePermissions.BedAllotment_View));
    private bool HasClinicalAccess(System.Security.Claims.ClaimsPrincipal user) => user.HasClaim(c => c.Type == CareSphereClaimTypes.Permission && (c.Value == CareSpherePermissions.Queue_View || c.Value == CareSpherePermissions.Encounters_Create || c.Value == CareSpherePermissions.Encounters_View || c.Value == CareSpherePermissions.TeleConsult_Start));
    private bool HasNursingAccess(System.Security.Claims.ClaimsPrincipal user) => user.HasClaim(c => c.Type == CareSphereClaimTypes.Permission && (c.Value == CareSpherePermissions.Vitals_View || c.Value == CareSpherePermissions.NursingNotes_View || c.Value == CareSpherePermissions.MedicationAdmin_View));
    private bool HasLabAccess(System.Security.Claims.ClaimsPrincipal user) => user.HasClaim(c => c.Type == CareSphereClaimTypes.Permission && (c.Value == CareSpherePermissions.Lab_ViewReports || c.Value == CareSpherePermissions.Lab_OrderTests || c.Value == CareSpherePermissions.Lab_CollectSample || c.Value == CareSpherePermissions.Lab_EnterResults));
    private bool HasPharmacyAccess(System.Security.Claims.ClaimsPrincipal user) => user.HasClaim(c => c.Type == CareSphereClaimTypes.Permission && (c.Value == CareSpherePermissions.Pharmacy_ViewStock || c.Value == CareSpherePermissions.Pharmacy_ManagePO || c.Value == CareSpherePermissions.Pharmacy_Dispense || c.Value == CareSpherePermissions.Pharmacy_OtcSale || c.Value == CareSpherePermissions.Pharmacy_ManageInventory));
    private bool HasBillingAccess(System.Security.Claims.ClaimsPrincipal user) => user.HasClaim(c => c.Type == CareSphereClaimTypes.Permission && (c.Value == CareSpherePermissions.Billing_ViewInvoices || c.Value == CareSpherePermissions.Payments_Manage || c.Value == CareSpherePermissions.InsuranceClaims_View || c.Value == CareSpherePermissions.Billing_View));
    private bool HasNotificationAccess(System.Security.Claims.ClaimsPrincipal user) => user.HasClaim(c => c.Type == CareSphereClaimTypes.Permission && (c.Value == CareSpherePermissions.Admin_ManageTenant));
'''

content = content.replace('private string hospitalName = "Hospital";', helpers + '\n    private string hospitalName = "Hospital";')

replacements = [
    ('<!-- ── Administration ───────────────────────────────── -->', '@if (CurrentUser != null && HasAdminAccess(CurrentUser))\n            {\n            <!-- ── Administration ───────────────────────────────── -->', '<!-- ── Patients ─────────────────────────────────────── -->', '            }\n\n            <!-- ── Patients ─────────────────────────────────────── -->'),
    ('<!-- ── Patients ─────────────────────────────────────── -->', '@if (CurrentUser != null && HasPatientAccess(CurrentUser))\n            {\n            <!-- ── Patients ─────────────────────────────────────── -->', '<!-- ── Ward & Beds ──────────────────────────────────── -->', '            }\n\n            <!-- ── Ward & Beds ──────────────────────────────────── -->'),
    ('<!-- ── Ward & Beds ──────────────────────────────────── -->', '@if (CurrentUser != null && HasWardAccess(CurrentUser))\n            {\n            <!-- ── Ward & Beds ──────────────────────────────────── -->', '<!-- ── Clinical ────────────────────────────────────── -->', '            }\n\n            <!-- ── Clinical ────────────────────────────────────── -->'),
    ('<!-- ── Clinical ────────────────────────────────────── -->', '@if (CurrentUser != null && HasClinicalAccess(CurrentUser))\n            {\n            <!-- ── Clinical ────────────────────────────────────── -->', '<!-- ── Nursing ──────────────────────────────────────── -->', '            }\n\n            <!-- ── Nursing ──────────────────────────────────────── -->'),
    ('<!-- ── Nursing ──────────────────────────────────────── -->', '@if (CurrentUser != null && HasNursingAccess(CurrentUser))\n            {\n            <!-- ── Nursing ──────────────────────────────────────── -->', '<!-- ── Laboratory ──────────────────────────────────── -->', '            }\n\n            <!-- ── Laboratory ──────────────────────────────────── -->'),
    ('<!-- ── Laboratory ──────────────────────────────────── -->', '@if (CurrentUser != null && HasLabAccess(CurrentUser))\n            {\n            <!-- ── Laboratory ──────────────────────────────────── -->', '<!-- ── Pharmacy ─────────────────────────────────────── -->', '            }\n\n            <!-- ── Pharmacy ─────────────────────────────────────── -->'),
    ('<!-- ── Pharmacy ─────────────────────────────────────── -->', '@if (CurrentUser != null && HasPharmacyAccess(CurrentUser))\n            {\n            <!-- ── Pharmacy ─────────────────────────────────────── -->', '<!-- ── Billing ──────────────────────────────────────── -->', '            }\n\n            <!-- ── Billing ──────────────────────────────────────── -->'),
    ('<!-- ── Billing ──────────────────────────────────────── -->', '@if (CurrentUser != null && HasBillingAccess(CurrentUser))\n            {\n            <!-- ── Billing ──────────────────────────────────────── -->', '<!-- ── Notifications ────────────────────────────────── -->', '            }\n\n            <!-- ── Notifications ────────────────────────────────── -->'),
    ('<!-- ── Notifications ────────────────────────────────── -->', '@if (CurrentUser != null && HasNotificationAccess(CurrentUser))\n            {\n            <!-- ── Notifications ────────────────────────────────── -->', '<!-- Collapse Button -->', '            }\n\n            <!-- Collapse Button -->')
]

for start_tag, start_rep, end_tag, end_rep in replacements:
    content = content.replace(start_tag, start_rep)
    content = content.replace(end_tag, end_rep)

content = content.replace('Hospital Admin</span>', '@(CurrentUserHelper.GetUserRole(context.User) ?? "")</span>')
content = content.replace('<span class="cs-brand-sub">Admin Panel</span>', '<span class="cs-brand-sub">CareSphere</span>')

navlink_policies = {
    '/admin/users': 'PolicyNames.Permission_Admin_ManageUsers',
    '/admin/roles': 'PolicyNames.Permission_Admin_ManageRoles',
    '/admin/sessions': 'PolicyNames.Permission_Admin_ViewAuditLog',
    '/admin/audit-log': 'PolicyNames.Permission_Admin_ViewAuditLog',
    '/admin/tenant-settings': 'PolicyNames.Permission_Admin_ManageTenant',
    '/admin/analytics': 'PolicyNames.Permission_Analytics_View',
    '/patients" Match="NavLinkMatch.All"': 'PolicyNames.Permission_Patients_View',
    '/patients/create': 'PolicyNames.Permission_Patients_Create',
    '/appointments" Match="NavLinkMatch.All"': 'PolicyNames.Permission_Appointments_View',
    '/appointments/book': 'PolicyNames.Permission_Appointments_Book',
    '/appointments/schedules': 'PolicyNames.Permission_DoctorSchedule_Manage',
    '/wards/dashboard': 'PolicyNames.Permission_Beds_View',
    '/beds/dashboard': 'PolicyNames.Permission_Beds_View',
    '/wards" Match="NavLinkMatch.All"': 'PolicyNames.Permission_Beds_View',
    '/wards/create': 'PolicyNames.Permission_Beds_Manage',
    '/beds" Match="NavLinkMatch.All"': 'PolicyNames.Permission_Beds_View',
    '/beds/create': 'PolicyNames.Permission_Beds_Manage',
    '/allotments" Match="NavLinkMatch.All"': 'PolicyNames.Permission_BedAllotment_View',
    '/allotments/create': 'PolicyNames.Permission_Beds_Allocate',
    '/allotments/transfer': 'PolicyNames.Permission_Beds_Allocate',
    '/encounters': 'PolicyNames.Permission_Encounters_View',
    '/encounter/new': 'PolicyNames.Permission_Encounters_Create',
    '/prescriptions': 'PolicyNames.Permission_Encounters_View',
    '/doctor/queue': 'PolicyNames.Permission_Queue_View',
    '/teleconsult': 'PolicyNames.Permission_TeleConsult_Start',
    '/nursing" Match="NavLinkMatch.All"': 'PolicyNames.Permission_Vitals_View',
    '/nursing/vitals': 'PolicyNames.Permission_Vitals_View',
    '/nursing/notes': 'PolicyNames.Permission_NursingNotes_View',
    '/nursing/mar': 'PolicyNames.Permission_MedicationAdmin_View',
    '/lab/dashboard': 'PolicyNames.Permission_Lab_ViewReports',
    '/laboratory/catalog': 'PolicyNames.Permission_Lab_ViewReports',
    '/laboratory/requisitions': 'PolicyNames.Permission_Lab_OrderTests',
    '/laboratory/samples': 'PolicyNames.Permission_Lab_CollectSample',
    '/laboratory/results': 'PolicyNames.Permission_Lab_EnterResults',
    '/pharmacy/dashboard': 'PolicyNames.Permission_Pharmacy_ViewStock',
    '/pharmacy/items': 'PolicyNames.Permission_Pharmacy_ViewStock',
    '/pharmacy/dispense': 'PolicyNames.Permission_Pharmacy_Dispense',
    '/pharmacy/otc-sale': 'PolicyNames.Permission_Pharmacy_OtcSale',
    '/pharmacy/po': 'PolicyNames.Permission_Pharmacy_ManagePO',
    '/pharmacy/grn': 'PolicyNames.Permission_Pharmacy_ManagePO',
    '/pharmacy/suppliers': 'PolicyNames.Permission_Pharmacy_ManagePO',
    '/pharmacy/stock-ledger': 'PolicyNames.Permission_Pharmacy_ViewStock',
    '/pharmacy/expiry-alerts': 'PolicyNames.Permission_Pharmacy_ViewStock',
    '/pharmacy/controlled-register': 'PolicyNames.Permission_Pharmacy_ManageInventory',
    '/billing/dashboard': 'PolicyNames.Permission_Billing_View',
    '/billing/invoices': 'PolicyNames.Permission_Billing_ViewInvoices',
    '/billing/payments': 'PolicyNames.Permission_Payments_Manage',
    '/billing/claims': 'PolicyNames.Permission_InsuranceClaims_View',
    '/notifications/dashboard': 'PolicyNames.Permission_Admin_ManageTenant',
    '/notifications/templates': 'PolicyNames.Permission_Admin_ManageTenant',
    '/notifications/logs': 'PolicyNames.Permission_Admin_ManageTenant',
    '/notifications/reminders': 'PolicyNames.Permission_Admin_ManageTenant'
}

for href, policy in navlink_policies.items():
    pattern = r'(<NavLink.*?href="' + href + r'".*?</NavLink>)'
    replacement = r'<AuthorizeView Policy="@' + policy + r'"><Authorized>\1</Authorized></AuthorizeView>'
    content = re.sub(pattern, replacement, content)

with open('d:/CareSphere/Modules/Admin/Layout/AdminLayout.razor', 'w', encoding='utf-8') as f:
    f.write(content)

print("success")
