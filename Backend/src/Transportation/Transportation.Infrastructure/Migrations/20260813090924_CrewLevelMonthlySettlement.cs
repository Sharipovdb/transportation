using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Transportation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CrewLevelMonthlySettlement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A sheet used to be a bundle of per-member payout lines; it is now one
            // settlement with the crew's lead, and RecipientId has no value that could be
            // derived from the old rows. Sheets are recomputed from the transport days
            // that produced them, so the existing ones are cleared and regenerated rather
            // than back-filled with a guess at who should have been paid.
            migrationBuilder.Sql(@"DELETE FROM transportation.""MonthlyTransportSheets"";");

            migrationBuilder.DropForeignKey(
                name: "FK_MonthlyTransportSheets_AspNetUsers_CreatedById",
                schema: "transportation",
                table: "MonthlyTransportSheets");

            migrationBuilder.DropForeignKey(
                name: "FK_MonthlyTransportSheets_Crews_CrewId",
                schema: "transportation",
                table: "MonthlyTransportSheets");

            migrationBuilder.DropTable(
                name: "PayoutLineTaxiExpenses",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "payout_lines",
                schema: "transportation");

            migrationBuilder.DropIndex(
                name: "IX_vehicles_Plate",
                schema: "transportation",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "IX_TaxiExpense_TransportDayId_Leg_PaidById",
                schema: "transportation",
                table: "TaxiExpense");

            migrationBuilder.DropIndex(
                name: "IX_Crews_Name",
                schema: "transportation",
                table: "Crews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MonthlyTransportSheets",
                schema: "transportation",
                table: "MonthlyTransportSheets");

            migrationBuilder.DropIndex(
                name: "IX_MonthlyTransportSheets_CrewId_Year_Month",
                schema: "transportation",
                table: "MonthlyTransportSheets");

            migrationBuilder.RenameTable(
                name: "MonthlyTransportSheets",
                schema: "transportation",
                newName: "monthly_transport_sheets",
                newSchema: "transportation");

            migrationBuilder.RenameIndex(
                name: "IX_MonthlyTransportSheets_CreatedById",
                schema: "transportation",
                table: "monthly_transport_sheets",
                newName: "IX_monthly_transport_sheets_CreatedById");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                schema: "transportation",
                table: "monthly_transport_sheets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                schema: "transportation",
                table: "monthly_transport_sheets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PaidById",
                schema: "transportation",
                table: "monthly_transport_sheets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RecipientId",
                schema: "transportation",
                table: "monthly_transport_sheets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_monthly_transport_sheets",
                schema: "transportation",
                table: "monthly_transport_sheets",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "monthly_transport_sheet_days",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MonthlyTransportSheetId = table.Column<long>(type: "bigint", nullable: false),
                    TransportDayId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    DrivenKm = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: false),
                    ExtraBusinessKm = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: false),
                    TaxiAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monthly_transport_sheet_days", x => x.Id);
                    table.ForeignKey(
                        name: "FK_monthly_transport_sheet_days_monthly_transport_sheets_Month~",
                        column: x => x.MonthlyTransportSheetId,
                        principalSchema: "transportation",
                        principalTable: "monthly_transport_sheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_monthly_transport_sheet_days_transport_days_TransportDayId",
                        column: x => x.TransportDayId,
                        principalSchema: "transportation",
                        principalTable: "transport_days",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_Plate",
                schema: "transportation",
                table: "vehicles",
                column: "Plate",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TaxiExpense_TransportDayId_Leg_PaidById",
                schema: "transportation",
                table: "TaxiExpense",
                columns: new[] { "TransportDayId", "Leg", "PaidById" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Crews_Name",
                schema: "transportation",
                table: "Crews",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_transport_sheets_CrewId_Year_Month",
                schema: "transportation",
                table: "monthly_transport_sheets",
                columns: new[] { "CrewId", "Year", "Month" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_transport_sheets_PaidById",
                schema: "transportation",
                table: "monthly_transport_sheets",
                column: "PaidById");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_transport_sheets_RecipientId",
                schema: "transportation",
                table: "monthly_transport_sheets",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_transport_sheet_days_MonthlyTransportSheetId_Transp~",
                schema: "transportation",
                table: "monthly_transport_sheet_days",
                columns: new[] { "MonthlyTransportSheetId", "TransportDayId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_monthly_transport_sheet_days_TransportDayId",
                schema: "transportation",
                table: "monthly_transport_sheet_days",
                column: "TransportDayId");

            migrationBuilder.AddForeignKey(
                name: "FK_monthly_transport_sheets_AspNetUsers_CreatedById",
                schema: "transportation",
                table: "monthly_transport_sheets",
                column: "CreatedById",
                principalSchema: "transportation",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_monthly_transport_sheets_AspNetUsers_PaidById",
                schema: "transportation",
                table: "monthly_transport_sheets",
                column: "PaidById",
                principalSchema: "transportation",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_monthly_transport_sheets_AspNetUsers_RecipientId",
                schema: "transportation",
                table: "monthly_transport_sheets",
                column: "RecipientId",
                principalSchema: "transportation",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_monthly_transport_sheets_Crews_CrewId",
                schema: "transportation",
                table: "monthly_transport_sheets",
                column: "CrewId",
                principalSchema: "transportation",
                principalTable: "Crews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_monthly_transport_sheets_AspNetUsers_CreatedById",
                schema: "transportation",
                table: "monthly_transport_sheets");

            migrationBuilder.DropForeignKey(
                name: "FK_monthly_transport_sheets_AspNetUsers_PaidById",
                schema: "transportation",
                table: "monthly_transport_sheets");

            migrationBuilder.DropForeignKey(
                name: "FK_monthly_transport_sheets_AspNetUsers_RecipientId",
                schema: "transportation",
                table: "monthly_transport_sheets");

            migrationBuilder.DropForeignKey(
                name: "FK_monthly_transport_sheets_Crews_CrewId",
                schema: "transportation",
                table: "monthly_transport_sheets");

            migrationBuilder.DropTable(
                name: "monthly_transport_sheet_days",
                schema: "transportation");

            migrationBuilder.DropIndex(
                name: "IX_vehicles_Plate",
                schema: "transportation",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "IX_TaxiExpense_TransportDayId_Leg_PaidById",
                schema: "transportation",
                table: "TaxiExpense");

            migrationBuilder.DropIndex(
                name: "IX_Crews_Name",
                schema: "transportation",
                table: "Crews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_monthly_transport_sheets",
                schema: "transportation",
                table: "monthly_transport_sheets");

            migrationBuilder.DropIndex(
                name: "IX_monthly_transport_sheets_CrewId_Year_Month",
                schema: "transportation",
                table: "monthly_transport_sheets");

            migrationBuilder.DropIndex(
                name: "IX_monthly_transport_sheets_PaidById",
                schema: "transportation",
                table: "monthly_transport_sheets");

            migrationBuilder.DropIndex(
                name: "IX_monthly_transport_sheets_RecipientId",
                schema: "transportation",
                table: "monthly_transport_sheets");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                schema: "transportation",
                table: "monthly_transport_sheets");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                schema: "transportation",
                table: "monthly_transport_sheets");

            migrationBuilder.DropColumn(
                name: "PaidById",
                schema: "transportation",
                table: "monthly_transport_sheets");

            migrationBuilder.DropColumn(
                name: "RecipientId",
                schema: "transportation",
                table: "monthly_transport_sheets");

            migrationBuilder.RenameTable(
                name: "monthly_transport_sheets",
                schema: "transportation",
                newName: "MonthlyTransportSheets",
                newSchema: "transportation");

            migrationBuilder.RenameIndex(
                name: "IX_monthly_transport_sheets_CreatedById",
                schema: "transportation",
                table: "MonthlyTransportSheets",
                newName: "IX_MonthlyTransportSheets_CreatedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MonthlyTransportSheets",
                schema: "transportation",
                table: "MonthlyTransportSheets",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "payout_lines",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MonthlyTransportSheetId = table.Column<long>(type: "bigint", nullable: false),
                    PaidById = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DriverKm = table.Column<double>(type: "double precision", nullable: false),
                    ExtraBusinessKm = table.Column<double>(type: "double precision", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TaxiCompensation = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payout_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payout_lines_AspNetUsers_PaidById",
                        column: x => x.PaidById,
                        principalSchema: "transportation",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payout_lines_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "transportation",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payout_lines_MonthlyTransportSheets_MonthlyTransportSheetId",
                        column: x => x.MonthlyTransportSheetId,
                        principalSchema: "transportation",
                        principalTable: "MonthlyTransportSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayoutLineTaxiExpenses",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PayoutLineId = table.Column<long>(type: "bigint", nullable: false),
                    TaxiExpenseId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutLineTaxiExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayoutLineTaxiExpenses_TaxiExpense_TaxiExpenseId",
                        column: x => x.TaxiExpenseId,
                        principalSchema: "transportation",
                        principalTable: "TaxiExpense",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayoutLineTaxiExpenses_payout_lines_PayoutLineId",
                        column: x => x.PayoutLineId,
                        principalSchema: "transportation",
                        principalTable: "payout_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_Plate",
                schema: "transportation",
                table: "vehicles",
                column: "Plate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxiExpense_TransportDayId_Leg_PaidById",
                schema: "transportation",
                table: "TaxiExpense",
                columns: new[] { "TransportDayId", "Leg", "PaidById" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Crews_Name",
                schema: "transportation",
                table: "Crews",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyTransportSheets_CrewId_Year_Month",
                schema: "transportation",
                table: "MonthlyTransportSheets",
                columns: new[] { "CrewId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payout_lines_MonthlyTransportSheetId",
                schema: "transportation",
                table: "payout_lines",
                column: "MonthlyTransportSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_payout_lines_PaidById",
                schema: "transportation",
                table: "payout_lines",
                column: "PaidById");

            migrationBuilder.CreateIndex(
                name: "IX_payout_lines_UserId",
                schema: "transportation",
                table: "payout_lines",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutLineTaxiExpenses_PayoutLineId",
                schema: "transportation",
                table: "PayoutLineTaxiExpenses",
                column: "PayoutLineId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutLineTaxiExpenses_TaxiExpenseId",
                schema: "transportation",
                table: "PayoutLineTaxiExpenses",
                column: "TaxiExpenseId");

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlyTransportSheets_AspNetUsers_CreatedById",
                schema: "transportation",
                table: "MonthlyTransportSheets",
                column: "CreatedById",
                principalSchema: "transportation",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlyTransportSheets_Crews_CrewId",
                schema: "transportation",
                table: "MonthlyTransportSheets",
                column: "CrewId",
                principalSchema: "transportation",
                principalTable: "Crews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
