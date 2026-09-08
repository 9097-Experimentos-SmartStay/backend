using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Entities;
using BackendAwSmartstay.API.Accommodations.Infrastructure.Persistence.EFC.Configuration.Extensions;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Infrastructure.Persistence.EFC.Configuration.Extensions;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Infrastructure.Persistence.EFC.Configuration.Extensions;
using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Infrastructure.Persistence.EFC.Configuration.Extensions;
using BackendAwSmartstay.API.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Profiles.Infrastructure.Persistence.EFC.Configuration.Extensions;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Represents the primary database context for the SmartStay application.
/// This class coordinates the functionality of the Entity Framework Core with the application's data models.
/// </summary>
/// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    #region IAM Context

    /// <summary>
    /// Gets or sets the set of User aggregates.
    /// </summary>
    public DbSet<User> Users { get; set; }

    #endregion

    #region Profiles Context

    /// <summary>
    /// Gets or sets the set of Profile aggregates.
    /// </summary>
    public DbSet<Profile> Profiles { get; set; }

    #endregion

    #region Accommodations Context

    /// <summary>
    /// Gets or sets the set of Hotel aggregates.
    /// </summary>
    public DbSet<Hotel> Hotels { get; set; }

    /// <summary>
    /// Gets or sets the set of Room aggregates.
    /// </summary>
    public DbSet<Room> Rooms { get; set; }

    /// <summary>
    /// Gets or sets the set of RoomType entities.
    /// </summary>
    public DbSet<RoomType> RoomTypes { get; set; }

    /// <summary>
    /// Gets or sets the set of HotelCategory entities (Master Data).
    /// </summary>
    public DbSet<HotelCategory> HotelCategories { get; set; }

    /// <summary>
    /// Gets or sets the set of Amenity entities (Master Data).
    /// </summary>
    public DbSet<Amenity> AmenitiesCatalog { get; set; }

    #endregion

    #region Bookings Context

    /// <summary>
    /// Gets or sets the set of Booking aggregates.
    /// </summary>
    public DbSet<Booking> Bookings { get; set; }

    #endregion

    #region Payments Context

    /// <summary>
    /// Gets or sets the set of Payment aggregates.
    /// </summary>
    public DbSet<Payment> Payments { get; set; }

    #endregion

    /// <summary>
    /// Configures the database context options.
    /// Injects interceptors for auditing (CreatedDate/UpdatedDate).
    /// </summary>
    /// <param name="builder">The builder used to configure the context options.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(builder);
    }

    /// <summary>
    /// Configures the model schema and relationships for the database.
    /// Applies configurations for each Bounded Context and sets naming conventions.
    /// </summary>
    /// <param name="builder">The builder used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply configuration for IAM Bounded Context
        builder.ApplyIamConfiguration();

        // Apply configuration for Profiles Bounded Context
        builder.ApplyProfilesConfiguration();

        // Apply configuration for Accommodations Bounded Context
        builder.ApplyAccommodationsConfiguration();

        // Apply configuration for Bookings Bounded Context
        builder.ApplyBookingsConfiguration();

        // Apply configuration for Payments Bounded Context
        builder.ApplyPaymentsConfiguration();

        // Apply snake_case naming convention for database compatibility (e.g., MySQL)
        builder.UseSnakeCaseNamingConvention();
    }
}