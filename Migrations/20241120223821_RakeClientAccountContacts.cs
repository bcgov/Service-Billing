using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Billing.Migrations
{
    /// <inheritdoc />
    public partial class RakeClientAccountContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AI generated query.
            string query = "-- Assuming you are using SQL Server\r\n\r\n" +
            "BEGIN TRANSACTION;\r\n\r\n" +
            "-- Declare variables to hold the values from the ClientAccounts table\r\n" +
            "DECLARE @ClientAccountId INT;\r\nDECLARE @ExpenseAuthorityName NVARCHAR(255);\r\n" +
            "DECLARE @Approver NVARCHAR(255);\r\nDECLARE @FinancialContact NVARCHAR(255);\r\n" +
            "DECLARE @PrimaryContact NVARCHAR(255);\r\n" +
            "DECLARE @PersonId INT;\r\n\r\n" +
            "-- Cursor to iterate over each ClientAccount\r\n" +
            "DECLARE ClientAccountCursor CURSOR FOR\r\n" +
            "SELECT Id, ExpenseAuthorityName, Approver, FinancialContact, PrimaryContact\r\n" +
            "FROM ClientAccounts;\r\n\r\n" +
            "OPEN ClientAccountCursor;\r\n\r\n" +
            "FETCH NEXT FROM ClientAccountCursor INTO @ClientAccountId, @ExpenseAuthorityName, @Approver, @FinancialContact, @PrimaryContact;\r\n\r\n" +
            "WHILE @@FETCH_STATUS = 0\r\n" +
            "BEGIN\r\n" +
            "    -- Check and insert ExpenseAuthorityName if it does not exist\r\n" +
            "    IF NOT EXISTS (SELECT 1 FROM People WHERE DisplayName = @ExpenseAuthorityName)\r\n" +
            "    BEGIN\r\n" +
            "        INSERT INTO People (Name, DisplayName, Mail) -- Adjust fields as necessary\r\n" +
            "        VALUES (@ExpenseAuthorityName, @ExpenseAuthorityName, NULL); -- Assuming Mail is NULL or you have a value\r\n" +
            "    END\r\n    SELECT @PersonId = Id FROM People WHERE DisplayName = @ExpenseAuthorityName;\r\n" +
            "    IF @PersonId IS NOT NULL\r\n    BEGIN\r\n        INSERT INTO Contacts (PersonId, ClientAccountId, ContactType)\r\n" +
            "        VALUES (@PersonId, @ClientAccountId, 'expense');\r\n" +
            "    END\r\n\r\n" +
            "    -- Check and insert Approver if it does not exist\r\n" +
            "    IF NOT EXISTS (SELECT 1 FROM People WHERE DisplayName = @Approver)\r\n" +
            "    BEGIN\r\n" +
            "        INSERT INTO People (Name, DisplayName, Mail) -- Adjust fields as necessary\r\n" +
            "        VALUES (@Approver, @Approver, NULL); -- Assuming Mail is NULL or you have a value\r\n" +
            "    END\r\n" +
            "    SELECT @PersonId = Id FROM People WHERE DisplayName = @Approver;\r\n" +
            "    IF @PersonId IS NOT NULL\r\n" +
            "    BEGIN\r\n" +
            "        INSERT INTO Contacts (PersonId, ClientAccountId, ContactType)\r\n" +
            "        VALUES (@PersonId, @ClientAccountId, 'approver');\r\n" +
            "    END\r\n\r\n" +
            "    -- Check and insert FinancialContact if it does not exist\r\n" +
            "    IF NOT EXISTS (SELECT 1 FROM People WHERE DisplayName = @FinancialContact)\r\n" +
            "    BEGIN\r\n" +
            "        INSERT INTO People (Name, DisplayName, Mail) -- Adjust fields as necessary\r\n" +
            "        VALUES (@FinancialContact, @FinancialContact, NULL); -- Assuming Mail is NULL or you have a value\r\n" +
            "    END\r\n" +
            "    SELECT @PersonId = Id FROM People WHERE DisplayName = @FinancialContact;\r\n" +
            "    IF @PersonId IS NOT NULL\r\n" +
            "    BEGIN\r\n" +
            "        INSERT INTO Contacts (PersonId, ClientAccountId, ContactType)\r\n" +
            "        VALUES (@PersonId, @ClientAccountId, 'financial');\r\n" +
            "    END\r\n\r\n" +
            "    -- Check and insert PrimaryContact if it does not exist\r\n" +
            "    IF NOT EXISTS (SELECT 1 FROM People WHERE DisplayName = @PrimaryContact)\r\n" +
            "    BEGIN\r\n" +
            "        INSERT INTO People (Name, DisplayName, Mail) -- Adjust fields as necessary\r\n" +
            "        VALUES (@PrimaryContact, @PrimaryContact, NULL); -- Assuming Mail is NULL or you have a value\r\n" +
            "    END\r\n" +
            "    SELECT @PersonId = Id FROM People WHERE DisplayName = @PrimaryContact;\r\n" +
            "    IF @PersonId IS NOT NULL\r\n" +
            "    BEGIN\r\n" +
            "        INSERT INTO Contacts (PersonId, ClientAccountId, ContactType)\r\n" +
            "        VALUES (@PersonId, @ClientAccountId, 'primary');\r\n" +
            "    END\r\n\r\n" +
            "    FETCH NEXT FROM ClientAccountCursor INTO @ClientAccountId, @ExpenseAuthorityName, @Approver, @FinancialContact, @PrimaryContact;\r\n" +
            "END\r\n\r\n" +
            "CLOSE ClientAccountCursor;\r\n" +
            "DEALLOCATE ClientAccountCursor;\r\n\r\n" +
            "COMMIT TRANSACTION;";

            migrationBuilder.Sql(query);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete from Contacts");
            migrationBuilder.Sql("Delete from People");
        }
    }
}
