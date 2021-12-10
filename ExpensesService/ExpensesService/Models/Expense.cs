using System;
using System.Text.Json.Serialization;

namespace ExpensesService.Models
{
    public record Expense
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; init; }

        [JsonPropertyName("details")]
        public ExpenseDetails ExpenseDetails { get; init; }
    }
}