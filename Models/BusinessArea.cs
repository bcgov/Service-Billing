namespace Service_Billing.Models
{
    public class BusinessArea
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Acronym { get; set; } = string.Empty;

        public virtual ICollection<ServiceCategory> Categories { get; set; }
    }
}
