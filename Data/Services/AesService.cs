using ProyectoEncriptacion.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;

namespace ProyectoEncriptacion.Data.Services
{
    public class AesService : IAesService
    {
        private readonly byte[] _key;

        public AesService(string key)
        {
            if (key.Length != 32)
                throw new ArgumentException("La clave debe tener 32 caracteres para AES-256.");
            _key = Encoding.UTF8.GetBytes(key);

            if (_key.Length != 32)
                throw new Exception("La clave debe tener 32 bytes (256 bits).");
        }

        public string Encrypt(string plaintext)
        {
            using var aes = new AesGcm(_key);
            byte[] nonce = RandomNumberGenerator.GetBytes(12); // 12 bytes para GCM
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[16]; // 16 bytes para GCM tag

            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            // Concatenar nonce + tag + ciphertext
            byte[] combined = new byte[nonce.Length + tag.Length + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, combined, nonce.Length, tag.Length);
            Buffer.BlockCopy(ciphertext, 0, combined, nonce.Length + tag.Length, ciphertext.Length);

            return Convert.ToBase64String(combined);
        }
    }
}