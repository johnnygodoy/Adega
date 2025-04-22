using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Adega.Data;
using Adega.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Adega.Controllers
{
    public class VendaDiretaController : Controller
    {
        private readonly AppDbContext _context;

        public VendaDiretaController(AppDbContext context) {
            _context = context;
        }

        public IActionResult Index(string buscaNome, int? categoriaId, int page = 1) {
            int pageSize = 10;

            var query = _context.Produtos
                .Include(p => p.Categoria)
                .Where(p => !p.IsComboVirtual) // Se tiver flag para combos, evita eles aqui
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscaNome))
                query = query.Where(p => p.Nome.ToUpper().Contains(buscaNome.ToUpper()));

            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId.Value);

            int totalRegistros = query.Count();

            var produtos = query
                .OrderBy(p => p.Nome)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Categorias = _context.Categorias.OrderBy(c => c.Nome).ToList();
            ViewBag.BuscaNome = buscaNome;
            ViewBag.CategoriaId = categoriaId;
            ViewBag.PaginaAtual = page;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize);

            return View(produtos);
        }

        [HttpPost]
        public IActionResult FinalizarVenda([FromForm] string dadosCarrinho) {
            if (string.IsNullOrEmpty(dadosCarrinho))
            {
                TempData["Erro"] = "Nenhum item informado.";
                return RedirectToAction("Index");
            }

            var carrinho = JsonSerializer.Deserialize<List<ItemCarrinho>>(dadosCarrinho, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (carrinho == null || !carrinho.Any())
            {
                TempData["Erro"] = "Erro ao processar os itens.";
                return RedirectToAction("Index");
            }

            decimal totalVenda = 0m;
            List<string> itensDescricao = new();

            foreach (var item in carrinho)
            {
                var produto = _context.Produtos.FirstOrDefault(p => p.Id == item.Id);
                if (produto == null)
                {
                    TempData["Erro"] = $"Produto ID {item.Id} não encontrado.";
                    return RedirectToAction("Index");
                }

                if (produto.QuantidadeEstoque < item.Quantidade)
                {
                    TempData["Erro"] = $"Estoque insuficiente para o produto {produto.Nome}.";
                    return RedirectToAction("Index");
                }

                produto.QuantidadeEstoque -= item.Quantidade;
                _context.Produtos.Update(produto);

                totalVenda += item.Quantidade * item.Preco;

                itensDescricao.Add($"{item.Quantidade}x {produto.Nome}");
            }

            // Descrição personalizada com os nomes dos produtos
            var descricao = "Venda direta: " + string.Join(", ", itensDescricao);

            _context.Movimentacoes.Add(new MovimentacaoFinanceira
            {
                Data = DateTime.Now,
                Entrada = totalVenda,
                Descricao = descricao
            });

            _context.SaveChanges();

            TempData["Sucesso"] = "Venda registrada com sucesso!";
            return RedirectToAction("Index");
        }


        public class ItemCarrinho
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("quantidade")]
            public int Quantidade { get; set; }

            [JsonPropertyName("preco")]
            public decimal Preco { get; set; }
        }

    }
}
