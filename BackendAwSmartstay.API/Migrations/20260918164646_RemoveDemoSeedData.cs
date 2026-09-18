using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAwSmartstay.API.Migrations
{
    /// <summary>
    ///     Migrations describe the schema (and the reference catalogs the application needs), never business data. This
    ///     migration removes the demo rows that earlier migrations seeded (hotels 1 and 2 with their placeholder payment
    ///     methods, rooms 101, 102 and 201, room types 1 to 3), but only where they are still untouched demo data:
    ///     <list type="bullet">
    ///         <item>a room is deleted only if it still has its seed values, was never booked and has no status
    ///         history;</item>
    ///         <item>a hotel only if it still has its seed values (payment methods included), has no rooms left, no
    ///         bookings and no staff account assigned to it;</item>
    ///         <item>a room type only if it still has its seed values and no room uses it.</item>
    ///     </list>
    ///     Anything that was used or edited is kept, so no real booking, payment or account can lose its references.
    ///     Demo data is now created at startup by <c>DemoDataSeeder</c> when <c>DemoData__Enabled=true</c>.
    /// </summary>
    public partial class RemoveDemoSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bookings reference rooms and hotels by id across bounded contexts (no foreign keys): check them explicitly.
            migrationBuilder.Sql("""
                DELETE FROM rooms
                WHERE ((id = 101 AND hotel_id = 1 AND room_type_id = 1 AND number = '101' AND price = 85.00 AND description = 'Room 101 - Standard view.')
                    OR (id = 102 AND hotel_id = 1 AND room_type_id = 2 AND number = '102' AND price = 150.00 AND description = 'Room 102 - Plaza view with balcony.')
                    OR (id = 201 AND hotel_id = 2 AND room_type_id = 3 AND number = '201' AND price = 320.00 AND description = 'Suite 201 - Panoramic mountain view.'))
                  AND status = 'Available'
                  AND NOT EXISTS (SELECT 1 FROM bookings b WHERE b.room_id = rooms.id)
                  AND NOT EXISTS (SELECT 1 FROM room_status_changes c WHERE c.room_id = rooms.id);
                """);

            migrationBuilder.Sql("""
                DELETE FROM hotels
                WHERE ((id = 1 AND name = 'Grand Hotel Bolivar' AND address = 'Jr. de la Unión 958' AND city = 'Lima'
                        AND image_url = 'https://placehold.co/600x400/3498DB/FFFFFF?text=Bolivar'
                        AND (payment_account_holder IS NULL OR (payment_account_holder = 'Grand Hotel Bolivar S.A.C. (demo)'
                             AND payment_yape_number = '999000111' AND payment_plin_number IS NULL AND payment_bank_name = 'BCP'
                             AND payment_bank_account_number = '191-0000000-0-00' AND payment_bank_account_cci = '00219100000000000000')))
                    OR (id = 2 AND name = 'Cusco Andean Lodge' AND address = 'San Blas 123' AND city = 'Cusco'
                        AND image_url = 'https://placehold.co/600x400/E67E22/FFFFFF?text=Andean'
                        AND (payment_account_holder IS NULL OR (payment_account_holder = 'Cusco Andean Lodge E.I.R.L. (demo)'
                             AND payment_yape_number IS NULL AND payment_plin_number = '999000222' AND payment_bank_name IS NULL
                             AND payment_bank_account_number IS NULL AND payment_bank_account_cci IS NULL))))
                  AND host_id = 1
                  AND NOT EXISTS (SELECT 1 FROM rooms r WHERE r.hotel_id = hotels.id)
                  AND NOT EXISTS (SELECT 1 FROM bookings b WHERE b.hotel_id = hotels.id)
                  AND NOT EXISTS (SELECT 1 FROM users u WHERE u.hotel_id = hotels.id AND u.role <> 'chain_admin');
                """);

            migrationBuilder.Sql("""
                DELETE FROM room_types
                WHERE ((id = 1 AND name = 'Single Standard' AND description = 'Cozy room for solo travelers.')
                    OR (id = 2 AND name = 'Double Deluxe' AND description = 'Spacious room for couples or business.')
                    OR (id = 3 AND name = 'Presidential Suite' AND description = 'Luxury suite with best views.'))
                  AND NOT EXISTS (SELECT 1 FROM rooms r WHERE r.room_type_id = room_types.id);
                """);

            // The initial chain administrator was assigned hotel 1 by default. A chain administrator operates every
            // hotel, so the reference to a demo hotel that no longer exists is simply cleared.
            migrationBuilder.Sql("""
                UPDATE users SET hotel_id = NULL
                WHERE role = 'chain_admin' AND hotel_id IN (1, 2)
                  AND NOT EXISTS (SELECT 1 FROM hotels h WHERE h.id = users.hotel_id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restores the old demo rows that are missing (rows kept by Up are left as they are).
            migrationBuilder.Sql("""
                INSERT IGNORE INTO hotels (id, address, amenities, city, country, description, host_id, image_url, name, type,
                    payment_account_holder, payment_bank_account_cci, payment_bank_account_number, payment_bank_name, payment_plin_number, payment_yape_number)
                VALUES
                    (1, 'Jr. de la Unión 958', '["Wifi","Restaurante","Bar"]', 'Lima', 'Peru', 'Historic hotel in the center of Lima.', 1,
                     'https://placehold.co/600x400/3498DB/FFFFFF?text=Bolivar', 'Grand Hotel Bolivar', 'Hotel',
                     'Grand Hotel Bolivar S.A.C. (demo)', '00219100000000000000', '191-0000000-0-00', 'BCP', NULL, '999000111'),
                    (2, 'San Blas 123', '["Desayuno","Wifi","Gimnasio"]', 'Cusco', 'Peru', 'Experience the mystic energy of the Andes.', 1,
                     'https://placehold.co/600x400/E67E22/FFFFFF?text=Andean', 'Cusco Andean Lodge', 'Lodge',
                     'Cusco Andean Lodge E.I.R.L. (demo)', NULL, NULL, NULL, '999000222', NULL);
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO room_types (id, description, name)
                VALUES (1, 'Cozy room for solo travelers.', 'Single Standard'),
                       (2, 'Spacious room for couples or business.', 'Double Deluxe'),
                       (3, 'Luxury suite with best views.', 'Presidential Suite');
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO rooms (id, amenities, description, hotel_id, maintenance_alert_sent_at, number, price, room_type_id, status, status_changed_at)
                VALUES (101, '["Wifi","TV"]', 'Room 101 - Standard view.', 1, NULL, '101', 85.00, 1, 'Available', '2026-09-01 00:00:00'),
                       (102, '["Wifi","TV","Minibar"]', 'Room 102 - Plaza view with balcony.', 1, NULL, '102', 150.00, 2, 'Available', '2026-09-01 00:00:00'),
                       (201, '["Jacuzzi","Wifi","Desayuno","Chimenea"]', 'Suite 201 - Panoramic mountain view.', 2, NULL, '201', 320.00, 3, 'Available', '2026-09-01 00:00:00');
                """);
        }
    }
}
