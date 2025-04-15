namespace Adega.ViewModels
{
    public class ProdutoCheckbox
    {
        public int ProdutoId { get; set; }
        public string Nome { get; set; }
        public bool Selecionado { get; set; }
        public int Quantidade { get; set; }
        public int EstoqueAtual { get; set; } // NOVO: estoque real
    }

    public class ComboViewModel
    {
        public int? ComboId { get; set; } // ← adicionado para suportar edição
        public string Nome { get; set; }
        public decimal PrecoCombo { get; set; }
        public List<ProdutoCheckbox> ProdutosDisponiveis { get; set; }
    }
}
