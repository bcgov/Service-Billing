using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class MapChargeToFiscalPeriodEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentFiscalPeriodId",
                table: "Bills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
               $"update Bills " +
               $"set CurrentFiscalPeriodId = FiscalPeriods.Id " +
               $"from FiscalPeriods " +
               $"where FiscalPeriods.Period = Bills.FiscalPeriod;"
           );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentFiscalPeriodId",
                table: "Bills");
        }
    }
}
