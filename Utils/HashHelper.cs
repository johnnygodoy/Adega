using System.Security.Cryptography;
using System.Text;

namespace Adega.Utils
{
    public static class HashHelper
    {
        public static string CalcularSHA256(string texto) {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(texto);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
