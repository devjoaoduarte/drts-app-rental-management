using Amazon.SQS;
using Amazon.SQS.Model;
using Rental.Management.Domain.Interfaces.Repositories;
using System.Text.Json;

namespace Rental.Management.Infra.Repositories;

public class SQSRepository(IAmazonSQS sqsClient) : ISQSRepository
{
    private readonly IAmazonSQS _sqsClient = sqsClient;

    public async Task SendMessageAsync<T>(string queueName, T message)
    {
        var queues = await _sqsClient.ListQueuesAsync(new ListQueuesRequest());
        var queueUrl = queues.QueueUrls.FirstOrDefault(q => q.EndsWith(queueName));

        if (queueUrl == null)
            throw new InvalidOperationException($"Fila '{queueName}' não encontrada.");

        var json = JsonSerializer.Serialize(message);

        var sendRequest = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = json,
            MessageGroupId = "motos-group",
            MessageDeduplicationId = Guid.NewGuid().ToString()
        };

        await _sqsClient.SendMessageAsync(sendRequest);
        Console.WriteLine($"Mensagem enviada para a fila '{queueName}'");
    }
}
