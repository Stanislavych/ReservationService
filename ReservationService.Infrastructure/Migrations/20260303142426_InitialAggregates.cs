using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialAggregates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Tables_TableId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_UserId",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Reservations",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "TableId",
                table: "Reservations",
                newName: "table_id");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_UserId",
                table: "Reservations",
                newName: "IX_Reservations_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_TableId_Reservation_start_time",
                table: "Reservations",
                newName: "IX_Reservations_table_id_Reservation_start_time");

            migrationBuilder.AddCheckConstraint(
                name: "FK_Reservations_Table",
                table: "Reservations",
                sql: "table_id > 0");

            migrationBuilder.AddCheckConstraint(
                name: "FK_Reservations_User",
                table: "Reservations",
                sql: "user_id > 0");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Reservations_table_id",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Reservations_user_id",
                table: "Reservations");

            migrationBuilder.DropCheckConstraint(
                name: "FK_Reservations_Table",
                table: "Reservations");

            migrationBuilder.DropCheckConstraint(
                name: "FK_Reservations_User",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Reservations",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "table_id",
                table: "Reservations",
                newName: "TableId");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_user_id",
                table: "Reservations",
                newName: "IX_Reservations_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_table_id_Reservation_start_time",
                table: "Reservations",
                newName: "IX_Reservations_TableId_Reservation_start_time");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Tables_TableId",
                table: "Reservations",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_UserId",
                table: "Reservations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
