using Microsoft.AspNetCore.Mvc;

namespace Rental.Management.App.Controllers
{
    [ApiController]
    public class LocacaoController(IDeliveryMenService deliveryMenService, IRentalService rentalService) : ControllerBase
    {
        private readonly IDeliveryMenService _deliveryMenService = deliveryMenService;
        private readonly IRentalService _rentalService = rentalService;

        [HttpPost("locacao")]
        public async Task<IActionResult> CreateRental([FromBody] RentalRequest request)
        {
            if (request.Plano == null)
                return BadRequest(new { mensagem = "Dados inválidos" });

            var plans = new[] { 7, 15, 30, 45, 50 };

            if (!plans.Contains(request.Plano))
                return BadRequest(new { mensagem = "Dados inválidos" });

            if (request.DataInicio == null || request.DataTermino == null || request.DataPrevisaoTermino == null)
                return BadRequest(new { mensagem = "Dados inválidos" });

            if (request.DataInicio.Date != DateTime.Now.AddDays(1).Date)
                return BadRequest(new { mensagem = "Dados inválidos" });

            var existDeliveryMen = await _deliveryMenService.ExistsByFilterAsync("Identificador", request.EntregadorId);
            if (!existDeliveryMen) return BadRequest(new { mensagem = "Dados inválidos" });

            var deliveryMen = await _deliveryMenService.GetDeliveryMenByIdAsync(request.EntregadorId);

            if (!deliveryMen.TipoCnh.Equals("A", StringComparison.CurrentCultureIgnoreCase))
                return BadRequest(new { mensagem = "Dados inválidos" });

            await _rentalService.InsertRentalAsync(request);

            return Created("", null);
        }

        [HttpGet("locacao/{id}")]
        public async Task<IActionResult> GetRental([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { mensagem = "Dados inválidos" });

            var rental = await _rentalService.GetRentalByIdAsync(id);
            if (rental == null)
                return NotFound(new { mensagem = "Locação não encontrada" });

            return Ok(rental);
        }

        [HttpPut("locacao/{id}/devolucao")]
        public async Task<IActionResult> GenerateRentalReturn([FromRoute] string id, [FromBody] ReturnsRentalRequest request)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { mensagem = "Dados inválidos" });

            if (request.DataDevolucao == null)
                return BadRequest(new { mensagem = "Dados inválidos" });

            var rental = await _rentalService.GetRentalByIdAsync(id);
            if (rental == null)
                return NotFound(new { mensagem = "Locação não encontrada" });

            rental.data_devolucao = request.DataDevolucao;

            await _rentalService.RentalReturn(rental);

            return Ok(new { mensagem = "Data de devolução informada com sucesso" });
        }
    }
}
