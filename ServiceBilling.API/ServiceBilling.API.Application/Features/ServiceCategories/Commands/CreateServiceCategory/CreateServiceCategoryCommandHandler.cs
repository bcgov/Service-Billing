using AutoMapper;
using MediatR;
using ServiceBilling.API.Application.Contracts.Infrastructure;
using ServiceBilling.API.Application.Contracts.Persistence;
using ServiceBilling.API.Application.Models.Mail;
using ServiceBilling.API.Domain.Entities;

namespace ServiceBilling.API.Application.Features.ServiceCategories.Commands.CreateServiceCategory
{
    public class CreateServiceCategoryCommandHandler : IRequestHandler<CreateServiceCategoryCommand, Guid>
    {
        private readonly IAsyncRepository<ServiceCategory> _serviceCategoryRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public CreateServiceCategoryCommandHandler(
            IMapper mapper,
            IAsyncRepository<ServiceCategory> serviceCategoryRepository,
            IEmailService emailService)
        {
            _mapper = mapper;
            _serviceCategoryRepository = serviceCategoryRepository;
            _emailService = emailService;
        }

        public async Task<Guid> Handle(CreateServiceCategoryCommand request, CancellationToken cancellationToken)
        {
            var createCategoryCommandResponse = new CreateServiceCategoryCommandResponse();

            //var validator = new CreateCategoryCommandValidator();
            //var validationResult = await validator.ValidateAsync(request);

            //if (validationResult.Errors.Count > 0)
            //{
            //    createCategoryCommandResponse.Success = false;
            //    createCategoryCommandResponse.ValidationErrors = new List<string>();
            //    foreach (var error in validationResult.Errors)
            //    {
            //        createCategoryCommandResponse.ValidationErrors.Add(error.ErrorMessage);
            //    }
            //}
            //if (createCategoryCommandResponse.Success)
            //{
            //var category = new Category() { Name = request.Name };
            //category = await _categoryRepository.AddAsync(category);
            //createCategoryCommandResponse.Category = _mapper.Map<CreateCategoryDto>(category);
            //}

            var serviceCategory = new ServiceCategory()
            {
                Name = request.Name,
                Costs = request.Costs,
                GdxBusinessArea = request.GdxBusinessArea,
                CreatedBy = "Test",
                CreatedDate = DateTime.Now,
                LastModifiedBy = "Test",
                LastModifiedDate = DateTime.Now
            };

            try
            {
                await _emailService.SendEmail(new Email { To = "andre.lashley@gmail.com", Body = "A new service category was created!", Subject = "A new service category was created" });
            }
            catch (Exception ex)
            {
            }

            serviceCategory = await _serviceCategoryRepository.AddAsync(serviceCategory);

            return serviceCategory.ServiceCategoryId;
        }
    }
}
