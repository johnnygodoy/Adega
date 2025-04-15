using System.ComponentModel.DataAnnotations;

namespace Adega.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Telefone é obrigatório.")]
        [Phone(ErrorMessage = "Formato de telefone inválido.")]
        [Display(Name = "Telefone")]
        public string Telefone { get; set; }
    }
}
