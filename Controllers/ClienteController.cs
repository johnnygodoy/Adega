using Microsoft.AspNetCore.Mvc;
using Adega.Data;
using Adega.Models;
using System.Linq;

namespace Adega.Controllers
{
    public class ClienteController : Controller
    {
        private readonly AppDbContext _context;

        public ClienteController(AppDbContext context) {
            _context = context;
        }

        public IActionResult Index(string busca, int page = 1) {
            int pageSize = 10;

            var query = _context.Clientes.AsQueryable();

            if (!string.IsNullOrEmpty(busca))
            {
                busca = busca.ToUpper();
                query = query.Where(c => c.Nome.ToUpper().Contains(busca));
            }

            int totalRegistros = query.Count();

            var clientesPaginados = query
                .OrderBy(c => c.Nome)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize);
            ViewBag.PaginaAtual = page;
            ViewBag.Busca = busca;

            return View(clientesPaginados);
        }


        public IActionResult Criar() {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Cliente cliente) {
            if (ModelState.IsValid)
            {
                cliente.Nome = cliente.Nome.ToUpper();
                _context.Clientes.Add(cliente);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(cliente);
        }

        public IActionResult Editar(int id) {
            var cliente = _context.Clientes.Find(id);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        [HttpPost]
        public IActionResult Editar(Cliente cliente) {
            if (ModelState.IsValid)
            {
                cliente.Nome = cliente.Nome.ToUpper();
                _context.Clientes.Update(cliente);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(cliente);
        }

        public IActionResult Excluir(int id) {
            var cliente = _context.Clientes.Find(id);
            if (cliente == null) return NotFound();

            _context.Clientes.Remove(cliente);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
