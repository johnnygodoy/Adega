using Microsoft.AspNetCore.Mvc;
using Adega.Data;
using Adega.Models;
using System.Text.Json;

namespace Adega.Controllers
{
    [ApiController]
    [Route("whatsapp-pedido")]
    public class PedidoApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PedidoApiController(AppDbContext context) {
            _context = context;
        }

        [HttpPost]
        public IActionResult ReceberPedido([FromBody] PedidoWhatsappDto dto) {
            if (string.IsNullOrWhiteSpace(dto.Telefone) || string.IsNullOrWhiteSpace(dto.Mensagem))
                return BadRequest("Dados incompletos");

            var pedido = new Pedido
            {
                Telefone = dto.Telefone,
                Mensagem = dto.Mensagem,
                DataHora = DateTime.Now,
                Respondido = false
            };

            _context.PedidosWhatsapp.Add(pedido);
            _context.SaveChanges();

            // 🔥 Enviar resposta automática
            EnviarResposta(dto.Telefone, $"Olá! Recebemos seu pedido:\n\"{dto.Mensagem}\"\nJá estamos analisando. 🍷");

            return Ok(new { mensagem = "Pedido salvo com sucesso!" });
        }

        public class PedidoWhatsappDto
        {
            public string Telefone { get; set; }
            public string Mensagem { get; set; }
        }

        private void EnviarResposta(string telefone, string mensagem) {
            using var client = new HttpClient();
            var payload = new
            {
                telefone,
                mensagem
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            try
            {
                var result = client.PostAsync("http://localhost:3000/enviar-mensagem", content).Result;
                if (!result.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Falha ao enviar resposta automática: {result.StatusCode}");
                }
                else
                {
                    Console.WriteLine($"✅ Mensagem enviada para {telefone}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Erro ao chamar middleware: " + ex.Message);
            }
        }

        [HttpGet("ativas")]
        public IActionResult BuscarPromocoesAtivas() {
            var promocoes = _context.Promocoes
                .Where(p => p.Ativa)
                .OrderByDescending(p => p.CriadaEm)
                .Select(p => new {
                    titulo = p.Titulo,
                    descricao = p.Descricao
                })
                .ToList();

            return Ok(promocoes);
        }

    }
}
