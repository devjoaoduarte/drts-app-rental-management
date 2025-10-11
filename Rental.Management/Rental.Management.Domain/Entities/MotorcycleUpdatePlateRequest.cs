using System.Text.Json.Serialization;

namespace Rental.Management.Domain.Entities;

public class MotorcycleUpdatePlateRequest
{
    [JsonPropertyName("placa")]
    public string Plate { get; set; }
}
