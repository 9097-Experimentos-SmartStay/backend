namespace BackendAwSmartstay.API.Profiles.Interfaces.ACL;

public interface IGuestProfilesContextFacade
{
    Task<Guid?> CreateGuestProfileAsync(string firstName, string lastName, string phone, string? email = null, int? userId = null);
    Task<Guid?> FetchGuestProfileIdByUserIdAsync(int userId);
    Task<Guid?> FetchGuestProfileIdByEmailAsync(string email);
    Task<bool> LinkGuestProfileToUserAsync(Guid guestProfileId, int userId, string email);
}
