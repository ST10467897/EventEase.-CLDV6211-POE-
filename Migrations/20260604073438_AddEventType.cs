using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventEaseLocal.Migrations
{
    /// <inheritdoc />
    public partial class AddEventType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventTypeId",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    EventTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.EventTypeId);
                });

            migrationBuilder.InsertData(
                table: "EventTypes",
                columns: new[] { "EventTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "Conference" },
                    { 2, "Wedding" },
                    { 3, "Corporate" },
                    { 4, "Concert / Festival" },
                    { 5, "Birthday / Private" },
                    { 6, "Charity" },
                    { 7, "Workshop" },
                    { 8, "Other" }
                });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 1,
                column: "EventTypeId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 2,
                column: "EventTypeId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 3,
                column: "EventTypeId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 4,
                column: "EventTypeId",
                value: 6);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 5,
                column: "EventTypeId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 6,
                column: "EventTypeId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 7,
                column: "EventTypeId",
                value: 7);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 8,
                column: "EventTypeId",
                value: 4);

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventTypeId",
                table: "Events",
                column: "EventTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_EventTypes_EventTypeId",
                table: "Events",
                column: "EventTypeId",
                principalTable: "EventTypes",
                principalColumn: "EventTypeId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_EventTypes_EventTypeId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropIndex(
                name: "IX_Events_EventTypeId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventTypeId",
                table: "Events");
        }
    }
}
