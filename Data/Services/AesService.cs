using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;

namespace Data.Services
{
    public class AesService : IAesService
    {
        private readonly byte[] _key;

        public AesService(string key)
        {
            _key = Encoding.UTF8.GetBytes(key);

            if (_key.Length != 32)
                throw new Exception("La clave debe tener 32 bytes (256 bits).");
        }

        public string Decrypt(string encryptedBase64)
        {
            var fullCipher = Convert.FromBase64String(encryptedBase64);

            byte[] nonce = fullCipher[..12];
            byte[] tag = fullCipher[^16..];
            byte[] cipherText = fullCipher[12..^16];

            byte[] plaintext = new byte[cipherText.Length];

            using var aes = new AesGcm(_key);
            aes.Decrypt(nonce, cipherText, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }
    }
}