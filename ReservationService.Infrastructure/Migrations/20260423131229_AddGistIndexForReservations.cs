using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGistIndexForReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_table_id_Reservation_start_time",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Reservation_start_time",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_table_id",
                table: "Reservations",
                column: "table_id");

            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            migrationBuilder.Sql(@"
            CREATE INDEX IX_Reservations_Gist_NoOverlap 
            ON public.""Reservations"" 
            USING gist (table_id, tstzrange(start_time, end_time))
            WHERE status != 'Cancelled';
        ");

            migrationBuilder.Sql(@"
            ALTER TABLE public.""Reservations"" 
            ADD CONSTRAINT CK_Reservations_NoOverlap 
            EXCLUDE USING gist (
                table_id WITH =,
                tstzrange(start_time, end_time) WITH &&
            ) WHERE (status != 'Cancelled');
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_table_id",
                table: "Reservations");

            migrationBuilder.AddColumn<DateTime>(
                name: "Reservation_start_time",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_table_id_Reservation_start_time",
                table: "Reservations",
                columns: new[] { "table_id", "Reservation_start_time" },
                unique: true);

            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Reservations_Gist_NoOverlap;");
            migrationBuilder.Sql(@"ALTER TABLE public.""Reservations"" DROP CONSTRAINT IF EXISTS CK_Reservations_NoOverlap;");
        }
    }
}
