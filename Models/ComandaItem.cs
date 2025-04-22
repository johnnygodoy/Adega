namespace Adega.Models
{
    public class ComandaItem
    {
        public int Id { get; set; }
        public int ComandaId { get; set; }
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }

        public Comanda Comanda { get; set; }
        public Produto Produto { get; set; }
    }
}
