using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Queries;
using BackendAwSmartstay.API.Accommodations.Domain.Repositories;
using BackendAwSmartstay.API.Accommodations.Domain.Services;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;

namespace BackendAwSmartstay.API.Accommodations.Application.ACL;

public class AccommodationsContextFacade(
    IHotelQueryService hotelQueryService,
    IRoomQueryService roomQueryService,
    IRoomCommandService roomCommandService,
    IRoomRepository roomRepository,
    IHotelRepository hotelRepository) : IAccommodationsContextFacade
{
    public Task OccupyRoomForCheckInAsync(int roomId, int? guestUserId, string? guestEmail) =>
        roomCommandService.Handle(new OccupyRoomForCheckInCommand(roomId, guestUserId, guestEmail));

    public async Task<RoomOffer?> LockRoomForBookingAsync(int roomId)
    {
        if (roomId <= 0) return null;
        var room = await roomRepository.FindByIdForUpdateAsync(roomId);
        return room is null ? null : ToOffer(room, await hotelRepository.AcceptsBookingsAsync(room.HotelId));
    }

    public async Task<IReadOnlyList<RoomOffer>> FetchRoomsOfferedForBookingAsync(int? hotelId)
    {
        // Only rooms of hotels that accept bookings are offered (see the query).
        var rooms = await roomQueryService.Handle(new GetRoomsOfferedForBookingQuery(hotelId));
        return rooms.Select(room => ToOffer(room, hotelAcceptsBookings: true)).ToList();
    }

    private static RoomOffer ToOffer(Domain.Model.Aggregates.Room room, bool hotelAcceptsBookings) =>
        new(room.Id, room.HotelId, room.RoomTypeId, room.RoomType?.Name ?? string.Empty,
            room.Price, room.Description, room.Amenities, room.Status.ToString(), room.Number, hotelAcceptsBookings);

    public async Task<IReadOnlyDictionary<int, string>> FetchRoomNumbersAsync(IReadOnlyCollection<int> roomIds) =>
        roomIds.Count == 0 ? new Dictionary<int, string>() : await roomRepository.FindNumbersAsync(roomIds);

    public async Task<RoomOffer?> FetchRoomAsync(int roomId)
    {
        if (roomId <= 0) return null;
        var room = await roomQueryService.Handle(new GetRoomByIdQuery(roomId));
        return room is null ? null : ToOffer(room, room.Hotel?.AcceptsBookings ?? false);
    }

    public async Task<HotelSummary?> FetchHotelAsync(int hotelId)
    {
        if (hotelId <= 0) return null;
        var hotel = await hotelQueryService.Handle(new GetHotelByIdQuery(hotelId));
        return hotel is null ? null : new HotelSummary(hotel.Id, hotel.Name, $"{hotel.Address}, {hotel.City}, {hotel.Country}");
    }

    public async Task<HotelPaymentInstructions?> FetchPaymentInstructionsAsync(int hotelId)
    {
        if (hotelId <= 0) return null;
        var settings = await hotelRepository.FindPaymentSettingsAsync(hotelId);
        return settings is null
            ? null
            : new HotelPaymentInstructions(settings.AccountHolder, settings.YapeNumber, settings.PlinNumber,
                settings.OffersBankTransfer ? settings.BankName : null,
                settings.OffersBankTransfer ? settings.BankAccountNumber : null,
                settings.OffersBankTransfer ? settings.BankAccountCci : null);
    }

    public async Task<bool> HotelExistsAsync(int hotelId)
    {
        if (hotelId <= 0)
            return false;

        var query = new GetHotelByIdQuery(hotelId);
        var hotel = await hotelQueryService.Handle(query);
        return hotel != null;
    }

    public async Task<bool> RoomExistsAsync(int roomId)
    {
        return await FetchRoomPricePerNightAsync(roomId) is not null;
    }

    public async Task<decimal?> FetchRoomPricePerNightAsync(int roomId)
    {
        if (roomId <= 0)
            return null;

        var room = await roomQueryService.Handle(new GetRoomByIdQuery(roomId));
        return room?.Price;
    }

    public async Task<int?> FetchHotelIdOfRoomAsync(int roomId)
    {
        if (roomId <= 0)
            return null;

        var room = await roomQueryService.Handle(new GetRoomByIdQuery(roomId));
        return room?.HotelId;
    }
}
