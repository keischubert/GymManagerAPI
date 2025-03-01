using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagerAPI.Migrations
{
    /// <inheritdoc />
    public partial class InsertRowsPlansTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "Id", "Name", "Price", "DurationInDays" },
                values: new object[,]
                {
                    { 1, "Plan Diario", 10000, 1 },
                    { 2, "Plan 2-Dias", 15000, 2 },
                    { 3, "Plan Semanal", 25000, 7 },
                    { 4, "Plan Mensual", 80000, 30 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Plans",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4});
        }
    }
}
