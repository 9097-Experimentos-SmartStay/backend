using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAwSmartstay.API.Migrations
{
    /// <inheritdoc />
    public partial class BookingLifecycleAndManualPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "card_holder_name",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "card_number_masked",
                table: "payments");

            migrationBuilder.AlterColumn<string>(
                name: "transaction_id",
                table: "payments",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "payment_method",
                table: "payments",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "failure_reason",
                table: "payments",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "payments",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "operation_number",
                table: "payments",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "recorded_by_user_id",
                table: "payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "refunded_at",
                table: "payments",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason",
                table: "bookings",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "cancelled_at",
                table: "bookings",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "checked_in_at",
                table: "bookings",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "bookings",
                type: "varchar(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "confirmed_at",
                table: "bookings",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "bookings",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "guest_phone",
                table: "bookings",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "hotel_id",
                table: "bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "payment_due_at",
                table: "bookings",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "price_per_night",
                table: "bookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // Existing rows: hotel and agreed price come from the booked room; a random code (hexadecimal) for each
            // booking; Pending bookings get the payment hold from now on (they expire in 24 h without a payment).
            migrationBuilder.Sql("""
                UPDATE bookings b JOIN rooms r ON r.id = b.room_id
                SET b.hotel_id = r.hotel_id, b.price_per_night = r.price;
                """);
            migrationBuilder.Sql("""
                UPDATE bookings
                SET code = CONCAT('SS-', UPPER(SUBSTRING(SHA2(CONCAT(id, '-', RAND(), '-', UUID()), 256), 1, 8))),
                    created_at = UTC_TIMESTAMP(6);
                """);
            migrationBuilder.Sql("""
                UPDATE bookings SET payment_due_at = DATE_ADD(UTC_TIMESTAMP(6), INTERVAL 24 HOUR) WHERE status = 0;
                """);
            migrationBuilder.Sql("""
                UPDATE bookings SET confirmed_at = UTC_TIMESTAMP(6) WHERE status = 1;
                """);
            // Card payments of the old simulated gateway: kept as payments made at the front desk.
            migrationBuilder.Sql("""
                UPDATE payments SET payment_method = 'CardAtFrontDesk'
                WHERE payment_method NOT IN ('Yape', 'Plin', 'BankTransfer', 'Cash', 'CardAtFrontDesk');
                """);

            migrationBuilder.CreateIndex(
                name: "i_x_payments__booking_id",
                table: "payments",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "i_x_bookings__code",
                table: "bookings",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_bookings__hotel_id__check_in_date",
                table: "bookings",
                columns: new[] { "hotel_id", "check_in_date" });

            migrationBuilder.CreateIndex(
                name: "i_x_bookings__status__payment_due_at",
                table: "bookings",
                columns: new[] { "status", "payment_due_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "i_x_payments__booking_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "i_x_bookings__code",
                table: "bookings");

            migrationBuilder.DropIndex(
                name: "i_x_bookings__hotel_id__check_in_date",
                table: "bookings");

            migrationBuilder.DropIndex(
                name: "i_x_bookings__status__payment_due_at",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "failure_reason",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "note",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "operation_number",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "recorded_by_user_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "refunded_at",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "cancellation_reason",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "checked_in_at",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "code",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "confirmed_at",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "guest_phone",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "hotel_id",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "payment_due_at",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "price_per_night",
                table: "bookings");

            migrationBuilder.AlterColumn<string>(
                name: "transaction_id",
                table: "payments",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "payment_method",
                table: "payments",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "card_holder_name",
                table: "payments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "card_number_masked",
                table: "payments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
