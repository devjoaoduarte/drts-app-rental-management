using Rental.Management.Domain.Entities;
using Rental.Management.Domain.Interfaces.Repositories;
using Rental.Management.Domain.Interfaces.Services;

namespace Rental.Management.Domain.Services;

public class MotorcycleService(IDynamoDbRepository<MotorcycleRequest> repository) : IMotorcycleServices
{
    private readonly IDynamoDbRepository<MotorcycleRequest> _repository = repository;

    public async Task<MotorcycleRequest> InsertMotorcycleAsync(MotorcycleRequest motorcycle)
    {
        await _repository.SaveAsync(motorcycle);
        return motorcycle;
    }

    public async Task<IEnumerable<MotorcycleRequest>> GetMotorcycleByPlateAsync(string plate)
    {
        return await _repository.GetByFilterAsync("Placa", plate);
    }

    public async Task<MotorcycleRequest> GetMotorcycleByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> UpdatePlateMotorcycleAsync(MotorcycleRequest motorcycle, string newPlate)
    {
        motorcycle.Placa = newPlate;
        await _repository.UpdateAsync(motorcycle);
        return true;
    }

    public async Task<bool> DeleteMotorcycleAsync(string id)
    {
        await _repository.DeleteAsync(id);
        return true;
    }
}
