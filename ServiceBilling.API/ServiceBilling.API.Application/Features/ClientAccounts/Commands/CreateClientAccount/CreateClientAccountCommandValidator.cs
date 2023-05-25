using FluentValidation;

namespace ServiceBilling.API.Application.Features.ClientAccounts.Commands.CreateClientAccount
{
    public class CreateClientAccountCommandValidator : AbstractValidator<CreateClientAccountCommand>
    {
        public CreateClientAccountCommandValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed 50 characters.");

            RuleFor(p => p.Number)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .GreaterThanOrEqualTo(0);
        }
    }
}
