# 🏥 CareSphere - End-to-End Manual Dry Run & Verification Manual

This document guides you through testing every single page and functionality of the **CareSphere** Hospital Management System. It uses the pre-seeded, fully relative test data generated during the database clean-reset phase.

---

## 🔑 Test Setup & Login Credentials

The database has been cleared and pre-seeded with relative records linked across all tables.

Use these credentials to log in as different roles during the dry run:

| Role | Username / Email | Password | Landing Page / Target Route |
| :--- | :--- | :--- | :--- |
| **Super Admin** | `admin@caresphere.in` | `Admin@123456` | `/admin/dashboard` |
| **Receptionist** | `receptionist@caresphere.dev` | `Receptionist@123` | `/patients` |
| **Doctor** | `doctor@caresphere.dev` | `Doctor@123` | `/doctor/queue` |
| **Nurse** | `nurse@caresphere.dev` | `Nurse@123` | `/beds/dashboard` |
| **Lab Technician** | `labtech@caresphere.dev` | `LabTech@123` | `/lab/dashboard` |
| **Pharmacist** | `pharmacist@caresphere.dev` | `Pharmacist@123` | `/pharmacy/dashboard` |
| **Billing Staff** | `billingstaff@caresphere.dev` | `BillingStaff@123` | `/billing/dashboard` |

---

## 🔐 Module 1: Platform Administration & Access Control

### 🖥️ Page 1: Login (`/auth/login`)
*   **Action**: Enter `admin@caresphere.in` and `Admin@123456`. Click **Sign In**.
*   **Expected**: Successfully redirects to `/admin/dashboard`. The browser loads user identity and role.

### 🖥️ Page 2: Administration Dashboard (`/admin/dashboard`)
*   **Expected**: Displays summary KPI cards (Total Users, Active Sessions, Total Beds, Audit Logs).
*   **Verification**: Check that the cards match the seeded totals.

### 🖥️ Page 3: User Management List (`/admin/users`)
*   **Action**: View the grid of system users. Find the 7 seeded role accounts.
*   **Expected**: All users are shown as `Active`.

### 🖥️ Page 4: Create User (`/admin/users/create` or via "Register New User")
*   **Action**: Click **Register New User**. Enter a test user:
    *   Email: `elena.rostova@caresphere.dev`
    *   Full Name: `Dr. Elena Rostova`
    *   Role: `Doctor`
    *   Password: `Doctor@123456`
*   **Action**: Click **Save**.
*   **Expected**: The user is created and appears in the users grid.

### 🖥️ Page 5: Edit User (`/admin/users/edit/{id}`)
*   **Action**: Click **Edit** on `elena.rostova@caresphere.dev`. Uncheck **Is Active**. Click **Update**.
*   **Expected**: User status updates to inactive. 
*   **Action**: Try logging in with her credentials in an incognito window.
*   **Expected**: Access is blocked.

### 🖥️ Page 6: Role Permissions (`/admin/roles`)
*   **Action**: Select the role `Doctor` or `Nurse`.
*   **Expected**: View the list of permissions assigned to each role.

### 🖥️ Page 7: Session Monitor (`/admin/sessions`)
*   **Action**: View list of active user sessions.
*   **Expected**: Shows at least your current Super Admin session (IP address, browser user-agent).
*   **Action**: Click **Revoke** on any other session.
*   **Expected**: That session is immediately invalidated.

### 🖥️ Page 8: Tenant Settings (`/admin/tenant-settings`)
*   **Action**: Edit the Hospital Name to `CareSphere General Hospital` and click **Save**.
*   **Action**: Toggle the `Is Active` switch to **false** (Freeze Tenant) and click save. Log out.
*   **Action**: Attempt to log in as `nurse@caresphere.dev`.
*   **Expected**: Access is denied, showing a tenant frozen message.
*   **Action**: Log back in as `admin@caresphere.in` and reactivate the tenant.

