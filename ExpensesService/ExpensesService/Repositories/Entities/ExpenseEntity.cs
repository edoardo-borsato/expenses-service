using Newtonsoft.Json;

namespace ExpensesService.Repositories.Entities
{
    // Apparently [JsonPropertyName("<name>")] does not work, but [JsonProperty(PropertyName = "<name>")] does the job
    public record ExpenseEntity
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; init; }

        [JsonProperty(PropertyName = "value")]
        public double Value { get; set; }

        [JsonProperty(PropertyName = "date")]
        public string Date { get; set; }

        [JsonProperty(PropertyName = "reason")]
        public string Reason { get; set; }

        [JsonProperty(PropertyName = "paymentMethod")]
        public PaymentMethod PaymentMethod { get; set; }
    }

    public enum PaymentMethod
    {
        Undefined = 0,
        Cash = 1,
        DebitCard = 2,
        CreditCard = 3
    }
}
