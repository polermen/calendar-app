using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CalendarApp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarSharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalendarShares",
                columns: table => new
                {
                    CalendarShareId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    SpectatorEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SpectatorUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarShares", x => x.CalendarShareId);
                    table.ForeignKey(
                        name: "FK_CalendarShares_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalendarShares_Users_SpectatorUserId",
                        column: x => x.SpectatorUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarShares_OwnerId",
                table: "CalendarShares",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarShares_SpectatorEmail",
                table: "CalendarShares",
                column: "SpectatorEmail");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarShares_SpectatorUserId",
                table: "CalendarShares",
                column: "SpectatorUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarShares");
        }
    }
}
