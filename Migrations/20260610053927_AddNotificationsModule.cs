using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notification_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: true),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    recipient_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    recipient_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    notification_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "en"),
                    message_body = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    provider_message_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provider_response = table.Column<string>(type: "text", nullable: true),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_retries = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    service_bus_message_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_notification_logs_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_notification_logs_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "notification_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    notification_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "en"),
                    template_body = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "patient_preferences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    preferred_language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "en"),
                    preferred_channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "SMS"),
                    opt_out_sms = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    opt_out_whats_app = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    opt_out_voice = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    allow_appointment_reminders = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    allow_follow_up_reminders = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    allow_discharge_notifications = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    allow_lab_notifications = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_preferences", x => x.id);
                    table.ForeignKey(
                        name: "FK_patient_preferences_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_bus_outbox",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    scheduled_enqueue_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    enqueued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_bus_outbox", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "appointment_reminders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    appointment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    appointment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reminder_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "en"),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Scheduled"),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notification_log_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointment_reminders", x => x.id);
                    table.ForeignKey(
                        name: "FK_appointment_reminders_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_appointment_reminders_notification_logs_notification_log_id",
                        column: x => x.notification_log_id,
                        principalTable: "notification_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_appointment_reminders_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "discharge_notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    allotment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discharged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "en"),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    notification_log_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discharge_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_discharge_notifications_bed_allotments_allotment_id",
                        column: x => x.allotment_id,
                        principalTable: "bed_allotments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_discharge_notifications_notification_logs_notification_log_~",
                        column: x => x.notification_log_id,
                        principalTable: "notification_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_discharge_notifications_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appointment_reminders_doctor_id",
                table: "appointment_reminders",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_reminders_notification_log_id",
                table: "appointment_reminders",
                column: "notification_log_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_reminders_patient_id",
                table: "appointment_reminders",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_discharge_notifications_allotment_id",
                table: "discharge_notifications",
                column: "allotment_id");

            migrationBuilder.CreateIndex(
                name: "IX_discharge_notifications_notification_log_id",
                table: "discharge_notifications",
                column: "notification_log_id");

            migrationBuilder.CreateIndex(
                name: "IX_discharge_notifications_patient_id",
                table: "discharge_notifications",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_logs_doctor_id",
                table: "notification_logs",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_logs_patient_id",
                table: "notification_logs",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_tenant_id_template_name_channel_lang~",
                table: "notification_templates",
                columns: new[] { "tenant_id", "template_name", "channel", "language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_patient_preferences_patient_id",
                table: "patient_preferences",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_patient_preferences_tenant_id_patient_id",
                table: "patient_preferences",
                columns: new[] { "tenant_id", "patient_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appointment_reminders");

            migrationBuilder.DropTable(
                name: "discharge_notifications");

            migrationBuilder.DropTable(
                name: "notification_templates");

            migrationBuilder.DropTable(
                name: "patient_preferences");

            migrationBuilder.DropTable(
                name: "service_bus_outbox");

            migrationBuilder.DropTable(
                name: "notification_logs");
        }
    }
}
