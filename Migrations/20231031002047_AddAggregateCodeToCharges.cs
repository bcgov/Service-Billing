using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class AddAggregateCodeToCharges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientIntakeViewModel");

            migrationBuilder.AddColumn<string>(
                name: "AggregateGLCode",
                table: "Bills",
                type: "nvarchar(50)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AggregateGLCode",
                table: "Bills");

            migrationBuilder.CreateTable(
                name: "ClientIntakeViewModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    DivisionOrBranch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinistryAcronym = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
    }
}
