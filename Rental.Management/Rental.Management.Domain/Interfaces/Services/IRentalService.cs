using Rental.Management.Domain.Entities;

namespace Rental.Management.Domain.Interfaces.Services;

public interface IRentalService
{
    Task<bool> InsertRentalAsync(RentalRequest request);
    Task<bool> RentalReturn(RentalTable request);
    Task<RentalTable> GetRentalByIdAsync(string id);
    Task<RentalTable> GetRentalByIdMotorcycleAsync(string idMoto);
}
