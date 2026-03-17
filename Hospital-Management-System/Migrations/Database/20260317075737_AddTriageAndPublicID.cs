using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital_Management_System.Migrations.Database
{
    /// <inheritdoc />
    public partial class AddTriageAndPublicID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdmissionStatus",
                table: "Visits",
                type: "enum('Admitted', 'Not Admitted', 'Discharged', 'Triage Pending')",
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ArrivalSource",
                table: "Visits",
                type: "enum('Appointment', 'Walk-in')",
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DoctorID",
                table: "Visits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientClass",
                table: "Visits",
                type: "enum('Inpatient', 'Outpatient', 'Emergency', 'ER Referral')",
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicID",
                table: "Visits",
                type: "varchar(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Visits",
                type: "enum('Active', 'Completed')",
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "VisitNotes",
                table: "Visits",
                type: "text",
                nullable: false,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Fee",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Fee",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PatientName",
                table: "Fee",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsTriageQualified",
                table: "Doctor",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_DoctorID",
                table: "Visits",
                column: "DoctorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Doctor_DoctorID",
                table: "Visits",
                column: "DoctorID",
                principalTable: "Doctor",
                principalColumn: "DoctorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Doctor_DoctorID",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_DoctorID",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "AdmissionStatus",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "ArrivalSource",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "DoctorID",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "PatientClass",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "PublicID",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "VisitNotes",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Fee");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Fee");

            migrationBuilder.DropColumn(
                name: "PatientName",
                table: "Fee");

            migrationBuilder.DropColumn(
                name: "IsTriageQualified",
                table: "Doctor");
        }
    }
}
