namespace Rental.Management.Domain.Interfaces.Repositories;

public interface ISQSRepository
{
    Task SendMessageAsync<T>(string queueName, T message);
}
