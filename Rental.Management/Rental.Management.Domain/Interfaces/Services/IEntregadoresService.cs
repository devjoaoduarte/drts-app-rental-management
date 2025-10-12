using Rental.Management.Domain.Entities;

namespace Rental.Management.Domain.Interfaces.Services;

public interface IEntregadoresService
{
    Task<EntregadorRequest> InsertEntregadorAsync(EntregadorRequest motorcycle);
    Task<bool> ExistsByFilterAsync(string filter, string filterValue);
    Task<EntregadorRequest> GetEntregadorByIdAsync(string id);
    Task<bool> UploadBase64Async(string fileName, string base64);
    Task<bool> DownloadBase64(string fileName);
}
