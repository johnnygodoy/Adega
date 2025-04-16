using Adega.Data;
using Adega.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


namespace Adega.Controllers
{
    
   
    public class PromocaoController : Controller
    {
        private readonly AppDbContext _context;

        public PromocaoController(AppDbContext context) {
            _context = context;
        }

        public IActionResult Index() {
            var promocoes = _context.Promocoes
                .OrderByDescending(p => p.CriadaEm)
                .ToList();
            return View(promocoes);
        }

        [HttpPost]
        public IActionResult Cadastrar(string titulo, string descricao) {
            if (!string.IsNullOrWhiteSpace(titulo) && !string.IsNullOrWhiteSpace(descricao))
            {
                var nova = new Promocao
                {
                    Titulo = titulo,
                    Descricao = descricao,
                    Ativa = true
                };
                _context.Promocoes.Add(nova);
                _context.SaveChanges();
                TempData["Sucesso"] = "Promoção cadastrada com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Preencha todos os campos.";
            }

            return RedirectToAction("Index");
        }

        public IActionResult AlternarStatus(int id) {
            var p = _context.Promocoes.FirstOrDefault(x => x.Id == id);
            if (p != null)
            {
                p.Ativa = !p.Ativa;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Excluir(int id) {
            var p = _context.Promocoes.FirstOrDefault(x => x.Id == id);
            if (p != null)
            {
                _context.Promocoes.Remove(p);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("Promocao/Ativas")]
        public IActionResult Ativas() {
            var promocoesAtivas = _context.Promocoes
                .Where(p => p.Ativa)
                .OrderByDescending(p => p.CriadaEm)
                .Select(p => new
                {
                    titulo = p.Titulo,
                    descricao = p.Descricao
                })
                .ToList();

            return Ok(promocoesAtivas);
        }

    }

}
