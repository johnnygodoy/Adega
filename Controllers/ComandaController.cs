using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Adega.Data;
using Adega.Models;
using System.Linq;

namespace Adega.Controllers
{
    public class ComandaController : Controller
    {
        private readonly AppDbContext _context;

        public ComandaController(AppDbContext context) {
            _context = context;
        }

        public IActionResult Index(DateTime? dataInicio, DateTime? dataFim, string nomeCliente, string status, int page = 1) {
            int pageSize = 10;

            var query = _context.Comandas
                .Include(c => c.Cliente)
                .Include(c => c.Itens)
                .ThenInclude(i => i.Produto)
                .AsQueryable();

            if (!dataInicio.HasValue && !dataFim.HasValue && string.IsNullOrEmpty(nomeCliente) && string.IsNullOrEmpty(status))
            {
                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var fimMes = inicioMes.AddMonths(1).AddDays(-1);
                dataInicio = inicioMes;
                dataFim = fimMes;
            }

            if (dataInicio.HasValue)
                query = query.Where(c => c.DataAbertura.Date >= dataInicio.Value.Date);

            if (dataFim.HasValue)
                query = query.Where(c => c.DataAbertura.Date <= dataFim.Value.Date);

            if (!string.IsNullOrEmpty(nomeCliente))
                query = query.Where(c => c.Cliente.Nome.ToUpper().Contains(nomeCliente.ToUpper()));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status.ToUpper() == status.ToUpper());

            int totalRegistros = query.Count();

            var comandasPaginadas = query
                .OrderByDescending(c => c.DataAbertura)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
            ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");
            ViewBag.NomeCliente = nomeCliente;
            ViewBag.Status = status;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize);
            ViewBag.PaginaAtual = page;

