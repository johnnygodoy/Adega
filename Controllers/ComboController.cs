using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Adega.Data;
using Adega.Models;
using Adega.ViewModels;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Adega.Controllers
{
    public class ComboController : Controller
    {
        private readonly AppDbContext _context;

        public ComboController(AppDbContext context) {
            _context = context;
        }

        public IActionResult Index(string busca, int page = 1) {
            int pageSize = 10;

            var query = _context.Combos
                .Include(c => c.Produtos)
                .ThenInclude(cp => cp.Produto)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busca))
            {
                busca = busca.ToUpper();
                query = query.Where(c => c.Nome.ToUpper().Contains(busca));
            }

            int totalRegistros = query.Count();

            var combosPaginados = query
                .OrderBy(c => c.Nome)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize);
            ViewBag.PaginaAtual = page;
            ViewBag.Busca = busca;

            return View(combosPaginados);
        }

        public IActionResult Criar(string busca, int page = 1) {
            int pageSize = 10;

            var query = _context.Produtos.AsQueryable();

            if (!string.IsNullOrEmpty(busca))
            {
                busca = busca.ToUpper();
                query = query.Where(p => p.Nome.ToUpper().Contains(busca));
            }

            var produtos = query
                .OrderBy(p => p.Nome)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new ComboViewModel
            {
                ProdutosDisponiveis = produtos.Select(p => new ProdutoCheckbox
                {
                    ProdutoId = p.Id,
                    Nome = p.Nome,
                    Quantidade = 1,
                    EstoqueAtual = p.QuantidadeEstoque
                }).ToList()
            };

            ViewBag.TotalPaginas = (int)Math.Ceiling(query.Count() / (double)pageSize);
            ViewBag.PaginaAtual = page;
            ViewBag.Busca = busca;

            return View(viewModel);
        }





        [HttpPost]
        public IActionResult Criar(ComboViewModel model) {
            if (!ModelState.IsValid)
            {
                // Recarregar produtos disponíveis para exibir de volta
                model.ProdutosDisponiveis = CarregarProdutosParaCombo();
                return View(model);
            }

            // 1. Criar o produto virtual
            var produtoCombo = new Produto
            {
                Nome = model.Nome.ToUpper(),
                PrecoVenda = model.PrecoCombo,
                PrecoCompra = 0,
                QuantidadeEstoque = 0,
                CategoriaId = 1, // ou outra categoria padrão
                IsComboVirtual = true // Marcação clara
            };

            _context.Produtos.Add(produtoCombo);
            _context.SaveChanges();

            // 2. Criar o combo com o produto virtual recém-criado
            var combo = new Combo
            {
                Nome = model.Nome.ToUpper(),
                PrecoCombo = model.PrecoCombo,
                ProdutoId = produtoCombo.Id,
                Produtos = new List<ComboProduto>()
            };

            foreach (var item in model.ProdutosDisponiveis.Where(p => p.Selecionado))
            {
                combo.Produtos.Add(new ComboProduto
                {
                    ProdutoId = item.ProdutoId,
                    Quantidade = item.Quantidade
                });
            }

            _context.Combos.Add(combo);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        public IActionResult Excluir(int id) {
            var combo = _context.Combos.Include(c => c.Produtos).FirstOrDefault(c => c.Id == id);
            if (combo == null) return NotFound();

            _context.ComboProdutos.RemoveRange(combo.Produtos);
            _context.Combos.Remove(combo);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id, string busca, int page = 1) {
            int pageSize = 10;

            var combo = _context.Combos
                .Include(c => c.Produtos)
                .ThenInclude(cp => cp.Produto)
                .FirstOrDefault(c => c.Id == id);

            if (combo == null)
                return NotFound();

            var produtosQuery = _context.Produtos.AsQueryable();

            if (!string.IsNullOrEmpty(busca))
            {
                produtosQuery = produtosQuery.Where(p => p.Nome.ToUpper().Contains(busca.ToUpper()));
                ViewBag.Busca = busca;
            }

            var todosProdutos = produtosQuery
                .OrderBy(p => p.Nome)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var produtosViewModel = todosProdutos.Select(p => new ProdutoCheckbox
            {
                ProdutoId = p.Id,
                Nome = p.Nome,
                EstoqueAtual = p.QuantidadeEstoque,
                Quantidade = 1,
                Selecionado = false
            }).ToList();

            foreach (var item in produtosViewModel)
            {
                var existente = combo.Produtos.FirstOrDefault(p => p.ProdutoId == item.ProdutoId);
                if (existente != null)
                {
                    item.Selecionado = true;
                    item.Quantidade = existente.Quantidade;
                }
            }

            var model = new ComboViewModel
            {
                ComboId = combo.Id,
                Nome = combo.Nome,
                PrecoCombo = combo.PrecoCombo,
                ProdutosDisponiveis = produtosViewModel
            };

            ViewBag.ComboId = combo.Id;
            ViewBag.TotalPaginas = (int)Math.Ceiling(produtosQuery.Count() / (double)pageSize);
            ViewBag.PaginaAtual = page;

            return View("Editar", model);
        }





        [HttpPost]
        public IActionResult Editar(int id, ComboViewModel viewModel) {
            var comboExistente = _context.Combos
                .Include(c => c.Produtos)
                .FirstOrDefault(c => c.Id == id);

            if (comboExistente == null) return NotFound();

            if (string.IsNullOrEmpty(viewModel.Nome) || viewModel.PrecoCombo <= 0)
                ModelState.AddModelError("", "Preencha o nome e o preço corretamente.");

            var produtosSelecionados = viewModel.ProdutosDisponiveis
                .Where(p => p.Selecionado && p.Quantidade > 0).ToList();

            if (!produtosSelecionados.Any())
                ModelState.AddModelError("", "Selecione pelo menos um produto com quantidade.");

            if (!ModelState.IsValid)
            {
                viewModel.ProdutosDisponiveis = CarregarProdutosParaCombo(id);
                ViewBag.ComboId = id;
                return View("Criar", viewModel); // ← retorna para a mesma view
            }

            // Atualiza combo
            comboExistente.Nome = viewModel.Nome.ToUpper();
            comboExistente.PrecoCombo = viewModel.PrecoCombo;

            _context.ComboProdutos.RemoveRange(comboExistente.Produtos);

            comboExistente.Produtos = produtosSelecionados.Select(p => new ComboProduto
            {
                ProdutoId = p.ProdutoId,
                Quantidade = p.Quantidade
            }).ToList();

            _context.Combos.Update(comboExistente);
            _context.SaveChanges();

            TempData["Sucesso"] = "Combo atualizado com sucesso!";
            return RedirectToAction("Index");
        }



        private List<ProdutoCheckbox> CarregarProdutosParaCombo(int? comboId = null) {
            // Pega os IDs dos produtos virtuais dos combos
            var idsProdutosVirtuais = _context.Combos
                .Select(c => c.ProdutoId)
                .Distinct()
                .ToList();

            // Remove o produto virtual do combo atual da blacklist (para edição funcionar corretamente)
            if (comboId.HasValue)
            {
                var comboAtual = _context.Combos.FirstOrDefault(c => c.Id == comboId.Value);
                if (comboAtual != null && idsProdutosVirtuais.Contains(comboAtual.ProdutoId))
                {
                    idsProdutosVirtuais.Remove(comboAtual.ProdutoId);
                }
            }

            // Pega todos os produtos que NÃO são produtos virtuais de combos
            var produtosDisponiveis = _context.Produtos
             .Where(p => !p.IsComboVirtual)
             .OrderBy(p => p.Nome)
             .Select(p => new ProdutoCheckbox
             {
                 ProdutoId = p.Id,
                 Nome = p.Nome,
                 Quantidade = 1,
                 Selecionado = false
             })
             .ToList();

            return produtosDisponiveis;
        }

    }
}
