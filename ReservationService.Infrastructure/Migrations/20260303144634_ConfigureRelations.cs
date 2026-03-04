using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Reservations_table_id",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Reservations_user_id",
                table: "Reservations");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Tables_table_id",
                table: "Reservations",
                column: "table_id",
                principalTable: "Tables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_user_id",
                table: "Reservations",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Tables_table_id",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_user_id",
                table: "Reservations");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Reservations_table_id",
                table: "Reservations",
                column: "table_id",
                principalTable: "Reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Reservations_user_id",
                table: "Reservations",
                column: "user_id",
                principalTable: "Reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
