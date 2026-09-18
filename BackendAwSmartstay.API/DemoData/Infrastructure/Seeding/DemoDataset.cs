using BackendAwSmartstay.API.IAM.Domain.Model.Constants;

namespace BackendAwSmartstay.API.DemoData.Infrastructure.Seeding;

/// <summary>A seeded account: <c>{EmailBase local}+{Alias}@{domain}</c>.</summary>
public sealed record DemoAccount(string Alias, string FirstName, string LastName, string Role, string? Phone = null);

/// <summary>A room type of the demo dataset (room types are a global catalog: reused when the name exists).</summary>
public sealed record DemoRoomType(string Name, string Description);

/// <summary>A room of a demo hotel.</summary>
public sealed record DemoRoom(string Number, string RoomType, decimal Price, string Description, IReadOnlyList<string> Amenities);

/// <summary>A demo hotel with its rooms.</summary>
public sealed record DemoHotel(string Name, string Address, string City, string Country, string Description, string Type,
    IReadOnlyList<string> Amenities, IReadOnlyList<DemoRoom> Rooms);

/// <summary>
///     The demo dataset: two fictional lodgings of San Martín, Peru (where the team's interviews took place), their
///     staff and two guests. Names, addresses and descriptions are invented; prices are soles per night, in the range
///     of boutique lodgings of Tarapoto. Hotel images are left empty (no third-party pictures).
/// </summary>
public static class DemoDataset
{
    public const string Simple = "Simple";
    public const string Doble = "Doble";
    public const string Matrimonial = "Matrimonial";
    public const string Suite = "Suite";
    public const string BungalowFamiliar = "Bungalow familiar";

    public static readonly IReadOnlyList<DemoRoomType> RoomTypes =
    [
        new(Simple, "Habitación para una persona con cama de plaza y media, escritorio y baño privado con agua caliente."),
        new(Doble, "Habitación con dos camas de plaza y media, ideal para amigos o familia; baño privado con agua caliente."),
        new(Matrimonial, "Habitación para pareja con cama queen y baño privado con agua caliente."),
        new(Suite, "Habitación amplia con cama king, sala de estar, minibar y balcón o terraza."),
        new(BungalowFamiliar, "Bungalow independiente de madera con cama queen, dos camas de plaza y media y terraza con hamaca. Hasta cuatro personas.")
    ];

    // ── Hotel 1: boutique hotel in Tarapoto, accepts bookings (payment methods from DemoData__Hotel1Payment__*) ──

    public static readonly DemoHotel Hotel1 = new(
        Name: "Casa Ungurahui Hotel Boutique",
        Address: "Jr. Ramón Castilla 427",
        City: "Tarapoto",
        Country: "Perú",
        Description: "Hotel boutique de ocho habitaciones a cuatro cuadras de la Plaza de Armas de Tarapoto. Jardín interior con " +
                     "plantas amazónicas, piscina pequeña y desayuno regional con tacacho, cecina y café de San Martín.",
        Type: "Hotel",
        Amenities: ["Wifi", "Desayuno", "Piscina", "Bar"],
        Rooms:
        [
            new("101", Simple, 150m, "Primer piso, con vista al jardín interior. Ventilador de techo y escritorio.", ["Wifi", "TV", "Ventilador", "Agua caliente"]),
            new("102", Doble, 210m, "Primer piso, dos camas de plaza y media y aire acondicionado.", ["Wifi", "TV", "Aire acondicionado", "Agua caliente"]),
            new("103", Matrimonial, 230m, "Primer piso, cama queen, aire acondicionado y salida directa al jardín.", ["Wifi", "TV", "Aire acondicionado", "Agua caliente"]),
            new("104", Matrimonial, 230m, "Primer piso, cama queen, aire acondicionado y ventana con doble vidrio.", ["Wifi", "TV", "Aire acondicionado", "Agua caliente"]),
            new("201", Doble, 220m, "Segundo piso, dos camas de plaza y media y balcón con vista a la Cordillera Escalera.", ["Wifi", "TV", "Aire acondicionado", "Balcón"]),
            new("202", Simple, 160m, "Segundo piso, cama de plaza y media y aire acondicionado.", ["Wifi", "TV", "Aire acondicionado", "Agua caliente"]),
            new("203", Suite, 380m, "Segundo piso, cama king, sala de estar, minibar y balcón con hamaca.", ["Wifi", "TV", "Aire acondicionado", "Minibar", "Balcón"]),
            new("204", Suite, 380m, "Segundo piso, cama king, tina y terraza privada.", ["Wifi", "TV", "Aire acondicionado", "Minibar", "Tina"])
        ]);

