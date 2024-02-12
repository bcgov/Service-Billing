using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class ClientTeamContactToAccountModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Approver",
                table: "ClientAccounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinancialContact",
                table: "ClientAccounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryContact",
                table: "ClientAccounts",
                type: "nvarchar(max)",
                nullable: true);
            migrationBuilder.DropForeignKey(
                name: "FK_ClientAccounts_ClientTeams_TeamId",
                table: "ClientAccounts");
            migrationBuilder.DropIndex(
                name: "IX_ClientAccounts_TeamId",
                table: "ClientAccounts");
            
            migrationBuilder.Sql($"update ClientAccounts " + 
                $"set PrimaryContact = ClientTeams.PrimaryContact, FinancialContact = ClientTeams.FinancialContact, Approver = ClientTeams.Approver " 
                + $"from ClientTeams " 
                + $"where ClientAccounts.TeamId = ClientTeams.Id; " 
                );
            migrationBuilder.DropColumn(
                name: "ClientTeam",
                table: "ClientAccounts");
            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "ClientAccounts");
            migrationBuilder.DropTable(
                name: "ClientTeams");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approver",
                table: "ClientAccounts");

            migrationBuilder.DropColumn(
                name: "FinancialContact",
                table: "ClientAccounts");

            migrationBuilder.DropColumn(
                name: "PrimaryContact",
                table: "ClientAccounts");
        }
    }
}
