using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientIdToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "patient_id",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "patient_id",
                table: "AspNetUsers");
        }
    }
}
