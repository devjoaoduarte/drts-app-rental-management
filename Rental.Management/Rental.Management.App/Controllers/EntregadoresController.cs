using Microsoft.AspNetCore.Mvc;

namespace Rental.Management.App.Controllers
{
    [ApiController]
    public class EntregadoresController(IEntregadoresService entregadoresService) : ControllerBase
    {
        private readonly IEntregadoresService _entregadoresService = entregadoresService;

        [HttpPost("entregadores")]
        public async Task<IActionResult> CreateEntregador([FromBody] EntregadorRequest request)
        {
            if (request == null)
                return BadRequest(new { mensagem = "Dados inválidos" });

            if (request.Identificador == null || request.Nome == null || request.Cnpj == null || request.TipoCnh == null || request.NumeroCnh == null || request.DataNascimento == null)
                return BadRequest(new { mensagem = "Dados inválidos" });

            var tiposValidos = new[] { "A", "B", "A+B" };
            if (!tiposValidos.Contains(request.TipoCnh.ToUpperInvariant()))
                return BadRequest(new { mensagem = "Dados inválidos" });

            var CnpjInUse = await _entregadoresService.ExistsByFilterAsync("Cnpj", request.Cnpj);
            if (CnpjInUse) return BadRequest(new { mensagem = "Dados inválidos" });

            var CnhInUse = await _entregadoresService.ExistsByFilterAsync("NumeroCnh", request.NumeroCnh);
            if (CnhInUse) return BadRequest(new { mensagem = "Dados inválidos" });

            await _entregadoresService.InsertEntregadorAsync(request);
            return Created("", null);
        }

        [HttpPost("entregadores/{id}/cnh")]
        public async Task<IActionResult> UploadCnhEntregador([FromRoute] string id, [FromBody] EntregadorCnhUploadRequest request)
        {
            if (string.IsNullOrEmpty(id) || request == null)
                return BadRequest(new { mensagem = "Dados inválidos" });

            if (string.IsNullOrEmpty(request.ImagemCnh))
                return BadRequest(new { mensagem = "Dados inválidos" });

            var existsEntregador = await _entregadoresService.ExistsByFilterAsync("Identificador", id);
            if (!existsEntregador) return BadRequest(new { mensagem = "Dados inválidos" });

            var entregador = await _entregadoresService.GetEntregadorByIdAsync(id);
            entregador.ImagemCnh = request.ImagemCnh;

            await _entregadoresService.InsertEntregadorAsync(entregador);
            await _entregadoresService.UploadBase64Async($"{entregador.Identificador}-cnh", request.ImagemCnh);
            

            //await _entregadoresService.DownloadBase64($"{entregador.Identificador}-cnh.png");

            return Created("", null);
        }

    }
}
