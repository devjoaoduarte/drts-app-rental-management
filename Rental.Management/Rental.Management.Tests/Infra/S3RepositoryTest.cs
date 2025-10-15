using Amazon.S3.Model;
using Amazon.S3;
using Rental.Management.Infra.Repositories;

namespace Rental.Management.Tests.Infra;

public class S3RepositoryTest
{
    private readonly Mock<IAmazonS3> _s3ClientMock;
    private readonly S3Repository _repository;

    public S3RepositoryTest()
    {
        _s3ClientMock = new Mock<IAmazonS3>();
        _repository = new S3Repository(_s3ClientMock.Object);
    }

    [Fact(DisplayName = "UploadBase64Async should upload PNG image successfully")]
    public async Task UploadBase64Async_ShouldUploadPngImageSuccessfully()
    {
        // Arrange
        string validBase64 = "data:image/png;base64," + Convert.ToBase64String(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        string fileName = "test.png";

        _s3ClientMock
            .Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse());

        // Act
        var result = await _repository.UploadBase64Async(fileName, validBase64);

        // Assert
        Assert.True(result);
        _s3ClientMock.Verify(s3 => s3.PutObjectAsync(It.Is<PutObjectRequest>(
            r => r.BucketName == "fotos-cnh"
                 && r.ContentType == "image/png"
                 && r.Key.EndsWith(".png")), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UploadBase64Async should upload BMP image successfully")]
    public async Task UploadBase64Async_ShouldUploadBmpImageSuccessfully()
    {
        // Arrange
        string validBase64 = "data:image/bmp;base64," + Convert.ToBase64String(new byte[] { 0x42, 0x4D });
        string fileName = "cnh_image.bmp";

        _s3ClientMock
            .Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse());

        // Act
        var result = await _repository.UploadBase64Async(fileName, validBase64);

        // Assert
        Assert.True(result);
        _s3ClientMock.Verify(s3 => s3.PutObjectAsync(It.Is<PutObjectRequest>(
            r => r.ContentType == "image/bmp" && r.Key.EndsWith(".bmp")), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UploadBase64Async should return false when invalid base64 is provided")]
    public async Task UploadBase64Async_ShouldReturnFalse_WhenInvalidBase64()
    {
        // Arrange
        string invalidBase64 = "data:image/png;base64,INVALID==";
        string fileName = "invalid.png";

        // Act
        var result = await _repository.UploadBase64Async(fileName, invalidBase64);

        // Assert
        Assert.False(result);
        _s3ClientMock.Verify(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "UploadBase64Async should return false when MIME type is not supported")]
    public async Task UploadBase64Async_ShouldReturnFalse_WhenMimeTypeIsInvalid()
    {
        // Arrange
        string invalidMimeBase64 = "data:image/jpeg;base64," + Convert.ToBase64String(new byte[] { 0xFF, 0xD8 });
        string fileName = "image.jpg";

        // Act
        var result = await _repository.UploadBase64Async(fileName, invalidMimeBase64);

        // Assert
        Assert.False(result);
        _s3ClientMock.Verify(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "UploadBase64Async should return false when AmazonS3Exception occurs")]
    public async Task UploadBase64Async_ShouldReturnFalse_WhenAmazonS3ExceptionOccurs()
    {
        // Arrange
        string validBase64 = "data:image/png;base64," + Convert.ToBase64String(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        string fileName = "error.png";

        _s3ClientMock
            .Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("AWS error"));

        // Act
        var result = await _repository.UploadBase64Async(fileName, validBase64);

        // Assert
        Assert.False(result);
    }

    [Fact(DisplayName = "UploadBase64Async should change file extension to correct format when mismatched")]
    public async Task UploadBase64Async_ShouldFixExtension_WhenDifferentExtension()
    {
        // Arrange
        string validBase64 = "data:image/png;base64," + Convert.ToBase64String(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        string fileName = "wrong_extension.bmp";

        _s3ClientMock
            .Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse());

        // Act
        var result = await _repository.UploadBase64Async(fileName, validBase64);

        // Assert
        Assert.True(result);
        _s3ClientMock.Verify(s3 => s3.PutObjectAsync(It.Is<PutObjectRequest>(
            r => r.Key.EndsWith(".png")), It.IsAny<CancellationToken>()), Times.Once);
    }
}
