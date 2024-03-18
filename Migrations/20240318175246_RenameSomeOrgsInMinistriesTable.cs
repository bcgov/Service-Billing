using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

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
                value: "Small Business and Round Table");
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

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
