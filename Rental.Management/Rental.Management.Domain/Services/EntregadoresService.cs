using Rental.Management.Domain.Entities;
using Rental.Management.Domain.Interfaces.Repositories;
using Rental.Management.Domain.Interfaces.Services;

namespace Rental.Management.Domain.Services;

public class EntregadoresService(IDynamoDbRepository<EntregadorRequest> repository, IS3Repository s3Repository) : IEntregadoresService
{
    private readonly IDynamoDbRepository<EntregadorRequest> _repository = repository;
    private readonly IS3Repository _s3Repository = s3Repository;

    public async Task<bool> ExistsByFilterAsync(string filter, string filterValue)
    {
        var result = await _repository.GetByFilterAsync(filter, filterValue);
        return result.Any();
    }

    public async Task<EntregadorRequest> InsertEntregadorAsync(EntregadorRequest motorcycle)
    {
        await _repository.SaveAsync(motorcycle);
        return motorcycle;
    }

    public async Task<EntregadorRequest> GetEntregadorByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UploadBase64Async(string fileName, string base64)
    {
        return await _s3Repository.UploadBase64Async(fileName, base64);
    }

    public async Task<bool> DownloadBase64(string fileName)
    {
        return await _s3Repository.DownloadImageToFileAsync(fileName);
    }
}
