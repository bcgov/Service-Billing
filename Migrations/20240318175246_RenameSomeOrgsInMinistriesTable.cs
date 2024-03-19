using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Graph.Search;

#nullable disable

/* This fixes up a few names and acronyms in the Organizations table */
namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class RenameSomeOrgsInMinistriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Ministries",
                keyColumn: "Title",
                keyValue: "Mental Health & Addictions",
                column: "Acronym",
                value: "MMHA");
            migrationBuilder.UpdateData(
                table: "Ministries",
                keyColumn: "Title",
                keyValue: "JEDI – Small Business and Round Table",
                column: "Acronym",
                value: "JEDI");
            migrationBuilder.UpdateData(
                table: "Ministries",
                keyColumn: "Title",
                keyValue: "JEDI – Small Business and Round Table",
                column: "Title",
                value: "Small Business Round Table");
            migrationBuilder.UpdateData(
                table: "Ministries",
                keyColumn: "Title",
                keyValue: "Public Safety & Solicitor General & Emergency B.C.",
                column: "Title",
                value: "Public Safety & Solicitor General");
            migrationBuilder.UpdateData(
                table: "Ministries",
                keyColumn: "Title",
                keyValue: "Transportation and Infrastructure Corporation",
                column: "Title",
                value: "Transportation and Investment Corporation");
            migrationBuilder.UpdateData(
                table: "Ministries",
                keyColumn: "Title",
                keyValue: "Children & Family Development",
                column: "Acronym",
                value: "CFD");

            //using sql command, 'cause I need that Scope_Identity bit
            migrationBuilder.Sql(@"insert into Ministries (Title, Acronym)
                    values ('INBCInvest', 'JEDI');
               update ClientAccounts
                set OrganizationId = SCOPE_IDENTITY()
                where [Name] = 'JEDI - Invest in BC WordPress';"
            );

            migrationBuilder.DeleteData(
                table: "Ministries",
                keyColumn: "Title",
                keyValue: "inbcInvestment");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Ministries",
                keyColumn: "Title",
                keyValue: "Mental Health & Addictions",
                column: "Acronym",
                value: "MHA");
            migrationBuilder.UpdateData(
                table: "Ministries",
                keyColumn: "Title",
                keyValue: "JEDI – Small Business and Round Table",
                column: "Acronym",
                value: "ECON");
            migrationBuilder.UpdateData(
                table: "Ministries",
                keyColumn: "Title",
                keyValue: "JEDI – Small Business and Round Table",
                column: "Title",
                value: "JEDI – Small Business and Round Table");
            migrationBuilder.UpdateData(
                table: "Ministries",
                keyColumn: "Title",
                keyValue: "Public Safety & Solicitor General & Emergency B.C.",
                column: "Title",
                value: "Public Safety & Solicitor General & Emergency B.C.");
            migrationBuilder.UpdateData(
                table: "Ministries",
                keyColumn: "Title",
                keyValue: "Transportation and Infrastructure Corporation",
                column: "Title",
                value: "Transportation and Infrastructure Corporation");
        }
    }
}
