using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bills",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    clientAccountId = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    idirOrUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    serviceCategoryId = table.Column<int>(type: "int", nullable: false),
                    fiscalPeriod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    UOM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    expenseAuthorityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ticketNumberAndRequesterName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    itemType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bills", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clientAccounts",
                columns: table => new
                {
                    Client_Account_Number = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_Account_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    client = table.Column<int>(type: "int", nullable: false),
                    Responsibility_Centre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Service_Line = table.Column<int>(type: "int", nullable: false),
                    STOB = table.Column<int>(type: "int", nullable: false),
                    project = table.Column<int>(type: "int", nullable: false),
                    Expense_Authority_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Services_Enabled = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Client_Team = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientAccounts", x => x.Client_Account_Number);
                });

            migrationBuilder.CreateTable(
                name: "serviceCategories",
                columns: table => new
                {
                    serviceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    unitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_serviceCategories", x => x.serviceId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bills");

            migrationBuilder.DropTable(
                name: "clientAccounts");

            migrationBuilder.DropTable(
                name: "serviceCategories");
        }
    }
}
