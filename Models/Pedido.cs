namespace Adega.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public string Telefone { get; set; }
        public string Mensagem { get; set; }
        public DateTime DataHora { get; set; }

        public bool Respondido { get; set; } = false; // novo campodotnet ef database update
    }

}
