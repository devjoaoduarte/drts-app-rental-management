using System.Text.Json.Serialization;

namespace Rental.Management.Domain.Entities;

public class DeliveryMenCnhUploadRequest
{
    [JsonPropertyName("imagem_cnh")]
    public string ImagemCnh { get; set; }
}
