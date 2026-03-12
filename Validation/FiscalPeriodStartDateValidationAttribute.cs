using System.ComponentModel.DataAnnotations;
using Service_Billing.Models.Repositories;

namespace Service_Billing.Validation
{
    public class FiscalPeriodStartDateValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Allow null values, use [Required] separately if needed
            }

            if (value is not DateTimeOffset startDate)
            {
                return new ValidationResult("Invalid date format.");
            }

            // Get the service provider from ValidationContext
            var serviceProvider = validationContext.GetService<IServiceProvider>();
            if (serviceProvider == null)
            {
                return new ValidationResult("Unable to validate fiscal period - service provider not available.");
            }

            // Get the BillRepository from the service provider
            var billRepository = serviceProvider.GetService(typeof(IBillRepository)) as IBillRepository;
            if (billRepository == null)
            {
                return new ValidationResult("Unable to validate fiscal period - repository not available.");
            }

            try
            {
                DateTimeOffset currentQuarterStart = billRepository.DetermineStartOfCurrentQuarter();
                DateTimeOffset nextQuarterStart = billRepository.DetermineStartOfNextQuarter();
                DateTimeOffset quarterAfterNextStart = GetQuarterAfterNext(nextQuarterStart);

                // Allow dates from the start of current quarter up to (but not including) the quarter after next
                if (startDate >= currentQuarterStart && startDate < quarterAfterNextStart)
                {
                    return ValidationResult.Success;
                }

                string currentQuarter = billRepository.DetermineCurrentQuarter();
                string nextQuarter = billRepository.DetermineCurrentQuarter(nextQuarterStart.DateTime);

                return new ValidationResult(
                    $"Start date must be within the current fiscal period ({currentQuarter}) or the next fiscal period ({nextQuarter}). " +
                    $"Please contact an administrator if you need to create a charge for a future period."
                );
            }
            catch (Exception ex)
            {
                // Log the exception if possible
                return new ValidationResult($"Unable to validate fiscal period: {ex.Message}");
            }
        }

        private DateTimeOffset GetQuarterAfterNext(DateTimeOffset nextQuarterStart)
        {
            // Add 3 months to get the start of the quarter after next
            int month = nextQuarterStart.Month;
            int year = nextQuarterStart.Year;

            // Determine the start month of the quarter after next
            int quarterAfterNextMonth = month switch
            {
                4 => 7,   // Q1 -> Q2
                7 => 10,  // Q2 -> Q3
                10 => 1,  // Q3 -> Q4 (next calendar year)
                1 => 4,   // Q4 -> Q1
                _ => throw new InvalidOperationException($"Invalid quarter start month: {month}")
            };

            // Handle year rollover for Q3 -> Q4
            if (month == 10)
            {
                year++;
            }

            return new DateTimeOffset(year, quarterAfterNextMonth, 1, 0, 0, 0, nextQuarterStart.Offset);
        }
    }
}