using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class adduniqueindex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_table_id_Reservation_start_time",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_table_id_Reservation_start_time",
                table: "Reservations",
                columns: new[] { "table_id", "Reservation_start_time" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_table_id_Reservation_start_time",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_table_id_Reservation_start_time",
                table: "Reservations",
                columns: new[] { "table_id", "Reservation_start_time" });
        }
    }
}
