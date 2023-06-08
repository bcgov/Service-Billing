using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class TeamID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "unitPrice",
                table: "serviceCategories");

            migrationBuilder.DropColumn(
                name: "itemType",
                table: "bills");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "serviceCategories",
                newName: "Service_Category");

            migrationBuilder.RenameColumn(
                name: "serviceId",
                table: "serviceCategories",
                newName: "Service_Id");

            migrationBuilder.AddColumn<string>(
                name: "Costs",
                table: "serviceCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Details_Description",
                table: "serviceCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GDX_Bus_Area",
                table: "serviceCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "project",
                table: "clientAccounts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "client",
                table: "clientAccounts",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Service_Line",
                table: "clientAccounts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "STOB",
                table: "clientAccounts",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Client_Team",
                table: "clientAccounts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "Client_Account_Number",
                table: "clientAccounts",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateTable(
                name: "clientTeams",
                columns: table => new
                {
                    Team_Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Client_Team_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Financial_Contacts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Primary_Contact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approvers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientTeams", x => x.Team_Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clientTeams");

            migrationBuilder.DropColumn(
                name: "Costs",
                table: "serviceCategories");

            migrationBuilder.DropColumn(
                name: "Details_Description",
                table: "serviceCategories");

            migrationBuilder.DropColumn(
                name: "GDX_Bus_Area",
                table: "serviceCategories");

            migrationBuilder.RenameColumn(
                name: "Service_Category",
                table: "serviceCategories",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Service_Id",
                table: "serviceCategories",
                newName: "serviceId");

            migrationBuilder.AddColumn<decimal>(
                name: "unitPrice",
                table: "serviceCategories",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "project",
                table: "clientAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "client",
                table: "clientAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Service_Line",
                table: "clientAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "STOB",
                table: "clientAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Client_Team",
                table: "clientAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Client_Account_Number",
                table: "clientAccounts",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "itemType",
                table: "bills",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
