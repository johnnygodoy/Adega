using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Adega.Data;
using Adega.Utils;
using System.Linq;

namespace Adega.Filters
{
    public class LicencaValidaFilter : IActionFilter
    {
        private readonly AppDbContext _context;

        public LicencaValidaFilter(AppDbContext context) {
            _context = context;
        }

        public void OnActionExecuting(ActionExecutingContext context) {
            // ✅ 1. Ignorar rotas com [IgnorarLicenca]
            var ignorar = context.ActionDescriptor.EndpointMetadata
                .Any(m => m.GetType() == typeof(IgnorarLicencaAttribute));
            if (ignorar)
                return;

            // ✅ 2. Recuperar licença
            var licenca = _context.ConfiguracoesSistema.FirstOrDefault();
            var chaveSecreta = "kajoka@30";

            if (licenca == null)
            {
                context.Result = new RedirectToActionResult("Expirada", "Licenca", null);
                return;
            }

            // ✅ 3. Validar integridade (hash da licença)
            var hashEsperado = HashHelper.CalcularSHA256(
                licenca.LicencaValidaAte.ToString("yyyy-MM-dd") + chaveSecreta
            );

            if (licenca.HashLicenca != hashEsperado)
            {
                context.Result = new RedirectToActionResult("Expirada", "Licenca", null);
                return;
            }

            // ✅ 4. Verifica se a licença expirou
            var diasRestantes = (licenca.LicencaValidaAte.Date - DateTime.Today).Days;
            if (diasRestantes < 0)
            {
                context.Result = new RedirectToActionResult("Expirada", "Licenca", null);
                return;
            }

            // ✅ 5. Passa infos pro layout
            if (context.Controller is Controller controller)
            {
                controller.ViewBag.DiasRestantes = diasRestantes;
                controller.ViewBag.SuporteEmail = "joaorogodoy@gmail.com";
                controller.ViewBag.SuporteTelefone = "(11) 97132-9882";
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
