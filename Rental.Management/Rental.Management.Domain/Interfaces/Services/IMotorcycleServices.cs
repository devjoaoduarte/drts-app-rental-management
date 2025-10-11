using Rental.Management.Domain.Entities;

namespace Rental.Management.Domain.Interfaces.Services;

public interface IMotorcycleServices
{
    Task<MotorcycleRequest> InsertMotorcycleAsync(MotorcycleRequest motorcycle);
    Task<IEnumerable<MotorcycleRequest>> GetMotorcycleByPlateAsync(string plate);
    Task<MotorcycleRequest> GetMotorcycleByIdAsync(string id);
    Task<bool> UpdatePlateMotorcycleAsync(MotorcycleRequest motorcycle, string newPlate);
    Task<bool> DeleteMotorcycleAsync(string id);
}
