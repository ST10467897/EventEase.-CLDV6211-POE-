using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventEaseLocal.Migrations
{
    /// <inheritdoc />
    public partial class AddBookedByField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookedBy",
                table: "Bookings",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Bookings",
                keyColumn: "BookingId",
                keyValue: 1,
                column: "BookedBy",
                value: "Thandi Mokoena");

            migrationBuilder.UpdateData(
                table: "Bookings",
                keyColumn: "BookingId",
                keyValue: 2,
                column: "BookedBy",
                value: "Sipho Dlamini");

            migrationBuilder.UpdateData(
                table: "Bookings",
                keyColumn: "BookingId",
                keyValue: 3,
                column: "BookedBy",
                value: "Zephyr Corp Events Team");

            migrationBuilder.UpdateData(
                table: "Bookings",
                keyColumn: "BookingId",
                keyValue: 4,
                column: "BookedBy",
                value: "Sunshine Charity");

            migrationBuilder.UpdateData(
                table: "Bookings",
                keyColumn: "BookingId",
                keyValue: 5,
                column: "BookedBy",
                value: "Nova Marketing");

            migrationBuilder.UpdateData(
                table: "Bookings",
                keyColumn: "BookingId",
                keyValue: 6,
                column: "BookedBy",
                value: "Priya Naidoo");

            migrationBuilder.UpdateData(
                table: "Bookings",
                keyColumn: "BookingId",
                keyValue: 7,
                column: "BookedBy",
                value: "Apex Solutions HR");

            migrationBuilder.UpdateData(
                table: "Bookings",
                keyColumn: "BookingId",
                keyValue: 8,
                column: "BookedBy",
                value: "Live Beats Productions");

            migrationBuilder.Sql(@"
                CREATE OR ALTER VIEW vw_BookingDetails AS
                SELECT
                    b.BookingId,
                    b.EventDate,
                    b.StartTime,
                    b.EndTime,
                    b.BookedBy,
                    e.EventId,
                    e.EventName,
                    e.Description AS EventDescription,
                    v.VenueId,
                    v.VenueName,
                    v.Location AS VenueLocation,
                    v.Capacity AS VenueCapacity,
                    v.ImageUrl AS VenueImageUrl
                FROM Bookings b
                INNER JOIN Events e ON b.EventId = e.EventId
                INNER JOIN Venues v ON b.VenueId = v.VenueId;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER VIEW vw_BookingDetails AS
                SELECT
                    b.BookingId,
                    b.EventDate,
                    b.StartTime,
                    b.EndTime,
                    e.EventId,
                    e.EventName,
                    e.Description AS EventDescription,
                    v.VenueId,
                    v.VenueName,
                    v.Location AS VenueLocation,
                    v.Capacity AS VenueCapacity,
                    v.ImageUrl AS VenueImageUrl
                FROM Bookings b
                INNER JOIN Events e ON b.EventId = e.EventId
                INNER JOIN Venues v ON b.VenueId = v.VenueId;
            ");

            migrationBuilder.DropColumn(
                name: "BookedBy",
                table: "Bookings");
        }
    }
}
