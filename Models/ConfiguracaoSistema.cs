// Models/ConfiguracaoSistema.cs
using System;

namespace Adega.Models
{
    public class ConfiguracaoSistema
    {
        public int Id { get; set; }
        public DateTime DataInstalacao { get; set; }
        public DateTime LicencaValidaAte { get; set; }

        public string HashLicenca { get; set; }  // Novo campo
    }
}
