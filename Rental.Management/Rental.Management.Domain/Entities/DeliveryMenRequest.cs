using Amazon.DynamoDBv2.DataModel;
using System.Text.Json.Serialization;

namespace Rental.Management.Domain.Entities;

[DynamoDBTable("Entregadores")]
public class DeliveryMenRequest
{
    [DynamoDBHashKey]
    [JsonPropertyName("identificador")]
    public string Identificador { get; set; }
    
    [DynamoDBProperty]
    [JsonPropertyName("nome")]
    public string Nome { get; set; }
    
    [DynamoDBProperty]
    [JsonPropertyName("cnpj")]
    public string Cnpj { get; set; }
    
    [DynamoDBProperty]
    [JsonPropertyName("data_nascimento")]
    public DateTime DataNascimento { get; set; }
    
    [DynamoDBProperty]
    [JsonPropertyName("numero_cnh")]
    public string NumeroCnh { get; set; }
    
    [DynamoDBProperty]
    [JsonPropertyName("tipo_cnh")]
    public string TipoCnh { get; set; }
    
    [DynamoDBProperty]
    [JsonPropertyName("imagem_cnh")]
    public string ImagemCnh { get; set; }
}