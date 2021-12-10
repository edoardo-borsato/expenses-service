using ExpensesService.Models;

namespace ExpensesService.Registries
{
    public record FilterParameters
    {
        public string From { get; set; }
        public string To { get; set; }
        public string In { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
    }
}