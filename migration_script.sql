CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221150503_InitialCreate') THEN
    CREATE TABLE patients (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        mrn character varying(50) NOT NULL,
        first_name character varying(100) NOT NULL,
        last_name character varying(100) NOT NULL,
        date_of_birth date NOT NULL,
        gender character varying(20) NOT NULL,
        phone character varying(30) NOT NULL,
        email character varying(150),
        address text,
        abha_id character varying(50),
        blood_group character varying(10),
        tenant_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone,
        CONSTRAINT "PK_patients" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221150503_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_patients_mrn" ON patients (mrn);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260221150503_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260221150503_InitialCreate', '9.0.0');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223165123_AddBedManagement') THEN
    CREATE TABLE wards (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        name character varying(100) NOT NULL,
        ward_type character varying(50) NOT NULL,
        floor character varying(50),
        building character varying(100),
        tenant_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_wards" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223165123_AddBedManagement') THEN
    CREATE TABLE beds (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        bed_number character varying(50) NOT NULL,
        ward_id uuid NOT NULL,
        bed_type character varying(50) NOT NULL,
        status character varying(50) NOT NULL,
        is_active boolean NOT NULL,
        tenant_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_beds" PRIMARY KEY (id),
        CONSTRAINT "FK_beds_wards_ward_id" FOREIGN KEY (ward_id) REFERENCES wards (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223165123_AddBedManagement') THEN
    CREATE TABLE bed_allotments (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        bed_id uuid NOT NULL,
        patient_id uuid NOT NULL,
        admission_date timestamp with time zone NOT NULL,
        discharge_date timestamp with time zone,
        admission_type character varying(50) NOT NULL,
        admitting_doctor character varying(100),
        notes text,
        discharge_notes text,
        status character varying(50) NOT NULL,
        tenant_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_bed_allotments" PRIMARY KEY (id),
        CONSTRAINT "FK_bed_allotments_beds_bed_id" FOREIGN KEY (bed_id) REFERENCES beds (id) ON DELETE CASCADE,
        CONSTRAINT "FK_bed_allotments_patients_patient_id" FOREIGN KEY (patient_id) REFERENCES patients (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223165123_AddBedManagement') THEN
    CREATE TABLE bed_transfers (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        allotment_id uuid NOT NULL,
        from_bed_id uuid NOT NULL,
        to_bed_id uuid NOT NULL,
        transfer_reason text NOT NULL,
        transferred_at timestamp with time zone NOT NULL,
        tenant_id uuid NOT NULL,
        CONSTRAINT "PK_bed_transfers" PRIMARY KEY (id),
        CONSTRAINT "FK_bed_transfers_bed_allotments_allotment_id" FOREIGN KEY (allotment_id) REFERENCES bed_allotments (id) ON DELETE CASCADE,
        CONSTRAINT "FK_bed_transfers_beds_from_bed_id" FOREIGN KEY (from_bed_id) REFERENCES beds (id) ON DELETE CASCADE,
        CONSTRAINT "FK_bed_transfers_beds_to_bed_id" FOREIGN KEY (to_bed_id) REFERENCES beds (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223165123_AddBedManagement') THEN
    CREATE INDEX "IX_bed_allotments_bed_id" ON bed_allotments (bed_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223165123_AddBedManagement') THEN
    CREATE INDEX "IX_bed_allotments_patient_id" ON bed_allotments (patient_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223165123_AddBedManagement') THEN
    CREATE INDEX "IX_bed_transfers_allotment_id" ON bed_transfers (allotment_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223165123_AddBedManagement') THEN
    CREATE INDEX "IX_bed_transfers_from_bed_id" ON bed_transfers (from_bed_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223165123_AddBedManagement') THEN
    CREATE INDEX "IX_bed_transfers_to_bed_id" ON bed_transfers (to_bed_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223165123_AddBedManagement') THEN
    CREATE UNIQUE INDEX "IX_beds_bed_number" ON beds (bed_number);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223165123_AddBedManagement') THEN
    CREATE INDEX "IX_beds_ward_id" ON beds (ward_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260223165123_AddBedManagement') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260223165123_AddBedManagement', '9.0.0');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE TABLE audit_events (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        user_id character varying(100) NOT NULL,
        action character varying(100) NOT NULL,
        resource_type character varying(100) NOT NULL,
        resource_id character varying(100) NOT NULL,
        ip_address character varying(50),
        timestamp timestamp with time zone NOT NULL DEFAULT (now()),
        tenant_id uuid NOT NULL,
        CONSTRAINT "PK_audit_events" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE TABLE doctors (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        first_name character varying(100) NOT NULL,
        last_name character varying(100) NOT NULL,
        specialization character varying(100) NOT NULL,
        registration_number character varying(50) NOT NULL,
        phone character varying(30),
        email character varying(150),
        is_active boolean NOT NULL,
        tenant_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_doctors" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE TABLE drug_formulary (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        drug_code character varying(50) NOT NULL,
        generic_name character varying(200) NOT NULL,
        brand_name character varying(200),
        form character varying(50) NOT NULL,
        strength character varying(50) NOT NULL,
        unit character varying(50) NOT NULL,
        is_controlled boolean NOT NULL,
        is_active boolean NOT NULL,
        tenant_id uuid NOT NULL,
        CONSTRAINT "PK_drug_formulary" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE TABLE drug_interactions (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        drug_code_a character varying(50) NOT NULL,
        drug_code_b character varying(50) NOT NULL,
        severity character varying(50) NOT NULL,
        description text,
        tenant_id uuid NOT NULL,
        CONSTRAINT "PK_drug_interactions" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE TABLE doctor_queue_entries (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        doctor_id uuid NOT NULL,
        patient_id uuid NOT NULL,
        status character varying(50) NOT NULL,
        queue_position integer NOT NULL,
        estimated_wait_minutes integer,
        checked_in_at timestamp with time zone NOT NULL DEFAULT (now()),
        started_at timestamp with time zone,
        completed_at timestamp with time zone,
        notes text,
        tenant_id uuid NOT NULL,
        CONSTRAINT "PK_doctor_queue_entries" PRIMARY KEY (id),
        CONSTRAINT "FK_doctor_queue_entries_doctors_doctor_id" FOREIGN KEY (doctor_id) REFERENCES doctors (id) ON DELETE CASCADE,
        CONSTRAINT "FK_doctor_queue_entries_patients_patient_id" FOREIGN KEY (patient_id) REFERENCES patients (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE TABLE encounters (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        patient_id uuid NOT NULL,
        doctor_id uuid NOT NULL,
        encounter_type character varying(50) NOT NULL,
        status character varying(50) NOT NULL,
        admission_date timestamp with time zone NOT NULL,
        discharge_date timestamp with time zone,
        chief_complaint text,
        tenant_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_encounters" PRIMARY KEY (id),
        CONSTRAINT "FK_encounters_doctors_doctor_id" FOREIGN KEY (doctor_id) REFERENCES doctors (id) ON DELETE CASCADE,
        CONSTRAINT "FK_encounters_patients_patient_id" FOREIGN KEY (patient_id) REFERENCES patients (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE TABLE prescriptions (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        encounter_id uuid NOT NULL,
        patient_id uuid NOT NULL,
        doctor_id uuid NOT NULL,
        drug_name character varying(200) NOT NULL,
        drug_code character varying(50),
        form character varying(50) NOT NULL,
        strength character varying(50) NOT NULL,
        frequency character varying(100) NOT NULL,
        duration character varying(50) NOT NULL,
        route character varying(50) NOT NULL,
        quantity integer NOT NULL,
        notes text,
        status character varying(50) NOT NULL,
        issued_at timestamp with time zone NOT NULL DEFAULT (now()),
        cancelled_at timestamp with time zone,
        cancellation_reason text,
        tenant_id uuid NOT NULL,
        CONSTRAINT "PK_prescriptions" PRIMARY KEY (id),
        CONSTRAINT "FK_prescriptions_doctors_doctor_id" FOREIGN KEY (doctor_id) REFERENCES doctors (id) ON DELETE CASCADE,
        CONSTRAINT "FK_prescriptions_encounters_encounter_id" FOREIGN KEY (encounter_id) REFERENCES encounters (id) ON DELETE CASCADE,
        CONSTRAINT "FK_prescriptions_patients_patient_id" FOREIGN KEY (patient_id) REFERENCES patients (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE TABLE soap_notes (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        encounter_id uuid NOT NULL,
        subjective text NOT NULL,
        objective text NOT NULL,
        assessment text NOT NULL,
        plan text NOT NULL,
        status character varying(50) NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        finalized_at timestamp with time zone,
        created_by_doctor_id uuid NOT NULL,
        tenant_id uuid NOT NULL,
        CONSTRAINT "PK_soap_notes" PRIMARY KEY (id),
        CONSTRAINT "FK_soap_notes_doctors_created_by_doctor_id" FOREIGN KEY (created_by_doctor_id) REFERENCES doctors (id) ON DELETE CASCADE,
        CONSTRAINT "FK_soap_notes_encounters_encounter_id" FOREIGN KEY (encounter_id) REFERENCES encounters (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE TABLE tele_consult_sessions (
        id uuid NOT NULL DEFAULT (gen_random_uuid()),
        encounter_id uuid NOT NULL,
        doctor_id uuid NOT NULL,
        patient_id uuid NOT NULL,
        session_type character varying(50) NOT NULL,
        status character varying(50) NOT NULL,
        meeting_link character varying(500),
        started_at timestamp with time zone,
        ended_at timestamp with time zone,
        duration_minutes integer,
        notes text,
        tenant_id uuid NOT NULL,
        CONSTRAINT "PK_tele_consult_sessions" PRIMARY KEY (id),
        CONSTRAINT "FK_tele_consult_sessions_doctors_doctor_id" FOREIGN KEY (doctor_id) REFERENCES doctors (id) ON DELETE CASCADE,
        CONSTRAINT "FK_tele_consult_sessions_encounters_encounter_id" FOREIGN KEY (encounter_id) REFERENCES encounters (id) ON DELETE CASCADE,
        CONSTRAINT "FK_tele_consult_sessions_patients_patient_id" FOREIGN KEY (patient_id) REFERENCES patients (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE INDEX "IX_doctor_queue_entries_doctor_id" ON doctor_queue_entries (doctor_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE INDEX "IX_doctor_queue_entries_patient_id" ON doctor_queue_entries (patient_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE UNIQUE INDEX "IX_doctors_registration_number" ON doctors (registration_number);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE UNIQUE INDEX "IX_drug_formulary_drug_code" ON drug_formulary (drug_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE INDEX "IX_encounters_doctor_id" ON encounters (doctor_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE INDEX "IX_encounters_patient_id" ON encounters (patient_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE INDEX "IX_prescriptions_doctor_id" ON prescriptions (doctor_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE INDEX "IX_prescriptions_encounter_id" ON prescriptions (encounter_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE INDEX "IX_prescriptions_patient_id" ON prescriptions (patient_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE INDEX "IX_soap_notes_created_by_doctor_id" ON soap_notes (created_by_doctor_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE INDEX "IX_soap_notes_encounter_id" ON soap_notes (encounter_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE INDEX "IX_tele_consult_sessions_doctor_id" ON tele_consult_sessions (doctor_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE INDEX "IX_tele_consult_sessions_encounter_id" ON tele_consult_sessions (encounter_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    CREATE INDEX "IX_tele_consult_sessions_patient_id" ON tele_consult_sessions (patient_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260609044814_AddDoctorWorkflowAndEMR') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260609044814_AddDoctorWorkflowAndEMR', '9.0.0');
    END IF;
END $EF$;
COMMIT;

