using Amazon.DynamoDBv2.DataModel;
using System.Text.Json.Serialization;

namespace Rental.Management.Domain.Entities;

[DynamoDBTable("Motos")]
public class MotorcycleRequest
{
    [DynamoDBHashKey]
    [JsonPropertyName("identificador")]
    public string Identificador { get; set; }

    [DynamoDBProperty]
    [JsonPropertyName("ano")]
    public int Ano { get; set; }

    [DynamoDBProperty]
    [JsonPropertyName("modelo")]
    public string Modelo { get; set; }

    [DynamoDBProperty]
    [JsonPropertyName("placa")]
    public string Placa { get; set; }
}
