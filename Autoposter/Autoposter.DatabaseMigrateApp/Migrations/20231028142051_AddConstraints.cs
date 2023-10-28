using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autoposter.DatabaseMigrateApp.Migrations
{
    /// <inheritdoc />
    public partial class AddConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_branches_roles_branches_branch_id",
                table: "branches_roles");

            migrationBuilder.AddForeignKey(
                name: "fk_branches_roles_branches_branch_id",
                table: "branches_roles",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_branches_roles_branches_branch_id",
                table: "branches_roles");

            migrationBuilder.AddForeignKey(
                name: "fk_branches_roles_branches_branch_id",
                table: "branches_roles",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id");
        }
    }
}
