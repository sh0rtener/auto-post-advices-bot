using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autoposter.DatabaseMigrateApp.Migrations
{
    /// <inheritdoc />
    public partial class AddGuilds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "guild_id",
                table: "tags",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "guild_id",
                table: "servers",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "guild_id",
                table: "posts",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "guild_id",
                table: "branches_roles",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "guild_id",
                table: "branches",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "guild_id",
                table: "bot_settings",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "guild_id",
                table: "bot_roles",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "guild_id",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "guild_id",
                table: "servers");

            migrationBuilder.DropColumn(
                name: "guild_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "guild_id",
                table: "branches_roles");

            migrationBuilder.DropColumn(
                name: "guild_id",
                table: "branches");

            migrationBuilder.DropColumn(
                name: "guild_id",
                table: "bot_settings");

            migrationBuilder.DropColumn(
                name: "guild_id",
                table: "bot_roles");
        }
    }
}
