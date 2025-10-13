using Microsoft.AspNetCore.Mvc;

namespace Rental.Management.App.Controllers
{
    [ApiController]
    public class MotosController(IMotorcycleService motoService, IRentalService locacaoService) : ControllerBase
    {
        private readonly IMotorcycleService _motoService = motoService;   
        private readonly IRentalService _locacaoService = locacaoService;

        [HttpPost("motos")]
        public async Task<IActionResult> CreateMotorcycle([FromBody] MotorcycleRequest request)
        {
            if (request.Identificador == null || request.Ano <= 0 || request.Modelo.Length == 0 || request.Placa.Length == 0)
                return BadRequest(new { mensagem = "Dados inválidos" });

            var motorcycleByPlateExists = await _motoService.GetMotorcycleByPlateAsync(request.Placa);
            if (motorcycleByPlateExists.Any())
                return BadRequest(new { mensagem = "Dados inválidos" });

            await _motoService.InsertMotorcycleAsync(request);
            await _motoService.EnqueueNotificationMotorcycleCreationAsync(request);

            return Created("", null);
        }

        [HttpGet("motos")]
        public async Task<IActionResult> GetMotorcycleByPlate([FromQuery] string placa)
        {
            var motorcycle = await _motoService.GetMotorcycleByPlateAsync(placa);
            return Ok(motorcycle);
        }

        [HttpPut("motos/{id}/placa")]
        public async Task<IActionResult> UpdatePlateMotorcycle([FromRoute] string id, [FromBody] MotorcycleUpdatePlateRequest request)
        {
            if (string.IsNullOrEmpty(id) || request.Plate.Length == 0)
                return BadRequest(new { mensagem = "Dados inválidos" });

            var motorcycle = await _motoService.GetMotorcycleByIdAsync(id);

            if (motorcycle == null)
                return BadRequest(new { mensagem = "Dados inválidos" });
           
            await _motoService.UpdatePlateMotorcycleAsync(motorcycle, request.Plate);

            return Ok(new { mensagem = "Placa modificada com sucesso" });
        }

        [HttpGet("motos/{id}")]
        public async Task<IActionResult> GetMotorcycleById([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { mensagem = "Request mal formada" });

            var motorcycle = await _motoService.GetMotorcycleByIdAsync(id);

            if (motorcycle == null)
                return NotFound(new { mensagem = "Moto não encontrada" });

            return Ok(motorcycle);
        }

        [HttpDelete("motos/{id}")]
        public async Task<IActionResult> DeleteMotorcycle([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { mensagem = "Dados inválidos" });

            var motorcycle = await _motoService.GetMotorcycleByIdAsync(id);
            if (motorcycle == null)
                return BadRequest(new { mensagem = "Dados inválidos" });

            var rental = await _locacaoService.GetRentalByIdMotorcycleAsync(id);
            if(rental != null)
            {
                if (rental.data_devolucao == null || rental.data_devolucao.Value.Date == DateTime.MinValue)
                    return BadRequest(new { mensagem = "Dados inválidos" });
            }

            await _motoService.DeleteMotorcycleAsync(id);
            return Ok();
        }
    }
}