    public static readonly DemoAccount Admin1 = new("admin1", "Carmen", "Reátegui Pinedo", UserRoles.Admin);
    public static readonly DemoAccount Reception1 = new("recepcion1", "Luis", "Sangama Tuanama", UserRoles.Reception);
    public static readonly DemoAccount Housekeeping1 = new("limpieza1", "Rosa", "Amasifuen Isuiza", UserRoles.Housekeeping);
    public static readonly DemoAccount Maintenance1 = new("mantenimiento1", "Jorge", "Tapullima Chujutalli", UserRoles.Maintenance);

    // ── Hotel 2: ecolodge near Lamas, no payment methods (the admin sets them live, US-53) ──

    public static readonly DemoHotel Hotel2 = new(
        Name: "Wayra Sacha Ecolodge",
        Address: "Carretera a Lamas km 18, caserío Pamashto",
        City: "Lamas",
        Country: "Perú",
        Description: "Ecolodge de bungalows de madera en medio del bosque, a 30 minutos de Tarapoto y a 10 de Lamas. Punto de " +
                     "partida para observar aves, visitar el barrio Wayku y las cataratas de la zona. Desayuno con productos de las chacras vecinas.",
        Type: "Lodge",
        Amenities: ["Desayuno", "Restaurante", "Wifi", "Parking"],
        Rooms:
        [
            new("B1", BungalowFamiliar, 420m, "Bungalow junto a la quebrada, con terraza y hamaca.", ["Mosquitero", "Terraza", "Agua caliente"]),
            new("B2", BungalowFamiliar, 420m, "Bungalow con vista al bosque, con terraza y hamaca.", ["Mosquitero", "Terraza", "Agua caliente"]),
            new("M1", Matrimonial, 260m, "Cama queen, mosquitero y ventanal hacia el bosque.", ["Mosquitero", "Ventilador", "Agua caliente"]),
            new("M2", Matrimonial, 260m, "Cama queen, mosquitero y terraza al jardín de heliconias.", ["Mosquitero", "Ventilador", "Terraza"]),
            new("D1", Doble, 240m, "Dos camas de plaza y media, mosquiteros y ventilador de techo.", ["Mosquitero", "Ventilador", "Agua caliente"])
        ]);

    public static readonly DemoAccount Admin2 = new("admin2", "Mariela", "Del Águila Ríos", UserRoles.Admin);

    // ── Guests (with guest profiles) ──

    public static readonly DemoAccount Guest1 = new("huesped1", "Lucía", "Panduro Saavedra", UserRoles.Guest, "+51942617358");
    public static readonly DemoAccount Guest2 = new("huesped2", "Andrés", "Vásquez Arévalo", UserRoles.Guest, "+51987204516");

    public static IReadOnlyList<DemoAccount> Accounts =>
        [Admin1, Reception1, Housekeeping1, Maintenance1, Admin2, Guest1, Guest2];

    /// <summary>Rooms of hotel 1 whose status changes (US-06: map colours, history, overdue alert).</summary>
    public const string OccupiedRoom = "103";
    public const string CleaningRoom = "202";
    public const string MaintenanceRoom = "204";

    /// <summary>Hours the maintenance room has been out of service: more than the 24 h of the overdue alert.</summary>
    public static readonly TimeSpan MaintenanceAge = TimeSpan.FromHours(30);

    /// <summary>Operation numbers of the seeded payments: plain operation ids, as the wallet or the bank prints them.</summary>
    public const string YapeOperationNumber = "07214938";
    public const string CancelledBookingOperationNumber = "30581476";
}
