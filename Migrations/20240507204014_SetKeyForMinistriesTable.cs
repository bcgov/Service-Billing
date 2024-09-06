using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class SetKeyForMinistriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddPrimaryKey(
                name: "PK_Ministries",
                table: "Ministries",
                column: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Ministries",
                type: "nvarchar(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Acronym",
                table: "Ministries",
                type: "nvarchar(20)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bills_CurrentFiscalPeriodId",
                table: "Bills",
                column: "CurrentFiscalPeriodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bills_FiscalPeriods_CurrentFiscalPeriodId",
                table: "Bills");

            migrationBuilder.DropIndex(
                name: "IX_Bills_CurrentFiscalPeriodId",
                table: "Bills");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Ministries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Acronym",
                table: "Ministries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "FiscalPeriod",
                table: "Bills",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
