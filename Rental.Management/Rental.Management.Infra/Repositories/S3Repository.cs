using Amazon.S3;
using Amazon.S3.Model;
using Rental.Management.Domain.Interfaces.Repositories;

namespace Rental.Management.Infra.Repositories;

public class S3Repository(IAmazonS3 client) : IS3Repository
{
    private readonly IAmazonS3 _client = client;

    public async Task<bool> UploadBase64Async(string fileName, string base64Content)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(base64Content))
                throw new ArgumentException("O conteúdo base64 não pode ser vazio.");

            const string bucketName = "fotos-cnh";

            // Detecta o tipo da imagem a partir do prefixo ou dos bytes iniciais
            string mimeType = DetectMimeType(base64Content);

            if (mimeType != "image/png" && mimeType != "image/bmp")
                throw new InvalidOperationException("Formato inválido. Apenas imagens PNG ou BMP são permitidas.");

            // Remove prefixos como "data:image/png;base64,"
            var base64Data = base64Content.Contains(",")
                ? base64Content.Split(',')[1]
                : base64Content;

            var imageBytes = Convert.FromBase64String(base64Data);

            var expectedExtension = mimeType == "image/png" ? ".png" : ".bmp";
            if (!fileName.EndsWith(expectedExtension, StringComparison.OrdinalIgnoreCase))
                fileName = Path.ChangeExtension(fileName, expectedExtension);

            using var stream = new MemoryStream(imageBytes);

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = fileName,
                InputStream = stream,
                ContentType = mimeType,
                AutoCloseStream = true
            };

            await _client.PutObjectAsync(putRequest);
            return true;
        }
        catch (FormatException)
        {
            Console.WriteLine("A string Base64 informada é inválida.");
            return false;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Erro AWS ao enviar imagem: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro inesperado: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DownloadImageToFileAsync(string s3Key)
    {
        const string bucketName = "fotos-cnh";
        try
        {
            const string localFilePath = "C:\\Users\\Joao\\Pictures\\s3 - teste";

            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = s3Key
            };

            using var response = await _client.GetObjectAsync(request);

            // Cria diretório local se não existir
            var directory = Path.GetDirectoryName(localFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Salva o arquivo localmente
            await response.WriteResponseStreamToFileAsync(localFilePath, false, default);

            Console.WriteLine($"Arquivo '{s3Key}' baixado com sucesso para '{localFilePath}'.");
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Console.WriteLine($"Arquivo '{s3Key}' não encontrado no bucket '{bucketName}'.");
            return false;
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Erro AWS ao baixar arquivo: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro inesperado ao baixar arquivo: {ex.Message}");
            return false;
        }
    }

    private static string DetectMimeType(string base64)
    {
        if (base64.StartsWith("data:image/png", StringComparison.OrdinalIgnoreCase))
            return "image/png";
        if (base64.StartsWith("data:image/bmp", StringComparison.OrdinalIgnoreCase))
            return "image/bmp";

        // Se vier sem prefixo, verificar bytes mágicos
        try
        {
            var bytes = Convert.FromBase64String(base64.Contains(",") ? base64.Split(',')[1] : base64);

            // PNG → bytes 0x89 0x50 0x4E 0x47
            if (bytes.Length > 4 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                return "image/png";

            // BMP → bytes 0x42 0x4D
            if (bytes.Length > 2 && bytes[0] == 0x42 && bytes[1] == 0x4D)
                return "image/bmp";
        }
        catch { }

        return "unknown";
    }
}
