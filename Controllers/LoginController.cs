using Microsoft.AspNetCore.Mvc;
using Adega.Data;
using Adega.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Adega.Data;

namespace Adega.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context) {
            _context = context;
        }

        public IActionResult Index() {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string nome, string senha) {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Nome == nome && u.Senha == senha);

            if (usuario == null)
            {
                ViewBag.MensagemErro = "Usuário ou senha inválidos!";
                return View();
            }

            var config = _context.ConfiguracoesSistema.FirstOrDefault();
            if (config != null && config.LicencaValidaAte < DateTime.Today)
            {
                TempData["Erro"] = "A licença do sistema expirou. Entre em contato com o suporte.";
                return RedirectToAction("Expirada", "Licenca");
            }


            // Salvar sessão
            HttpContext.Session.SetString("UsuarioNome", usuario.Nome);
            HttpContext.Session.SetString("UsuarioTipo", usuario.TipoUsuario);

            return RedirectToAction("Index", "Home"); // ou redirecionar para Dashboard
        }

        public IActionResult Sair() {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