### 🖥️ Page 9: Append-Only Audit Logs (`/admin/audit-log`)
*   **Action**: Review the logs list.
*   **Expected**: System login and action logs are displayed.
*   **Verification**: Verify that there are no Edit or Delete buttons. These logs are append-only.

### 🖥️ Page 10: Platform Admin Dashboard (`/platform-admin`)
*   **Action**: Enter URL `/platform-admin` manually (restricted to Super Admin).
*   **Expected**: Renders system-level stats (Total Tenants, Subscriptions, Active Hostings).

### 🖥️ Page 11: Tenants Registry (`/platform-admin/tenants`)
*   **Action**: View the global list of tenants.
*   **Expected**: Displays `CareSphere General Hospital` with its Pro subscription details.

---

## 👥 Module 2: Patient Registration & Demographics

*Log out and log in as **Receptionist** (`receptionist@caresphere.dev` / `Receptionist@123`)*

### 🖥️ Page 12: Patients List (`/patients`)
*   **Expected**: Displays the pre-seeded patient `John Doe` with his Phone (`9999999999`) and MRN (`MRN-000001`).

### 🖥️ Page 13: Register New Patient (`/patients/create`)
*   **Action**: Click **Register New Patient**. Enter:
    *   First Name: `Sarah`
    *   Last Name: `Jenkins`
    *   DOB: `1988-11-15`
    *   Gender: `Female`
    *   Phone: `9876549870`
    *   Email: `sarah.jenkins@example.com`
    *   Blood Group: `B+`
*   **Action**: Click **Save**.
*   **Expected**: Patient profile is created and redirects to details page with auto-generated MRN.

### 🖥️ Page 14: Patient Details (`/patients/{id}`)
*   **Expected**: Renders Sarah Jenkins' demographics, MRN, and blank timelines for encounters, allotments, and prescriptions.

### 🖥️ Page 15: Edit Patient (`/patients/edit/{id}`)
*   **Action**: Click **Edit Profile** on Sarah Jenkins' details page.
*   **Action**: Change Address to `789 Maple Drive, Sector 4` and click **Save**.
*   **Expected**: Details page updates with the new address.

### 🖥️ Page 16: Notification Preferences (`/patients/preferences/{patientId}`)
*   **Action**: Click **Notification Preferences** on the patient's details page.
*   **Action**: Change channel to `WhatsApp`, language to `en`, and enable "Discharge Notifications". Click **Save**.
*   **Expected**: Updated preferences are displayed on the patient's dashboard.

---

## 🛏️ Module 3: Ward & Bed Management

*Log out and log in as **Nurse** (`nurse@caresphere.dev` / `Nurse@123`)*

### 🖥️ Page 17: Bed Dashboard (`/beds/dashboard`)
*   **Expected**: Shows ward breakdown (General Ward A, occupancy progress bar, total occupied vs available beds).

### 🖥️ Page 18: Wards List (`/wards`)
*   **Expected**: Displays `General Ward A` (`1st Floor`, `Main Building`).

### 🖥️ Page 19: Create Ward (`/wards/create`)
*   **Action**: Click **Create Ward**. Enter:
    *   Ward Name: `Cardiac Unit A`
    *   Ward Type: `ICU`
    *   Floor: `3rd Floor`
    *   Building: `Wing D`
*   **Action**: Click **Save**.
*   **Expected**: Appears in `/wards` list.

### 🖥️ Page 20: Edit Ward (`/wards/edit/{id}`)
*   **Action**: Click **Edit** on `Cardiac Unit A`. Change floor to `4th Floor`. Save.
*   **Expected**: Updates successfully in the list.

### 🖥️ Page 21: Beds List (`/beds`)
*   **Expected**: Shows pre-seeded bed `B-101` in `General Ward A`.

### 🖥️ Page 22: Create Bed (`/beds/create`)
*   **Action**: Click **Create Bed**. Enter:
    *   Bed Number: `CCU-301`
    *   Ward: `Cardiac Unit A`
    *   Bed Type: `ICU`
    *   Status: `Available`
