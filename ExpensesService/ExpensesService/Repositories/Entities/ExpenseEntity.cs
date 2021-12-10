using System;
using System.Text.Json.Serialization;

namespace ExpensesService.Repositories.Entities
{
    public record ExpenseEntity
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("value")]
        public double Value { get; init; }

        [JsonPropertyName("date")]
        public DateTimeOffset? Date { get; init; }

        [JsonPropertyName("reason")]
        public string Reason { get; init; }

        [JsonPropertyName("paymentMethod")]
        public PaymentMethod PaymentMethod { get; init; }
    }

    public enum PaymentMethod
    {
        Undefined = 0,
        Cash = 1,
        DebitCard = 2,
        CreditCard = 3
    }
}
