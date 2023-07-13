using AutoMapper;
using MediatR;
using ServiceBilling.API.Application.Contracts.Persistence;
using ServiceBilling.API.Domain.Entities;

namespace ServiceBilling.API.Application.Features.Bills.Commands
{
    public class CreateBillCommandHandler : IRequestHandler<CreateBillCommand, Guid>
    {
        private readonly IAsyncRepository<Bill> _billRepository;
        private readonly IMapper _mapper;

        public CreateBillCommandHandler(IAsyncRepository<Bill> billRepository,
            IMapper mapper)
        {
            _billRepository = billRepository;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateBillCommand request, CancellationToken cancellationToken)
        {
            var bill = _mapper.Map<Bill>(request);

            await _billRepository.AddAsync(bill);

            return bill.Id;
        }
    }
}
