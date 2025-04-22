// Controllers/LicencaController.cs
using Microsoft.AspNetCore.Mvc;
using Adega.Filters;
using Adega.Data;

namespace Adega.Controllers
{
    public class LicencaController : Controller
    {
        private readonly AppDbContext _context;
        public LicencaController(AppDbContext context) {
            _context = context;
        }

        [IgnorarLicenca]
        public IActionResult Expirada() {
            return View(); // Vai tentar renderizar Views/Licenca/Expirada.cshtml
        }

        [HttpPost]
        [IgnorarLicenca]
        public IActionResult Ativar(DateTime novaData, string senha) {
            var chave = "kajoka@30";
            if (senha != "joaorogodoy")
            {
                TempData["Erro"] = "Senha inválida para ativação!";
                return RedirectToAction("Expirada");
            }

            var config = _context.ConfiguracoesSistema.FirstOrDefault();
            if (config != null)
            {
                // 🔐 Usa apenas a data (sem hora)
                var dataAjustada = novaData.Date;

                config.LicencaValidaAte = dataAjustada;
                config.HashLicenca = Utils.HashHelper.CalcularSHA256(dataAjustada.ToString("yyyy-MM-dd") + chave);
                _context.SaveChanges();

                TempData["Sucesso"] = "Licença ativada com sucesso!";
                return RedirectToAction("Index", "Home");
            }

            TempData["Erro"] = "Configuração não encontrada.";
            return RedirectToAction("Expirada");
        }

    }
}
