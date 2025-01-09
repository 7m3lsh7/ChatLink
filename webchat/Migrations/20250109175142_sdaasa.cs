using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webchat.Migrations
{
    /// <inheritdoc />
    public partial class sdaasa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "users");

            migrationBuilder.DropColumn(
                name: "VerificationToken",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VerificationToken",
                table: "users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
