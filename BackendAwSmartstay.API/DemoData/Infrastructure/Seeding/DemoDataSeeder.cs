using BackendAwSmartstay.API.Accommodations.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Commands;
using BackendAwSmartstay.API.Accommodations.Domain.Model.Entities;
using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Accommodations.Domain.Repositories;
using BackendAwSmartstay.API.Accommodations.Domain.Services;
using BackendAwSmartstay.API.Accommodations.Interfaces.ACL;
using BackendAwSmartstay.API.Bookings.Application.Internal.Configuration;
using BackendAwSmartstay.API.Bookings.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Bookings.Domain.Repositories;
using BackendAwSmartstay.API.Bookings.Domain.Services;
using BackendAwSmartstay.API.DemoData.Infrastructure.Configuration;
using BackendAwSmartstay.API.IAM.Application.Internal.CommandServices;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;
using BackendAwSmartstay.API.Payments.Application.OutboundServices;
using BackendAwSmartstay.API.Payments.Domain.Model.Aggregates;
using BackendAwSmartstay.API.Payments.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.Payments.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.Domain.Profiles.Domain.Repositories;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using IamPersonName = BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects.PersonName;
using DateRange = BackendAwSmartstay.API.Bookings.Domain.Model.ValueObjects.DateRange;
using ProfilePersonName = BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects.PersonName;

namespace BackendAwSmartstay.API.DemoData.Infrastructure.Seeding;

