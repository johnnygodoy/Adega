using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Adega.Data;
using Adega.Models;
using System.Linq;
using Adega.Filters;

namespace Adega.Controllers
{
    [AuthorizeAdmin]
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context) {
            _context = context;
        }

        private bool UsuarioEhAdmin() {
            return HttpContext.Session.GetString("UsuarioTipo") == "Admin";
        }

        public IActionResult Index() {
            if (!UsuarioEhAdmin())
                return View("~/Views/Shared/Unauthorized.cshtml");

            var usuarios = _context.Usuarios.ToList();
            return View(usuarios);
        }

        public IActionResult Criar() {
            ViewData["Title"] = "Criar Novo Usuário";

            if (!UsuarioEhAdmin())
                return Unauthorized();

            // retorna um modelo limpo
            return View(new Usuario { TipoUsuario = "Usuario" });

        }


        [HttpPost]
        public IActionResult Criar(Usuario usuario) {
            if (!UsuarioEhAdmin())
                return Unauthorized();

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id) {
            ViewData["Title"] = "Editar Usuário";

            if (!UsuarioEhAdmin())
                return Unauthorized();

            var usuario = _context.Usuarios.Find(id);
            if (usuario == null)
                return NotFound();

            usuario.Senha = string.Empty; // Limpa a senha após garantir que o usuário existe

            return View(usuario);
        }


        [HttpPost]
        public IActionResult Editar(Usuario usuario) {
            if (!UsuarioEhAdmin())
                return Unauthorized();

            _context.Usuarios.Update(usuario);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Excluir(int id) {
            if (!UsuarioEhAdmin())
                return Unauthorized();

            var usuario = _context.Usuarios.Find(id);
            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
