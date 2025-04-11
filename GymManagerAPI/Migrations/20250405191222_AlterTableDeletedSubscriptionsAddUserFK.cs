using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagerAPI.Migrations
{
    /// <inheritdoc />
    public partial class AlterTableDeletedSubscriptionsAddUserFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "DeletedSubscriptions");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "DeletedSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DeletedSubscriptions_UserId",
                table: "DeletedSubscriptions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeletedSubscriptions_Users_UserId",
                table: "DeletedSubscriptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeletedSubscriptions_Users_UserId",
                table: "DeletedSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_DeletedSubscriptions_UserId",
                table: "DeletedSubscriptions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DeletedSubscriptions");

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "DeletedSubscriptions",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
