namespace Adega.Models
{
    public class LogSistema
    {
        public int Id { get; set; }
        public DateTime DataHora { get; set; }
        public string Usuario { get; set; }
        public string Acao { get; set; }
        public string Tabela { get; set; }
        public string Observacao { get; set; }
    }
}
