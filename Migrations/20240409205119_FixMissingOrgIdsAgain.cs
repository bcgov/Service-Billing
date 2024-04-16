using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingOrgIdsAgain : Migration
    {
        /// <inheritdoc />
        /* this Migration was necessary because of a code error where when client accounts were edited,
         it caused the OrganizationId to be set to null. This was Alex's fault. My bad!*/
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            object[] clientsMissingOrgs = { 742, 755, 765 };
            object[] orgIDsToAssign = { 7, 7, 20 };
            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValues: clientsMissingOrgs,
                column: "OrganizationId",
                values: orgIDsToAssign);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            object[] clientsMissingOrgs = { 742, 755, 765 };
            object[] orgIDsToAssign = { null, null, null };
            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValues: clientsMissingOrgs,
                column: "OrganizationId",
                values: orgIDsToAssign);
        }
    }
}
