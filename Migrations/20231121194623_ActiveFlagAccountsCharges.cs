using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class ActiveFlagAccountsCharges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CasClientNumber",
                table: "ClientAccounts");

            migrationBuilder.RenameColumn(
                name: "AggregateGLCode",
                table: "Bills",
                newName: "AggregateGLCoding");

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryContact",
                table: "ClientTeams",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FinancialContact",
                table: "ClientTeams",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Approver",
                table: "ClientTeams",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ClientAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Bills",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ClientAccounts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Bills");

            migrationBuilder.RenameColumn(
                name: "AggregateGLCoding",
                table: "Bills",
                newName: "AggregateGLCode");

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryContact",
                table: "ClientTeams",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FinancialContact",
                table: "ClientTeams",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Approver",
                table: "ClientTeams",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CasClientNumber",
                table: "ClientAccounts",
                type: "int",
                nullable: true);
        }
    }
}
