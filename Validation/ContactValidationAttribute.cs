namespace Service_Billing.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Models;

    public class ContactValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var contacts = value as ICollection<Contact>;

            //if (contacts == null || !contacts.Any())
            //{
            //    return new ValidationResult("At least one contact is required.");
            //}

            var primaryContacts = contacts.Where(c => c.ContactType == "primary").ToList();
            var approverContacts = contacts.Where(c => c.ContactType == "approver").ToList();
            var financialContacts = contacts.Where(c => c.ContactType == "financial").ToList();

            if (primaryContacts.Count < 1)
            {
                return new ValidationResult("Please include a primary contact");
            }
            if(primaryContacts.Count > 2)
            {
                return new ValidationResult("There may be no more than two primary contacts");
            }

            if (!approverContacts.Any())
            {
                return new ValidationResult("Please provide at least one approver contact");
            }
            if(!financialContacts.Any())
            {
                return new ValidationResult("Please provide at least one financial contact");
            }

            return ValidationResult.Success;
        }
    }
}
