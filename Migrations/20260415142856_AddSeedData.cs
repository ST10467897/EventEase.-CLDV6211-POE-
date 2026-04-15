using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventEaseLocal.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Bookings",
                columns: new[] { "BookingId", "EndTime", "EventDate", "EventId", "StartTime", "VenueId" },
                values: new object[] { 2, new TimeSpan(0, 23, 0, 0, 0), new DateTime(2026, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, new TimeSpan(0, 14, 0, 0, 0), 2 });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "EventId", "Description", "EventName", "VenueId" },
                values: new object[,]
                {
                    { 4, "Kickoff event for the annual Sunshine Charity 10K.", "Charity Fun Run Launch", 3 },
                    { 8, "Live music performances across multiple stages.", "Music Festival Day Pass", 1 }
                });

            migrationBuilder.InsertData(
                table: "Venues",
                columns: new[] { "VenueId", "Capacity", "ImageUrl", "Location", "VenueName" },
                values: new object[,]
                {
                    { 4, 300, "https://images.unsplash.com/photo-1544928147-79a2dbc1f389?w=600", "12 Beach Road, Durban", "Ocean View Hall" },
                    { 5, 80, "https://images.unsplash.com/photo-1470770841497-7b3212e54211?w=600", "5 Peak Drive, Drakensberg", "Mountain Lodge" },
                    { 6, 250, "https://images.unsplash.com/photo-1505236858219-8359eb29e329?w=600", "90 Wine Route, Stellenbosch", "The Vineyard Estate" },
                    { 7, 1000, "https://images.unsplash.com/photo-1431540015159-0b624694d7da?w=600", "200 Commissioner Street, Johannesburg", "City Conference Centre" }
                });

            migrationBuilder.InsertData(
                table: "Bookings",
                columns: new[] { "BookingId", "EndTime", "EventDate", "EventId", "StartTime", "VenueId" },
                values: new object[,]
                {
                    { 4, new TimeSpan(0, 12, 0, 0, 0), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, new TimeSpan(0, 7, 0, 0, 0), 3 },
                    { 8, new TimeSpan(0, 22, 0, 0, 0), new DateTime(2026, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, new TimeSpan(0, 12, 0, 0, 0), 1 }
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "EventId", "Description", "EventName", "VenueId" },
                values: new object[,]
                {
                    { 3, "Formal dinner and awards ceremony for Zephyr Corp employees.", "Corporate Year-End Gala", 6 },
                    { 5, "Exclusive launch event for the new Nova smartphone line.", "Product Launch - Nova Phone", 7 },
                    { 6, "50th birthday celebration for the Naidoo family.", "Birthday Celebration - Naidoo", 4 },
                    { 7, "Two-day team building workshop for Apex Solutions.", "Team Building Retreat", 5 }
                });

            migrationBuilder.InsertData(
                table: "Bookings",
                columns: new[] { "BookingId", "EndTime", "EventDate", "EventId", "StartTime", "VenueId" },
                values: new object[,]
                {
                    { 3, new TimeSpan(0, 23, 30, 0, 0), new DateTime(2026, 12, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, new TimeSpan(0, 18, 0, 0, 0), 6 },
                    { 5, new TimeSpan(0, 15, 0, 0, 0), new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, new TimeSpan(0, 10, 0, 0, 0), 7 },
                    { 6, new TimeSpan(0, 22, 0, 0, 0), new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, new TimeSpan(0, 17, 0, 0, 0), 4 },
                    { 7, new TimeSpan(0, 16, 0, 0, 0), new DateTime(2026, 9, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, new TimeSpan(0, 8, 0, 0, 0), 5 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Bookings",
                keyColumn: "BookingId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Bookings",
                keyColumn: "BookingId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Bookings",
                keyColumn: "BookingId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Bookings",
                keyColumn: "BookingId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Bookings",
                keyColumn: "BookingId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Bookings",
                keyColumn: "BookingId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Bookings",
                keyColumn: "BookingId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Venues",
                keyColumn: "VenueId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Venues",
                keyColumn: "VenueId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Venues",
                keyColumn: "VenueId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Venues",
                keyColumn: "VenueId",
                keyValue: 7);
        }
    }
}
