using Microsoft.AspNetCore.Mvc;
using Adega.Data;
using System.Linq;
using System.Text.Json;

namespace Adega.Controllers
{
    public class PedidoController : Controller
    {
        private readonly AppDbContext _context;

        public PedidoController(AppDbContext context) {
            _context = context;
        }

        public IActionResult Index(string busca, string dataBusca, int page = 1) {
            int pageSize = 10;
            var pedidos = _context.PedidosWhatsapp.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                busca = busca.ToUpper();
                pedidos = pedidos.Where(p =>
                    p.Telefone.Contains(busca) ||
                    p.Mensagem.ToUpper().Contains(busca));
            }

            DateTime dataFiltro;
            if (!string.IsNullOrWhiteSpace(dataBusca) && DateTime.TryParse(dataBusca, out dataFiltro))
            {
                pedidos = pedidos.Where(p => p.DataHora.Date == dataFiltro.Date);
            }
            else
            {
                // Por padrão, carrega apenas pedidos do dia atual
                var hoje = DateTime.Today;
                pedidos = pedidos.Where(p => p.DataHora.Date == hoje);
            }

            int total = pedidos.Count();

            var lista = pedidos
                .OrderByDescending(p => p.DataHora)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Busca = busca;
            ViewBag.DataBusca = dataBusca;
            ViewBag.PaginaAtual = page;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)total / pageSize);

            return View(lista);
        }


        [HttpPost]
        public IActionResult Responder(int id) {
            var pedido = _context.PedidosWhatsapp.FirstOrDefault(p => p.Id == id);
            if (pedido == null)
            {
                TempData["Erro"] = "Pedido não encontrado.";
                return RedirectToAction("Index");
            }

            pedido.Respondido = true;
            _context.Update(pedido);
            _context.SaveChanges();

            string resposta = $"Olá! Recebemos seu pedido:\n\"{pedido.Mensagem}\".\nAgradecemos o contato! 🍷";
            EnviarRespostaWhatsapp(pedido.Telefone, resposta);

            TempData["Sucesso"] = "Mensagem enviada com sucesso!";
            return RedirectToAction("Index");
        }

        private void EnviarRespostaWhatsapp(string telefone, string mensagem) {
            using var client = new HttpClient();
            var payload = new
            {
                telefone,
                mensagem
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            client.PostAsync("http://localhost:3000/enviar-mensagem", content).Wait();
        }
    }
}
