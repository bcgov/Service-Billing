using Service_Billing.Models;
using Service_Billing.Models.Repositories;
using Service_Billing.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace Service_Billing.Validation
{
    public class ClientNameValidation: ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext )
        {
            try
            {
                IClientAccountRepository? clientAccountsRepository = validationContext.GetService(typeof(IClientAccountRepository)) as IClientAccountRepository;
                if( clientAccountsRepository != null)
                {
                    ClientAccount account = (ClientAccount)validationContext.ObjectInstance;
                    if(account.OrganizationId.HasValue )
                    {
                        if (clientAccountsRepository.GetAccountByName(account.Name) == null)
                        {
                            return ValidationResult.Success;
                        }
                        else
                            return new ValidationResult($"There already exists a Client Account with the name {account.Name}. Please try a different division or branch name");
                    }
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult($"An error has occurred. Exception message: {ex.Message}");
            }
            return ValidationResult.Success;
        }
    }
}
