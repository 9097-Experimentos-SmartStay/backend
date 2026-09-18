namespace BackendAwSmartstay.API.IAM.Interfaces.ACL;

public interface IIamContextFacade
{
    Task<int> FetchUserIdByEmail(string email);
    Task<string> FetchEmailByUserId(int userId);

    /// <summary>
    ///     D2: makes <paramref name="hotelId"/> the hotel administered by the hotel administrator
    ///     <paramref name="userId"/> (called when that administrator registers their hotel).
    /// </summary>
    Task AssignHotelToAdministratorAsync(int userId, int hotelId);
}