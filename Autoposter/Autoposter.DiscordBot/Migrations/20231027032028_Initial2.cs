using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autoposter.DiscordBot.Migrations
{
    /// <inheritdoc />
    public partial class Initial2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_posts_users_user_id",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "ix_posts_user_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "posts");

            migrationBuilder.AddColumn<string>(
                name: "branch_id",
                table: "posts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image_uri",
                table: "posts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "server_id",
                table: "posts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "image_uri",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "server_id",
                table: "posts");

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "posts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_posts_user_id",
                table: "posts",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_posts_users_user_id",
                table: "posts",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id");
        }
    }
}
