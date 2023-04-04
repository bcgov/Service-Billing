namespace Service_Billing.Reports
{
    public class ReportGenerator
    {
        /* 1.	The system must generate an exportable quarterly billing of all Client Accounts for all active services, charges & fees (or no longer active) to be invoiced for the past quarter.
                a.	The system should include all billable services logged and tracked in a ticketing system during the past quarter.
                b.	The system should organize or group quarterly billing cycle charges by the same Client Account coding.
                c.	The system should auto-generate an email to GDX financial staff with the past quarter’s Client Account billing when ready for invoicing.
                as 
        */
        public void GenerateQuarterlyReport(int clientId, DateTime? fromDate)
        {

        }
    }
}
