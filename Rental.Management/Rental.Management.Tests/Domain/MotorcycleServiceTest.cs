namespace Rental.Management.Tests.Domain;

public class MotorcycleServiceTest
{
    private readonly Mock<IDynamoDbRepository<MotorcycleRequest>> _repositoryMock;
    private readonly Mock<ISQSRepository> _sqsRepositoryMock;
    private readonly MotorcycleService _service;

    public MotorcycleServiceTest()
    {
        _repositoryMock = new Mock<IDynamoDbRepository<MotorcycleRequest>>();
        _sqsRepositoryMock = new Mock<ISQSRepository>();
        _service = new MotorcycleService(_repositoryMock.Object, _sqsRepositoryMock.Object);
    }

    [Fact(DisplayName = "InsertMotorcycleAsync should save motorcycle and return same object")]
    public async Task InsertMotorcycleAsync_ShouldSaveMotorcycle_AndReturnSameObject()
    {
        // Arrange
        var motorcycle = new MotorcycleRequest { Identificador = "1", Placa = "ABC1234" };

        _repositoryMock
            .Setup(r => r.SaveAsync(motorcycle))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.InsertMotorcycleAsync(motorcycle);

        // Assert
        Assert.Equal(motorcycle, result);
        _repositoryMock.Verify(r => r.SaveAsync(motorcycle), Times.Once);
    }

    [Fact(DisplayName = "GetMotorcycleByPlateAsync should return list of motorcycles when found")]
    public async Task GetMotorcycleByPlateAsync_ShouldReturnList_WhenFound()
    {
        // Arrange
        var motorcycles = new List<MotorcycleRequest>
            {
                new MotorcycleRequest { Identificador = "1", Placa = "XYZ9876" },
                new MotorcycleRequest { Identificador = "2", Placa = "XYZ9876" }
            };

        _repositoryMock
            .Setup(r => r.GetByFilterAsync("Placa", "XYZ9876"))
            .ReturnsAsync(motorcycles);

        // Act
        var result = await _service.GetMotorcycleByPlateAsync("XYZ9876");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _repositoryMock.Verify(r => r.GetByFilterAsync("Placa", "XYZ9876"), Times.Once);
    }

    [Fact(DisplayName = "GetMotorcycleByPlateAsync should return empty list when not found")]
    public async Task GetMotorcycleByPlateAsync_ShouldReturnEmptyList_WhenNotFound()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByFilterAsync("Placa", "NOTFOUND"))
            .ReturnsAsync(new List<MotorcycleRequest>());

        // Act
        var result = await _service.GetMotorcycleByPlateAsync("NOTFOUND");

        // Assert
        Assert.Empty(result);
        _repositoryMock.Verify(r => r.GetByFilterAsync("Placa", "NOTFOUND"), Times.Once);
    }

    [Fact(DisplayName = "GetMotorcycleByIdAsync should return motorcycle when found")]
    public async Task GetMotorcycleByIdAsync_ShouldReturnMotorcycle_WhenFound()
    {
        // Arrange
        var expected = new MotorcycleRequest { Identificador = "123", Placa = "QWE4321" };

        _repositoryMock
            .Setup(r => r.GetByIdAsync("123"))
            .ReturnsAsync(expected);

        // Act
        var result = await _service.GetMotorcycleByIdAsync("123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.Identificador, result.Identificador);
        Assert.Equal(expected.Placa, result.Placa);
        _repositoryMock.Verify(r => r.GetByIdAsync("123"), Times.Once);
    }

    [Fact(DisplayName = "GetMotorcycleByIdAsync should return null when not found")]
    public async Task GetMotorcycleByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("999"))
            .ReturnsAsync((MotorcycleRequest)null);

        // Act
        var result = await _service.GetMotorcycleByIdAsync("999");

        // Assert
        Assert.Null(result);
        _repositoryMock.Verify(r => r.GetByIdAsync("999"), Times.Once);
    }

    [Fact(DisplayName = "UpdatePlateMotorcycleAsync should update plate and return true")]
    public async Task UpdatePlateMotorcycleAsync_ShouldUpdatePlate_AndReturnTrue()
    {
        // Arrange
        var motorcycle = new MotorcycleRequest { Identificador = "1", Placa = "OLD1234" };
        var newPlate = "NEW5678";

        _repositoryMock
            .Setup(r => r.UpdateAsync(motorcycle))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdatePlateMotorcycleAsync(motorcycle, newPlate);

        // Assert
        Assert.True(result);
        Assert.Equal(newPlate, motorcycle.Placa);
        _repositoryMock.Verify(r => r.UpdateAsync(motorcycle), Times.Once);
    }

    [Fact(DisplayName = "DeleteMotorcycleAsync should delete motorcycle by id and return true")]
    public async Task DeleteMotorcycleAsync_ShouldDeleteMotorcycle_AndReturnTrue()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync("DEL123"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteMotorcycleAsync("DEL123");

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteAsync("DEL123"), Times.Once);
    }

    [Fact(DisplayName = "EnqueueNotificationMotorcycleCreationAsync should send message to queue")]
    public async Task EnqueueNotificationMotorcycleCreationAsync_ShouldSendMessageToQueue()
    {
        // Arrange
        var motorcycle = new MotorcycleRequest { Identificador = "Moto1", Placa = "XYZ9876" };

        _sqsRepositoryMock
            .Setup(s => s.SendMessageAsync("motos.fifo", motorcycle))
            .Returns(Task.CompletedTask);

        // Act
        await _service.EnqueueNotificationMotorcycleCreationAsync(motorcycle);

        // Assert
        _sqsRepositoryMock.Verify(s => s.SendMessageAsync("motos.fifo", motorcycle), Times.Once);
    }
}
