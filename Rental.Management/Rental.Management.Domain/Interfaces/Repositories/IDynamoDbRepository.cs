namespace Rental.Management.Domain.Interfaces.Repositories;

public interface IDynamoDbRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetByFilterAsync(string filter, string ValueFilter);
    Task SaveAsync(T item);
    Task UpdateAsync(T item);
    Task DeleteAsync(string id);
}
