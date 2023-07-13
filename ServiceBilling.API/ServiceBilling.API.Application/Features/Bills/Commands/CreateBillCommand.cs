using MediatR;

namespace ServiceBilling.API.Application.Features.Bills.Commands
{
    public class CreateBillCommand : IRequest<Guid>
    {
        public Guid Id { get; set; }
        // public Guid ClientAccountId { get; set; }
        public string Title { get; set; }
        public string IdirOrUrl { get; set; }

        // public Int16? serviceCategoryId { get; set; }

        public decimal Amount { get; set; }

        public string FiscalPeriod { get; set; }
        //  public decimal Unit_Price; //comes from service category lookup
        public decimal Quantity { get; set; }

        public string TicketNumber { get; set; }
        // public string Requester { get; set; }
        // public DateTime? billingCycle { get; set; }
    }
}
