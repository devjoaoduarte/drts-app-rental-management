using Rental.Management.Domain.Entities;

namespace Rental.Management.Domain.Interfaces.Services;

public interface IDeliveryMenService
{
    Task<DeliveryMenRequest> InsertDeliveryMenAsync(DeliveryMenRequest motorcycle);
    Task<bool> ExistsByFilterAsync(string filter, string filterValue);
    Task<DeliveryMenRequest> GetDeliveryMenByIdAsync(string id);
    Task<bool> UploadBase64BucketAsync(string fileName, string base64);
}
