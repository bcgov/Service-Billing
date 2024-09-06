using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class InsertQ12425FiscalHistoryRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "INSERT INTO FiscalHistory (BillId, PeriodId, UnitPriceAtFiscal)\r\nSELECT \r\n  B.Id,\r\n  FP2.Id,\r\n  SC.Costs\r\nFROM \r\n  FiscalPeriod FP\r\n  INNER JOIN FiscalPeriods FP2 ON FP.[Period] = FP2.[Period]\r\n  INNER JOIN Bills B ON FP.ChargeId = B.Id\r\n  INNER JOIN ServiceCategories SC ON B.ServiceCategoryId = SC.ServiceId\r\nWHERE \r\n  FP.[Period] = 'Fiscal 24/25 Quarter 1'\r\n  AND NOT EXISTS (\r\n    SELECT 1\r\n    FROM FiscalHistory FH\r\n    WHERE FH.BillId = B.Id AND FH.PeriodId = FP2.Id\r\n  );\r\n\r\n\r\n  update FiscalHistory\r\n  set QuantityAtFiscal = 3 \r\n  where\r\n  PeriodId = 3;\r\n\r\n  update FiscalHistory\r\n  set QuantityAtFiscal = 0\r\n  where\r\n  PeriodId = 3 and BillId = 11832 or BillId = 11833;"
             );  
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"delete from FiscalHistory " +
                $"where PeriodId = 3;"
            );
        }
    }
}
