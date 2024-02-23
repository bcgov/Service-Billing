using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Graph;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class CreateBusAreaTableAndGroupServiceCatsByThem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* The purpose of this migration is to create a table of business areas to group service categories into.
             * Before this migration, we have (had?) one table of service categories for creating charges, with multiple
             service categories falling under analytics, as one example. The idea here have a table containing  business
            areas like "analytics", and "Wordpress" to group services by. */

            // First, make a table for business area data
            //migrationBuilder.CreateTable(
            //     name: "BusAreas",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(nullable: false).Annotation(),
            //         Acronym = table.Column<string>(nullable: true),
            //         Name = table.Column<string>(nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_BusAreas", x => x.Id);
            //     });

            migrationBuilder.Sql(@"CREATE TABLE BusAreas(
               Id int IDENTITY(1,1) PRIMARY KEY,
                Acronym varchar(20) NOT NULL,
                Name varchar(100)); "
            );

            // Add a column to ServiceCategories to track which business area the service falls under
            migrationBuilder.AddColumn<int>(
                name: "BusAreaId",
                table: "ServiceCategories",
                type: "int",
                nullable: true); //this really ought never to be null

            // make the column a foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_ServiceCategories_BusAreas_Id",
                table: "ServiceCategories",
                column: "BusAreaId",
                principalTable: "BusAreas"
                );

            //// for each business area, add the data to the table, then use Scope_Identity() to tie the relevant services to it
            //// Analytics
            migrationBuilder.Sql($"insert into BusAreas " +
                $"(Acronym, [Name]) " +
                $"values ('ANA', 'Analytics'); " +

                $"update ServiceCategories " +
                $"set BusAreaId = Scope_Identity() " +
                $"where GDXBusArea = 'ANA'; "
                );

            migrationBuilder.Sql($"insert into BusAreas " +
               $"(Acronym, [Name]) " +
               $"values ('Any', 'Any'); " +

               $"update ServiceCategories " +
               $"set BusAreaId = Scope_Identity() " +
               $"where GDXBusArea = 'Any'; "
               );

            migrationBuilder.Sql($"insert into BusAreas " +
               $"(Acronym, [Name]) " +
               $"values ('DES', 'Digital Engagement Solutions'); " +

               $"update ServiceCategories " +
               $"set BusAreaId = Scope_Identity() " +
               $"where GDXBusArea = 'DES'; "
               );

            migrationBuilder.Sql($"insert into BusAreas " +
               $"(Acronym, [Name]) " +
               $"values ('DMS', 'Delivery Management Solutions'); "
                );

            migrationBuilder.Sql($"insert into BusAreas " +
               $"(Acronym, [Name]) " +
               $"values ('Multi', 'Multi'); "
                );

            migrationBuilder.Sql($"insert into BusAreas " +
               $"(Acronym, [Name]) " +
               $"values ('Other', 'Other'); " +

               $"update ServiceCategories " +
               $"set BusAreaId = Scope_Identity() " +
               $"where GDXBusArea = 'Other'; "
               );

            migrationBuilder.Sql($"insert into BusAreas " +
               $"(Acronym, [Name]) " +
               $"values ('OSS', 'Online Services Solutions'); " +

               $"update ServiceCategories " +
               $"set BusAreaId = Scope_Identity() " +
               $"where GDXBusArea = 'OSS'; "
               );
            // done

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //remove ServiceCategories -> BusArea FK constraint
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceCategories_BusAreas_BusAreaId",
                table: "ServiceCategories"
                );

            // drop the BusArea Id column form ServiceCategories
            migrationBuilder.DropColumn(
                name: "BusAreaId",
                table: "ServiceCategories");


            //drop BusAreas table
            migrationBuilder.DropTable(
                name: "BusAreas");

        }
    }
}
