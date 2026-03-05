using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hospital_Management_System.Database
{
    /// <inheritdoc />
    public partial class InitialSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Administrative_Assistant",
                columns: table => new
                {
                    AdminID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HourlyRate = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, defaultValueSql: "'20.00'"),
                    StreetAddress = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Province = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PostalCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdentityUserId = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.AdminID);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Doctor",
                columns: table => new
                {
                    DoctorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Specialization = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StreetAddress = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Province = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PostalCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdentityUserId = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.DoctorID);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Manager",
                columns: table => new
                {
                    ManagerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HourlyRate = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    StreetAddress = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Province = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PostalCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdentityUserId = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ManagerID);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Nurse",
                columns: table => new
                {
                    NurseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HourlyRate = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, defaultValueSql: "'35.00'"),
                    StreetAddress = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Province = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PostalCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdentityUserId = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.NurseID);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Secretary",
                columns: table => new
                {
                    SecretaryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HourlyRate = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, defaultValueSql: "'25.00'"),
                    StreetAddress = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Province = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PostalCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdentityUserId = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.SecretaryID);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Shift",
                columns: table => new
                {
                    ShiftID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ShiftType = table.Column<string>(type: "enum('Morning','Evening')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ShiftID);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Patient",
                columns: table => new
                {
                    PatientID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Address = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HealthCardNo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "enum('Enrolled','Walk-in')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DoctorID = table.Column<int>(type: "int", nullable: true),
                    PrimaryMemberID = table.Column<int>(type: "int", nullable: true),
                    Relationship = table.Column<string>(type: "enum('Husband','Wife','Son','Daughter','Father','Mother','Other')", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PatientID);
                    table.ForeignKey(
                        name: "patient_ibfk_1",
                        column: x => x.DoctorID,
                        principalTable: "Doctor",
                        principalColumn: "DoctorID");
                    table.ForeignKey(
                        name: "patient_ibfk_2",
                        column: x => x.PrimaryMemberID,
                        principalTable: "Patient",
                        principalColumn: "PatientID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "AdminAssistant_Shifts",
                columns: table => new
                {
                    AdminShiftID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ClockInTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    ClockOutTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    AdminID = table.Column<int>(type: "int", nullable: false),
                    ShiftID = table.Column<int>(type: "int", nullable: false),
                    HoursWorked = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true, computedColumnSql: "timestampdiff(HOUR,`ClockInTime`,`ClockOutTime`)", stored: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.AdminShiftID);
                    table.ForeignKey(
                        name: "adminassistant_shifts_ibfk_1",
                        column: x => x.AdminID,
                        principalTable: "Administrative_Assistant",
                        principalColumn: "AdminID");
                    table.ForeignKey(
                        name: "adminassistant_shifts_ibfk_2",
                        column: x => x.ShiftID,
                        principalTable: "Shift",
                        principalColumn: "ShiftID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Doctors_Shifts",
                columns: table => new
                {
                    Doctors_ShiftID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ClockInTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    ClockOutTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    DoctorID = table.Column<int>(type: "int", nullable: false),
                    ShiftID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Doctors_ShiftID);
                    table.ForeignKey(
                        name: "doctors_shifts_ibfk_1",
                        column: x => x.DoctorID,
                        principalTable: "Doctor",
                        principalColumn: "DoctorID");
                    table.ForeignKey(
                        name: "doctors_shifts_ibfk_2",
                        column: x => x.ShiftID,
                        principalTable: "Shift",
                        principalColumn: "ShiftID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "NurseShifts",
                columns: table => new
                {
                    NurseShiftID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ClockInTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    ClockOutTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    NurseID = table.Column<int>(type: "int", nullable: false),
                    ShiftID = table.Column<int>(type: "int", nullable: false),
                    HoursWorked = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true, computedColumnSql: "timestampdiff(HOUR,`ClockInTime`,`ClockOutTime`)", stored: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.NurseShiftID);
                    table.ForeignKey(
                        name: "nurseshifts_ibfk_1",
                        column: x => x.NurseID,
                        principalTable: "Nurse",
                        principalColumn: "NurseID");
                    table.ForeignKey(
                        name: "nurseshifts_ibfk_2",
                        column: x => x.ShiftID,
                        principalTable: "Shift",
                        principalColumn: "ShiftID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Secretary_Shifts",
                columns: table => new
                {
                    Secretary_ShiftID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SecretaryID = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ClockInTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    ClockOutTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    ShiftID = table.Column<int>(type: "int", nullable: false),
                    HoursWorked = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true, computedColumnSql: "timestampdiff(HOUR,`ClockInTime`,`ClockOutTime`)", stored: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Secretary_ShiftID);
                    table.ForeignKey(
                        name: "secretary_shifts_ibfk_1",
                        column: x => x.SecretaryID,
                        principalTable: "Secretary",
                        principalColumn: "SecretaryID");
                    table.ForeignKey(
                        name: "secretary_shifts_ibfk_2",
                        column: x => x.ShiftID,
                        principalTable: "Shift",
                        principalColumn: "ShiftID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Appointment",
                columns: table => new
                {
                    AppointmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PatientID = table.Column<int>(type: "int", nullable: true),
                    DoctorID = table.Column<int>(type: "int", nullable: true),
                    NurseID = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Time = table.Column<TimeOnly>(type: "time", nullable: false),
                    Status = table.Column<string>(type: "enum('Booked','Cancelled','Arrived','Checked In','Checked Out','LWT','No-Show')", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.AppointmentID);
                    table.ForeignKey(
                        name: "appointment_ibfk_1",
                        column: x => x.PatientID,
                        principalTable: "Patient",
                        principalColumn: "PatientID");
                    table.ForeignKey(
                        name: "appointment_ibfk_2",
                        column: x => x.DoctorID,
                        principalTable: "Doctor",
                        principalColumn: "DoctorID");
                    table.ForeignKey(
                        name: "appointment_ibfk_3",
                        column: x => x.NurseID,
                        principalTable: "Nurse",
                        principalColumn: "NurseID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Fee",
                columns: table => new
                {
                    FeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PatientID = table.Column<int>(type: "int", nullable: true),
                    DoctorID = table.Column<int>(type: "int", nullable: true),
                    FeeDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ServiceName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.FeeID);
                    table.ForeignKey(
                        name: "fee_ibfk_1",
                        column: x => x.PatientID,
                        principalTable: "Patient",
                        principalColumn: "PatientID");
                    table.ForeignKey(
                        name: "fee_ibfk_2",
                        column: x => x.DoctorID,
                        principalTable: "Doctor",
                        principalColumn: "DoctorID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Visits",
                columns: table => new
                {
                    VisitsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PatientID = table.Column<int>(type: "int", nullable: false),
                    Checkin_Time = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Checkout_Time = table.Column<DateTime>(type: "timestamp", nullable: true),
                    Symptoms = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Diagnosis = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Treatment = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AppointmentID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.VisitsID);
                    table.ForeignKey(
                        name: "visits_ibfk_1",
                        column: x => x.PatientID,
                        principalTable: "Patient",
                        principalColumn: "PatientID");
                    table.ForeignKey(
                        name: "visits_ibfk_2",
                        column: x => x.AppointmentID,
                        principalTable: "Appointment",
                        principalColumn: "AppointmentID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "DiagnosticTest",
                columns: table => new
                {
                    TestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VisitID = table.Column<int>(type: "int", nullable: false),
                    DoctorID = table.Column<int>(type: "int", nullable: false),
                    TestName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClinicalNotes = table.Column<string>(type: "text", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrderedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.TestID);
                    table.ForeignKey(
                        name: "diagnostictest_ibfk_1",
                        column: x => x.VisitID,
                        principalTable: "Visits",
                        principalColumn: "VisitsID");
                    table.ForeignKey(
                        name: "diagnostictest_ibfk_2",
                        column: x => x.DoctorID,
                        principalTable: "Doctor",
                        principalColumn: "DoctorID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Patient_Vitals",
                columns: table => new
                {
                    VitalsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VisitsID = table.Column<int>(type: "int", nullable: false),
                    NurseID = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Height = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    BloodPressure = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Temperature = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Recorded_At = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.VitalsID);
                    table.ForeignKey(
                        name: "patient_vitals_ibfk_1",
                        column: x => x.NurseID,
                        principalTable: "Nurse",
                        principalColumn: "NurseID");
                    table.ForeignKey(
                        name: "patient_vitals_ibfk_2",
                        column: x => x.VisitsID,
                        principalTable: "Visits",
                        principalColumn: "VisitsID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Referral",
                columns: table => new
                {
                    ReferralID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VisitID = table.Column<int>(type: "int", nullable: false),
                    ReferringDoctorID = table.Column<int>(type: "int", nullable: false),
                    SpecialistDoctorID = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ReferralID);
                    table.ForeignKey(
                        name: "referral_ibfk_1",
                        column: x => x.VisitID,
                        principalTable: "Visits",
                        principalColumn: "VisitsID");
                    table.ForeignKey(
                        name: "referral_ibfk_2",
                        column: x => x.ReferringDoctorID,
                        principalTable: "Doctor",
                        principalColumn: "DoctorID");
                    table.ForeignKey(
                        name: "referral_ibfk_3",
                        column: x => x.SpecialistDoctorID,
                        principalTable: "Doctor",
                        principalColumn: "DoctorID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "TestResult",
                columns: table => new
                {
                    ResultID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TestID = table.Column<int>(type: "int", nullable: false),
                    NurseID = table.Column<int>(type: "int", nullable: false),
                    Findings = table.Column<string>(type: "text", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResultDate = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ResultID);
                    table.ForeignKey(
                        name: "testresult_ibfk_1",
                        column: x => x.TestID,
                        principalTable: "DiagnosticTest",
                        principalColumn: "TestID");
                    table.ForeignKey(
                        name: "testresult_ibfk_2",
                        column: x => x.NurseID,
                        principalTable: "Nurse",
                        principalColumn: "NurseID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Prescription",
                columns: table => new
                {
                    PrescriptionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VisitsID = table.Column<int>(type: "int", nullable: true),
                    DoctorID = table.Column<int>(type: "int", nullable: true),
                    ResultID = table.Column<int>(type: "int", nullable: true),
                    MedicineName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dosage = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PrescriptionID);
                    table.ForeignKey(
                        name: "prescription_ibfk_1",
                        column: x => x.ResultID,
                        principalTable: "TestResult",
                        principalColumn: "ResultID");
                    table.ForeignKey(
                        name: "prescription_ibfk_2",
                        column: x => x.DoctorID,
                        principalTable: "Doctor",
                        principalColumn: "DoctorID");
                    table.ForeignKey(
                        name: "prescription_ibfk_3",
                        column: x => x.VisitsID,
                        principalTable: "Visits",
                        principalColumn: "VisitsID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "AdminID",
                table: "AdminAssistant_Shifts",
                column: "AdminID");

            migrationBuilder.CreateIndex(
                name: "ShiftID",
                table: "AdminAssistant_Shifts",
                column: "ShiftID");

            migrationBuilder.CreateIndex(
                name: "DoctorID",
                table: "Appointment",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "NurseID",
                table: "Appointment",
                column: "NurseID");

            migrationBuilder.CreateIndex(
                name: "PatientID",
                table: "Appointment",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "DoctorID1",
                table: "DiagnosticTest",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "VisitID",
                table: "DiagnosticTest",
                column: "VisitID");

            migrationBuilder.CreateIndex(
                name: "DoctorID2",
                table: "Doctors_Shifts",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "ShiftID1",
                table: "Doctors_Shifts",
                column: "ShiftID");

            migrationBuilder.CreateIndex(
                name: "DoctorID3",
                table: "Fee",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "PatientID1",
                table: "Fee",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "NurseID1",
                table: "NurseShifts",
                column: "NurseID");

            migrationBuilder.CreateIndex(
                name: "ShiftID2",
                table: "NurseShifts",
                column: "ShiftID");

            migrationBuilder.CreateIndex(
                name: "DoctorID4",
                table: "Patient",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "HealthCardNo",
                table: "Patient",
                column: "HealthCardNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "PhoneNumber",
                table: "Patient",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "PrimaryMemberID",
                table: "Patient",
                column: "PrimaryMemberID");

            migrationBuilder.CreateIndex(
                name: "NurseID2",
                table: "Patient_Vitals",
                column: "NurseID");

            migrationBuilder.CreateIndex(
                name: "VisitsID",
                table: "Patient_Vitals",
                column: "VisitsID");

            migrationBuilder.CreateIndex(
                name: "DoctorID5",
                table: "Prescription",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "ResultID",
                table: "Prescription",
                column: "ResultID");

            migrationBuilder.CreateIndex(
                name: "VisitsID1",
                table: "Prescription",
                column: "VisitsID");

            migrationBuilder.CreateIndex(
                name: "ReferringDoctorID",
                table: "Referral",
                column: "ReferringDoctorID");

            migrationBuilder.CreateIndex(
                name: "SpecialistDoctorID",
                table: "Referral",
                column: "SpecialistDoctorID");

            migrationBuilder.CreateIndex(
                name: "VisitID1",
                table: "Referral",
                column: "VisitID");

            migrationBuilder.CreateIndex(
                name: "SecretaryID",
                table: "Secretary_Shifts",
                column: "SecretaryID");

            migrationBuilder.CreateIndex(
                name: "ShiftID3",
                table: "Secretary_Shifts",
                column: "ShiftID");

            migrationBuilder.CreateIndex(
                name: "NurseID3",
                table: "TestResult",
                column: "NurseID");

            migrationBuilder.CreateIndex(
                name: "TestID",
                table: "TestResult",
                column: "TestID");

            migrationBuilder.CreateIndex(
                name: "AppointmentID",
                table: "Visits",
                column: "AppointmentID");

            migrationBuilder.CreateIndex(
                name: "PatientID2",
                table: "Visits",
                column: "PatientID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminAssistant_Shifts");

            migrationBuilder.DropTable(
                name: "Doctors_Shifts");

            migrationBuilder.DropTable(
                name: "Fee");

            migrationBuilder.DropTable(
                name: "Manager");

            migrationBuilder.DropTable(
                name: "NurseShifts");

            migrationBuilder.DropTable(
                name: "Patient_Vitals");

            migrationBuilder.DropTable(
                name: "Prescription");

            migrationBuilder.DropTable(
                name: "Referral");

            migrationBuilder.DropTable(
                name: "Secretary_Shifts");

            migrationBuilder.DropTable(
                name: "Administrative_Assistant");

            migrationBuilder.DropTable(
                name: "TestResult");

            migrationBuilder.DropTable(
                name: "Secretary");

            migrationBuilder.DropTable(
                name: "Shift");

            migrationBuilder.DropTable(
                name: "DiagnosticTest");

            migrationBuilder.DropTable(
                name: "Visits");

            migrationBuilder.DropTable(
                name: "Appointment");

            migrationBuilder.DropTable(
                name: "Patient");

            migrationBuilder.DropTable(
                name: "Nurse");

            migrationBuilder.DropTable(
                name: "Doctor");
        }
    }
}
