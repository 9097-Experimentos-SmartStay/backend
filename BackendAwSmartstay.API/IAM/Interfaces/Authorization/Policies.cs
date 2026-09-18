using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using Microsoft.AspNetCore.Authorization;

namespace BackendAwSmartstay.API.IAM.Interfaces.Authorization;

/// <summary>
///     Capability-named authorization policies and the ONLY place where capabilities are mapped to roles.
/// </summary>
/// <remarks>
///     Controllers declare <c>[Authorize(Policy = Policies.X)]</c>; they never mention role strings.
///     Changing who can do what means editing <see cref="RoleMatrix"/> below and nothing else.
///     Scope rules that depend on a concrete resource (an admin limited to their hotel, a guest limited to
///     their own profile) are resource-based authorization handlers in the owning bounded context, and
///     business rules (e.g. only the owner of a booking can cancel it) live in the domain model.
/// </remarks>
public static class Policies
{
    // ── Accommodations ───────────────────────────────────────────────
    /// <summary>Read hotels, rooms, room types and the accommodation catalog.</summary>
    public const string ReadInventory = nameof(ReadInventory);
    /// <summary>Create/update/delete hotels, rooms and room types (hotel scope checked per resource).</summary>
    public const string ManageHotels = nameof(ManageHotels);
    /// <summary>Change the operational status of a room (hotel scope checked per resource, US-29).</summary>
    public const string UpdateRoomStatus = nameof(UpdateRoomStatus);
    /// <summary>Add hotel categories and amenities to the shared master catalog.</summary>
    public const string ManageCatalog = nameof(ManageCatalog);

    // ── Bookings ─────────────────────────────────────────────────────
    /// <summary>List/read bookings (guests only see their own).</summary>
    public const string ReadBookings = nameof(ReadBookings);
    /// <summary>Read the bookings of a room (hotel operations).</summary>
    public const string ReadRoomBookings = nameof(ReadRoomBookings);
    /// <summary>Create a booking (guests for themselves, desk staff on behalf of a guest).</summary>
    public const string PlaceBookings = nameof(PlaceBookings);
    /// <summary>
    ///     Manage the bookings of the hotel: calendar, changes (US-07). Hotel scope enforced by the Booking aggregate.
    /// </summary>
    public const string ManageBookings = nameof(ManageBookings);
    /// <summary>Cancel a booking (guests only their own: enforced by the Booking aggregate).</summary>
    public const string CancelBookings = nameof(CancelBookings);

    // ── Payments ─────────────────────────────────────────────────────
    /// <summary>Read the payment of a booking (guests only of their own bookings).</summary>
    public const string ReadPayments = nameof(ReadPayments);
    /// <summary>Register the payment of a booking received by the hotel (US-07 scenario 5, D1).</summary>
    public const string RegisterPayments = nameof(RegisterPayments);

    // ── Analytics ────────────────────────────────────────────────────
    /// <summary>Read the performance dashboard (KPIs).</summary>
    public const string ViewAnalytics = nameof(ViewAnalytics);
    /// <summary>Use the analytics cache resilience lab (Redis/ActiveMQ).</summary>
    public const string OperateAnalyticsLab = nameof(OperateAnalyticsLab);

    // ── IAM / Profiles ───────────────────────────────────────────────
    /// <summary>Manage user accounts and roles (hierarchy and scope enforced by the IAM domain).</summary>
    public const string ManageUsers = nameof(ManageUsers);
    /// <summary>Read the access audit log (admins: their hotel's accounts; chain admins: all).</summary>
    public const string ViewAuditLog = nameof(ViewAuditLog);
    /// <summary>
    ///     US-52: enroll an authenticator app. Not a role capability: it only accepts the second-factor challenge
    ///     token of kind <c>enrollment</c> (registered in the IAM authorization setup).
    /// </summary>
    public const string EnrollSecondFactor = nameof(EnrollSecondFactor);
    /// <summary>US-52: present the second factor. Only accepts the challenge token of kind <c>verification</c>.</summary>
    public const string VerifySecondFactor = nameof(VerifySecondFactor);
    /// <summary>Manage staff profiles and their assignments.</summary>
    public const string ManageStaff = nameof(ManageStaff);
    /// <summary>Read, create and update guest profiles (guests only their own profile).</summary>
    public const string AccessGuestProfiles = nameof(AccessGuestProfiles);
    /// <summary>List and search guest profiles and register identification documents.</summary>
    public const string SearchGuestProfiles = nameof(SearchGuestProfiles);
    /// <summary>Link a guest profile to a user account (guests only to their own account).</summary>
    public const string LinkGuestProfiles = nameof(LinkGuestProfiles);
    /// <summary>Correct identification and activate/deactivate guest profiles.</summary>
    public const string AdministerGuestProfiles = nameof(AdministerGuestProfiles);

