namespace Adega.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public decimal PrecoCompra { get; set; }
        public decimal PrecoVenda { get; set; }
        public int QuantidadeEstoque { get; set; }
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
        public bool IsComboVirtual { get; set; }
    }
}
