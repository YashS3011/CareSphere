# 🏥 CareSphere - Manual Dry Run Guide (Fresh Data)

Use this document to copy and paste credentials and data inputs for executing a full end-to-end dry run of the hospital management system.

---

## 🔑 Login Credentials

| Role | Email / Username | Password | Landing Page / Target Route |
| :--- | :--- | :--- | :--- |
| **Receptionist** | `receptionist@caresphere.dev` | `Receptionist@123` | `/patients` |
| **Doctor** | `doctor@caresphere.dev` | `Doctor@123` | `/doctor/queue/{DoctorId}` |
| **Nurse** | `nurse@caresphere.dev` | `Nurse@123` | `/beds/dashboard` & `/nursing/notes` |
| **Lab Technician** | `labtech@caresphere.dev` | `LabTech@123` | `/lab/dashboard` |
| **Pharmacist** | `pharmacist@caresphere.dev` | `Pharmacist@123` | `/pharmacy/dashboard` |
| **Billing Staff** | `billingstaff@caresphere.dev` | `BillingStaff@123` | `/billing/dashboard` |

---

## 📋 Fresh Test Data Inputs

### 1. Patient Registration Demographics
*   **First Name:** `Sarah`
*   **Last Name:** `Jenkins`
*   **DOB:** `1988-11-15` (Age: 37)
*   **Gender:** `Female`
*   **Blood Group:** `B+`
*   **Phone:** `9876549870`
*   **Email:** `sarah.jenkins@example.com`
*   **Address:** `789 Maple Drive, Sector 4`

### 2. Clinical SOAP Note (Doctor)
*   **Subjective:** `Patient reports persistent dry cough, moderate fatigue, and fever over the last 3 days.`
*   **Objective:** `Temp 100.8 F, BP 115/75 mmHg, Pulse 82 bpm. Clear lungs bilaterally.`
*   **Assessment:** `Mild Acute Bronchitis.`
*   **Plan:** `Order CBC to evaluate infection; initiate symptom management with Paracetamol.`

### 3. Lab Order & Verification (Lab Tech)
*   **Lab Test ordered:** `Complete Blood Count (CBC)`
*   **Specimen Barcode:** `SMPL-CBC-SJ01`
*   **Test Parameter Code:** `HB` (Hemoglobin)
*   **Entered Result Value:** `11.0` (Reference range: `12.0` - `16.0` g/dL. System will flag as **Low** / Abnormal).

### 4. Ward Admission (Nurse)
*   **Bed Allotment:** General Ward A — Bed `B-101` (Or any available standard bed).
*   **Nursing Observation Log Entry:** `Sarah Jenkins admitted to Bed B-101. Vitals taken: Temp 100.2F. First dose of Paracetamol administered. Awaiting CBC lab reports.`

### 5. Pharmacy Dispensing (Pharmacist)
*   **Drug Name:** `Paracetamol 500mg` (`PAR-500`)
*   **Batch Number:** `BAT-PAR-99`
*   **Prescribed Qty:** `10`

---

## 🔄 Execution Workflow

### Step 1: Receptionist Role
1. Log in to the application as **Receptionist**.
2. Go to the **Patients** dashboard and register **Sarah Jenkins** with the demographics above.
3. Check her in to the queue of **Dr. Jane Smith**. Her queue entry status transitions to `Waiting`.

### Step 2: Doctor Role
1. Log in to the application as **Doctor**.
2. Open the **Doctor Queue**.
3. Select **Sarah Jenkins** and click **Start Consultation** (Encounter transitions to `InProgress`).
4. Complete the **SOAP Note** fields.
5. Create a new **Lab Requisition** for **Complete Blood Count (CBC)**.
6. Create a **Prescription** for **Paracetamol 500mg** (Qty: 10).
7. Click **Finalize SOAP Note** to lock it.
8. Complete the consultation.

### Step 3: Nurse Role
1. Log in to the application as **Nurse**.
2. Open `/beds/dashboard` and select Bed `B-101` (or another available bed).
3. Click **Allot Bed**, pick **Sarah Jenkins**, set type to `IPD`, and save.
4. Navigate to `/nursing/notes`.
5. Select **Sarah Jenkins** and write the nursing log entry observation. Save the log.

### Step 4: Lab Technician Role
1. Log in to the application as **Lab Technician**.
2. Open `/lab/dashboard` and locate the CBC order for **Sarah Jenkins**.
3. Click **Collect Specimen**, barcode label `SMPL-CBC-SJ01`, and set status to `Collected`.
4. Click **Receive Specimen** to mark it as received in the lab.
5. Under **Enter Results**, enter Hemoglobin level `11.0`. Verify the abnormal flag.
6. Click **Verify & Finalize Report** to trigger QuestPDF report building and dispatch outbox notifications.

### Step 5: Pharmacist Role
1. Log in to the application as **Pharmacist**.
2. Open `/pharmacy/dashboard`.
3. Locate **Sarah Jenkins** under active prescriptions.
4. Dispense the Paracetamol (Qty: 10) from batch `BAT-PAR-99`. Save.

### Step 6: Billing Staff Role
1. Log in to the application as **Billing Staff**.
2. Open `/billing/dashboard` and search for **Sarah Jenkins**.
3. Review the automatically compiled draft invoice items (Consultation fee + CBC lab test fee + Paracetamol fee + Bed stay fee).
4. Click **Finalize Invoice**.
5. Record a cash payment for the total balance. Status updates to `Paid`.
6. Download the generated receipt PDF.
