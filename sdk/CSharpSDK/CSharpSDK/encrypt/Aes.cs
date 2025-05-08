using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace CSharpSDK.encrypt
{
    public class AesHandle
    {
        public static string Encrypt(string plainText, string key, string iv)
        {
            byte[] encrypted;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = CipherMode.CBC;

                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] ivBytes = Encoding.UTF8.GetBytes(iv);

                if (keyBytes.Length != aesAlg.KeySize / 8)
                {
                    throw new ArgumentException($"Key must be {aesAlg.KeySize / 8} bytes long.");
                }

                if (ivBytes.Length != aesAlg.BlockSize / 8)
                {
                    throw new ArgumentException($"IV must be {aesAlg.BlockSize / 8} bytes long.");
                }

                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (System.IO.MemoryStream msEncrypt = new System.IO.MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (System.IO.StreamWriter swEncrypt = new System.IO.StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string cipherText, string keyString, string ivString)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Mode = CipherMode.CBC;

                byte[] keyBytes = Encoding.UTF8.GetBytes(keyString);
                byte[] ivBytes = Encoding.UTF8.GetBytes(ivString);

                if (keyBytes.Length != aesAlg.KeySize / 8)
                {
                    throw new ArgumentException($"Key must be {aesAlg.KeySize / 8} bytes long.");
                }

                if (ivBytes.Length != aesAlg.BlockSize / 8)
                {
                    throw new ArgumentException($"IV must be {aesAlg.BlockSize / 8} bytes long.");
                }

                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;

                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                using (MemoryStream memoryStream = new MemoryStream(cipherBytes))
                {
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader reader = new StreamReader(cryptoStream))
                            {
                                try
                                {
                                    return reader.ReadToEnd();
                                }
                                catch (CryptographicException ex)
                                {
                                    Console.WriteLine($"Decryption error: {ex.Message}");
                                    throw;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}