    // ── Marketing ────────────────────────────────────────────────────
    /// <summary>Read the demo requests of the landing (sales team).</summary>
    public const string ManageDemoRequests = nameof(ManageDemoRequests);

    // ── IoT emulator ─────────────────────────────────────────────────
    /// <summary>Read the emulated state of a room's devices (room scope checked per resource).</summary>
    public const string ReadRoomDevices = nameof(ReadRoomDevices);
    /// <summary>Send commands to a room's devices (thermostat; room scope checked per resource).</summary>
    public const string ControlRoomDevices = nameof(ControlRoomDevices);
    /// <summary>Inject simulated sensor telemetry (emulator tool; room scope checked per resource).</summary>
    public const string InjectTelemetry = nameof(InjectTelemetry);

    private static readonly string[] AllRoles =
    [
        UserRoles.Guest, UserRoles.Reception, UserRoles.Housekeeping,
        UserRoles.Maintenance, UserRoles.Admin, UserRoles.ChainAdmin
    ];

    private static readonly string[] HotelStaff =
    [
        UserRoles.Reception, UserRoles.Housekeeping, UserRoles.Maintenance,
        UserRoles.Admin, UserRoles.ChainAdmin
    ];

    private static readonly string[] Administrators = [UserRoles.Admin, UserRoles.ChainAdmin];

    private static readonly string[] FrontDesk = [UserRoles.Reception, UserRoles.Admin, UserRoles.ChainAdmin];

    private static readonly string[] GuestOrFrontDesk =
        [UserRoles.Guest, UserRoles.Reception, UserRoles.Admin, UserRoles.ChainAdmin];

    private static readonly string[] RoomDeviceOperators =
        [UserRoles.Guest, UserRoles.Maintenance, UserRoles.Admin, UserRoles.ChainAdmin];

    /// <summary>Capability → roles. Keep in sync with the role matrix of the API contract.</summary>
    public static readonly IReadOnlyDictionary<string, string[]> RoleMatrix = new Dictionary<string, string[]>
    {
        [ReadInventory] = AllRoles,
        [ManageHotels] = Administrators,
        [ManageCatalog] = [UserRoles.ChainAdmin],
        [UpdateRoomStatus] = HotelStaff,

        [ReadBookings] = AllRoles,
        [ReadRoomBookings] = HotelStaff,
        [PlaceBookings] = GuestOrFrontDesk,
        [ManageBookings] = FrontDesk,
        [CancelBookings] = GuestOrFrontDesk,

        [ReadPayments] = GuestOrFrontDesk,
        [RegisterPayments] = FrontDesk,

        [ViewAnalytics] = Administrators,
        [OperateAnalyticsLab] = [UserRoles.ChainAdmin],

        [ManageUsers] = Administrators,
        [ViewAuditLog] = Administrators,
        [ManageStaff] = Administrators,
        [AccessGuestProfiles] = GuestOrFrontDesk,
        [SearchGuestProfiles] = FrontDesk,
        [LinkGuestProfiles] = [UserRoles.Guest, UserRoles.Admin, UserRoles.ChainAdmin],
        [AdministerGuestProfiles] = Administrators,

        [ManageDemoRequests] = [UserRoles.ChainAdmin],

        // R5: guests (their current stay), maintenance and admins (their hotel), chain admins (all rooms).
        [ReadRoomDevices] = RoomDeviceOperators,
        [ControlRoomDevices] = RoomDeviceOperators,
        [InjectTelemetry] = Administrators,
    };

    /// <summary>
    ///     Registers every capability policy. Each one requires an authenticated user holding one of the roles.
    /// </summary>
    public static AuthorizationBuilder AddSmartStayPolicies(this AuthorizationBuilder builder)
    {
        foreach (var (policy, roles) in RoleMatrix)
            builder.AddPolicy(policy, p => p.RequireAuthenticatedUser().RequireRole(roles));

        return builder;
    }
}
