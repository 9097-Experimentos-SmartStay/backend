using System.ComponentModel.DataAnnotations.Schema;
using EntityFrameworkCore.CreatedUpdatedDate.Contracts;

namespace BackendAwSmartstay.API.Profiles.Domain.Model.Aggregates;

/// <summary>
/// Partial class that extends the Profile entity with temporal auditing capabilities.
/// Implements automatic tracking of creation and update timestamps.
/// </summary>
/// <remarks>
/// This partial class is used to separate audit properties from the main domain logic,
/// maintaining clean code organization.
/// Timestamps are managed automatically through Entity Framework Core interceptors.
/// </remarks>
public partial class Profile : IEntityWithCreatedUpdatedDate
{
    /// <summary>
    /// Gets or sets the date and time when the profile record was created.
    /// </summary>
    /// <value>
    /// Date and time with timezone (UTC) of the record creation.
    /// Can be null if the record has not yet been persisted to the database.
    /// </value>
    /// <remarks>
    /// This property maps to the "CreatedAt" column in the database.
    /// The value is automatically set when creating the record.
    /// </remarks>
    [Column("CreatedAt")] 
    public DateTimeOffset? CreatedDate { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time of the last update to the profile record.
    /// </summary>
    /// <value>
    /// Date and time with timezone (UTC) of the last record modification.
    /// Can be null if the record has never been updated after its creation.
    /// </value>
    /// <remarks>
    /// This property maps to the "UpdatedAt" column in the database.
    /// The value is automatically updated each time the record is modified.
    /// </remarks>
    [Column("UpdatedAt")] 
    public DateTimeOffset? UpdatedDate { get; set; }
}