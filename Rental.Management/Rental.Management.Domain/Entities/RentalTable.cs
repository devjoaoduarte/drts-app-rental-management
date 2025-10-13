using Amazon.DynamoDBv2.DataModel;
using System.Text.Json.Serialization;

namespace Rental.Management.Domain.Entities;

[DynamoDBTable("Locacoes")]
public class RentalTable
{
    [DynamoDBHashKey]
    [JsonPropertyName("identificador")]
    public string Identificador { get; set; }
    [DynamoDBProperty]
    [JsonPropertyName("valor_diaria")]
    public int ValorDiaria { get; set; }
    [DynamoDBProperty]
    [JsonPropertyName("entregador_id")]
    public string EntregadorId { get; set; }
    [DynamoDBProperty]
    [JsonPropertyName("moto_id")]
    public string MotoId { get; set; }
    [DynamoDBProperty]
    [JsonPropertyName("data_inicio")]
    public DateTime DataInicio { get; set; }
    [DynamoDBProperty]
    [JsonPropertyName("data_termino")]
    public DateTime DataTermino { get; set; }
    [DynamoDBProperty]
    [JsonPropertyName("data_previsao_termino")]
    public DateTime DataPrevisaoTermino { get; set; }
    [DynamoDBProperty]
    [JsonPropertyName("data_devolucao")]
    public DateTime? data_devolucao { get; set; }
}