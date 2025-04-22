using Microsoft.AspNetCore.Mvc;
using Adega.Data;
using Adega.Models;
using System.Linq;

namespace Adega.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriaController(AppDbContext context) {
            _context = context;
        }

        public IActionResult Index(string buscaNome, int page = 1) {
            int pageSize = 10;

            var query = _context.Categorias.AsQueryable();

            if (!string.IsNullOrEmpty(buscaNome))
            {
                buscaNome = buscaNome.ToUpper();
                query = query.Where(c => c.Nome.ToUpper().Contains(buscaNome));
            }

            int totalRegistros = query.Count();
            var categorias = query
                .OrderBy(c => c.Nome)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize);
            ViewBag.PaginaAtual = page;
            ViewBag.BuscaNome = buscaNome;

            return View(categorias);
        }


        public IActionResult Criar() {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Categoria categoria) {
            if (ModelState.IsValid)
            {
                categoria.Nome= categoria.Nome.ToUpper();
                _context.Categorias.Add(categoria);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(categoria);
        }

        public IActionResult Editar(int id) {
            var categoria = _context.Categorias.Find(id);
            categoria.Nome = categoria.Nome.ToUpper();
            if (categoria == null) return NotFound();

            return View(categoria);
        }

        [HttpPost]
        public IActionResult Editar(Categoria categoria) {
            if (ModelState.IsValid)
            {
                categoria.Nome = categoria.Nome.ToUpper();
                _context.Categorias.Update(categoria);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(categoria);
        }

        public IActionResult Excluir(int id) {
            var categoria = _context.Categorias.Find(id);
            if (categoria == null) return NotFound();

            // Verificar se existem produtos com essa categoria
            bool temProdutos = _context.Produtos.Any(p => p.CategoriaId == id);
            if (temProdutos)
            {
                TempData["Erro"] = "Esta categoria não pode ser excluída porque está sendo usada em produtos.";
                return RedirectToAction("Index");
            }

            _context.Categorias.Remove(categoria);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}
