using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventEaseLocal.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingDetailsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_BookingDetails;");
        }
    }
}
