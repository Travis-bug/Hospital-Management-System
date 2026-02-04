using System;
using System.Collections.Generic;
using Clinic_Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Management.Data;

public partial class ClinicContext : DbContext
{
    public ClinicContext(DbContextOptions<ClinicContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminAssistantShift> AdminAssistantShifts { get; set; }

    public virtual DbSet<AdministrativeAssistant> AdministrativeAssistants { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<DiagnosticTest> DiagnosticTests { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<DoctorsShift> DoctorsShifts { get; set; }

    public virtual DbSet<Fee> Fees { get; set; }

    public virtual DbSet<Manager> Managers { get; set; }

    public virtual DbSet<Nurse> Nurses { get; set; }

    public virtual DbSet<NurseShift> NurseShifts { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PatientVital> PatientVitals { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<Referral> Referrals { get; set; }

    public virtual DbSet<Secretary> Secretaries { get; set; }

    public virtual DbSet<SecretaryShift> SecretaryShifts { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<TestResult> TestResults { get; set; }

    public virtual DbSet<Visit> Visits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AdminAssistantShift>(entity =>
        {
            entity.HasKey(e => e.AdminShiftId).HasName("PRIMARY");

            entity.Property(e => e.HoursWorked).HasComputedColumnSql("timestampdiff(HOUR,`ClockInTime`,`ClockOutTime`)", false);

            entity.HasOne(d => d.Admin).WithMany(p => p.AdminAssistantShifts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("adminassistant_shifts_ibfk_1");

            entity.HasOne(d => d.Shift).WithMany(p => p.AdminAssistantShifts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("adminassistant_shifts_ibfk_2");
        });

        modelBuilder.Entity<AdministrativeAssistant>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PRIMARY");

            entity.Property(e => e.HourlyRate).HasDefaultValueSql("'20.00'");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PRIMARY");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments).HasConstraintName("appointment_ibfk_2");

            entity.HasOne(d => d.Nurse).WithMany(p => p.Appointments).HasConstraintName("appointment_ibfk_3");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments).HasConstraintName("appointment_ibfk_1");
        });

        modelBuilder.Entity<DiagnosticTest>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("PRIMARY");

            entity.Property(e => e.OrderedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Doctor).WithMany(p => p.DiagnosticTests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("diagnostictest_ibfk_2");

            entity.HasOne(d => d.Visit).WithMany(p => p.DiagnosticTests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("diagnostictest_ibfk_1");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DoctorId).HasName("PRIMARY");
        });

        modelBuilder.Entity<DoctorsShift>(entity =>
        {
            entity.HasKey(e => e.DoctorsShiftId).HasName("PRIMARY");

            entity.HasOne(d => d.Doctor).WithMany(p => p.DoctorsShifts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("doctors_shifts_ibfk_1");

            entity.HasOne(d => d.Shift).WithMany(p => p.DoctorsShifts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("doctors_shifts_ibfk_2");
        });

        modelBuilder.Entity<Fee>(entity =>
        {
            entity.HasKey(e => e.FeeId).HasName("PRIMARY");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Fees).HasConstraintName("fee_ibfk_2");

            entity.HasOne(d => d.Patient).WithMany(p => p.Fees).HasConstraintName("fee_ibfk_1");
        });

        modelBuilder.Entity<Manager>(entity =>
        {
            entity.HasKey(e => e.ManagerId).HasName("PRIMARY");
        });

        modelBuilder.Entity<Nurse>(entity =>
        {
            entity.HasKey(e => e.NurseId).HasName("PRIMARY");

            entity.Property(e => e.HourlyRate).HasDefaultValueSql("'35.00'");
        });

        modelBuilder.Entity<NurseShift>(entity =>
        {
            entity.HasKey(e => e.NurseShiftId).HasName("PRIMARY");

            entity.Property(e => e.HoursWorked).HasComputedColumnSql("timestampdiff(HOUR,`ClockInTime`,`ClockOutTime`)", false);

            entity.HasOne(d => d.Nurse).WithMany(p => p.NurseShifts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("nurseshifts_ibfk_1");

            entity.HasOne(d => d.Shift).WithMany(p => p.NurseShifts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("nurseshifts_ibfk_2");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PRIMARY");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Patients).HasConstraintName("patient_ibfk_1");

            entity.HasOne(d => d.PrimaryMember).WithMany(p => p.InversePrimaryMember).HasConstraintName("patient_ibfk_2");
        });

        modelBuilder.Entity<PatientVital>(entity =>
        {
            entity.HasKey(e => e.VitalsId).HasName("PRIMARY");

            entity.Property(e => e.RecordedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Nurse).WithMany(p => p.PatientVitals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("patient_vitals_ibfk_1");

            entity.HasOne(d => d.Visits).WithMany(p => p.PatientVitals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("patient_vitals_ibfk_2");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.PrescriptionId).HasName("PRIMARY");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Prescriptions).HasConstraintName("prescription_ibfk_2");

            entity.HasOne(d => d.Result).WithMany(p => p.Prescriptions).HasConstraintName("prescription_ibfk_1");

            entity.HasOne(d => d.Visits).WithMany(p => p.Prescriptions).HasConstraintName("prescription_ibfk_3");
        });

        modelBuilder.Entity<Referral>(entity =>
        {
            entity.HasKey(e => e.ReferralId).HasName("PRIMARY");

            entity.HasOne(d => d.ReferringDoctor).WithMany(p => p.ReferralReferringDoctors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("referral_ibfk_2");

            entity.HasOne(d => d.SpecialistDoctor).WithMany(p => p.ReferralSpecialistDoctors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("referral_ibfk_3");

            entity.HasOne(d => d.Visit).WithMany(p => p.Referrals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("referral_ibfk_1");
        });

        modelBuilder.Entity<Secretary>(entity =>
        {
            entity.HasKey(e => e.SecretaryId).HasName("PRIMARY");

            entity.Property(e => e.HourlyRate).HasDefaultValueSql("'25.00'");
        });

        modelBuilder.Entity<SecretaryShift>(entity =>
        {
            entity.HasKey(e => e.SecretaryShiftId).HasName("PRIMARY");

            entity.Property(e => e.HoursWorked).HasComputedColumnSql("timestampdiff(HOUR,`ClockInTime`,`ClockOutTime`)", false);

            entity.HasOne(d => d.Secretary).WithMany(p => p.SecretaryShifts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("secretary_shifts_ibfk_1");

            entity.HasOne(d => d.Shift).WithMany(p => p.SecretaryShifts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("secretary_shifts_ibfk_2");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.ShiftId).HasName("PRIMARY");
        });

        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PRIMARY");

            entity.Property(e => e.ResultDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Nurse).WithMany(p => p.TestResults)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("testresult_ibfk_2");

            entity.HasOne(d => d.Test).WithMany(p => p.TestResults)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("testresult_ibfk_1");
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.HasKey(e => e.VisitsId).HasName("PRIMARY");

            entity.Property(e => e.CheckinTime).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Visits).HasConstraintName("visits_ibfk_2");

            entity.HasOne(d => d.Patient).WithMany(p => p.Visits)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("visits_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
