using Microsoft.AspNetCore.Mvc;
using Adega.Data;
using Adega.Models;
using System;
using System.Linq;
using Adega.Filters;

namespace Adega.Controllers
{
    [AuthorizeAdmin]
    public class RelatorioController : Controller
    {
        private readonly AppDbContext _context;

        public RelatorioController(AppDbContext context) {
            _context = context;
        }

        public IActionResult Index(DateTime? data, string tipo, string periodo, int page = 1) {
            int pageSize = 10;
            var hoje = DateTime.Today;
            var dataSelecionada = data ?? hoje;

            var query = _context.Movimentacoes.AsQueryable();

            // Filtro por período
            if (!string.IsNullOrEmpty(periodo))
            {
                switch (periodo)
                {
                    case "hoje":
                        query = query.Where(m => m.Data.Date == hoje);
                        break;
                    case "semana":
                        var semanaInicio = hoje.AddDays(-7);
                        query = query.Where(m => m.Data.Date >= semanaInicio && m.Data.Date <= hoje);
                        break;
                    case "mes":
                        query = query.Where(m => m.Data.Month == hoje.Month && m.Data.Year == hoje.Year);
                        break;
                }
            }
            else
            {
                query = query.Where(m => m.Data.Date == dataSelecionada.Date);
            }

            // Filtro por tipo
            if (!string.IsNullOrEmpty(tipo))
            {
                if (tipo == "entrada")
                    query = query.Where(m => m.Entrada > 0);
                else if (tipo == "saida")
                    query = query.Where(m => m.Saida > 0);
            }

            int totalRegistros = query.Count();

            var lista = query
                .OrderByDescending(m => m.Data)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Data = dataSelecionada;
            ViewBag.Tipo = tipo ?? "";
            ViewBag.Periodo = periodo ?? "";
            ViewBag.EntradaTotal = query.Sum(m => m.Entrada);
            ViewBag.SaidaTotal = query.Sum(m => m.Saida);
            ViewBag.Saldo = query.Sum(m => m.Entrada - m.Saida);
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize);
            ViewBag.PaginaAtual = page;

            return View(lista);
        }



    }
}
