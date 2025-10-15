using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

namespace Rental.Management.App.Consumers;

public class MotorcycleQueueConsumerWorker(IAmazonSQS sqsClient, IDynamoDbRepository<MotorcycleNotificationsTable> repository, ILogger<MotorcycleQueueConsumerWorker> logger) : BackgroundService
{
    private readonly IAmazonSQS _sqsClient = sqsClient;
    private readonly IDynamoDbRepository<MotorcycleNotificationsTable> _repository = repository;
    private readonly ILogger<MotorcycleQueueConsumerWorker> _logger = logger;
    private const string QueueName = "motos.fifo";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker consumo notificação motos iniciado.");
        var queueUrlResponse = await _sqsClient.GetQueueUrlAsync(QueueName);
        var queueUrl = queueUrlResponse.QueueUrl;

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
                _logger.LogInformation($"Número de mensagens coletadas: {response.Messages.Count}");

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
                        _logger.LogError("Ocorreu um erro ao processar a mensagem.", ex.Message);
                    }
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
