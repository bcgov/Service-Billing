using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class AssignMoreMissingOrgIDs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            object[] clientsMissingOrgs = { 818, 2009, 835, 2053, 744, 705, 797, 859, 2111, 815, 854, 845, 675, 633, 2048, 2049, 812, 769,
            747, 862, 2110, 826, 837, 849, 621, 752, 681, 784, 820, 848, 750, 806, 802, 2079 };
            object[] orgIDsToAssign = { 4, 4, 5, 5, 5, 5, 5, 5, 7, 7, 7, 7, 7, 8, 8, 8, 10, 11,
            12, 13, 46, 16, 17, 17, 40, 40, 20, 20, 22, 22, 22, 44, 24, 24  };
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
            object[] clientsMissingOrgs = { };
            object[] orgIDsToAssign = { };
            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValues: clientsMissingOrgs,
                column: "OrganizationId",
                values: orgIDsToAssign);
        }
    }
}
