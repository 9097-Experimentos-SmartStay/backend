using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAwSmartstay.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "number",
                table: "rooms",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "rooms",
                keyColumn: "id",
                keyValue: 101,
                column: "number",
                value: "101");

            migrationBuilder.UpdateData(
                table: "rooms",
                keyColumn: "id",
                keyValue: 102,
                column: "number",
                value: "102");

            migrationBuilder.UpdateData(
                table: "rooms",
                keyColumn: "id",
                keyValue: 201,
                column: "number",
                value: "201");

            // Existing rooms are numbered with their id (unique per hotel) until the hotel renames them.
            migrationBuilder.Sql("UPDATE rooms SET number = CAST(id AS CHAR) WHERE number = '';");

            migrationBuilder.CreateIndex(
                name: "i_x_rooms__hotel_id__number",
                table: "rooms",
                columns: new[] { "hotel_id", "number" },
                unique: true);

            // Dropped after the composite index exists: MySQL needs an index on hotel_id for the foreign key.
            migrationBuilder.DropIndex(
                name: "i_x_rooms__hotel_id",
                table: "rooms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "i_x_rooms__hotel_id",
                table: "rooms",
                column: "hotel_id");

            migrationBuilder.DropIndex(
                name: "i_x_rooms__hotel_id__number",
                table: "rooms");

            migrationBuilder.DropColumn(
                name: "number",
                table: "rooms");
        }
    }
}
