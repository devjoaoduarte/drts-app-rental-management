using Rental.Management.Domain.Entities;
using Rental.Management.Domain.Interfaces.Repositories;
using Rental.Management.Domain.Interfaces.Services;

namespace Rental.Management.Domain.Services;

public class EntregadoresService(IDynamoDbRepository<DeliveryMenRequest> repository, IS3Repository s3Repository) : IDeliveryMenService
{
    private readonly IDynamoDbRepository<DeliveryMenRequest> _repository = repository;
    private readonly IS3Repository _s3Repository = s3Repository;

    public async Task<bool> ExistsByFilterAsync(string filter, string filterValue)
    {
        var result = await _repository.GetByFilterAsync(filter, filterValue);
        return result.Any();
    }

    public async Task<DeliveryMenRequest> InsertDeliveryMenAsync(DeliveryMenRequest motorcycle)
    {
        await _repository.SaveAsync(motorcycle);
        return motorcycle;
    }

    public async Task<DeliveryMenRequest> GetDeliveryMenByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UploadBase64BucketAsync(string fileName, string base64)
    {
        return await _s3Repository.UploadBase64Async(fileName, base64);
    }
}