/// <summary>
///     Creates the demo dataset (<see cref="DemoDataset"/>) at startup, after the migrations, when
///     <c>DemoData__Enabled=true</c>.
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item><b>Through the domain.</b> Every row comes from an aggregate factory or behaviour (<c>User.Register</c>,
///         <c>HotelRegistrationPolicy</c>, <c>HotelPaymentSettings.Create</c>, <c>Room.ChangeStatus</c>,
///         <c>Booking.Place/Confirm/Cancel/ExpireIfUnpaid</c>, <c>Payment.Register/Complete/Refund</c>), availability is
///         checked by <c>RoomAvailabilityService</c> under the room row lock of the Accommodations ACL, and the password
///         goes through the password policy and the breached password check. No SQL.</item>
///         <item><b>History, not live commands.</b> The command services act "now"; the dataset needs a past (a stay
///         that started two days ago, a payment registered yesterday). The aggregates take the instant as a parameter,
///         so the seeder replays each fact at its own moment, relative to today in the hotels' time zone: the calendar
///         looks alive whenever it is seeded.</item>
///         <item><b>No side effects.</b> The unit of work publishes nothing (<see cref="DiscardingDomainEventDispatcher"/>):
///         no e-mails, alerts or audit entries for a past that never happened. The accounts are created with their
///         e-mail already verified (nobody can open the links); staff still enroll their authenticator at first sign-in.</item>
///         <item><b>Idempotent and atomic.</b> One transaction; nothing is done when any demo account or hotel already
///         exists, so a restart never duplicates the data (and a failure leaves nothing half-created).</item>
///     </list>
/// </remarks>
public class DemoDataSeeder(
    AppDbContext context,
    IUserRepository userRepository,
    IHashingService hashingService,
    NewPasswordValidator newPasswordValidator,
    IHotelRepository hotelRepository,
    IRoomTypeRepository roomTypeRepository,
    IRoomRepository roomRepository,
    IRoomStatusChangeRepository roomStatusChangeRepository,
    IAccommodationsContextFacade accommodationsContextFacade,
    IGuestProfileRepository guestProfileRepository,
    IBookingRepository bookingRepository,
    RoomAvailabilityService roomAvailabilityService,
    IPaymentRepository paymentRepository,
    IPaymentGateway paymentGateway,
    IOptions<DemoDataSettings> settings,
    IOptions<BookingPolicySettings> bookingSettings,
    TimeProvider timeProvider,
    ILogger<DemoDataSeeder> logger)
{
    private const string PasswordSetting = "DemoData:DefaultPassword";

    private readonly Dictionary<string, User> _users = [];
    private readonly Dictionary<int, (DemoAccount Account, Guid ProfileId)> _guests = [];
    private readonly List<string> _createdBookings = [];
    private DateTimeOffset _now;
    private DateTime _today;
    private TimeZoneInfo _zone = TimeZoneInfo.Utc;

    public async Task SeedAsync()
    {
        var options = settings.Value;
        if (!options.Enabled)
        {
            logger.LogInformation("DemoData: disabled (DemoData__Enabled is not true); no demo data is created.");
            return;
        }

        if (await FindExistingDemoDataAsync(options) is { } found)
        {
            logger.LogInformation("DemoData: the demo dataset is already present ({Found}); nothing to do.", found);
            return;
        }

        _zone = TimeZoneInfo.FindSystemTimeZoneById(bookingSettings.Value.TimeZone);
        _now = timeProvider.GetUtcNow();
        _today = TimeZoneInfo.ConvertTime(_now, _zone).Date;

        await EnsurePasswordIsAcceptableAsync(options);
        var passwordHashes = DemoDataset.Accounts.ToDictionary(account => account.Alias,
            _ => hashingService.HashPassword(options.DefaultPassword!));

        var discardedEvents = new DiscardingDomainEventDispatcher(logger);
        var unitOfWork = new UnitOfWork(context, discardedEvents);
        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var roomTypes = await EnsureRoomTypesAsync(unitOfWork);

            // Hotel 1: its administrator registers it (D2), sets the payment methods and the rooms; then the staff.
            var admin1 = await RegisterAccountAsync(DemoDataset.Admin1, passwordHashes, hotelId: null, createdBy: null, At(-60, 9, 0), unitOfWork);
            var paymentSettings = BuildHotel1PaymentSettings(options.Hotel1Payment);
            var hotel1 = await RegisterHotelAsync(DemoDataset.Hotel1, admin1, paymentSettings, At(-60, 9, 30), unitOfWork);
            var rooms1 = await CreateRoomsAsync(hotel1, DemoDataset.Hotel1, roomTypes, unitOfWork);
            var reception = await RegisterAccountAsync(DemoDataset.Reception1, passwordHashes, hotel1.Id, admin1.Id, At(-58, 10, 0), unitOfWork);
            var housekeeping = await RegisterAccountAsync(DemoDataset.Housekeeping1, passwordHashes, hotel1.Id, admin1.Id, At(-58, 10, 5), unitOfWork);
            var maintenance = await RegisterAccountAsync(DemoDataset.Maintenance1, passwordHashes, hotel1.Id, admin1.Id, At(-58, 10, 10), unitOfWork);

            // Hotel 2: only its administrator; no payment methods and no staff (both are done live: US-53, US-03).
            var admin2 = await RegisterAccountAsync(DemoDataset.Admin2, passwordHashes, hotelId: null, createdBy: null, At(-20, 11, 0), unitOfWork);
            var hotel2 = await RegisterHotelAsync(DemoDataset.Hotel2, admin2, paymentSettings: null, At(-20, 11, 20), unitOfWork);
            await CreateRoomsAsync(hotel2, DemoDataset.Hotel2, roomTypes, unitOfWork);

            // Guests, with their guest profiles.
            var guest1 = await RegisterGuestAsync(DemoDataset.Guest1, passwordHashes, At(-15, 20, 12), unitOfWork);
            var guest2 = await RegisterGuestAsync(DemoDataset.Guest2, passwordHashes, At(-9, 8, 47), unitOfWork);

            // US-06: map colours, status history and the overdue maintenance alert.
            await ChangeRoomStatusAsync(rooms1[DemoDataset.MaintenanceRoom], RoomStatus.Maintenance, maintenance, _now - DemoDataset.MaintenanceAge, unitOfWork);
            await ChangeRoomStatusAsync(rooms1[DemoDataset.CleaningRoom], RoomStatus.Cleaning, housekeeping, _now - TimeSpan.FromMinutes(40), unitOfWork);

            if (paymentSettings is null)
            {
                logger.LogWarning("DemoData: hotel '{Hotel}' has no payment methods (DemoData__Hotel1Payment__* not set), so it " +
                                  "does not accept bookings: the demo bookings and the occupied room were not created. Set the payment " +
                                  "methods and reseed on a fresh database, or configure them in the app.", hotel1.Name);
            }
            else
            {
                await SeedBookingsAsync(hotel1, rooms1, paymentSettings, reception, guest1, guest2, unitOfWork);
            }
        });

        logger.LogInformation("DemoData: created hotels '{Hotel1}' ({Rooms1} rooms, payment methods: {Payments}) and '{Hotel2}' " +
                              "({Rooms2} rooms, no payment methods).",
            DemoDataset.Hotel1.Name, DemoDataset.Hotel1.Rooms.Count, options.Hotel1Payment.IsProvided ? "yes" : "none",
            DemoDataset.Hotel2.Name, DemoDataset.Hotel2.Rooms.Count);
        logger.LogInformation("DemoData: created accounts (e-mail verified; staff enroll MFA at first sign-in): {Accounts}.",
            string.Join(", ", _users.Values.Select(user => $"{user.Email.Value} ({user.Role.Value})")));
        if (_createdBookings.Count > 0)
            logger.LogInformation("DemoData: created bookings of '{Hotel}': {Bookings}.", DemoDataset.Hotel1.Name, string.Join("; ", _createdBookings));
        logger.LogInformation("DemoData: {Count} domain events of the seeded history were not published (no e-mails were sent).",
            discardedEvents.DiscardedCount);
    }

    // ── Idempotency and validation ──────────────────────────────────────────

    /// <summary>The marker of a seeded database: any demo account or demo hotel. Null when there is none.</summary>
    private async Task<string?> FindExistingDemoDataAsync(DemoDataSettings options)
    {
        foreach (var account in DemoDataset.Accounts)
        {
            var email = new Email(EmailOf(account, options));
            if (await userRepository.ExistsByEmailAsync(email)) return $"account {email.Value}";
        }

        var hotelNames = new[] { DemoDataset.Hotel1.Name, DemoDataset.Hotel2.Name };
        var hotel = await context.Hotels.AsNoTracking().Where(h => hotelNames.Contains(h.Name)).Select(h => h.Name).FirstOrDefaultAsync();
        return hotel is null ? null : $"hotel '{hotel}'";
    }

    /// <summary>
    ///     The same password rules as any account (policy for each role and e-mail, then the breached password check
    ///     once). An unacceptable password stops the startup: the operator asked for the demo data.
    /// </summary>
    private async Task EnsurePasswordIsAcceptableAsync(DemoDataSettings options)
    {
        try
        {
            foreach (var account in DemoDataset.Accounts)
            {
                var check = PasswordPolicy.Check(options.DefaultPassword, new Role(account.Role), new Email(EmailOf(account, options)));
                if (!check.IsAcceptable)
                    throw new InvalidFieldException(PasswordSetting, check.Code!, check.Problem!, check.Parameters);
            }
            var guest = DemoDataset.Guest1;
            await newPasswordValidator.EnsureAcceptableAsync(options.DefaultPassword, new Role(guest.Role),
                new Email(EmailOf(guest, options)), PasswordSetting);
        }
        catch (InvalidFieldException exception)
        {
            throw new InvalidOperationException($"{PasswordSetting} cannot be used for the demo accounts: {exception.Message}", exception);
        }
    }

    // ── Accounts ────────────────────────────────────────────────────────────

    private async Task<User> RegisterAccountAsync(DemoAccount account, IReadOnlyDictionary<string, string> passwordHashes,
        int? hotelId, int? createdBy, DateTimeOffset at, UnitOfWork unitOfWork)
    {
        var user = User.Register(new IamPersonName(account.FirstName, account.LastName), new Email(EmailOf(account, settings.Value)),
            passwordHashes[account.Alias], new Role(account.Role), hotelId, chainId: null, createdBy, at);
        // Seeding cannot open the verification link: the operator owns these inboxes.
        user.VerifyEmail(at);
        await userRepository.AddAsync(user);
        await unitOfWork.CompleteAsync();
        _users[account.Alias] = user;
        return user;
    }

    private async Task<User> RegisterGuestAsync(DemoAccount account, IReadOnlyDictionary<string, string> passwordHashes,
        DateTimeOffset at, UnitOfWork unitOfWork)
    {
        var user = await RegisterAccountAsync(account, passwordHashes, hotelId: null, createdBy: null, at, unitOfWork);
        var profile = new GuestProfile(GuestProfileId.New(), new ProfilePersonName(account.FirstName, account.LastName),
            new PhoneNumber(account.Phone!), new EmailAddress(user.Email.Value), userId: new UserId(user.Id));
        await guestProfileRepository.AddAsync(profile);
        await unitOfWork.CompleteAsync();
        _guests[user.Id] = (account, profile.Id.Value);
        return user;
    }

    private static string EmailOf(DemoAccount account, DemoDataSettings options)
    {
        var separator = options.EmailBase.LastIndexOf('@');
        return $"{options.EmailBase[..separator]}+{account.Alias}{options.EmailBase[separator..]}".ToLowerInvariant();
    }

    // ── Hotels and rooms ────────────────────────────────────────────────────

    /// <summary>Room types are a global catalog: an existing type with the same name is reused, never duplicated.</summary>
    private async Task<IReadOnlyDictionary<string, int>> EnsureRoomTypesAsync(UnitOfWork unitOfWork)
    {
        var names = DemoDataset.RoomTypes.Select(type => type.Name).ToList();
        var existing = await context.RoomTypes.Where(type => names.Contains(type.Name)).ToListAsync();
        var types = existing.GroupBy(type => type.Name).ToDictionary(group => group.Key, group => group.First());
        foreach (var demoType in DemoDataset.RoomTypes.Where(type => !types.ContainsKey(type.Name)))
        {
            var roomType = new RoomType(new CreateRoomTypeCommand(demoType.Name, demoType.Description));
            await roomTypeRepository.AddAsync(roomType);
            types[demoType.Name] = roomType;
        }
        await unitOfWork.CompleteAsync();
        return types.ToDictionary(pair => pair.Key, pair => pair.Value.Id);
    }

    private HotelPaymentSettings? BuildHotel1PaymentSettings(DemoHotelPaymentSettings payment) =>
        payment.IsProvided
            ? HotelPaymentSettings.Create(payment.AccountHolder, payment.Yape, payment.Plin, payment.BankName,
                payment.BankAccountNumber, payment.BankAccountCci)
            : null;

    /// <summary>D2: a hotel administrator registers their single hotel, becomes its host and takes charge of it.</summary>
    private async Task<Hotel> RegisterHotelAsync(DemoHotel demoHotel, User admin, HotelPaymentSettings? paymentSettings,
        DateTimeOffset at, UnitOfWork unitOfWork)
    {
        var registrant = new HotelRegistrant(admin.Id, ManagesChain: false, AssignedHotelId: admin.HotelId);
        var hostId = HotelRegistrationPolicy.ResolveHost(registrant, requestedHostId: null,
            registrantAlreadyHostsAHotel: await hotelRepository.ExistsByHostIdAsync(admin.Id));

        var hotel = new Hotel(hostId, new CreateHotelCommand(registrant, null, demoHotel.Name, demoHotel.Address,
            demoHotel.City, demoHotel.Country, ImageUrl: string.Empty, demoHotel.Description, demoHotel.Type,
            demoHotel.Amenities.ToList(), new SessionContext(null)));
        if (paymentSettings is not null) hotel.ConfigurePaymentSettings(paymentSettings);
        await hotelRepository.AddAsync(hotel);
        await unitOfWork.CompleteAsync();

        admin.TakeChargeOfHotel(hotel.Id, at);
        await unitOfWork.CompleteAsync();
        return hotel;
    }

    private async Task<IReadOnlyDictionary<string, Room>> CreateRoomsAsync(Hotel hotel, DemoHotel demoHotel,
        IReadOnlyDictionary<string, int> roomTypes, UnitOfWork unitOfWork)
    {
        var rooms = new Dictionary<string, Room>();
        foreach (var demoRoom in demoHotel.Rooms)
        {
            var room = new Room(new CreateRoomCommand(hotel.Id, roomTypes[demoRoom.RoomType], demoRoom.Price,
                demoRoom.Description, demoRoom.Amenities.ToList(), demoRoom.Number));
            await roomRepository.AddAsync(room);
            rooms[room.Number] = room;
        }
        await unitOfWork.CompleteAsync();
        return rooms;
    }

    /// <summary>US-06: a staff member changes the status; the history line is kept with the change.</summary>
    private async Task ChangeRoomStatusAsync(Room room, RoomStatus status, User staff, DateTimeOffset at, UnitOfWork unitOfWork)
    {
        var change = room.ChangeStatus(status, RoomStatusChangeOrigin.Staff, staff.Id, staff.Email.Value, at);
        if (change is not null) await roomStatusChangeRepository.AddAsync(change);
        await unitOfWork.CompleteAsync();
    }

    // ── Bookings and payments of hotel 1 ────────────────────────────────────

    private async Task SeedBookingsAsync(Hotel hotel, IReadOnlyDictionary<string, Room> rooms, HotelPaymentSettings payments,
        User reception, User guest1, User guest2, UnitOfWork unitOfWork)
    {
        var desk = BookingRequester.HotelStaff(reception.Id, hotel.Id);

        // 1. Current stay (matches the Occupied room): a walk-in taken by reception two days ago, paid in cash.
        var stayPlacedAt = At(-2, 13, 50);
        var stay = await PlaceAsync(desk, guest1, rooms[DemoDataset.OccupiedRoom], _today.AddDays(-2), _today.AddDays(2), stayPlacedAt, unitOfWork);
        await PayAsync(stay, PaymentMethod.Cash, operationNumber: null, "Pago en efectivo al registrarse en recepción.", reception, At(-2, 13, 58), unitOfWork);
        await ChangeRoomStatusAsync(rooms[DemoDataset.OccupiedRoom], RoomStatus.Occupied, reception, At(-2, 14, 10), unitOfWork);
        Record(stay, rooms[DemoDataset.OccupiedRoom], "estadía en curso, pagada en efectivo");

        // 2. Confirmed: booked by the guest two days ago, paid (Yape) and registered by reception the same afternoon.
        var confirmed = await PlaceAsync(AsGuest(guest2), guest2, rooms["203"], _today.AddDays(3), _today.AddDays(5), At(-2, 10, 15), unitOfWork);
        var confirmedMethod = PreferredMethod(payments, PaymentMethod.Yape, PaymentMethod.Plin, PaymentMethod.BankTransfer);
        await PayAsync(confirmed, confirmedMethod, DemoDataset.YapeOperationNumber, "Pago verificado en la app del hotel.", reception, At(-2, 16, 40), unitOfWork);
        Record(confirmed, rooms["203"], $"confirmada, pago {confirmedMethod}");

        // 3. Cancelled by the guest after paying: the payment is refunded (outside the system).
        var cancelled = await PlaceAsync(AsGuest(guest1), guest1, rooms["102"], _today.AddDays(10), _today.AddDays(13), At(-6, 21, 5), unitOfWork);
        var cancelledMethod = PreferredMethod(payments, PaymentMethod.BankTransfer, PaymentMethod.Plin, PaymentMethod.Yape);
        var refunded = await PayAsync(cancelled, cancelledMethod, DemoDataset.CancelledBookingOperationNumber, "Pago verificado en el estado de cuenta.", reception, At(-5, 9, 30), unitOfWork);
        var cancelledAt = At(-1, 18, 20);
        cancelled.Cancel(AsGuest(guest1), LocalDate(cancelledAt), cancelledAt);
        // What RefundPaymentOnBookingCancelledHandler does after a real cancellation (R2); events are not published here.
        refunded.Refund(cancelledAt);
        await unitOfWork.CompleteAsync();
        Record(cancelled, rooms["102"], $"cancelada por el huésped tras pagar ({cancelledMethod}), pago reembolsado");

        // 4. Expired: never paid, cancelled by the payment deadline job (PaymentNotReceived).
        var expired = await PlaceAsync(AsGuest(guest2), guest2, rooms["104"], _today.AddDays(4), _today.AddDays(6), At(-3, 19, 40), unitOfWork);
        expired.ExpireIfUnpaid(expired.PaymentDueAt!.Value + TimeSpan.FromMinutes(12));
        await unitOfWork.CompleteAsync();
        Record(expired, rooms["104"], "vencida sin pago");

        // 5. Pending: booked two hours ago, waiting for its payment until the deadline.
        var pendingPlacedAt = _now - TimeSpan.FromHours(2);
        var pending = await PlaceAsync(AsGuest(guest2), guest2, rooms["201"], _today.AddDays(7), _today.AddDays(10), pendingPlacedAt, unitOfWork);
        Record(pending, rooms["201"], $"pendiente de pago hasta {pending.PaymentDueAt:yyyy-MM-dd HH:mm} UTC");
    }

    private BookingRequester AsGuest(User guest) =>
        BookingRequester.Guest(guest.Id, guest.Email.Value).WithGuestProfile(_guests[guest.Id].ProfileId);

    /// <summary>US-51 / US-07: the room is locked, its availability checked (R1) and the booking placed at <paramref name="at"/>.</summary>
    private async Task<Booking> PlaceAsync(BookingRequester requester, User guest, Room room, DateTime checkIn, DateTime checkOut,
        DateTimeOffset at, UnitOfWork unitOfWork)
    {
        var dates = new DateRange(checkIn, checkOut);
        var offer = await accommodationsContextFacade.LockRoomForBookingAsync(room.Id)
                    ?? throw new InvalidOperationException($"DemoData: room {room.Number} was not found.");
        await roomAvailabilityService.EnsureRoomIsAvailableAsync(offer.RoomId, dates);

        var (account, profileId) = _guests[guest.Id];
        var contact = new GuestContact($"{account.FirstName} {account.LastName}", guest.Email.Value, account.Phone);
        var booking = Booking.Place(requester, offer, dates, contact, guest.Id, profileId, LocalDate(at), at,
            bookingSettings.Value.PaymentHold);
        await bookingRepository.AddAsync(booking);
        await unitOfWork.CompleteAsync();
        return booking;
    }

    /// <summary>US-07 scenario 5: reception registers the payment through the gateway port and it confirms the booking (D1).</summary>
    private async Task<Payment> PayAsync(Booking booking, PaymentMethod method, string? operationNumber, string note,
        User recordedBy, DateTimeOffset at, UnitOfWork unitOfWork)
    {
        var payment = Payment.Register(booking.Id, booking.TotalPrice, method, operationNumber, note, recordedBy.Id, at);
        var result = await paymentGateway.ChargeAsync(new PaymentCharge(booking.Id, booking.Code.Value, payment.Amount,
            payment.Method, payment.OperationNumber));
        if (!result.Approved)
            throw new InvalidOperationException($"DemoData: the payment of booking {booking.Code} was rejected: {result.FailureReason}");
        payment.Complete(result.TransactionReference, at);
        booking.Confirm(at);
        await paymentRepository.AddAsync(payment);
        await unitOfWork.CompleteAsync();
        return payment;
    }

    /// <summary>The first of <paramref name="preferred"/> that the hotel offers (a guest can only pay with those).</summary>
    private static PaymentMethod PreferredMethod(HotelPaymentSettings payments, params PaymentMethod[] preferred) =>
        preferred.First(method => method switch
        {
            PaymentMethod.Yape => payments.YapeNumber is not null,
            PaymentMethod.Plin => payments.PlinNumber is not null,
            PaymentMethod.BankTransfer => payments.OffersBankTransfer,
            _ => true
        });

    private void Record(Booking booking, Room room, string description) =>
        _createdBookings.Add($"{booking.Code.Value} room {room.Number} {booking.CheckInDate:yyyy-MM-dd}..{booking.CheckOutDate:yyyy-MM-dd}: " +
                             $"{booking.Status} ({description})");

    // ── Hotel time ──────────────────────────────────────────────────────────

    /// <summary>The instant of <paramref name="hour"/>:<paramref name="minute"/>, hotel time, <paramref name="days"/> from today.</summary>
    private DateTimeOffset At(int days, int hour, int minute)
    {
        var local = DateTime.SpecifyKind(_today.AddDays(days).AddHours(hour).AddMinutes(minute), DateTimeKind.Unspecified);
        var instant = new DateTimeOffset(local, _zone.GetUtcOffset(local)).ToUniversalTime();
        // The seeded past never reaches the future (e.g. a "today 14:10" seeded at 08:00).
        return instant <= _now ? instant : _now;
    }

    private DateTime LocalDate(DateTimeOffset instant) => TimeZoneInfo.ConvertTime(instant, _zone).Date;
}
