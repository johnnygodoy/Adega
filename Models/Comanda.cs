namespace Adega.Models
{
    public class Comanda
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public DateTime DataAbertura { get; set; }
        public string Status { get; set; } // "Aberta", "Fechada", etc.

        public Cliente Cliente { get; set; }
        public List<ComandaItem> Itens { get; set; }
    }
}
