using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class CreateBusinessAreaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GDXBusArea",
                table: "ServiceCategories");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ServiceCategories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "BusAreaId",
                table: "ServiceCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BusAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Acronym = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusAreas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_BusAreaId",
                table: "ServiceCategories",
                column: "BusAreaId");

            

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
               $"where GDXBusArea = 'Any' or [Name] = 'Client Credit'; "
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
               $"values ('DMS', 'Delivery Management Services'); "
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

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceCategories_BusAreas_BusAreaId",
                table: "ServiceCategories",
                column: "BusAreaId",
                principalTable: "BusAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            // done
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceCategories_BusAreas_BusAreaId",
                table: "ServiceCategories");

            migrationBuilder.DropTable(
                name: "BusAreas");

            migrationBuilder.DropIndex(
                name: "IX_ServiceCategories_BusAreaId",
                table: "ServiceCategories");

            migrationBuilder.DropColumn(
                name: "BusAreaId",
                table: "ServiceCategories");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ServiceCategories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GDXBusArea",
                table: "ServiceCategories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
