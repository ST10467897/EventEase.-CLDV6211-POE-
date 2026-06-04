using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventEaseLocal.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedImageUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Venues",
                keyColumn: "VenueId",
                keyValue: 5,
                column: "ImageUrl",
                value: "https://images.unsplash.com/photo-1587061949409-02df41d5e562?w=600");

            migrationBuilder.UpdateData(
                table: "Venues",
                keyColumn: "VenueId",
                keyValue: 7,
                column: "ImageUrl",
                value: "https://images.unsplash.com/photo-1497366216548-37526070297c?w=600");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Venues",
                keyColumn: "VenueId",
                keyValue: 5,
                column: "ImageUrl",
                value: "https://images.unsplash.com/photo-1470770841497-7b3212e54211?w=600");

            migrationBuilder.UpdateData(
                table: "Venues",
                keyColumn: "VenueId",
                keyValue: 7,
                column: "ImageUrl",
                value: "https://images.unsplash.com/photo-1431540015159-0b624694d7da?w=600");
        }
    }
}
