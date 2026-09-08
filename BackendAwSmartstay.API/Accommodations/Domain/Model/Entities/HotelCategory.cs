namespace BackendAwSmartstay.API.Accommodations.Domain.Model.Entities;

/// <summary>
/// Represents a hotel category entity in the accommodations domain.
/// </summary>
public class HotelCategory
{
    /// <summary>
    /// The unique identifier of the hotel category.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// The name of the hotel category.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}