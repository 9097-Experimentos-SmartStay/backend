namespace BackendAwSmartstay.API.Profiles.Interfaces.ACL;

public interface IStaffProfilesContextFacade
{
    Task<Guid?> CreateStaffProfileAsync(int userId, string firstName, string lastName, string email, string position, string shift);
    Task<Guid?> FetchStaffProfileIdByUserIdAsync(int userId);
    Task<bool> HasActiveRoleInHotelAsync(int userId, int hotelId, string requiredRole);
    Task<bool> IsChainAdminAsync(int userId);
}
