namespace Adega.Models
{
    public class Combo
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public decimal PrecoCombo { get; set; }

        public int ProdutoId { get; set; } // Produto virtual que representa o combo
        public Produto Produto { get; set; } // ← adicione isso
        public List<ComboProduto> Produtos { get; set; }
    }
}
