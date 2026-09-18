using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAwSmartstay.API.Migrations
{
    /// <inheritdoc />
    public partial class RoomStatusHistoryAndMaintenanceAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "maintenance_alert_sent_at",
                table: "rooms",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "status_changed_at",
                table: "rooms",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            // Existing rooms: their current status counts from the deployment (the maintenance alert starts then).
            migrationBuilder.Sql("UPDATE rooms SET status_changed_at = UTC_TIMESTAMP(6) WHERE status_changed_at < '2000-01-01';");

            migrationBuilder.CreateTable(
                name: "room_status_changes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    room_id = table.Column<int>(type: "int", nullable: false),
                    hotel_id = table.Column<int>(type: "int", nullable: false),
                    from_status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    to_status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    origin = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    changed_by_user_id = table.Column<int>(type: "int", nullable: true),
                    changed_by_email = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    changed_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_room_status_changes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_room_status_changes_rooms__room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "rooms",
                keyColumn: "id",
                keyValue: 101,
                columns: new[] { "maintenance_alert_sent_at", "status_changed_at" },
                values: new object[] { null, new DateTimeOffset(new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "rooms",
                keyColumn: "id",
                keyValue: 102,
                columns: new[] { "maintenance_alert_sent_at", "status_changed_at" },
                values: new object[] { null, new DateTimeOffset(new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "rooms",
                keyColumn: "id",
                keyValue: 201,
                columns: new[] { "maintenance_alert_sent_at", "status_changed_at" },
                values: new object[] { null, new DateTimeOffset(new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "i_x_room_status_changes__room_id__changed_at",
                table: "room_status_changes",
                columns: new[] { "room_id", "changed_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "room_status_changes");

            migrationBuilder.DropColumn(
                name: "maintenance_alert_sent_at",
                table: "rooms");

            migrationBuilder.DropColumn(
                name: "status_changed_at",
                table: "rooms");
        }
    }
}
