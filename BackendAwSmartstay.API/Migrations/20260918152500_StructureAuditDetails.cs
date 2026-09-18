using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAwSmartstay.API.Migrations
{
    /// <summary>
    ///     Audit entries keep structured facts (a JSON object) instead of English sentences: converts the rows written
    ///     before ("Reason: WrongPassword", "Role: a -> b", "Locked until ...", "Method: ...; Reason: ...",
    ///     "Remaining recovery codes: n"). The column type does not change, so the model snapshot stays the same.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260918152500_StructureAuditDetails")]
    public partial class StructureAuditDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE audit_entries SET details = JSON_OBJECT('method', SUBSTRING_INDEX(SUBSTRING(details, 9), '; ', 1), 'reason', SUBSTRING_INDEX(details, 'Reason: ', -1)) " +
                "WHERE details LIKE 'Method: %; Reason: %';");
            migrationBuilder.Sql(
                "UPDATE audit_entries SET details = JSON_OBJECT('method', SUBSTRING(details, 9)) WHERE details LIKE 'Method: %';");
            migrationBuilder.Sql(
                "UPDATE audit_entries SET details = JSON_OBJECT('reason', SUBSTRING(details, 9)) WHERE details LIKE 'Reason: %';");
            migrationBuilder.Sql(
                "UPDATE audit_entries SET details = JSON_OBJECT('previousRole', SUBSTRING_INDEX(SUBSTRING(details, 7), ' -> ', 1), 'newRole', SUBSTRING_INDEX(details, ' -> ', -1)) " +
                "WHERE details LIKE 'Role: % -> %';");
            migrationBuilder.Sql(
                "UPDATE audit_entries SET details = JSON_OBJECT('role', SUBSTRING(details, 7)) WHERE details LIKE 'Role: %';");
            migrationBuilder.Sql(
                "UPDATE audit_entries SET details = JSON_OBJECT('lockedUntil', SUBSTRING(details, 14)) WHERE details LIKE 'Locked until %';");
            migrationBuilder.Sql(
                "UPDATE audit_entries SET details = JSON_OBJECT('remainingRecoveryCodes', CAST(SUBSTRING(details, 27) AS UNSIGNED)) " +
                "WHERE details LIKE 'Remaining recovery codes: %';");
            // Anything else that is not JSON is dropped rather than shown as an untranslatable sentence.
            migrationBuilder.Sql(
                "UPDATE audit_entries SET details = NULL WHERE details IS NOT NULL AND details NOT LIKE '{%';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The structured facts are a superset of the old sentences; nothing to restore.
        }
    }
}
