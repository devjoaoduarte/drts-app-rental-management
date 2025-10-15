namespace Rental.Management.Tests.Domain;

public class RentalServiceTest
{
    private readonly Mock<IDynamoDbRepository<RentalTable>> _repositoryMock;
    private readonly RentalService _service;

    public RentalServiceTest()
    {
        _repositoryMock = new Mock<IDynamoDbRepository<RentalTable>>();
        _service = new RentalService(_repositoryMock.Object);
    }

    [Fact(DisplayName = "InsertRentalAsync should save rental and return true")]
    public async Task InsertRentalAsync_ShouldSaveRental_AndReturnTrue()
    {
        // Arrange
        var request = new RentalRequest
        {
            DataInicio = DateTime.UtcNow,
            DataTermino = DateTime.UtcNow.AddDays(7),
            DataPrevisaoTermino = DateTime.UtcNow.AddDays(10),
            EntregadorId = "Ent1",
            MotoId = "Moto1",
            Plano = 7
        };

        _repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<RentalTable>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.InsertRentalAsync(request);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.SaveAsync(It.Is<RentalTable>(l =>
            l.EntregadorId == request.EntregadorId &&
            l.MotoId == request.MotoId &&
            l.ValorDiaria == 30 &&
            !string.IsNullOrEmpty(l.Identificador)
        )), Times.Once);
    }

    [Theory(DisplayName = "InsertRentalAsync should set correct daily rate for each plan")]
    [InlineData(7, 30)]
    [InlineData(15, 28)]
    [InlineData(30, 22)]
    [InlineData(45, 20)]
    [InlineData(50, 18)]
    [InlineData(99, 0)] // invalid plan
    public async Task InsertRentalAsync_ShouldSetCorrectDailyRate_ByPlan(int plan, int expectedRate)
    {
        // Arrange
        var request = new RentalRequest
        {
            Plano = plan,
            DataInicio = DateTime.UtcNow,
            DataTermino = DateTime.UtcNow.AddDays(5)
        };

        RentalTable? savedRental = null;

        _repositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<RentalTable>()))
            .Callback<RentalTable>(r => savedRental = r)
            .Returns(Task.CompletedTask);

        // Act
        await _service.InsertRentalAsync(request);

        // Assert
        Assert.NotNull(savedRental);
        Assert.Equal(expectedRate, savedRental!.ValorDiaria);
    }

    [Fact(DisplayName = "GetRentalByIdAsync should return rental when found")]
    public async Task GetRentalByIdAsync_ShouldReturnRental_WhenFound()
    {
        // Arrange
        var expected = new RentalTable { Identificador = "123" };

        _repositoryMock
            .Setup(r => r.GetByIdAsync("123"))
            .ReturnsAsync(expected);

        // Act
        var result = await _service.GetRentalByIdAsync("123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.Identificador, result.Identificador);
        _repositoryMock.Verify(r => r.GetByIdAsync("123"), Times.Once);
    }

    [Fact(DisplayName = "GetRentalByIdAsync should return null when not found")]
    public async Task GetRentalByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("notfound"))
            .ReturnsAsync((RentalTable)null);

        // Act
        var result = await _service.GetRentalByIdAsync("notfound");

        // Assert
        Assert.Null(result);
        _repositoryMock.Verify(r => r.GetByIdAsync("notfound"), Times.Once);
    }

    [Fact(DisplayName = "RentalReturn should save rental and return true")]
    public async Task RentalReturn_ShouldSaveRental_AndReturnTrue()
    {
        // Arrange
        var rental = new RentalTable { Identificador = "Ret123" };
        _repositoryMock
            .Setup(r => r.SaveAsync(rental))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RentalReturn(rental);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.SaveAsync(rental), Times.Once);
    }

    [Fact(DisplayName = "GetRentalByIdMotorcycleAsync should return first rental when found")]
    public async Task GetRentalByIdMotorcycleAsync_ShouldReturnFirstRental_WhenFound()
    {
        // Arrange
        var rentals = new List<RentalTable>
            {
                new RentalTable { MotoId = "MotoX", Identificador = "1" },
                new RentalTable { MotoId = "MotoX", Identificador = "2" }
            };

        _repositoryMock
            .Setup(r => r.GetByFilterAsync("MotoId", "MotoX"))
            .ReturnsAsync(rentals);

        // Act
        var result = await _service.GetRentalByIdMotorcycleAsync("MotoX");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("1", result.Identificador);
        _repositoryMock.Verify(r => r.GetByFilterAsync("MotoId", "MotoX"), Times.Once);
    }

    [Fact(DisplayName = "GetRentalByIdMotorcycleAsync should return null when none found")]
    public async Task GetRentalByIdMotorcycleAsync_ShouldReturnNull_WhenNoneFound()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByFilterAsync("MotoId", "Unknown"))
            .ReturnsAsync(new List<RentalTable>());

        // Act
        var result = await _service.GetRentalByIdMotorcycleAsync("Unknown");

        // Assert
        Assert.Null(result);
        _repositoryMock.Verify(r => r.GetByFilterAsync("MotoId", "Unknown"), Times.Once);
    }
}
