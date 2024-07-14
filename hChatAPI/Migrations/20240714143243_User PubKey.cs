using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hChatAPI.Migrations
{
    /// <inheritdoc />
    public partial class UserPubKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PubKey",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PubKey",
                table: "Users");
        }
    }
}
