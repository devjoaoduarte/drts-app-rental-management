
using Amazon.DynamoDBv2.DataModel;

namespace Rental.Management.Domain.Entities;


[DynamoDBTable("MotosNotificacoes")]
public class MotorcycleNotificationsTable
{
    [DynamoDBHashKey]
    public string Identificador { get; set; }
    [DynamoDBProperty]
    public string Mensagem { get; set; }
    [DynamoDBProperty]
    public string Body { get; set; }
}
