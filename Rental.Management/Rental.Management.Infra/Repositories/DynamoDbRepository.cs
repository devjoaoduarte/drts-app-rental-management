using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Rental.Management.Domain.Interfaces.Repositories;
using Rental.Management.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Rental.Management.Infra.Repositories;

[ExcludeFromCodeCoverage]
public class DynamoDbRepository<T> : IDynamoDbRepository<T> where T : class
{
    private readonly DynamoDBContext _context;
    private readonly IAmazonDynamoDB _client;
    private readonly string _tableName;

    public DynamoDbRepository(IAmazonDynamoDB client)
    {
        _client = client;
        _context = new DynamoDBContext(client);
        _tableName = typeof(T).Name switch
        {
            nameof(DeliveryMenRequest) => "Entregadores",
            nameof(MotorcycleRequest) => "Motos",
            nameof(RentalTable) => "Locacoes",
            nameof(MotorcycleNotificationsTable) => "MotosNotificacoes",
            _ => throw new InvalidOperationException($"Tabela não configurada para {typeof(T).Name}")
        };
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        return await _context.LoadAsync<T>(id);
    }

    public async Task<IEnumerable<T>> GetByFilterAsync(string filter, string ValueFilter)
    {
        var conditions = new List<ScanCondition>()
        {
            new(filter, ScanOperator.Equal, ValueFilter)
        };
        return await _context.ScanAsync<T>(conditions).GetRemainingAsync();
    }

    public async Task SaveAsync(T item) => await _context.SaveAsync(item);

    public async Task UpdateAsync(T item) => await _context.SaveAsync(item);

    public async Task DeleteAsync(string id) => await _context.DeleteAsync<T>(id);
}
