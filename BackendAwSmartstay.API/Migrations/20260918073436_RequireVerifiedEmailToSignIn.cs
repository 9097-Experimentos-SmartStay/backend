using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAwSmartstay.API.Migrations
{
    /// <inheritdoc />
    public partial class RequireVerifiedEmailToSignIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // US-01: sign-in now requires a verified e-mail. Accounts that were never sent a verification link
            // (created before e-mail verification existed, or seeded) are grandfathered as verified; accounts that
            // did receive a link must open it (or request a new one) before signing in.
            migrationBuilder.Sql("""
                UPDATE users u
                SET u.email_verified = 1, u.email_verified_at = UTC_TIMESTAMP(6)
                WHERE u.email_verified = 0
                  AND NOT EXISTS (SELECT 1 FROM account_tokens t
                                  WHERE t.user_id = u.id AND t.purpose = 'EmailVerification');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data-only migration: the grandfathered verifications are kept.
        }
    }
}
