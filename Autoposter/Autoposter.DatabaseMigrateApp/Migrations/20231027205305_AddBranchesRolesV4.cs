using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autoposter.DatabaseMigrateApp.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchesRolesV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "branch_id1",
                table: "posts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_posts_branch_id1",
                table: "posts",
                column: "branch_id1");

            migrationBuilder.AddForeignKey(
                name: "fk_posts_branches_branch_id1",
                table: "posts",
                column: "branch_id1",
                principalTable: "branches",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_posts_branches_branch_id1",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "ix_posts_branch_id1",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "branch_id1",
                table: "posts");
        }
    }
}
