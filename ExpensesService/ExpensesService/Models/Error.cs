using System.Text.Json.Serialization;

namespace ExpensesService.Models
{
    public record Error
    {
        [JsonPropertyName("error")]
        public string ErrorMessage { get; set; }
    }
}
