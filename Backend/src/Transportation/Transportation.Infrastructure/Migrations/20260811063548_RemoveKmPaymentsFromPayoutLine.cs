using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transportation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveKmPaymentsFromPayoutLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverPayment",
                schema: "transportation",
                table: "payout_lines");

            migrationBuilder.DropColumn(
                name: "ExtraKmPayment",
                schema: "transportation",
                table: "payout_lines");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DriverPayment",
                schema: "transportation",
                table: "payout_lines",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtraKmPayment",
                schema: "transportation",
                table: "payout_lines",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }
    }
}
