using BackendAwSmartstay.API.Marketing.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Marketing.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Marketing.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyMarketingConfiguration(this ModelBuilder builder)
    {
        var entity = builder.Entity<DemoRequest>();
        entity.ToTable("demo_requests");
        entity.HasKey(r => r.Id);
        entity.Property(r => r.Id).ValueGeneratedOnAdd();
        entity.Property(r => r.FirstName).HasMaxLength(ContactDetails.NameMaxLength).IsRequired();
        entity.Property(r => r.LastName).HasMaxLength(ContactDetails.NameMaxLength).IsRequired();
        entity.Property(r => r.Email).HasMaxLength(ContactDetails.EmailMaxLength).IsRequired();
        entity.Property(r => r.Phone).HasMaxLength(16);
        entity.Property(r => r.HotelName).HasMaxLength(DemoRequest.HotelNameMaxLength).IsRequired();
        entity.Property(r => r.JobTitle).HasMaxLength(DemoRequest.JobTitleMaxLength).IsRequired();
        entity.Property(r => r.Message).HasMaxLength(DemoRequest.MessageMaxLength);
        entity.Property(r => r.AccommodationType).HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(r => r.RoomsRange).HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(r => r.ReferralSource).HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(r => r.Profile).HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Ignore(r => r.Contact);
        entity.HasIndex(r => new { r.Status, r.ReceivedAt });
        entity.HasIndex(r => r.Email);
    }
}
