using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityFieldToFiscalHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.AddColumn<decimal>(
                name: "QuantityAtFiscal",
                table: "FiscalHistory",
                type: "decimal(18,2)",
                nullable: true);

           
            migrationBuilder.AddForeignKey(
                name: "FK_FiscalHistory_FiscalPeriods_PeriodId",
                table: "FiscalHistory",
                column: "PeriodId",
                principalTable: "FiscalPeriods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql($"update FiscalHistory " +
                $"set QuantityAtFiscal = Bills.Quantity " +
                $"from Bills " +
                $"where BillId = Bills.Id ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.DropColumn(
                name: "QuantityAtFiscal",
                table: "FiscalHistory");

            migrationBuilder.RenameTable(
                name: "FiscalPeriods",
                newName: "FiscalPeriod");
        }
    }
}
