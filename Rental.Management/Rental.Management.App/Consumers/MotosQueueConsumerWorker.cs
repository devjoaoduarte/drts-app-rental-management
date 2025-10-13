using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

namespace Rental.Management.App.Consumers;

public class MotosQueueConsumerWorker(IAmazonSQS sqsClient, IDynamoDbRepository<MotorcycleNotificationsTable> repository) : BackgroundService
{
    private readonly IAmazonSQS _sqsClient = sqsClient;
    private readonly IDynamoDbRepository<MotorcycleNotificationsTable> _repository = repository;
    private const string QueueName = "motos.fifo";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueUrlResponse = await _sqsClient.GetQueueUrlAsync(QueueName);
        var queueUrl = queueUrlResponse.QueueUrl;

        Console.WriteLine($"Consumidor da fila '{QueueName}' iniciado...");

        while (!stoppingToken.IsCancellationRequested)
        {
            var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 5,
                WaitTimeSeconds = 10
            });

            if (response.Messages != null)
            {
                foreach (var message in response.Messages)
                {
                    try
                    {
                        var moto = JsonSerializer.Deserialize<MotorcycleRequest>(message.Body);

                        if(moto.Ano == 2024)
                        {
                            var mensagem = $"A moto {moto.Modelo} - {moto.Placa} foi cadastrada e é do ano {moto.Ano}.";
                            await _repository.SaveAsync(new MotorcycleNotificationsTable() { Identificador = Guid.NewGuid().ToString(), Mensagem = mensagem, Body = message.Body });
                        }

                        await _sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");
                    }
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
