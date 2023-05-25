using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class updateBillTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "uintPrice",
                table: "serviceCategories",
                newName: "unitPrice");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "clientAccounts",
                newName: "clientName");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "bills",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "bills",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "bills",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "URL_or_IDIR",
                table: "bills",
                newName: "ticketNumberAndRequesterName");

            migrationBuilder.RenameColumn(
                name: "Ticket_Number_and_Requester_Name",
                table: "bills",
                newName: "itemType");

            migrationBuilder.RenameColumn(
                name: "Item_Type",
                table: "bills",
                newName: "idirOrUrl");

            migrationBuilder.RenameColumn(
                name: "Fiscal_Period",
                table: "bills",
                newName: "fiscalPeriod");

            migrationBuilder.RenameColumn(
                name: "Expense_Authority_Name",
                table: "bills",
                newName: "expenseAuthorityName");

            migrationBuilder.RenameColumn(
                name: "Created_By",
                table: "bills",
                newName: "createdBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "unitPrice",
                table: "serviceCategories",
                newName: "uintPrice");

            migrationBuilder.RenameColumn(
                name: "clientName",
                table: "clientAccounts",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "bills",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "bills",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "bills",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ticketNumberAndRequesterName",
                table: "bills",
                newName: "URL_or_IDIR");

            migrationBuilder.RenameColumn(
                name: "itemType",
                table: "bills",
                newName: "Ticket_Number_and_Requester_Name");

            migrationBuilder.RenameColumn(
                name: "idirOrUrl",
                table: "bills",
                newName: "Item_Type");

            migrationBuilder.RenameColumn(
                name: "fiscalPeriod",
                table: "bills",
                newName: "Fiscal_Period");

            migrationBuilder.RenameColumn(
                name: "expenseAuthorityName",
                table: "bills",
                newName: "Expense_Authority_Name");

            migrationBuilder.RenameColumn(
                name: "createdBy",
                table: "bills",
                newName: "Created_By");
        }
    }
}
