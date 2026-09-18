namespace BackendAwSmartstay.API.IAM.Interfaces.ACL;

/// <summary>How to reach a user, exposed to other bounded contexts.</summary>
/// <param name="UserId">IAM user id.</param>
/// <param name="Email">Account e-mail.</param>
/// <param name="FullName">First and last name, or null for accounts created before names were required.</param>
/// <param name="Role">Role of the account.</param>
public sealed record UserContact(int UserId, string Email, string? FullName, string Role);

public interface IIamContextFacade
{
    /// <summary>The contact of an active user, or null when the user does not exist or is inactive.</summary>
    Task<UserContact?> FetchUserContactAsync(int userId);

    /// <summary>
    ///     Active users of <paramref name="hotelId"/> with one of <paramref name="roles"/> (e.g. the housekeeping staff
    ///     of a hotel to notify). Chain administrators are only included when <paramref name="roles"/> asks for them.
    /// </summary>
    Task<IReadOnlyList<UserContact>> ListHotelStaffAsync(int hotelId, IReadOnlyCollection<string> roles);

    Task<int> FetchUserIdByEmail(string email);
    Task<string> FetchEmailByUserId(int userId);

    /// <summary>
    ///     D2: makes <paramref name="hotelId"/> the hotel administered by the hotel administrator
    ///     <paramref name="userId"/> (called when that administrator registers their hotel).
    /// </summary>
    Task AssignHotelToAdministratorAsync(int userId, int hotelId);
}