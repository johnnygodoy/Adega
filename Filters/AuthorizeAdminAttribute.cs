using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Adega.Filters
{
    public class AuthorizeAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context) {
            var tipoUsuario = context.HttpContext.Session.GetString("UsuarioTipo");

            if (string.IsNullOrEmpty(tipoUsuario) || tipoUsuario != "Admin")
            {
                context.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Unauthorized.cshtml"
                };
            }

            base.OnActionExecuting(context);
        }
    }
}
