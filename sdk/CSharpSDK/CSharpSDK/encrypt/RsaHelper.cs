using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace CSharpSDK.encrypt
{
    public class RsaHelper
    {
        public static string EncryptWithPublicKey(string plainText, string publicKeyPem)
        {
            using (var reader = new System.IO.StringReader(publicKeyPem))
            {
                var pemReader = new PemReader(reader);
                var publicKeyParams = (RsaKeyParameters)pemReader.ReadObject();
                var rsaParameters = DotNetUtilities.ToRSAParameters(publicKeyParams);

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(rsaParameters);
                    byte[] dataToEncrypt = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedData = rsa.Encrypt(dataToEncrypt, false);
                    string encryptedText = Convert.ToBase64String(encryptedData);
                    return encryptedText;
                }
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