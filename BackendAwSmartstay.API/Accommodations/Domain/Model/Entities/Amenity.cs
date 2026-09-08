namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Entities;

/// <summary>
/// Represents an amenity entity in the accommodations domain.
/// </summary>
public class Amenity
{
    /// <summary>
    /// The unique identifier of the amenity.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// The name of the amenity.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// The category of the amenity for grouping purposes.
    /// </summary>
    public string Category { get; set; } = "General"; // Opcional: para agruparlas
}