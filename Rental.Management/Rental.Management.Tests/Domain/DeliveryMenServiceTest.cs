namespace Rental.Management.Tests.Domain;

public class DeliveryMenServiceTest
{
    private readonly Mock<IDynamoDbRepository<DeliveryMenRequest>> _repositoryMock;
    private readonly Mock<IS3Repository> _s3RepositoryMock;
    private readonly DeliveryMenService _service;

    public DeliveryMenServiceTest()
    {
        _repositoryMock = new Mock<IDynamoDbRepository<DeliveryMenRequest>>();
        _s3RepositoryMock = new Mock<IS3Repository>();
        _service = new DeliveryMenService(_repositoryMock.Object, _s3RepositoryMock.Object);
    }

    [Fact(DisplayName = "ExistsByFilterAsync should return true when records exist")]
    public async Task ExistsByFilterAsync_ShouldReturnTrue_WhenRecordsExist()
    {
        // Arrange
        var fakeData = new List<DeliveryMenRequest> { new DeliveryMenRequest() };
        _repositoryMock
            .Setup(r => r.GetByFilterAsync("Name", "John"))
            .ReturnsAsync(fakeData);

        // Act
        var result = await _service.ExistsByFilterAsync("Name", "John");

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.GetByFilterAsync("Name", "John"), Times.Once);
    }

    [Fact(DisplayName = "ExistsByFilterAsync should return false when no records exist")]
    public async Task ExistsByFilterAsync_ShouldReturnFalse_WhenNoRecordsExist()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByFilterAsync("Name", "Unknown"))
            .ReturnsAsync(new List<DeliveryMenRequest>());

        // Act
        var result = await _service.ExistsByFilterAsync("Name", "Unknown");

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(r => r.GetByFilterAsync("Name", "Unknown"), Times.Once);
    }

    [Fact(DisplayName = "InsertDeliveryMenAsync should call SaveAsync and return same object")]
    public async Task InsertDeliveryMenAsync_ShouldCallSaveAsync_AndReturnObject()
    {
        // Arrange
        var request = new DeliveryMenRequest { Identificador = "123", Nome = "John" };

        _repositoryMock
            .Setup(r => r.SaveAsync(request))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.InsertDeliveryMenAsync(request);

        // Assert
        Assert.Equal(request, result);
        _repositoryMock.Verify(r => r.SaveAsync(request), Times.Once);
    }

    [Fact(DisplayName = "GetDeliveryMenByIdAsync should return object when found")]
    public async Task GetDeliveryMenByIdAsync_ShouldReturnObject_WhenFound()
    {
        // Arrange
        var expected = new DeliveryMenRequest { Identificador = "123",  Nome= "John" };

        _repositoryMock
            .Setup(r => r.GetByIdAsync("123"))
            .ReturnsAsync(expected);

        // Act
        var result = await _service.GetDeliveryMenByIdAsync("123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.Identificador, result.Identificador);
        Assert.Equal(expected.Nome, result.Nome);
        _repositoryMock.Verify(r => r.GetByIdAsync("123"), Times.Once);
    }

    [Fact(DisplayName = "GetDeliveryMenByIdAsync should return null when not found")]
    public async Task GetDeliveryMenByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("999"))
            .ReturnsAsync((DeliveryMenRequest)null);

        // Act
        var result = await _service.GetDeliveryMenByIdAsync("999");

        // Assert
        Assert.Null(result);
        _repositoryMock.Verify(r => r.GetByIdAsync("999"), Times.Once);
    }

    [Fact(DisplayName = "UploadBase64BucketAsync should return true when upload succeeds")]
    public async Task UploadBase64BucketAsync_ShouldReturnTrue_WhenUploadSucceeds()
    {
        // Arrange
        _s3RepositoryMock
            .Setup(s => s.UploadBase64Async("file.jpg", "BASE64DATA"))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UploadBase64BucketAsync("file.jpg", "BASE64DATA");

        // Assert
        Assert.True(result);
        _s3RepositoryMock.Verify(s => s.UploadBase64Async("file.jpg", "BASE64DATA"), Times.Once);
    }

    [Fact(DisplayName = "UploadBase64BucketAsync should return false when upload fails")]
    public async Task UploadBase64BucketAsync_ShouldReturnFalse_WhenUploadFails()
    {
        // Arrange
        _s3RepositoryMock
            .Setup(s => s.UploadBase64Async("file.jpg", "INVALIDBASE64"))
            .ReturnsAsync(false);

        // Act
        var result = await _service.UploadBase64BucketAsync("file.jpg", "INVALIDBASE64");

        // Assert
        Assert.False(result);
        _s3RepositoryMock.Verify(s => s.UploadBase64Async("file.jpg", "INVALIDBASE64"), Times.Once);
    }
}
