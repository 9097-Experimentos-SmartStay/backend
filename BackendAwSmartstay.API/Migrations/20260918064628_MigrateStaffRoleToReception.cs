using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAwSmartstay.API.Migrations
{
    /// <summary>
    ///     D3: the generic <c>staff</c> role no longer exists (one user = one concrete role). Accounts that still hold
    ///     it become <c>reception</c>, the front-desk role closest to what <c>staff</c> could do. Their access tokens
    ///     keep working: the role is re-read from the user on every request.
    /// </summary>
    public partial class MigrateStaffRoleToReception : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE `users` SET `role` = 'reception' WHERE `role` = 'staff';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversible data migration: the former staff accounts cannot be told apart from real receptionists.
        }
    }
}
