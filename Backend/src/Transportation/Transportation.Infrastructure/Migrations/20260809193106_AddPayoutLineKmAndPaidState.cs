using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transportation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPayoutLineKmAndPaidState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DriverKm",
                schema: "transportation",
                table: "payout_lines",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ExtraBusinessKm",
                schema: "transportation",
                table: "payout_lines",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                schema: "transportation",
                table: "payout_lines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                schema: "transportation",
                table: "payout_lines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PaidById",
                schema: "transportation",
                table: "payout_lines",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_payout_lines_PaidById",
                schema: "transportation",
                table: "payout_lines",
                column: "PaidById");

            migrationBuilder.AddForeignKey(
                name: "FK_payout_lines_AspNetUsers_PaidById",
                schema: "transportation",
                table: "payout_lines",
                column: "PaidById",
                principalSchema: "transportation",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payout_lines_AspNetUsers_PaidById",
                schema: "transportation",
                table: "payout_lines");

            migrationBuilder.DropIndex(
                name: "IX_payout_lines_PaidById",
                schema: "transportation",
                table: "payout_lines");

            migrationBuilder.DropColumn(
                name: "DriverKm",
                schema: "transportation",
                table: "payout_lines");

            migrationBuilder.DropColumn(
                name: "ExtraBusinessKm",
                schema: "transportation",
                table: "payout_lines");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                schema: "transportation",
                table: "payout_lines");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                schema: "transportation",
                table: "payout_lines");

            migrationBuilder.DropColumn(
                name: "PaidById",
                schema: "transportation",
                table: "payout_lines");
        }
    }
}
