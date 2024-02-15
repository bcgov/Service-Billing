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

            migrationBuilder.DropColumn(
                name: "AggregateGLCode",
                table: "Bills");
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

            migrationBuilder.AddColumn<string>(
          name: "ClientTeam",
          table: "ClientAccounts",
          type: "nvarchar(max)",
          nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "ClientAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
               name: "ClientTeams",
               columns: table => new
               {
                   Id = table.Column<int>(type: "int", nullable: false)
                       .Annotation("SqlServer:Identity", "1, 1"),
                   Approver = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   FinancialContact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   PrimaryContact = table.Column<string>(type: "nvarchar(max)", nullable: true)
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_ClientTeams", x => x.Id);
               });

            migrationBuilder.CreateIndex(
                name: "IX_ClientAccounts_TeamId",
                table: "ClientAccounts",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientAccounts_ClientTeams_TeamId",
                table: "ClientAccounts",
                column: "TeamId",
                principalTable: "ClientTeams",
                principalColumn: "Id");

            migrationBuilder.AddColumn<string>(
               name: "AggregateGLCode",
               table: "Bills",
               type: "nvarchar(max)",
               nullable: true);
        }
    }
}
