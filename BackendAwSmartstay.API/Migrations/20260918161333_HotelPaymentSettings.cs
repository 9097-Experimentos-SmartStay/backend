using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAwSmartstay.API.Migrations
{
    /// <inheritdoc />
    public partial class HotelPaymentSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payment_account_holder",
                table: "hotels",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "payment_bank_account_cci",
                table: "hotels",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "payment_bank_account_number",
                table: "hotels",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "payment_bank_name",
                table: "hotels",
                type: "varchar(60)",
                maxLength: 60,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "payment_plin_number",
                table: "hotels",
                type: "varchar(9)",
                maxLength: 9,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "payment_yape_number",
                table: "hotels",
                type: "varchar(9)",
                maxLength: 9,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "hotels",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "payment_account_holder", "payment_bank_account_cci", "payment_bank_account_number", "payment_bank_name", "payment_plin_number", "payment_yape_number" },
                values: new object[] { "Grand Hotel Bolivar S.A.C. (demo)", "00219100000000000000", "191-0000000-0-00", "BCP", null, "999000111" });

            migrationBuilder.UpdateData(
                table: "hotels",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "payment_account_holder", "payment_bank_account_cci", "payment_bank_account_number", "payment_bank_name", "payment_plin_number", "payment_yape_number" },
                values: new object[] { "Cusco Andean Lodge E.I.R.L. (demo)", null, null, null, "999000222", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_account_holder",
                table: "hotels");

            migrationBuilder.DropColumn(
                name: "payment_bank_account_cci",
                table: "hotels");

            migrationBuilder.DropColumn(
                name: "payment_bank_account_number",
                table: "hotels");

            migrationBuilder.DropColumn(
                name: "payment_bank_name",
                table: "hotels");

            migrationBuilder.DropColumn(
                name: "payment_plin_number",
                table: "hotels");

            migrationBuilder.DropColumn(
                name: "payment_yape_number",
                table: "hotels");
        }
    }
}
