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

            if (contacts == null || !contacts.Any())
            {
                return new ValidationResult("At least one contact is required.");
            }

            var primaryContacts = contacts.Where(c => c.ContactType == "primary").ToList();
            var approverContacts = contacts.Where(c => c.ContactType == "approver").ToList();
            var financialContacts = contacts.Where(c => c.ContactType == "financial").ToList();

            if (primaryContacts.Count < 1)
            {
                return new ValidationResult("Please include a primary contact");
            }
            if(primaryContacts.Count > 2)
            {
                return new ValidationResult("There may be no more than two primart contacts");
            }

            if (!approverContacts.Any() && !financialContacts.Any())
            {
                return new ValidationResult("At least one contact of type 'approver' or 'financial' is required.");
            }

            return ValidationResult.Success;
        }
    }
}
