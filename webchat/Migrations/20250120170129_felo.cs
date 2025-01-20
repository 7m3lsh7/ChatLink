using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webchat.Migrations
{
    /// <inheritdoc />
    public partial class felo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                table: "users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivity",
                table: "users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnline",
                table: "users");

            migrationBuilder.DropColumn(
                name: "LastActivity",
                table: "users");
        }
    }
}
