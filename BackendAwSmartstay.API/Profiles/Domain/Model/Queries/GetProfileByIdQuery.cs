namespace BackendAwSmartstay.API.Profiles.Domain.Model.Queries;

/// <summary>
/// Query to retrieve a profile by its identifier.
/// </summary>
/// <param name="ProfileId">The identifier of the profile.</param>
public record GetProfileByIdQuery(int ProfileId);