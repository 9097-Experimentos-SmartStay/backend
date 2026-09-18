namespace BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;

/// <summary>
///     The administrator who registers a hotel, as seen by the Accommodations context.
/// </summary>
/// <param name="UserId">IAM user id of the registrant.</param>
/// <param name="ManagesChain">True for a chain administrator (manages several hotels).</param>
/// <param name="AssignedHotelId">The hotel the registrant already administers, if any.</param>
public sealed record HotelRegistrant(int UserId, bool ManagesChain, int? AssignedHotelId);
