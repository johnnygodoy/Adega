using Adega.Data;
using Adega.Filters;
using Adega.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;



var culturaBR = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culturaBR;
CultureInfo.DefaultThreadCurrentUICulture = culturaBR;
CultureInfo.CurrentCulture = culturaBR;
CultureInfo.CurrentUICulture = culturaBR;


var builder = WebApplication.CreateBuilder(args);

// Definir a cultura para pt-BR
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

var caminhoDb = Path.Combine(Directory.GetCurrentDirectory(), "Data", "adega.db");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={caminhoDb}"));


builder.Services.AddControllersWithViews();
builder.Services.AddSession();

builder.Services.AddScoped<LicencaValidaFilter>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<LicencaValidaFilter>();
});


var app = builder.Build();

app.UseSession();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!context.ConfiguracoesSistema.Any())
    {
        context.ConfiguracoesSistema.Add(new ConfiguracaoSistema
        {
            DataInstalacao = DateTime.Today,
            LicencaValidaAte = DateTime.Today.AddDays(30)
        });

        context.SaveChanges();
    }
}

app.Run();
