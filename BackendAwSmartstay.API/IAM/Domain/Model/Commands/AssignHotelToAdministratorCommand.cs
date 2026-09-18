namespace BackendAwSmartstay.API.IAM.Domain.Model.Commands;

/// <summary>
///     Makes <paramref name="HotelId"/> the hotel administered by the hotel administrator <paramref name="UserId"/>
///     (D2: an administrator registers and manages a single hotel). The administrator asked for it themselves (they
///     registered the hotel), so their old sessions end and a new one is issued with the hotel.
/// </summary>
/// <param name="UserId">The administrator.</param>
/// <param name="HotelId">The hotel they registered.</param>
/// <param name="RememberedSessionId">
///     The remembered session ("remember me") of the request that registered the hotel, if any: the new session is
///     remembered too.
/// </param>
public record AssignHotelToAdministratorCommand(int UserId, int HotelId, Guid? RememberedSessionId);
