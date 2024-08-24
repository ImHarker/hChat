using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hChatAPI.Migrations
{
    /// <inheritdoc />
    public partial class _2FA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User2FA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Is2FAEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SecretKey = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User2FA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User2FA_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBackupCode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User2FAId = table.Column<int>(type: "int", nullable: false),
                    HashedCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBackupCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBackupCode_User2FA_User2FAId",
                        column: x => x.User2FAId,
                        principalTable: "User2FA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User2FA_UserId",
                table: "User2FA",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBackupCode_User2FAId",
                table: "UserBackupCode",
                column: "User2FAId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBackupCode");

            migrationBuilder.DropTable(
                name: "User2FA");
        }
    }
}
