using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class AddContactsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
               name: "PK_People",
               table: "People");
            migrationBuilder.DropColumn(
                name: "Id",
                table: "People"
            );
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "People",
                type: "int",
                nullable: false
            ).Annotation("SqlServer:Identity", "1, 1");
            migrationBuilder.AddPrimaryKey(
            name: "PK_People",
            table: "People",
            column: "Id");

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    ClientAccountId = table.Column<int>(type: "int", nullable: false),
                    ContactType = table.Column<string>(type: "nvarchar(10)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contacts_ClientAccounts_ClientAccountId",
                        column: x => x.ClientAccountId,
                        principalTable: "ClientAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contacts_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id");
                });

            migrationBuilder.Sql("ALTER TABLE Contacts ADD CONSTRAINT CHK_ContactType CHECK (ContactType IN ('primary', 'approver', 'financial', 'expense'))");
          
            migrationBuilder.CreateIndex(
                name: "IX_Contact_ClientAccountId",
                table: "Contacts",
                column: "ClientAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Contact_PersonId",
                table: "Contacts",
                column: "PersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contact_PersonId",
                table: "Contacts");
            migrationBuilder.DropTable(
                name: "Contacts");
            migrationBuilder.DropPrimaryKey(
                name: "PK_People",
                table: "People");
            migrationBuilder.DropColumn(
                 name: "Id",
                 table: "People"
             );
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "People",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.NewGuid()); ;

            migrationBuilder.AddPrimaryKey(
                name: "PK_People",
                table: "People",
                column: "Id");
        }
    }
}
