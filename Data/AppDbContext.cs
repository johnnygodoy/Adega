
using Microsoft.EntityFrameworkCore;
using Adega.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Adega.Utils;

namespace Adega.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ComboProduto> ComboProdutos { get; set; }
        public DbSet<Comanda> Comandas { get; set; }
        public DbSet<ComandaItem> ComandaItens { get; set; }
        public DbSet<MovimentacaoFinanceira> Movimentacoes { get; set; }
        public DbSet<LogSistema> Logs { get; set; }

        public DbSet<ConfiguracaoSistema> ConfiguracoesSistema { get; set; }

        public DbSet<Pedido> PedidosWhatsapp { get; set; }

        public DbSet<Promocao>Promocoes { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    Nome = "admin",
                    Senha = "1234",
                    TipoUsuario = "Admin"
                }
            );

            var data = DateTime.Today;
            var validade = data.AddDays(30);
            var chaveSecreta = "joaorogodoy"; // 🔐 você pode alterar

            var hash = HashHelper.CalcularSHA256(validade.ToString("yyyy-MM-dd") + chaveSecreta);

            modelBuilder.Entity<ConfiguracaoSistema>().HasData(new ConfiguracaoSistema
            {
                Id = 1,
                DataInstalacao = data,
                LicencaValidaAte = validade,
                HashLicenca = hash
            });
        }
    }
}
