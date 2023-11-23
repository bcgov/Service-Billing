using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class IsActiveAndGLCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientIntakeViewModel");

            migrationBuilder.DropColumn(
                name: "CasClientNumber",
                table: "ClientAccounts");

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

            migrationBuilder.AlterColumn<int>(
                name: "ServiceCategoryId",
                table: "Bills",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AggregateGLCoding",
                table: "Bills",
                type: "nvarchar(max)",
                nullable: true);

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
                name: "AggregateGLCoding",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Bills");

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

            migrationBuilder.AlterColumn<int>(
                name: "ServiceCategoryId",
                table: "Bills",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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
