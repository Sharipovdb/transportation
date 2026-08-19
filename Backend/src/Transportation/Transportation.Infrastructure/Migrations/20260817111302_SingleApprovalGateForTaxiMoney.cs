using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transportation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SingleApprovalGateForTaxiMoney : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_transport_days_CrewId",
                schema: "transportation",
                table: "transport_days");

            migrationBuilder.DropColumn(
                name: "Confirmed",
                schema: "transportation",
                table: "transport_days");

            migrationBuilder.CreateIndex(
                name: "IX_transport_days_CrewId_Date",
                schema: "transportation",
                table: "transport_days",
                columns: new[] { "CrewId", "Date" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_transport_days_CrewId_Date",
                schema: "transportation",
                table: "transport_days");

            migrationBuilder.AddColumn<bool>(
                name: "Confirmed",
                schema: "transportation",
                table: "transport_days",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_transport_days_CrewId",
                schema: "transportation",
                table: "transport_days",
                column: "CrewId");
        }
    }
}
