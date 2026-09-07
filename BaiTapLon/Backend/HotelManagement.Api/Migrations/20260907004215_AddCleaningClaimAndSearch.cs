using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCleaningClaimAndSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CleaningClaimedAt",
                table: "rooms",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CleaningClaimedBy",
                table: "rooms",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_rooms_CleaningClaimedBy",
                table: "rooms",
                column: "CleaningClaimedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_rooms_employees_CleaningClaimedBy",
                table: "rooms",
                column: "CleaningClaimedBy",
                principalTable: "employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_rooms_employees_CleaningClaimedBy",
                table: "rooms");

            migrationBuilder.DropIndex(
                name: "IX_rooms_CleaningClaimedBy",
                table: "rooms");

            migrationBuilder.DropColumn(
                name: "CleaningClaimedAt",
                table: "rooms");

            migrationBuilder.DropColumn(
                name: "CleaningClaimedBy",
                table: "rooms");
        }
    }
}
