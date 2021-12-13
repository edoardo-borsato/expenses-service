using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExpensesService.Models
{
    public record ExpenseDetails
    {
        [Required]
        [JsonPropertyName("value")]
        public double Value { get; init; }

        [JsonPropertyName("date")]
        public DateTimeOffset? Date { get; init; }

        [Required]
        [JsonPropertyName("reason")]
        public string Reason { get; init; }

        [JsonPropertyName("paymentMethod")]
        public PaymentMethod? PaymentMethod { get; init; }
    }
}