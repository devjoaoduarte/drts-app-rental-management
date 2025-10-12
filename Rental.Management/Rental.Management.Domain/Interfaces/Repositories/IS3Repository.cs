namespace Rental.Management.Domain.Interfaces.Repositories;

public interface IS3Repository
{
    Task<bool> DownloadImageToFileAsync(string s3Key);
    Task<bool> UploadBase64Async(string fileName, string base64Content);
}
