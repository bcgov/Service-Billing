using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceBilling.BillingManagement.UI.Migrations
{
    /// <inheritdoc />
    public partial class IsActiveAddedToServiceCat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ServiceCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ServiceCategories");
        }
    }
}
