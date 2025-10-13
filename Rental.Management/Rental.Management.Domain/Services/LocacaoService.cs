using Rental.Management.Domain.Entities;
using Rental.Management.Domain.Interfaces.Repositories;
using Rental.Management.Domain.Interfaces.Services;

namespace Rental.Management.Domain.Services;

public class LocacaoService(IDynamoDbRepository<RentalTable> repository) : IRentalService
{
    private readonly IDynamoDbRepository<RentalTable> _repository = repository;

    public async Task<bool> InsertRentalAsync(RentalRequest request)
    {
        RentalTable locacao = new()
        {
            Identificador = Guid.NewGuid().ToString(),
            DataInicio = request.DataInicio,
            DataTermino = request.DataTermino,
            DataPrevisaoTermino = request.DataPrevisaoTermino,
            EntregadorId = request.EntregadorId,
            MotoId = request.MotoId,
            ValorDiaria = GetValorDiariaByPlano(request.Plano),           
        };

        await _repository.SaveAsync(locacao);
        return true;
    }

    public async Task<RentalTable> GetRentalByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> RentalReturn(RentalTable request)
    {
        await _repository.SaveAsync(request);
        return true;
    }

    public async Task<RentalTable> GetRentalByIdMotorcycleAsync(string idMoto)
    {
        var locacao = await _repository.GetByFilterAsync("MotoId", idMoto);
        return locacao.FirstOrDefault();
    }

    private static int GetValorDiariaByPlano(int plano)
    {
        return plano switch
        {
            7 => 30,
            15 => 28,
            30 => 22,
            45 => 20,
            50 => 18,
            _ => 0,
        };
    }
}
