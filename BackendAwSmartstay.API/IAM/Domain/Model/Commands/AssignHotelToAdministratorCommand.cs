namespace BackendAwSmartstay.API.IAM.Domain.Model.Commands;

/// <summary>
///     Makes <paramref name="HotelId"/> the hotel administered by the hotel administrator <paramref name="UserId"/>
///     (D2: an administrator registers and manages a single hotel).
/// </summary>
public record AssignHotelToAdministratorCommand(int UserId, int HotelId);
