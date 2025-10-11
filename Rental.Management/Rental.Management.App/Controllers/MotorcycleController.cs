using Microsoft.AspNetCore.Mvc;

namespace Rental.Management.App.Controllers
{
    [ApiController]
    public class MotorcycleController(IMotorcycleServices motoService) : ControllerBase
    {
        private readonly IMotorcycleServices _motoServices = motoService;

        [HttpPost("motos")]
        public async Task<IActionResult> CreateMoto([FromBody] MotorcycleRequest request)
        {
            if (request.Ano <= 0 || request.Modelo.Length == 0 || request.Placa.Length == 0)
                return BadRequest(new { mensagem = "Dados inválidos" });

            var motorcycleByPlateExists = await _motoServices.GetMotorcycleByPlateAsync(request.Placa);
            if (motorcycleByPlateExists.Any())
                return BadRequest(new { mensagem = "Dados inválidos" });

            await _motoServices.InsertMotorcycleAsync(request); // mover para consumer da fila

            return Created("", null);
        }

        [HttpGet("motos")]
        public async Task<IActionResult> GetMotorcycleByPlate([FromQuery] string placa)
        {
            var motorcycle = await _motoServices.GetMotorcycleByPlateAsync(placa);
            return Ok(motorcycle);
        }

        [HttpPut("motos/{id}/placa")]
        public async Task<IActionResult> UpdatePlateMotorcycle([FromRoute] string id, [FromBody] MotorcycleUpdatePlateRequest request)
        {
            if (id == null || request.Plate.Length == 0)
                return BadRequest(new { mensagem = "Dados inválidos" });

            var motorcycle = await _motoServices.GetMotorcycleByIdAsync(id);

            if (motorcycle == null)
                return BadRequest(new { mensagem = "Dados inválidos" });
           
            await _motoServices.UpdatePlateMotorcycleAsync(motorcycle, request.Plate);

            return Ok(new { mensagem = "Placa modificada com sucesso" });
        }

        [HttpGet("motos/{id}")]
        public async Task<IActionResult> GetMotoById([FromRoute] string id)
        {
            if (id == null)
                return BadRequest(new { mensagem = "Request mal formada" });

            var result = await _motoServices.GetMotorcycleByIdAsync(id);

            if (result == null)
                return NotFound(new { mensagem = "Moto não encontrada" });

            return Ok(result);
        }

        [HttpDelete("motos/{id}")]
        public async Task<IActionResult> DeleteMotorcycle([FromRoute] string id)
        {
            if (id == null)
                return BadRequest(new { mensagem = "Dados inválidos" });

            var motorcycle = await _motoServices.GetMotorcycleByIdAsync(id);
            if (motorcycle == null)
                return BadRequest(new { mensagem = "Dados inválidos" });

            // adicionar validacao sem tem locação ativa..

            await _motoServices.DeleteMotorcycleAsync(id);
            return Ok();
        }
    }
}
