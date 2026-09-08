using BackendAwSmartstay.API.Profiles.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Profiles.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Extension methods for configuring Profile entity mappings in Entity Framework Core.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Configures the Profile entity, including primary key and owned value objects (Name, Email, Address).
    /// </summary>
    /// <param name="builder">The model builder instance.</param>
    public static void ApplyProfilesConfiguration(this ModelBuilder builder)
    {
        // Configure Profile primary key
        builder.Entity<Profile>().HasKey(p => p.Id);
        builder.Entity<Profile>().Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();
        
        // Configure Name value object
        builder.Entity<Profile>().OwnsOne(p => p.Name,
            n =>
            {
                n.WithOwner().HasForeignKey("Id");
                n.Property(p => p.FirstName).HasColumnName("FirstName");
                n.Property(p => p.LastName).HasColumnName("LastName");
            });

        // Configure Email value object
        builder.Entity<Profile>().OwnsOne(p => p.Email,
            e =>
            {
                e.WithOwner().HasForeignKey("Id");
                e.Property(a => a.Address).HasColumnName("EmailAddress");
            });

        // Configure Address value object
        builder.Entity<Profile>().OwnsOne(p => p.Address,
            a =>
            {
                a.WithOwner().HasForeignKey("Id");
                a.Property(s => s.Street).HasColumnName("AddressStreet");
                a.Property(s => s.Number).HasColumnName("AddressNumber");
                a.Property(s => s.City).HasColumnName("AddressCity");
                a.Property(s => s.PostalCode).HasColumnName("AddressPostalCode");
                a.Property(s => s.Country).HasColumnName("AddressCountry");
            });
    }
}