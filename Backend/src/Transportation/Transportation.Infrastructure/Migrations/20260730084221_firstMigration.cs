using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Transportation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class firstMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "transportation");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    TelegramId = table.Column<string>(type: "text", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DistanceKm = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransportSettings",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommuteKmRate = table.Column<decimal>(type: "numeric", nullable: false),
                    ExtraBusinessKmRate = table.Column<decimal>(type: "numeric", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "transportation",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "absence_notices",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Scope = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsNotified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_absence_notices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_absence_notices_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "transportation",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "transportation",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "transportation",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "transportation",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "transportation",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "transportation",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "transportation",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "transportation",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "transportation",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DriverId = table.Column<long>(type: "bigint", nullable: false),
                    Plate = table.Column<string>(type: "text", nullable: false),
                    SeatCount = table.Column<int>(type: "integer", nullable: false),
                    AmortizationBasis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicles_AspNetUsers_DriverId",
                        column: x => x.DriverId,
                        principalSchema: "transportation",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Crews",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RouteId = table.Column<long>(type: "bigint", nullable: false),
                    CrewLeadId = table.Column<long>(type: "bigint", nullable: true),
                    DriverLeadId = table.Column<long>(type: "bigint", nullable: true),
                    SeatCapacity = table.Column<int>(type: "integer", maxLength: 5, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Crews_AspNetUsers_CrewLeadId",
                        column: x => x.CrewLeadId,
                        principalSchema: "transportation",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Crews_AspNetUsers_DriverLeadId",
                        column: x => x.DriverLeadId,
                        principalSchema: "transportation",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Crews_Routes_RouteId",
                        column: x => x.RouteId,
                        principalSchema: "transportation",
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CrewMemberships",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CrewId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ActiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewMemberships_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "transportation",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrewMemberships_Crews_CrewId",
                        column: x => x.CrewId,
                        principalSchema: "transportation",
                        principalTable: "Crews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyTransportSheets",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CrewId = table.Column<long>(type: "bigint", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ConfirmedById = table.Column<long>(type: "bigint", nullable: true),
                    CreatedById = table.Column<long>(type: "bigint", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyTransportSheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyTransportSheets_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalSchema: "transportation",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonthlyTransportSheets_Crews_CrewId",
                        column: x => x.CrewId,
                        principalSchema: "transportation",
                        principalTable: "Crews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transport_days",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CrewId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MorningMode = table.Column<int>(type: "integer", nullable: false),
                    AfternoonMode = table.Column<int>(type: "integer", nullable: true),
                    BaseRouteKm = table.Column<double>(type: "double precision", nullable: false),
                    ExtraCommuteKm = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: false),
                    ExtraBusinessKm = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: false),
                    DriverId = table.Column<long>(type: "bigint", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LoggedBy = table.Column<long>(type: "bigint", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Confirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transport_days", x => x.Id);
                    table.ForeignKey(
                        name: "FK_transport_days_AspNetUsers_DriverId",
                        column: x => x.DriverId,
                        principalSchema: "transportation",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_transport_days_AspNetUsers_LoggedBy",
                        column: x => x.LoggedBy,
                        principalSchema: "transportation",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transport_days_Crews_CrewId",
                        column: x => x.CrewId,
                        principalSchema: "transportation",
                        principalTable: "Crews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payout_lines",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MonthlyTransportSheetId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    DriverPayment = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ExtraKmPayment = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TaxiCompensation = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payout_lines", x => x.Id);
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
                name: "TaxiExpense",
                schema: "transportation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransportDayId = table.Column<long>(type: "bigint", nullable: false),
                    Leg = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidById = table.Column<long>(type: "bigint", nullable: false),
                    TaxiExpenseStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxiExpense", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxiExpense_AspNetUsers_PaidById",
                        column: x => x.PaidById,
                        principalSchema: "transportation",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaxiExpense_transport_days_TransportDayId",
                        column: x => x.TransportDayId,
                        principalSchema: "transportation",
                        principalTable: "transport_days",
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
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "IX_absence_notices_UserId",
                schema: "transportation",
                table: "absence_notices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "transportation",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "transportation",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "transportation",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "transportation",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "transportation",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "transportation",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "transportation",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrewMemberships_CrewId",
                schema: "transportation",
                table: "CrewMemberships",
                column: "CrewId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewMemberships_UserId",
                schema: "transportation",
                table: "CrewMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Crews_CrewLeadId",
                schema: "transportation",
                table: "Crews",
                column: "CrewLeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Crews_DriverLeadId",
                schema: "transportation",
                table: "Crews",
                column: "DriverLeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Crews_Name",
                schema: "transportation",
                table: "Crews",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Crews_RouteId",
                schema: "transportation",
                table: "Crews",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyTransportSheets_CreatedById",
                schema: "transportation",
                table: "MonthlyTransportSheets",
                column: "CreatedById");

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

            migrationBuilder.CreateIndex(
                name: "IX_TaxiExpense_PaidById",
                schema: "transportation",
                table: "TaxiExpense",
                column: "PaidById");

            migrationBuilder.CreateIndex(
                name: "IX_TaxiExpense_TransportDayId",
                schema: "transportation",
                table: "TaxiExpense",
                column: "TransportDayId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxiExpense_TransportDayId_Leg_PaidById",
                schema: "transportation",
                table: "TaxiExpense",
                columns: new[] { "TransportDayId", "Leg", "PaidById" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transport_days_CrewId",
                schema: "transportation",
                table: "transport_days",
                column: "CrewId");

            migrationBuilder.CreateIndex(
                name: "IX_transport_days_DriverId",
                schema: "transportation",
                table: "transport_days",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_transport_days_LoggedBy",
                schema: "transportation",
                table: "transport_days",
                column: "LoggedBy");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_DriverId",
                schema: "transportation",
                table: "vehicles",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_Plate",
                schema: "transportation",
                table: "vehicles",
                column: "Plate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "absence_notices",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "CrewMemberships",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "PayoutLineTaxiExpenses",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "TransportSettings",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "vehicles",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "TaxiExpense",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "payout_lines",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "transport_days",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "MonthlyTransportSheets",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "Crews",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "transportation");

            migrationBuilder.DropTable(
                name: "Routes",
                schema: "transportation");
        }
    }
}
