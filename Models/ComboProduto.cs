namespace Adega.Models
{
    public class ComboProduto
    {
        public int Id { get; set; }
        public int ComboId { get; set; }
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }

        public Combo Combo { get; set; }
        public Produto Produto { get; set; }
    }
}
