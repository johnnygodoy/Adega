namespace Adega.Models
{ 
    public class MovimentacaoFinanceira
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public decimal Entrada { get; set; }
        public decimal Saida { get; set; }
        public string Descricao { get; set; }
    }

}
