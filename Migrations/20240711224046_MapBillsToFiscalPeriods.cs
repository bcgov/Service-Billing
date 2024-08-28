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
            migrationBuilder.Sql($"insert into FiscalPeriods " +
                $"([Period]) " +    
                $"select distinct Bills.FiscalPeriod " +
                $"from Bills " + 
                $"where not exists (select 1 from FiscalPeriods where FiscalPeriods.[Period] = Bills.FiscalPeriod); " + 
                $"GO " +
                $"update Bills " + 
                $"set CurrentFiscalPeriodId = FiscalPeriods.Id " + 
                $"from Bills " + 
                $"INNER JOIN " +   
                $"FiscalPeriods " + 
                $"ON " +    
                $"Bills.FiscalPeriod = FiscalPeriods.[Period]; " +
                $"GO"
                );
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
