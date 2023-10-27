using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autoposter.DiscordBot.Migrations
{
    /// <inheritdoc />
    public partial class AddBotRoles1233 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "bot_settings",
                keyColumn: "id",
                keyValue: new Guid("ba3ef91e-ef92-43cf-baf4-486d4b3bc70e"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "bot_settings",
                columns: new[] { "id", "interval" },
                values: new object[] { new Guid("ba3ef91e-ef92-43cf-baf4-486d4b3bc70e"), 60 });
        }
    }
}
