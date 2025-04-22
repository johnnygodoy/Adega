namespace Adega.Models
{
    public class Promocao
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public bool Ativa { get; set; }
        public DateTime CriadaEm { get; set; } = DateTime.Now;
    }

}
