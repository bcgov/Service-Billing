using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Graph;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class AddChangeLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
               name: "ChangeLogs",
               columns: table => new
               {
                   Id = table.Column<int>(type: "int", nullable: false)
                       .Annotation("SqlServer:Identity", "1, 1"),
                   EntityId = table.Column<int>(type: "int", nullable: false), //see if it's possible to reference more than one table with these
                   EntityType = table.Column<string>(maxLength: 16, nullable: false),
                   ChangedBy = table.Column<string>(maxLength: 64, nullable: false),
                   LogEntry = table.Column<string>(type: "nvarchar(4000)", nullable: false),
                   DateModified = table.Column<DateTime>(type:  "dateTimeOffset(7)", nullable: false)
               },
            constraints: table =>
            {
                   table.PrimaryKey("PK_ChangeLogs", x => x.Id);
               });
            migrationBuilder.Sql("ALTER TABLE ChangeLogs ADD CONSTRAINT CHK_EntityType CHECK (EntityType IN ('charge', 'clientAccount', 'service'))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChangeLogs");
        }
    }
}
