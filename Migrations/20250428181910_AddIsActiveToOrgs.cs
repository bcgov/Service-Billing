using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToOrgs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Ministries",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Ministries");
        }
    }
}
