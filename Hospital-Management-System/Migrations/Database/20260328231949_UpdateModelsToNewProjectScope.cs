using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital_Management_System.Migrations.Database
{
    /// <inheritdoc />
    public partial class UpdateModelsToNewProjectScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "appointment_ibfk_1",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "Appointment");

            migrationBuilder.RenameIndex(
                name: "VisitID1",
                table: "Referral",
                newName: "VisitID2");

            migrationBuilder.AddColumn<int>(
                name: "NurseId",
                table: "Visits",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PublicTestId",
                table: "TestResult",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(string),
                oldType: "varchar(12)",
                oldMaxLength: 12)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "Shift",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicID",
                table: "Secretary_Shifts",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicID",
                table: "Secretary",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicID",
                table: "Prescription",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicID",
                table: "Patient",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicID",
                table: "NurseShifts",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicID",
                table: "Nurse",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicID",
                table: "Manager",
                type: "varchar(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicID",
                table: "Fee",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "VisitID",
                table: "Fee",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PublicID",
                table: "Doctors_Shifts",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicID",
                table: "Doctor",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicID",
                table: "DiagnosticTest",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "PatientID",
                table: "Appointment",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AppointmentDate",
                table: "Appointment",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "BookedAt",
                table: "Appointment",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicID",
                table: "Appointment",
                type: "varchar(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicID",
                table: "Administrative_Assistant",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicID",
                table: "AdminAssistant_Shifts",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_NurseId",
                table: "Visits",
                column: "NurseId");

            migrationBuilder.CreateIndex(
                name: "VisitID1",
                table: "Fee",
                column: "VisitID");

            migrationBuilder.AddForeignKey(
                name: "appointment_ibfk_1",
                table: "Appointment",
                column: "PatientID",
                principalTable: "Patient",
                principalColumn: "PatientID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fee_Visits_VisitID",
                table: "Fee",
                column: "VisitID",
                principalTable: "Visits",
                principalColumn: "VisitsID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Nurse_NurseId",
                table: "Visits",
                column: "NurseId",
                principalTable: "Nurse",
                principalColumn: "NurseID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "appointment_ibfk_1",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Fee_Visits_VisitID",
                table: "Fee");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Nurse_NurseId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_NurseId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "VisitID1",
                table: "Fee");

            migrationBuilder.DropColumn(
                name: "NurseId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Shift");

            migrationBuilder.DropColumn(
                name: "PublicID",
                table: "Secretary_Shifts");

            migrationBuilder.DropColumn(
                name: "PublicID",
                table: "Secretary");

            migrationBuilder.DropColumn(
                name: "PublicID",
                table: "Prescription");

            migrationBuilder.DropColumn(
                name: "PublicID",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "PublicID",
                table: "NurseShifts");

            migrationBuilder.DropColumn(
                name: "PublicID",
                table: "Nurse");

            migrationBuilder.DropColumn(
                name: "PublicID",
                table: "Manager");

            migrationBuilder.DropColumn(
                name: "PublicID",
                table: "Fee");

            migrationBuilder.DropColumn(
                name: "VisitID",
                table: "Fee");

            migrationBuilder.DropColumn(
                name: "PublicID",
                table: "Doctors_Shifts");

            migrationBuilder.DropColumn(
                name: "PublicID",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "PublicID",
                table: "DiagnosticTest");

            migrationBuilder.DropColumn(
                name: "AppointmentDate",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "BookedAt",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "PublicID",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "PublicID",
                table: "Administrative_Assistant");

            migrationBuilder.DropColumn(
                name: "PublicID",
                table: "AdminAssistant_Shifts");

            migrationBuilder.RenameIndex(
                name: "VisitID2",
                table: "Referral",
                newName: "VisitID1");

            migrationBuilder.AlterColumn<string>(
                name: "PublicTestId",
                table: "TestResult",
                type: "varchar(12)",
                maxLength: 12,
                nullable: false,
                collation: "utf8mb4_0900_ai_ci",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.AlterColumn<int>(
                name: "PatientID",
                table: "Appointment",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "Appointment",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "Time",
                table: "Appointment",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddForeignKey(
                name: "appointment_ibfk_1",
                table: "Appointment",
                column: "PatientID",
                principalTable: "Patient",
                principalColumn: "PatientID");
        }
    }
}
