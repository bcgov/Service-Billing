using AutoMapper;
using MediatR;
using ServiceBilling.API.Application.Contracts.Infrastructure;
using ServiceBilling.API.Application.Contracts.Persistence;
using ServiceBilling.API.Application.Models.Mail;
using ServiceBilling.API.Domain.Entities;

namespace ServiceBilling.API.Application.Features.Bills.Commands
{
    public class CreateBillCommandHandler : IRequestHandler<CreateBillCommand, Guid>
    {
        private readonly IAsyncRepository<Bill> _billRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public CreateBillCommandHandler(
            IAsyncRepository<Bill> billRepository,
            IMapper mapper,
            IEmailService emailService)
        {
            _billRepository = billRepository;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<Guid> Handle(CreateBillCommand request, CancellationToken cancellationToken)
        {
            var bill = _mapper.Map<Bill>(request);

            try
            {
                await _emailService.SendEmail(new Email { To = "", Body = "", Subject = "" });
            }
            catch (Exception)
            {

                throw;
            }
           

            await _billRepository.AddAsync(bill);

            return bill.Id;
        }
    }
}
