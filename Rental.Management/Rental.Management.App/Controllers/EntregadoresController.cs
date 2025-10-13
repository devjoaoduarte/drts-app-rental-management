using Microsoft.AspNetCore.Mvc;

namespace Rental.Management.App.Controllers
{
    [ApiController]
    public class EntregadoresController(IDeliveryMenService deliveryMenService) : ControllerBase
    {
        private readonly IDeliveryMenService _deliveryMenService = deliveryMenService;

        [HttpPost("entregadores")]
        public async Task<IActionResult> CreateDeliveryMen([FromBody] DeliveryMenRequest request)
        {
            if (request == null)
                return BadRequest(new { mensagem = "Dados inválidos" });

            if (request.Identificador == null || request.Nome == null || request.Cnpj == null || request.TipoCnh == null || request.NumeroCnh == null || request.DataNascimento == null)
                return BadRequest(new { mensagem = "Dados inválidos" });

            var cnhTypes = new[] { "A", "B", "A+B" };
            if (!cnhTypes.Contains(request.TipoCnh.ToUpperInvariant()))
                return BadRequest(new { mensagem = "Dados inválidos" });

            var cnpjInUse = await _deliveryMenService.ExistsByFilterAsync("Cnpj", request.Cnpj);
            if (cnpjInUse) return BadRequest(new { mensagem = "Dados inválidos" });

            var cnhInUse = await _deliveryMenService.ExistsByFilterAsync("NumeroCnh", request.NumeroCnh);
            if (cnhInUse) return BadRequest(new { mensagem = "Dados inválidos" });

            await _deliveryMenService.InsertDeliveryMenAsync(request);
            return Created("", null);
        }

        [HttpPost("entregadores/{id}/cnh")]
        public async Task<IActionResult> UploadCnhDeliveryMen([FromRoute] string id, [FromBody] DeliveryMenCnhUploadRequest request)
        {
            if (string.IsNullOrEmpty(id) || request == null)
                return BadRequest(new { mensagem = "Dados inválidos" });

            if (string.IsNullOrEmpty(request.ImagemCnh))
                return BadRequest(new { mensagem = "Dados inválidos" });

            var existsDeliveryMen = await _deliveryMenService.ExistsByFilterAsync("Identificador", id);
            if (!existsDeliveryMen) return BadRequest(new { mensagem = "Dados inválidos" });

            var deliveryMen = await _deliveryMenService.GetDeliveryMenByIdAsync(id);
            deliveryMen.ImagemCnh = request.ImagemCnh;

            await _deliveryMenService.InsertDeliveryMenAsync(deliveryMen);
            await _deliveryMenService.UploadBase64BucketAsync($"{deliveryMen.Identificador}-cnh", request.ImagemCnh);

            return Created("", null);
        }

    }
}
