using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientAccountId = table.Column<int>(type: "int", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdirOrUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServiceCategoryId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FiscalPeriod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ticketNumberAndRequesterName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BillingCycle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientNumber = table.Column<short>(type: "smallint", maxLength: 3, nullable: true),
                    ResponsibilityCentre = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    ServiceLine = table.Column<int>(type: "int", maxLength: 5, nullable: true),
                    STOB = table.Column<short>(type: "smallint", maxLength: 4, nullable: true),
                    Project = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    ExpenseAuthorityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServicesEnabled = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientTeam = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    IsApprovedByEA = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientTeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryContact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Approver = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FinancialContact = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientTeams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ministries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Acronym = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ministries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCategories",
                columns: table => new
                {
                    ServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GDXBusArea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Costs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UOM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServiceOwner = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCategories", x => x.ServiceId);
                });

            migrationBuilder.CreateTable(
                name: "ClientIntakeViewModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    MinistryAcronym = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DivisionOrBranch = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientIntakeViewModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientIntakeViewModel_ClientAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "ClientAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientIntakeViewModel_ClientTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "ClientTeams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientIntakeViewModel_AccountId",
                table: "ClientIntakeViewModel",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientIntakeViewModel_TeamId",
                table: "ClientIntakeViewModel",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bills");

            migrationBuilder.DropTable(
                name: "ClientIntakeViewModel");

            migrationBuilder.DropTable(
                name: "Ministries");

            migrationBuilder.DropTable(
                name: "ServiceCategories");

            migrationBuilder.DropTable(
                name: "ClientAccounts");

            migrationBuilder.DropTable(
                name: "ClientTeams");
        }
    }
}
