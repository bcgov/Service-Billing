using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class MapBillsToFiscalPeriods : Migration
    {
        /// <inheritdoc />
        /* 
         * See also MapChargeToFiscalPeriod. If there is no existing fiscal period for a charge, we create it, then map 
         * that period ID to the charge. 
         */
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("insert into FiscalPeriods ([Period])\r\n    select distinct Bills.FiscalPeriod\r\n    from Bills\r\n    where not exists (select 1 from FiscalPeriods where FiscalPeriods.[Period] = Bills.FiscalPeriod);\r\nGO\r\n\r\nupdate Bills\r\nset CurrentFiscalPeriodId = FiscalPeriods.Id\r\nfrom Bills\r\nINNER JOIN\r\n    FiscalPeriods\r\nON \r\n    Bills.FiscalPeriod = FiscalPeriods.[Period];\r\n\r\n\tGO");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
