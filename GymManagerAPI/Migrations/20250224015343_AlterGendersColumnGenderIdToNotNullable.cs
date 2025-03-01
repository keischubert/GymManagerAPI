using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagerAPI.Migrations
{
    /// <inheritdoc />
    public partial class AlterGendersColumnGenderIdToNotNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Genders_GenderId",
                table: "Members");

            migrationBuilder.AlterColumn<int>(
                name: "GenderId",
                table: "Members",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Genders_GenderId",
                table: "Members",
                column: "GenderId",
                principalTable: "Genders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Genders_GenderId",
                table: "Members");

            migrationBuilder.AlterColumn<int>(
                name: "GenderId",
                table: "Members",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Genders_GenderId",
                table: "Members",
                column: "GenderId",
                principalTable: "Genders",
                principalColumn: "Id");
        }
    }
}
