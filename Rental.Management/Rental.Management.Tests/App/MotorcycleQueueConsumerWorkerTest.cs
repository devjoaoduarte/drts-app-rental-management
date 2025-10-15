using Amazon.SQS.Model;
using Amazon.SQS;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Rental.Management.Tests.App;

public class MotorcycleQueueConsumerWorkerTest
{
    private readonly Mock<IAmazonSQS> _sqsMock;
    private readonly Mock<IDynamoDbRepository<MotorcycleNotificationsTable>> _repoMock;
    private readonly Mock<ILogger<MotorcycleQueueConsumerWorker>> _loggerMock;

    public MotorcycleQueueConsumerWorkerTest()
    {
        _sqsMock = new Mock<IAmazonSQS>();
        _repoMock = new Mock<IDynamoDbRepository<MotorcycleNotificationsTable>>();
        _loggerMock = new Mock<ILogger<MotorcycleQueueConsumerWorker>>();
    }

    [Fact(DisplayName = "ExecuteAsync should process messages and save notification when year is 2024")]
    public async Task ExecuteAsync_ShouldSaveNotification_WhenMotorcycleYearIs2024()
    {
        // Arrange
        var queueUrl = "https://sqs.us-east-1.amazonaws.com/123456789/motos.fifo";
        var motoMessage = new MotorcycleRequest { Modelo = "Yamaha", Placa = "ABC1234", Ano = 2024 };
        var messageBody = JsonSerializer.Serialize(motoMessage);

        _sqsMock
            .Setup(s => s.GetQueueUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetQueueUrlResponse { QueueUrl = queueUrl });

        _sqsMock
            .SetupSequence(s => s.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReceiveMessageResponse
            {
                Messages = new List<Message> { new Message { Body = messageBody, ReceiptHandle = "RH1" } }
            })
            .ReturnsAsync(new ReceiveMessageResponse { Messages = new List<Message>() });

        _sqsMock
            .Setup(s => s.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteMessageResponse());

        var worker = new MotorcycleQueueConsumerWorker(_sqsMock.Object, _repoMock.Object, _loggerMock.Object);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(1000); // cancela após 1s para não travar o teste

        // Act
        await worker.StartAsync(cts.Token);

        // Assert
        _repoMock.Verify(r => r.SaveAsync(It.Is<MotorcycleNotificationsTable>(
            n => n.Mensagem.Contains("Yamaha") && n.Body == messageBody
        )), Times.Once);

        _sqsMock.Verify(s => s.DeleteMessageAsync(queueUrl, "RH1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "ExecuteAsync should not save notification when year is not 2024")]
    public async Task ExecuteAsync_ShouldNotSaveNotification_WhenMotorcycleYearIsNot2024()
    {
        // Arrange
        var queueUrl = "https://sqs.us-east-1.amazonaws.com/123456789/motos.fifo";
        var motoMessage = new MotorcycleRequest { Modelo = "Honda", Placa = "XYZ9999", Ano = 2023 };
        var messageBody = JsonSerializer.Serialize(motoMessage);

        _sqsMock
            .Setup(s => s.GetQueueUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetQueueUrlResponse { QueueUrl = queueUrl });

        _sqsMock
            .SetupSequence(s => s.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReceiveMessageResponse
            {
                Messages = new List<Message> { new Message { Body = messageBody, ReceiptHandle = "RH2" } }
            })
            .ReturnsAsync(new ReceiveMessageResponse { Messages = new List<Message>() });

        _sqsMock
            .Setup(s => s.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteMessageResponse());

        var worker = new MotorcycleQueueConsumerWorker(_sqsMock.Object, _repoMock.Object, _loggerMock.Object);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(1000);

        // Act
        await worker.StartAsync(cts.Token);

        // Assert
        _repoMock.Verify(r => r.SaveAsync(It.IsAny<MotorcycleNotificationsTable>()), Times.Never);
        _sqsMock.Verify(s => s.DeleteMessageAsync(queueUrl, "RH2", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "ExecuteAsync should log error when message processing fails")]
    public async Task ExecuteAsync_ShouldLogError_WhenMessageProcessingFails()
    {
        // Arrange
        var queueUrl = "https://sqs.us-east-1.amazonaws.com/123456789/motos.fifo";
        var invalidMessage = "invalid-json";

        _sqsMock
            .Setup(s => s.GetQueueUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetQueueUrlResponse { QueueUrl = queueUrl });

        _sqsMock
            .SetupSequence(s => s.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReceiveMessageResponse
            {
                Messages = new List<Message> { new Message { Body = invalidMessage, ReceiptHandle = "RH3" } }
            })
            .ReturnsAsync(new ReceiveMessageResponse { Messages = new List<Message>() });

        _sqsMock
            .Setup(s => s.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteMessageResponse());

        var worker = new MotorcycleQueueConsumerWorker(_sqsMock.Object, _repoMock.Object, _loggerMock.Object);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(1000);

        // Act
        await worker.StartAsync(cts.Token);

        // Assert
        _loggerMock.VerifyLog(LogLevel.Error, "Ocorreu um erro ao processar a mensagem.");
        _repoMock.Verify(r => r.SaveAsync(It.IsAny<MotorcycleNotificationsTable>()), Times.Never);
        _sqsMock.Verify(s => s.DeleteMessageAsync(queueUrl, "RH3", It.IsAny<CancellationToken>()), Times.Never);
    }
}

internal static class LoggerExtensions
{
    public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel level, string containsMessage)
    {
        loggerMock.Verify(l =>
            l.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains(containsMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }
}