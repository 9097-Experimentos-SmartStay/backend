using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAwSmartstay.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "rooms",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                // Every existing room starts Available (US-29).
                defaultValue: "Available")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "rooms",
                keyColumn: "id",
                keyValue: 101,
                column: "status",
                value: "Available");

            migrationBuilder.UpdateData(
                table: "rooms",
                keyColumn: "id",
                keyValue: 102,
                column: "status",
                value: "Available");

            migrationBuilder.UpdateData(
                table: "rooms",
                keyColumn: "id",
                keyValue: 201,
                column: "status",
                value: "Available");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "rooms");
        }
    }
}
