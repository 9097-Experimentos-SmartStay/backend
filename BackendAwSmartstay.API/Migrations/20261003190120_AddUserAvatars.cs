using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAwSmartstay.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAvatars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_avatars",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    content_type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content = table.Column<byte[]>(type: "longblob", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_user_avatars", x => x.user_id);
                    table.ForeignKey(
                        name: "f_k_user_avatars_users__user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_avatars");
        }
    }
}
