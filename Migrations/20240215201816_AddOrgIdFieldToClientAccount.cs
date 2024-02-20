using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgIdFieldToClientAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "ClientAccounts",
                type: "int",
                nullable: true);
            
            migrationBuilder.Sql($"update Ministries " +
                $"set Acronym = 'MMHA'"
                + $"where Acronym = 'MHA';" +

                //awe geez, this is going to be a nightmare...
                $"update ClientAccounts " +
                $"set OrganizationId = Ministries.Id " +
                $"from Ministries " +
                $"where ClientAccounts.[Name] like Ministries.Acronym " +
                $"and ClientAccounts.[Name] like '%' + Ministries.Title + '%'; " + 
                
                $"update ClientAccounts " +
                $"set OrganizationId = 4 " +
                $"where ClientAccounts.[Name] like 'AF%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 7 " +
                $"where ClientAccounts.[Name] like 'CITZ%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 34 " +
                $"where ClientAccounts.[Name] like 'ECON%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 1 " +
                $"where ClientAccounts.[Name] like 'EMLI%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 35 " +
                $"where ClientAccounts.[Name] like 'ENV%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 11 " +
                $"where ClientAccounts.[Name] like 'FIN%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 12 " +
                $"where ClientAccounts.[Name] like 'FOR%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 36 " +
                $"where ClientAccounts.[Name] like 'GCPE%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 16 " +
                $"where ClientAccounts.[Name] like 'JEDI%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 39 " +
                $"where ClientAccounts.[Name] like 'LRB%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 17 " +
                $"where ClientAccounts.[Name] like 'LBR%'; " +


                $"update ClientAccounts " +
                $"set OrganizationId = 19 " +
                $"where ClientAccounts.[Name] like 'MUNI%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 40 " +
                $"where ClientAccounts.[Name] like 'PSA%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 20 " +
                $"where ClientAccounts.[Name] like 'PSFS%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 21 " +
                $"where ClientAccounts.[Name] like 'PSSG%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 22 " +
                $"where ClientAccounts.[Name] like 'SDPR%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 41 " +
                $"where ClientAccounts.[Name] like 'SDPR - The Q%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 2 " +
                $"where ClientAccounts.[Name] like 'TACS%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 23 " +
                $"where ClientAccounts.[Name] like 'TRAN%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 44 " +
                $"where ClientAccounts.[Name] like 'TI Corp%'; " +


                // Now deal with all the stragglers
                $"update ClientAccounts " +
                $"set OrganizationId = 5 " +
                $"where ClientAccounts.[Name] like 'AG - Hate Crimes BC - Analytics%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 45 " +
                $"where ClientAccounts.[Name] like 'WCAT%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 5 " +
                $"where ClientAccounts.[Name] like 'AG - Hate Crimes BC - Analytics%' " +
                $"or ClientAccounts.[Name] like 'AG - Tickets Solutions Explorer Analytics%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 30 " +
                $"where ClientAccounts.[Name] like 'AG - JSB Justice Services Branch%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 13 " +
                $"where ClientAccounts.[Name] like 'HLTH%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 37 " +
                $"where ClientAccounts.[Name] like 'HLTH - Seniors%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 24 " +
                $"where ClientAccounts.[Name] like 'WLRS%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 8 " +
                $"where ClientAccounts.[Name] like 'ECC%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 46 " +
                $"where ClientAccounts.[Name] like 'IGRS%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 14 " +
                $"where ClientAccounts.[Name] like 'HOUS%'; " +

                 $"update ClientAccounts " +
                $"set OrganizationId = 15" +
                $"where ClientAccounts.[Name] like 'IRR%'; " +

                 $"update ClientAccounts " +
                $"set OrganizationId = 9 " +
                $"where ClientAccounts.[Name] like 'EMCR%'; " +

                 $"update ClientAccounts " +
                $"set OrganizationId = 6 " +
                $"where ClientAccounts.[Name] like 'MCF%'; " +

                 $"update ClientAccounts " +
                $"set OrganizationId = 14 " +
                $"where ClientAccounts.[Name] like 'HOUS%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 32 " +
                $"where ClientAccounts.[Name] like 'BCAA%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 25 " +
                $"where ClientAccounts.[Name] like 'AG - Inde%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 28 " +
                $"where ClientAccounts.[Name] like 'AG - Office%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 27 " +
                $"where ClientAccounts.[Name] like 'AG - Mental%'; " +

                 $"update ClientAccounts " +
                $"set OrganizationId = 31 " +
                $"where ClientAccounts.[Name] like 'ALC%'; " +

                 $"update ClientAccounts " +
                $"set OrganizationId = 33 " +
                $"where ClientAccounts.[Name] like 'CLBC%'; " +

                 $"update ClientAccounts " +
                $"set OrganizationId = 35 " +
                $"where ClientAccounts.[Name] like 'ENV - Climate%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 10 " +
                $"where ClientAccounts.[Name] like 'ENV - Environ%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 37 " +
                $"where ClientAccounts.[Name] like 'HLTH%'; " +

                $"update ClientAccounts " +
                $"set OrganizationId = 18 " +
                $"where ClientAccounts.[Name] like 'MMH%';"
                );
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "ClientAccounts");
        }
    }
}
