using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFiscalHistorySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // We'll get rid of the "FiscalPeriod" (non-plural) table, but we need its data for now.
            migrationBuilder.CreateTable(
                name: "FiscalPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Period = table.Column<string>(type: "varchar(50)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalPeriods", x => x.Id);
                });

            migrationBuilder.Sql(
                $"insert into FiscalPeriods " +
                $"select distinct Period from FiscalPeriod;"
            );

            migrationBuilder.CreateTable(
                name: "FiscalHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillId = table.Column<int>(type: "int", nullable: false),
                    PeriodId = table.Column<int>(type: "int", nullable: false),
                    UnitPriceAtFiscal = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiscalHistory_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FiscalHistory_FiscalPeriod_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "FiscalPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FiscalHistory_BillId",
                table: "FiscalHistory",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalHistory_PeriodId",
                table: "FiscalHistory",
                column: "PeriodId");

            migrationBuilder.Sql(
                $"insert into FiscalHistory (BillId, PeriodId, UnitPriceAtFiscal) " +
                $"select ChargeId, FiscalPeriods.Id, Amount " +
                $"from FiscalPeriod " +
                $"inner join FiscalPeriods on FiscalPeriod.[Period] = FiscalPeriods.[Period];"
            );

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FiscalHistory");

            migrationBuilder.DropTable(
                name: "FiscalPeriods");
        }
    }
}
