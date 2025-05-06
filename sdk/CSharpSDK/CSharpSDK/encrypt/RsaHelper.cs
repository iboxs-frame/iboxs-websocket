using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpSDK.encrypt
{
    public class RsaHelper
    {
        public static string EncryptWithPublicKey(string plainText, string publicKeyPem)
        {
            MessageBox.Show(plainText,publicKeyPem);
            byte[] publicKeyBytes = Convert.FromBase64String(ExtractBase64FromPem(publicKeyPem));
            using (RSA rsa = RSA.Create())
            {
                // 旧版本.NET框架使用RSAParameters导入公钥
                var rsaParameters = new RSAParameters();
                rsaParameters.Modulus = new byte[256];
                rsaParameters.Exponent = new byte[3] { 1, 0, 1 };
                Buffer.BlockCopy(publicKeyBytes, 29, rsaParameters.Modulus, 0, 256);
                rsa.ImportParameters(rsaParameters);

                byte[] dataToEncrypt = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedData = rsa.Encrypt(dataToEncrypt, RSAEncryptionPadding.Pkcs1);
                return Convert.ToBase64String(encryptedData);
            }
        }

        static string ExtractBase64FromPem(string pem)
        {
            string[] lines = pem.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder base64 = new StringBuilder();
            bool inKey = false;
            foreach (string line in lines)
            {
                if (line.StartsWith("-----BEGIN"))
                {
                    inKey = true;
                    continue;
                }
                if (line.StartsWith("-----END"))
                {
                    inKey = false;
                    continue;
                }
                if (inKey)
                {
                    base64.Append(line);
                }
            }
            return base64.ToString();
        }

        public static string DecryptWithPrivateKey(string encryptedText, string privateKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                byte[] encryptedData = Convert.FromBase64String(encryptedText);
                byte[] decryptedData = rsa.Decrypt(encryptedData, false);
                return Encoding.UTF8.GetString(decryptedData);
            }
        }
    }
}