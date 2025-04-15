using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Adega.Data;
using Adega.Filters;
using Adega.Models;

namespace Adega.Controllers
{

    [AuthorizeAdmin]
    public class ProdutoController : Controller
    {
       

        private readonly AppDbContext _context;

        public ProdutoController(AppDbContext context) {
            _context = context;
        }

        public IActionResult Gerenciar(string buscaNome, int? categoriaId, int page = 1) {
            int pageSize = 10;
            var query = _context.Produtos.AsQueryable();

            if (!string.IsNullOrEmpty(buscaNome))
                query = query.Where(p => p.Nome.ToUpper().Contains(buscaNome.ToUpper()));

            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId.Value);

            var produtos = query
                .OrderBy(p => p.Nome)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalItems = query.Count();
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.PaginaAtual = page;
            ViewBag.BuscaNome = buscaNome;
            ViewBag.CategoriaId = categoriaId;

            ViewBag.Categorias = _context.Categorias.ToList();

            return View(produtos);
        }

        [HttpPost]
        public IActionResult AtualizarEstoque(List<Produto> produtos) {
            foreach (var produto in produtos)
            {
                var existente = _context.Produtos.FirstOrDefault(p => p.Id == produto.Id);
                if (existente != null)
                {
                    int quantidadeAdicional = produto.QuantidadeEstoque - existente.QuantidadeEstoque;

                    // Atualiza os valores básicos
                    existente.PrecoCompra = produto.PrecoCompra;
                    existente.PrecoVenda = produto.PrecoVenda;
                    existente.CategoriaId = produto.CategoriaId;
                    existente.QuantidadeEstoque = produto.QuantidadeEstoque;

                    // Se a quantidade aumentou, registrar saída
                    if (quantidadeAdicional > 0)
                    {
                        var mov = new MovimentacaoFinanceira
                        {
                            Data = DateTime.Now,
                            Saida = quantidadeAdicional * produto.PrecoCompra,
                            Descricao = $"Reforço de estoque: {quantidadeAdicional} unid. do produto '{produto.Nome}'"
                        };
                        _context.Movimentacoes.Add(mov);
                    }
                }
            }

            _context.SaveChanges();
            TempData["Sucesso"] = "Produtos atualizados com sucesso!";
            return RedirectToAction("Gerenciar");
        }



        [HttpPost]
        public IActionResult AdicionarNovo(Produto novoProduto) {
            if (string.IsNullOrWhiteSpace(novoProduto.Nome) ||
                novoProduto.PrecoVenda <= 0 ||
                novoProduto.PrecoCompra <= 0 ||
                novoProduto.QuantidadeEstoque <= 0)
            {
                TempData["Erro"] = "Preencha todos os campos corretamente.";
                return RedirectToAction("Gerenciar");
            }

            string nomeNormalizado = novoProduto.Nome.ToUpper().Trim();
            var produtoExistente = _context.Produtos.FirstOrDefault(p => p.Nome.ToUpper() == nomeNormalizado);

            if (produtoExistente != null)
            {
                // Produto já existe → Atualiza quantidade e preços
                produtoExistente.QuantidadeEstoque += novoProduto.QuantidadeEstoque;

                if (produtoExistente.PrecoCompra != novoProduto.PrecoCompra)
                    produtoExistente.PrecoCompra = novoProduto.PrecoCompra;

                if (produtoExistente.PrecoVenda != novoProduto.PrecoVenda)
                    produtoExistente.PrecoVenda = novoProduto.PrecoVenda;

                _context.Produtos.Update(produtoExistente);

                // REGISTRA A SAÍDA FINANCEIRA DA COMPRA
                var mov = new MovimentacaoFinanceira
                {
                    Data = DateTime.Now,
                    Saida = novoProduto.PrecoCompra * novoProduto.QuantidadeEstoque,
                    Descricao = $"Reabastecimento de {novoProduto.QuantidadeEstoque} unid. do produto '{produtoExistente.Nome}'"
                };
                _context.Movimentacoes.Add(mov);

                TempData["Sucesso"] = "Produto existente atualizado com nova quantidade.";
            }
            else
            {
                // Produto novo → insere normalmente
                novoProduto.Nome = nomeNormalizado;
                _context.Produtos.Add(novoProduto);

                var mov = new MovimentacaoFinanceira
                {
                    Data = DateTime.Now,
                    Saida = novoProduto.PrecoCompra * novoProduto.QuantidadeEstoque,
                    Descricao = $"Compra de {novoProduto.QuantidadeEstoque} unid. do produto '{novoProduto.Nome}'"
                };
                _context.Movimentacoes.Add(mov);

                TempData["Sucesso"] = "Novo produto adicionado com sucesso.";
            }

            _context.SaveChanges();
            return RedirectToAction("Gerenciar");
        }

