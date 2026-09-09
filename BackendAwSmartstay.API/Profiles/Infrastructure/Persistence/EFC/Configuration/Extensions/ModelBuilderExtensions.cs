using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Entities;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Profiles.Infrastructure.Persistence.EFC.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Profiles.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Extension methods for configuring Profile entity mappings in Entity Framework Core.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Configures GuestProfile, StaffProfile, and StaffAssignment entity mappings.
    /// </summary>
    /// <param name="builder">The model builder instance.</param>
    public static void ApplyProfilesConfiguration(this ModelBuilder builder)
    {
        // ==========================================
        // 1. GuestProfile Mapping
        // ==========================================
        builder.Entity<GuestProfile>(entity =>
        {
            entity.ToTable("guest_profiles");

            // Ignore Domain Events
            entity.Ignore(g => g.DomainEvents);

            // Primary Key
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Id)
                .HasConversion(id => id.Value, value => new GuestProfileId(value))
                .IsRequired();

            // UserId (logical reference to IAM, nullable)
            entity.Property(g => g.UserId)
                .HasConversion(
                    u => u.HasValue ? u.Value.Value : (int?)null,
                    v => v.HasValue ? new UserId(v.Value) : (UserId?)null)
                .IsRequired(false);

            // Phone (required Value Object)
            entity.Property(g => g.Phone)
                .HasConversion(p => p.Value, v => new PhoneNumber(v))
                .HasMaxLength(20)
                .IsRequired();

            // Status (Enum)
            entity.Property(g => g.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Timestamps
            entity.Property(g => g.CreatedAt)
                .IsRequired();
            entity.Property(g => g.UpdatedAt)
                .IsRequired(false);

            // Name (Owned Value Object)
            entity.OwnsOne(g => g.Name, name =>
            {
                name.Property(n => n.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
                name.Property(n => n.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
            });

            // Email (Owned Value Object, optional)
            entity.OwnsOne(g => g.Email, email =>
            {
                email.Property(e => e.Address).HasColumnName("email_address").HasMaxLength(255).IsRequired();
                email.HasIndex(e => e.Address, "ix_guest_profiles_email_address").IsUnique();
            });

            // Document (Owned Value Object, optional)
            entity.OwnsOne(g => g.Document, doc =>
            {
                doc.Property(d => d.Type).HasColumnName("document_type").HasConversion<string>().HasMaxLength(20).IsRequired();
                doc.Property(d => d.Number).HasColumnName("document_number").HasMaxLength(50).IsRequired();
                doc.HasIndex(d => new { d.Type, d.Number }, "ix_guest_profiles_document").IsUnique();
            });

            // Address (Owned Value Object, optional)
            entity.OwnsOne(g => g.Address, addr =>
            {
                addr.Property(a => a.Street).HasColumnName("address_street").HasMaxLength(150).IsRequired();
                addr.Property(a => a.Number).HasColumnName("address_number").HasMaxLength(20).IsRequired();
                addr.Property(a => a.City).HasColumnName("address_city").HasMaxLength(100).IsRequired();
                addr.Property(a => a.PostalCode).HasColumnName("address_postal_code").HasMaxLength(20).IsRequired();
                addr.Property(a => a.Country).HasColumnName("address_country").HasMaxLength(100).IsRequired();
            });

            // Unique Index on UserId
            entity.HasIndex(g => g.UserId, "ix_guest_profiles_user_id").IsUnique();
        });

        // ==========================================
        // 2. StaffProfile Mapping
        // ==========================================
        builder.Entity<StaffProfile>(entity =>
        {
            entity.ToTable("staff_profiles");

            // Ignore Domain Events
            entity.Ignore(s => s.DomainEvents);

            // Primary Key
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id)
                .HasConversion(id => id.Value, value => new StaffProfileId(value))
                .IsRequired();

            // UserId (logical reference to IAM, required)
            entity.Property(s => s.UserId)
                .HasConversion(u => u.Value, v => new UserId(v))
                .IsRequired();

            // EmployeeCode
            entity.Property(s => s.Code)
                .HasConversion(c => c.Value, v => new EmployeeCode(v))
                .HasMaxLength(20)
                .IsRequired();

            // JobPosition
            entity.Property(s => s.Position)
                .HasConversion(p => p.Value, v => new JobPosition(v))
                .HasMaxLength(100)
                .IsRequired();

            // HabitualShift
            entity.Property(s => s.Shift)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Status
            entity.Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Phone (optional Value Object)
            entity.Property(s => s.Phone)
                .HasConversion(
                    p => p != null ? p.Value : null,
                    v => v != null ? new PhoneNumber(v) : null)
                .HasMaxLength(20)
                .IsRequired(false);

            // Timestamps
            entity.Property(s => s.CreatedAt)
                .IsRequired();
            entity.Property(s => s.UpdatedAt)
                .IsRequired(false);

            // Name (Owned Value Object)
            entity.OwnsOne(s => s.Name, name =>
            {
                name.Property(n => n.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
                name.Property(n => n.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
            });

            // Email (Owned Value Object, required)
            entity.OwnsOne(s => s.Email, email =>
            {
                email.Property(e => e.Address).HasColumnName("email_address").HasMaxLength(255).IsRequired();
                email.HasIndex(e => e.Address, "ix_staff_profiles_email_address").IsUnique();
            });

            // Document (Owned Value Object, optional)
            entity.OwnsOne(s => s.Document, doc =>
            {
                doc.Property(d => d.Type).HasColumnName("document_type").HasConversion<string>().HasMaxLength(20).IsRequired();
                doc.Property(d => d.Number).HasColumnName("document_number").HasMaxLength(50).IsRequired();
            });

            // Address (Owned Value Object, optional)
            entity.OwnsOne(s => s.Address, addr =>
            {
                addr.Property(a => a.Street).HasColumnName("address_street").HasMaxLength(150).IsRequired();
                addr.Property(a => a.Number).HasColumnName("address_number").HasMaxLength(20).IsRequired();
                addr.Property(a => a.City).HasColumnName("address_city").HasMaxLength(100).IsRequired();
                addr.Property(a => a.PostalCode).HasColumnName("address_postal_code").HasMaxLength(20).IsRequired();
                addr.Property(a => a.Country).HasColumnName("address_country").HasMaxLength(100).IsRequired();
            });

            // Unique Indexes
            entity.HasIndex(s => s.UserId, "ix_staff_profiles_user_id").IsUnique();
            entity.HasIndex(s => s.Code, "ix_staff_profiles_employee_code").IsUnique();

            // Relationship StaffProfile 1 -> N StaffAssignment (internal collection)
            entity.HasMany(s => s.Assignments)
                .WithOne()
                .HasForeignKey("StaffProfileId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(s => s.Assignments)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        // ==========================================
        // 3. StaffAssignment Mapping
        // ==========================================
        builder.Entity<StaffAssignment>(entity =>
        {
            entity.ToTable("staff_assignments");

            // Primary Key
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id)
                .HasConversion(id => id.Value, value => new AssignmentId(value))
                .IsRequired();

            // TargetId
            entity.Property(a => a.TargetId)
                .HasConversion(t => t.Value, value => new TargetId(value))
                .IsRequired();

            // ScopeLevel
            entity.Property(a => a.Scope)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // StaffRole
            entity.Property(a => a.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // AssignmentStatus
            entity.Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // DateRange (Owned Value Object)
            entity.OwnsOne(a => a.Period, period =>
            {
                period.Property(p => p.StartDate).HasColumnName("start_date").IsRequired();
                period.Property(p => p.EndDate).HasColumnName("end_date").IsRequired(false);
            });

            // Foreign Key shadow property to StaffProfile with conversion
            entity.Property<StaffProfileId>("StaffProfileId")
                .HasConversion(id => id.Value, value => new StaffProfileId(value))
                .IsRequired();

            // Indexes
            entity.HasIndex(a => a.TargetId, "ix_staff_assignments_target_id");
            entity.HasIndex(["StaffProfileId"], "ix_staff_assignments_staff_profile_id");
        });

        // ==========================================
        // 4. EmployeeCodeSequence Mapping
        // ==========================================
        builder.Entity<EmployeeCodeSequence>(entity =>
        {
            entity.ToTable("employee_code_sequence");

            entity.HasKey(s => s.Id);

            entity.Property(s => s.Id)
                .ValueGeneratedNever();

            entity.Property(s => s.LastValue)
                .IsRequired();

            entity.HasData(new EmployeeCodeSequence { Id = 1, LastValue = 0 });
        });
    }
}