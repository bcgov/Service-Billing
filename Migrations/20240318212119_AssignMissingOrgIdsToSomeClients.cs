using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class AssignMissingOrgIdsToSomeClients : Migration
    {
        /// <inheritdoc />
        /* Assigns OrganizationId values for a few client accounts that are missing them, and updates a few misidentified ones */
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // the missing values
            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValue: 500, //GCPE Marketing
                column: "OrganizationId",
                value: 36);

            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValue: 736, // JEDI - Small Business Roundtable WordPress
                column: "OrganizationId",
                value: 34);
            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id", // TACS Analytics
                keyValue: 756,
                column: "OrganizationId",
                value: 2);
            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValue: 757, //Labour Analytics
                column: "OrganizationId",
                value: 17);
            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValue: 758, // JEDI analytics
                column: "OrganizationId",
                value: 16);
            object[] muniIds = { 759, 786 };
            object[] nineteen = { 19, 19 };
            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValues: muniIds, // MUNI
                column: "OrganizationId",
                values: nineteen);
         
            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValue: 829,
                column: "OrganizationId",// GCPE - Digital Hub... Analytics
                value: 36);
            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValue: 830,
                column: "OrganizationId",
                value: 44);
            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValue: 841,
                column: "OrganizationId",
                value: 7);

            // the misidentified values

            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValue: 678,
                column: "OrganizationId",
                value: 37);

            object[] tranTIIds = { 831, 832, 855, 2010 };
            object[] fortyFour = { 44, 44, 44, 44 };
            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValues: tranTIIds,
                column: "OrganizationId",
                values: fortyFour
                );

            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValue: 678,
                column: "OrganizationId",
                value: 37);
            // Client accounts with NULL organization Ids
            object[] nullOrgClients = { 626, 659, 661, 683, 687, 691, 699, 721, 723, 746, 751, 761, 762, 789, 794, 795, 798, 801, 805, 857, 863, 865, 2115 };
            object[] nullOrgClientValues = { 10, 10, 15, 5, 6, 29, 6, 14, 18, 10, 7, 39, 7, 7, 7, 33, 5, 31, 24, 10, 24, 41, 20 };
            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValues: nullOrgClients,
                column: "OrganizationId",
                values: nullOrgClientValues);

            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValue: 780,
                column: "OrganizationId",
                value: 42);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            object[] ids = { 500, 736, 756, 757, 758, 759, 786, 829, 830, 841 };
            object[] values = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
   
            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValues: ids,
                column: "OrganizationId",
                values: values);

            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValue: 678,
                column: "OrganizationId",
                value: 13);

            object[] tranTIIds = { 831, 832, 855, 2010 };
            object[] twentyThree = { 23, 23, 23, 23 };
            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValues: tranTIIds,
                column: "OrganizationId",
                values: twentyThree
                );

            object[] nullOrgClients = { 626, 659, 661, 683, 687, 691, 699, 721, 723, 746, 751, 761, 762, 789, 794, 795, 798, 801, 805, 857, 863, 865 };
            object[] nullOrgClientValues = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            migrationBuilder.UpdateData(
                table: "ClientAccounts",
                keyColumn: "Id",
                keyValues: nullOrgClients,
                column: "OrganizationId",
                values: nullOrgClientValues);

        }
    }
}
