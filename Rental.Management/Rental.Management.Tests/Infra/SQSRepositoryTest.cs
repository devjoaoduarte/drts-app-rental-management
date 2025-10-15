using Amazon.SQS.Model;
using Amazon.SQS;
using Microsoft.Extensions.Logging;
using Rental.Management.Infra.Repositories;

namespace Rental.Management.Tests.Infra;

public class SQSRepositoryTest
{
    private readonly Mock<IAmazonSQS> _sqsClientMock;
    private readonly Mock<ILogger<SQSRepository>> _loggerMock;
    private readonly SQSRepository _repository;

    public SQSRepositoryTest()
    {
        _sqsClientMock = new Mock<IAmazonSQS>();
        _loggerMock = new Mock<ILogger<SQSRepository>>();
        _repository = new SQSRepository(_sqsClientMock.Object, _loggerMock.Object);
    }

    [Fact(DisplayName = "SendMessageAsync should send message successfully when queue exists")]
    public async Task SendMessageAsync_ShouldSendMessageSuccessfully_WhenQueueExists()
    {
        // Arrange
        string queueName = "motos.fifo";
        string queueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/motos.fifo";
        var testMessage = new { Id = 1, Name = "Moto Test" };

        _sqsClientMock
            .Setup(s => s.ListQueuesAsync(It.IsAny<ListQueuesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListQueuesResponse
            {
                QueueUrls = new List<string> { queueUrl }
            });

        _sqsClientMock
            .Setup(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SendMessageResponse());

        // Act
        await _repository.SendMessageAsync(queueName, testMessage);

        // Assert
        _sqsClientMock.Verify(s => s.ListQueuesAsync(It.IsAny<ListQueuesRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.VerifyLog(LogLevel.Information, "Iniciado envio da mensagem.");
        _loggerMock.VerifyLog(LogLevel.Information, $"Mensagem enviada para a fila '{queueName}'");
    }

    [Fact(DisplayName = "SendMessageAsync should throw exception when queue not found")]
    public async Task SendMessageAsync_ShouldThrowException_WhenQueueNotFound()
    {
        // Arrange
        string queueName = "notfound.fifo";
        var testMessage = new { Id = 2, Name = "Not Found" };

        _sqsClientMock
            .Setup(s => s.ListQueuesAsync(It.IsAny<ListQueuesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListQueuesResponse
            {
                QueueUrls = new List<string>() // Nenhuma fila
            });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.SendMessageAsync(queueName, testMessage));

        Assert.Equal($"Fila '{queueName}' não encontrada.", ex.Message);

        _loggerMock.VerifyLog(LogLevel.Error, $"Fila não encontrada. '{queueName}'");
        _sqsClientMock.Verify(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "SendMessageAsync should log and serialize message correctly")]
    public async Task SendMessageAsync_ShouldSerializeMessageAndLogProperly()
    {
        // Arrange
        string queueName = "motos.fifo";
        string queueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/motos.fifo";
        var testMessage = new { Id = 99, Value = "Test123" };

        _sqsClientMock
            .Setup(s => s.ListQueuesAsync(It.IsAny<ListQueuesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListQueuesResponse
            {
                QueueUrls = new List<string> { queueUrl }
            });

        _sqsClientMock
            .Setup(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SendMessageResponse());

        // Act
        await _repository.SendMessageAsync(queueName, testMessage);

        // Assert
        _loggerMock.VerifyLog(LogLevel.Information, "Iniciado envio da mensagem.");
        _loggerMock.VerifyLog(LogLevel.Information, $"Mensagem enviada para a fila '{queueName}'");
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