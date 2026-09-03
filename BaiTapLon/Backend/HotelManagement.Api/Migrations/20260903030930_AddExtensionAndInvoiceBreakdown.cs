using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddExtensionAndInvoiceBreakdown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedExtraHours",
                table: "bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "OriginalCheckOutDate",
                table: "bookings",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "ApprovedExtraHours",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "OriginalCheckOutDate",
                table: "bookings");
        }
    }
}