            return View(comandasPaginadas);
        }




        public IActionResult Criar() {
            ViewBag.Clientes = _context.Clientes.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Criar(int clienteId) {
            var comanda = new Comanda
            {
                ClienteId = clienteId,
                DataAbertura = DateTime.Now,
                Status = "Aberta"
            };

            _context.Comandas.Add(comanda);
            _context.SaveChanges();

            return RedirectToAction("Detalhes", new { id = comanda.Id });
        }

        public IActionResult Detalhes(int id) {
            var comanda = _context.Comandas
                .Include(c => c.Cliente)
                .Include(c => c.Itens)
                .ThenInclude(i => i.Produto)
                .FirstOrDefault(c => c.Id == id);

            if (comanda == null) return NotFound();

            // Produtos virtuais (ProdutoId que representa um Combo)
            var idsProdutosVirtuais = _context.Combos
                .Select(c => c.ProdutoId)
                .Distinct()
                .ToList();

            // Produtos normais (tudo que não é produto virtual de combo)
            var produtos = _context.Produtos
                .Where(p => !idsProdutosVirtuais.Contains(p.Id))
                .OrderBy(p => p.Nome)
                .ToList();

            // Produtos que representam combos (produto virtual)
            var combos = _context.Produtos
                .Where(p => idsProdutosVirtuais.Contains(p.Id))
                .OrderBy(p => p.Nome)
                .ToList();

            ViewBag.Produtos = produtos;  // dropdown de produtos reais
            ViewBag.Combos = combos;      // dropdown de combos

            return View(comanda);
        }


        [HttpPost]
        public IActionResult Fechar(int id) {
            var comanda = _context.Comandas
                .Include(c => c.Cliente)
                .Include(c => c.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefault(c => c.Id == id);

            if (comanda == null || comanda.Status == "Fechada")
                return RedirectToAction("Detalhes", new { id });

            // PREPARA DADOS: Busca combos com seus produtos
            var combos = _context.Combos
                .Include(c => c.Produtos)
                .ThenInclude(cp => cp.Produto)
                .ToList();

            // 1. VALIDAÇÃO: Verifica se todos os itens têm estoque suficiente
            foreach (var item in comanda.Itens)
            {
                var produto = item.Produto;

                var combo = combos.FirstOrDefault(c => c.ProdutoId == produto.Id); // <- é um combo?

                if (combo != null)
                {
                    foreach (var cp in combo.Produtos)
                    {
                        int qtdNecessaria = cp.Quantidade * item.Quantidade;

                        if (cp.Produto.QuantidadeEstoque < qtdNecessaria)
                        {
                            TempData["Erro"] = $"Estoque insuficiente do produto '{cp.Produto.Nome}' para o combo '{combo.Nome}'.";
                            return RedirectToAction("Detalhes", new { id });
                        }
                    }
                }
                else
                {
                    // Produto individual
                    if (produto.QuantidadeEstoque < item.Quantidade)
                    {
                        TempData["Erro"] = $"Estoque insuficiente do produto '{produto.Nome}'.";
                        return RedirectToAction("Detalhes", new { id });
                    }
                }
            }

            // 2. AÇÃO: Desconta o estoque
            foreach (var item in comanda.Itens)
            {
                var produto = item.Produto;

                var combo = combos.FirstOrDefault(c => c.ProdutoId == produto.Id);

                if (combo != null)
                {
                    foreach (var cp in combo.Produtos)
                    {
                        int qtdDescontar = cp.Quantidade * item.Quantidade;
                        cp.Produto.QuantidadeEstoque -= qtdDescontar;
                        _context.Produtos.Update(cp.Produto);
                    }
                }
                else
                {
                    produto.QuantidadeEstoque -= item.Quantidade;
                    _context.Produtos.Update(produto);
                }
            }

            // 3. LANÇAMENTO: Movimentação Financeira
            var total = comanda.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);

            var mov = new MovimentacaoFinanceira
            {
                Data = DateTime.Now,
                Entrada = total,
                Descricao = $"Fechamento da Comanda #{comanda.Id} - Cliente: {comanda.Cliente?.Nome}"
            };

            // 4. ATUALIZAÇÃO: Fechar comanda
            comanda.Status = "Fechada";

            _context.Movimentacoes.Add(mov);
            _context.Comandas.Update(comanda);
            _context.SaveChanges();

            return RedirectToAction("Detalhes", new { id });
        }



        [HttpPost]
        public IActionResult AdicionarItem(int comandaId, int? produtoId, int quantidadeProduto, int? comboId, int quantidadeCombo) {
            var comanda = _context.Comandas
                .Include(c => c.Itens)
                .FirstOrDefault(c => c.Id == comandaId);

            if (comanda == null) return NotFound();

            // ✅ Se for produto
            if (produtoId.HasValue)
            {
                var produto = _context.Produtos.Find(produtoId.Value);
                if (produto == null) return NotFound();

                var itemExistente = comanda.Itens.FirstOrDefault(i => i.ProdutoId == produtoId.Value);

                if (itemExistente != null)
                {
                    itemExistente.Quantidade += quantidadeProduto;
                }
                else
                {
                    comanda.Itens.Add(new ComandaItem
                    {
                        ProdutoId = produtoId.Value,
                        Quantidade = quantidadeProduto,
                        PrecoUnitario = produto.PrecoVenda
                    });
                }
            }

            // ✅ Se for combo (tratamento igual se desejar evitar duplicação de combo)
            if (comboId.HasValue)
            {
                var combo = _context.Produtos.Find(comboId.Value);
                if (combo == null) return NotFound();

                var itemExistente = comanda.Itens.FirstOrDefault(i => i.ProdutoId == comboId.Value);

                if (itemExistente != null)
                {
                    itemExistente.Quantidade += quantidadeCombo;
                }
                else
                {
                    comanda.Itens.Add(new ComandaItem
                    {
                        ProdutoId = comboId.Value,
                        Quantidade = quantidadeCombo,
                        PrecoUnitario = combo.PrecoVenda
                    });
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Detalhes", new { id = comandaId });
        }



        public IActionResult RemoverItem(int id) {
            var item = _context.ComandaItens.FirstOrDefault(i => i.Id == id);
            if (item == null)
                return NotFound();

            var comandaId = item.ComandaId;

            _context.ComandaItens.Remove(item);
            _context.SaveChanges();

            return RedirectToAction("Detalhes", new { id = comandaId });
        }

        [HttpPost]
        public IActionResult AtualizarItem(int itemId, int quantidade) {
            var item = _context.ComandaItens
                .Include(i => i.Produto)
                .FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                TempData["Erro"] = "Item não encontrado.";
                return RedirectToAction("Detalhes", new { id = item.ComandaId });
            }

            // Verifica estoque antes de atualizar
            if (item.Produto.QuantidadeEstoque < quantidade)
            {
                TempData["Erro"] = $"Estoque insuficiente. Disponível: {item.Produto.QuantidadeEstoque}";
                return RedirectToAction("Detalhes", new { id = item.ComandaId });
            }

            item.Quantidade = quantidade;
            _context.ComandaItens.Update(item);
            _context.SaveChanges();

            TempData["Sucesso"] = "Item atualizado com sucesso!";
            return RedirectToAction("Detalhes", new { id = item.ComandaId });
        }

    }
}
