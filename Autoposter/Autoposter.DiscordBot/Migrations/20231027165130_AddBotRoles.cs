using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autoposter.DiscordBot.Migrations
{
    /// <inheritdoc />
    public partial class AddBotRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bot_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bot_roles", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "bot_settings",
                columns: new[] { "id", "interval" },
                values: new object[] { new Guid("ba3ef91e-ef92-43cf-baf4-486d4b3bc70e"), 60 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bot_roles");

            migrationBuilder.DeleteData(
                table: "bot_settings",
                keyColumn: "id",
                keyValue: new Guid("ba3ef91e-ef92-43cf-baf4-486d4b3bc70e"));
        }
    }
}
