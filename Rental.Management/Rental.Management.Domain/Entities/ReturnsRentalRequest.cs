using System.Text.Json.Serialization;

namespace Rental.Management.Domain.Entities;

public class ReturnsRentalRequest
{
    [JsonPropertyName("data_devolucao")]
    public DateTime DataDevolucao { get; set; }
}