*   **Action**: Click **Save**.
*   **Expected**: Bed CCU-301 appears in beds list and Bed Dashboard.

### 🖥️ Page 23: Edit Bed (`/beds/edit/{id}`)
*   **Action**: Click **Edit** on `CCU-301`. Set status to `Available` (if changed). Save.

### 🖥️ Page 24: Active Allotments (`/allotments`)
*   **Expected**: Displays active bed allotment of `John Doe` on Bed `B-101` (Status: `Active`).

### 🖥️ Page 25: New Bed Allotment (`/allotments/create`)
*   **Action**: Go to `/beds/dashboard`. Select Bed `CCU-301` (shows Available). Click **Admit Patient**.
*   **Action**: Select Patient: `Sarah Jenkins`. Admitting Doctor: `Dr. Jane Smith`. Type: `IPD`. Save.
*   **Expected**: CCU-301 status changes to `Occupied`. Ward occupancy statistics update.

### 🖥️ Page 26: Bed Transfer (`/allotments/transfer/{id}`)
*   **Action**: Go to `/allotments`. Locate Sarah Jenkins. Click **Transfer**.
*   **Action**: Select target bed: `B-101` (make sure it's free, or register another standard bed if occupied).
*   **Action**: Reason: `Patient stabilized, shifting to General Ward`. Click **Confirm Transfer**.
*   **Expected**: Sarah Jenkins is transferred. CCU-301 is freed. B-101 is occupied.

### 🖥️ Page 27: Patient Discharge (Via `/allotments`)
*   **Action**: In `/allotments`, locate Sarah Jenkins. Click **Discharge**.
*   **Action**: Enter discharge notes: `Discharged in stable condition`. Click **Confirm Discharge**.
*   **Expected**: Allotment status transitions to `Discharged`. Bed is marked `Available` again.

---

## 🩺 Module 4: Clinical EMR Workspace & Doctor Queue

*Log out and log in as **Doctor** (`doctor@caresphere.dev` / `Doctor@123`)*

### 🖥️ Page 28: Doctor Queue (`/doctor/queue`)
*   **Expected**: Renders lists of waiting patients.
*   **Action**: Select `Dr. Jane Smith`. You should see `John Doe` waiting in the queue.
*   **Action**: Click **Start Consultation**. Status changes to `In Consultation`.

### 🖥️ Page 29: Start Encounter (`/encounter/new`)
*   **Action**: Click **Start New Encounter** in sidebar.
*   **Action**: Search patient `Sarah Jenkins`. Click **Select**.
*   **Action**: Choose Doctor: `Dr. Jane Smith`. Type: `OPD`. Chief Complaint: `Fever and dry cough`. Click **Start Encounter**.
*   **Expected**: Redirects to EMR workspace (`/encounter/view/{id}`).

### 🖥️ Page 30: Encounters List (`/encounters`)
*   **Expected**: Displays encounters for both John Doe (`InProgress`) and Sarah Jenkins (`InProgress`).

### 🖥️ Page 31: Clinical Workspace & SOAP Note (`/encounter/view/{id}`)
*   **Action**: On Sarah Jenkins' encounter page:
    *   *Subjective*: Persistent dry cough and moderate fatigue for 3 days.
    *   *Objective*: Temp 100.8F, BP 115/75, Pulse 82.
    *   *Assessment*: Mild Acute Bronchitis.
    *   *Plan*: Order CBC, prescribe Paracetamol.
*   **Action**: Click **Save Draft**. Status remains `Draft` (editable).
*   **Action**: Click **Finalize SOAP Note**.
*   **Expected**: SOAP Note is saved and locked (read-only fields).

### 🧪 CDS Drug Interaction Check (Same page)
*   **Action**: In the Prescriptions panel, search and select `Aspirin` (ASP-75). Input Qty: `7` and click **Add**.
*   **Action**: Now search and select `Warfarin` (WAR-5). Click **Add**.
*   **Expected**: A warning banner pops up: `Warning: Increased risk of bleeding when Aspirin is co-administered with Warfarin.`.
*   **Action**: Enter justification: `Cardiovascular protection needed` and click add.
*   **Expected**: Prescription is added with interaction warning justification.
*   **Action**: Click **Cancel** next to Warfarin. Status changes to `Cancelled`.

### 🖥️ Page 32: Prescriptions List (`/prescriptions`)
*   **Expected**: Displays active prescriptions, including the Aspirin prescription for Sarah Jenkins.

---

## 🧪 Module 5: Laboratory Management Lifecycle

*Log out and log in as **Lab Technician** (`labtech@caresphere.dev` / `LabTech@123`)*

### 🖥️ Page 33: Lab Test Catalog (`/laboratory/catalog`)
*   **Expected**: Displays CBC (Complete Blood Count) priced at `350.00` and its parameter `Hemoglobin (HB)`.

### 🖥️ Page 34: Lab Requisitions List (`/laboratory/requisitions`)
*   **Expected**: Displays the pre-seeded CBC order for John Doe (REQ-2026-0001, Status: `Ordered`).

### 🖥️ Page 35: Requisition Details (`/laboratory/requisitions/{id}`)
*   **Action**: Select John Doe's requisition. Renders patient details and test items.

### 🖥️ Page 36: Specimen Collection (`/laboratory/requisitions/{id}/collect-sample`)
*   **Action**: Click **Record Sample Collection** on the requisition page.
*   **Action**: Input Specimen: `Blood`, Barcode: `SMPL-CBC-JD01`. Click **Save Collection**.
*   **Expected**: Status transitions to `Collected`. Click **Receive Specimen** to transition to `Sample Received`.

### 🖥️ Page 37: Result Entry Form (`/laboratory/requisitions/{itemId}/results`)
*   **Action**: On the details page, click **Enter Results** next to the CBC item.
*   **Action**: In the Hemoglobin (HB) input, enter `11.0` (Reference: 12.0 - 16.0).
*   **Expected**: System flags this value as `L` (Low) in blue.
*   **Action**: Change to `17.5` and notice it changes to `H` (High) in red.
*   **Action**: Set value to `14.5` (Normal). Click **Save Results**.
*   **Action**: Click **Verify & Finalize Report**.
*   **Expected**: Requisition status changes to `Completed`. PDF report compiles.

### 🖥️ Page 38: Lab Results List (`/laboratory/results`)
*   **Expected**: Displays list of verified results.

---

## 💊 Module 6: Pharmacy & Inventory Management

*Log out and log in as **Pharmacist** (`pharmacist@caresphere.dev` / `Pharmacist@123`)*

### 🖥️ Page 39: Pharmacy Dashboard (`/pharmacy/dashboard`)
*   **Expected**: Shows low stock alerts, pending prescriptions, and daily sales metrics.

### 🖥️ Page 40: Drug Catalog (`/pharmacy/items`)
*   **Expected**: Shows `Paracetamol 500mg` and `Aspirin 75mg`.

### 🖥️ Page 41: Register Drug (`/pharmacy/items/create`)
*   **Action**: Click **Add Drug**. Create a medicine `Ibuprofen 400mg` (`IBU-400`). Save.

### 🖥️ Page 42: Suppliers List (`/pharmacy/suppliers`)
*   **Expected**: Shows `MedLife Pharmaceuticals`.

### 🖥️ Page 43: Create Supplier (`/pharmacy/suppliers/create`)
*   **Action**: Create `Apex Pharma Distributors`. Save.

### 🖥️ Page 44: Purchase Orders (`/pharmacy/purchase-orders`)
*   **Expected**: Displays PO list.
*   **Action**: Create a PO for `Ibuprofen 400mg`, Qty: `200`. Save as draft, then change status to `Approved`.

### 🖥️ Page 45: Goods Received Notes (`/pharmacy/grn`)
*   **Action**: Create a new GRN linked to the approved PO. Set Batch: `BAT-IBU-01`, Expiry: `2 years from now`, received qty: `200`. Click **Save & Receive Stock**.
*   **Expected**: Stock level for Ibuprofen increases by 200.

### 🖥️ Page 46: Dispense Prescription (`/pharmacy/dispense`)
*   **Action**: Search for patient `John Doe` (he has an active Aspirin prescription).
*   **Action**: Scan/Enter barcode `501234567800` (Aspirin barcode) to confirm drug matching.
*   **Action**: Set Qty: `7`. The FEFO forecast automatically allocates from batch `BAT-ASP-75`. Click **Complete Dispensing**.
*   **Expected**: Prescription marked dispensed, stock decremented, stock ledger updated.

### 🖥️ Page 47: OTC POS Sale (`/pharmacy/otc-sale`)
*   **Action**: Add `Paracetamol 500mg` to checkout cart. Enter Qty: `3`.
*   **Action**: Set Customer: `Jane Miller`, Method: `Cash`. Click **Complete Cash Sale**.
*   **Expected**: Invoice prints, cash today updates, stock level decrements.

### 🖥️ Page 48: Stock Ledger (`/pharmacy/stock-ledger`)
*   **Expected**: Audits all transactions. Verify entries exist for `GRN`, `Dispense`, and `OTCSale`.

### 🖥️ Page 49: Expiry Alerts (`/pharmacy/expiry-alerts`)
*   **Expected**: Lists batches expiring within 90 days.

---

## 💵 Module 7: Billing, Payments & Claims

*Log out and log in as **Billing Staff** (`billingstaff@caresphere.dev` / `BillingStaff@123`)*

### 🖥️ Page 50: Billing Dashboard (`/billing/dashboard`)
*   **Expected**: Renders total invoiced, collected, pending, and claims statuses.

### 🖥️ Page 51: Invoices Grid (`/billing/invoices`)
*   **Expected**: Shows pre-seeded draft invoice `INV-2026-0001` for `John Doe` (Total: `495.00`).

### 🖥️ Page 52: Compile Invoice (`/billing/invoices/create`)
*   **Action**: Select Patient: `John Doe`. Add lines:
    *   Type: `Consultation`, Price: `500.00`
    *   Type: `LabTest`, Price: `350.00`
*   **Action**: Click **Save & Finalize**.
*   **Expected**: Invoice status changes to `Finalized`.

### 🖥️ Page 53: Invoice details & Payments (`/billing/invoices/{id}`)
*   **Action**: On John Doe's invoice details page, click **Add Payment / Settlement**.
*   **Action**: Select `Cash`, Enter Amount: `500.00`. Save.
*   **Expected**: Balance decreases. Invoice status is `PartiallyPaid`. Click **Download Invoice PDF** to generate the receipt.

### 🖥️ Page 54: Payments Log (`/billing/payments`)
*   **Expected**: Displays the payment transactions log including the $500.00 cash payment.

### 🖥️ Page 55: Insurance Claims List (`/billing/claims`)
*   **Expected**: Shows draft claim `CLM-2026-0001` for John Doe.
*   **Action**: View details, transition status from `Draft` to `Submitted`, and enter insurance settlement details.

---

## ⚙️ Module 8: Notifications & Reminders

*Log out and log in as **Super Admin** (`admin@caresphere.in` / `Admin@123456`)*

### 🖥️ Page 56: Notifications Dashboard (`/notifications/dashboard`)
*   **Expected**: View metrics for delivery channels (SMS, Email, WhatsApp) and logs.

### 🖥️ Page 57: Templates Catalog (`/notifications/templates`)
*   **Expected**: Displays templates like `General Alert Template`.

### 🖥️ Page 58: Delivery Logs (`/notifications/logs`)
*   **Expected**: Review history of dispatched alerts (e.g. John Doe's seeded SMS log).

### 🖥️ Page 59: Scheduled Reminders (`/notifications/reminders`)
*   **Expected**: Displays upcoming reminders (e.g. John Doe's scheduled 24h appointment reminder).
