using ProyectoEncriptacion.Data.Interfaces;
using System;
using System.Security.Cryptography;
using System.Text;

namespace ProyectoEncriptacion.Data.Services
{
    public class AesService : IAesService
    {
        private readonly byte[] _key;

        public AesService(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (key.Length != 32)
                throw new ArgumentException("La clave debe tener 32 caracteres para AES-256.");
            _key = Encoding.UTF8.GetBytes(key);
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

        public string Decrypt(string base64Combined)
        {
            if (string.IsNullOrWhiteSpace(base64Combined))
                throw new ArgumentException("El texto encriptado no puede estar vacío.", nameof(base64Combined));

            byte[] combined;
            try
            {
                combined = Convert.FromBase64String(base64Combined);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("El texto proporcionado no es Base64 válido.", ex);
            }

            const int nonceSize = 12;
            const int tagSize = 16;

            if (combined.Length < nonceSize + tagSize + 1) // al menos 1 byte de ciphertext
                throw new ArgumentException("Datos encriptados inválidos o truncados.");

            byte[] nonce = new byte[nonceSize];
            byte[] tag = new byte[tagSize];
            int ciphertextLen = combined.Length - nonceSize - tagSize;
            byte[] ciphertext = new byte[ciphertextLen];

            Buffer.BlockCopy(combined, 0, nonce, 0, nonceSize);
            Buffer.BlockCopy(combined, nonceSize, tag, 0, tagSize);
            Buffer.BlockCopy(combined, nonceSize + tagSize, ciphertext, 0, ciphertextLen);

            var plaintext = new byte[ciphertextLen];

            try
            {
                using var aes = new AesGcm(_key);
                aes.Decrypt(nonce, ciphertext, tag, plaintext);
            }
            catch (CryptographicException ex)
            {
                // autenticación falló o datos corruptos
                throw new CryptographicException("Fallo de autenticación al desencriptar. Datos inválidos o clave/nonce incorrectos.", ex);
            }

            return Encoding.UTF8.GetString(plaintext);
        }
    }
}