        [HttpPost]
        public IActionResult AtualizarEstoquePorId(Produto produto) {
            var existente = _context.Produtos.FirstOrDefault(p => p.Id == produto.Id);
            if (existente != null)
            {
                int quantidadeAdicional = produto.QuantidadeEstoque;

                existente.PrecoCompra = produto.PrecoCompra;
                existente.PrecoVenda = produto.PrecoVenda;
                existente.CategoriaId = produto.CategoriaId;
                existente.QuantidadeEstoque += quantidadeAdicional;

                // Registra movimentação de saída
                if (quantidadeAdicional > 0)
                {
                    var mov = new MovimentacaoFinanceira
                    {
                        Data = DateTime.Now,
                        Saida = quantidadeAdicional * produto.PrecoCompra,
                        Descricao = $"Reforço de estoque: {quantidadeAdicional} unid. do produto '{existente.Nome.ToUpper()}'"
                    };
                    _context.Movimentacoes.Add(mov);
                }

                _context.SaveChanges();
                TempData["Sucesso"] = $"Produto '{existente.Nome.ToUpper()}' atualizado.";
            }

            return RedirectToAction("Gerenciar");
        }

        [HttpPost]
        public IActionResult Excluir(int id) {
            var produto = _context.Produtos.FirstOrDefault(p => p.Id == id);

            bool ehCombo = _context.Combos.Any(c => c.ProdutoId == id);
            bool fazParteDeCombo = _context.ComboProdutos.Any(cp => cp.ProdutoId == id);

            if (ehCombo || fazParteDeCombo)
            {
                TempData["Erro"] = $"O produto '{produto?.Nome}' está vinculado a um combo e não pode ser excluído.";
                return RedirectToAction("Gerenciar");
            }

            if (produto != null)
            {
                // Registra perda de estoque se ainda houver unidades
                if (produto.QuantidadeEstoque > 0)
                {
                    var mov = new MovimentacaoFinanceira
                    {
                        Data = DateTime.Now,
                        Saida = produto.QuantidadeEstoque * produto.PrecoCompra,
                        Descricao = $"Exclusão do produto '{produto.Nome}' com {produto.QuantidadeEstoque} unid."
                    };
                    _context.Movimentacoes.Add(mov);
                }

                _context.Produtos.Remove(produto);
                _context.SaveChanges();
                TempData["Sucesso"] = $"Produto '{produto.Nome}' excluído com sucesso!";
            }

            return RedirectToAction("Gerenciar");
        }

        [HttpGet]
        public JsonResult ObterProdutoPorNome(string nome) {
            var produto = _context.Produtos
                .FirstOrDefault(p => p.Nome.ToUpper() == nome.ToUpper());

            if (produto == null)
                return Json(null);

            return Json(new
            {
                precoCompra = produto.PrecoCompra,
                precoVenda = produto.PrecoVenda,
                estoque = produto.QuantidadeEstoque,
                categoriaId = produto.CategoriaId
            });
        }


    }

}
