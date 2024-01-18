using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class addedNotesFieldToClientAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientName",
                table: "Bills");

            migrationBuilder.AlterColumn<string>(
                name: "Project",
                table: "ClientAccounts",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(7)",
                oldMaxLength: 7,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ClientAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ClientAccounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientAccounts_TeamId",
                table: "ClientAccounts",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_ClientAccountId",
                table: "Bills",
                column: "ClientAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_ServiceCategoryId",
                table: "Bills",
                column: "ServiceCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_ClientAccounts_ClientAccountId",
                table: "Bills",
                column: "ClientAccountId",
                principalTable: "ClientAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_ServiceCategories_ServiceCategoryId",
                table: "Bills",
                column: "ServiceCategoryId",
                principalTable: "ServiceCategories",
                principalColumn: "ServiceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientAccounts_ClientTeams_TeamId",
                table: "ClientAccounts",
                column: "TeamId",
                principalTable: "ClientTeams",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bills_ClientAccounts_ClientAccountId",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "FK_Bills_ServiceCategories_ServiceCategoryId",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientAccounts_ClientTeams_TeamId",
                table: "ClientAccounts");

            migrationBuilder.DropIndex(
                name: "IX_ClientAccounts_TeamId",
                table: "ClientAccounts");

            migrationBuilder.DropIndex(
                name: "IX_Bills_ClientAccountId",
                table: "Bills");

            migrationBuilder.DropIndex(
                name: "IX_Bills_ServiceCategoryId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ClientAccounts");

            migrationBuilder.AlterColumn<string>(
                name: "Project",
                table: "ClientAccounts",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(7)",
                oldMaxLength: 7);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ClientAccounts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ClientName",
                table: "Bills",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
