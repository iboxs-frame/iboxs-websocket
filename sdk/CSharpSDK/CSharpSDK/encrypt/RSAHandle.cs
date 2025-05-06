using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace CSharpSDK.encrypt
{
    class RSAHandle
    {
        public static void createRSAKey()
        {
            // 创建一个新的 RSA 实例
            using (RSA rsa = RSA.Create())
            {
                // 获取公钥参数
                RSAParameters publicKeyParams = rsa.ExportParameters(false);
                // 获取私钥参数
                RSAParameters privateKeyParams = rsa.ExportParameters(true);

                // 将公钥参数转换为字符串
                string publicKey = Convert.ToBase64String(publicKeyParams.Modulus) + "|" +
                                   Convert.ToBase64String(publicKeyParams.Exponent);

                // 将私钥参数转换为字符串
                string privateKey = Convert.ToBase64String(privateKeyParams.Modulus) + "|" +
                                    Convert.ToBase64String(privateKeyParams.Exponent) + "|" +
                                    Convert.ToBase64String(privateKeyParams.D) + "|" +
                                    Convert.ToBase64String(privateKeyParams.P) + "|" +
                                    Convert.ToBase64String(privateKeyParams.Q) + "|" +
                                    Convert.ToBase64String(privateKeyParams.DP) + "|" +
                                    Convert.ToBase64String(privateKeyParams.DQ) + "|" +
                                    Convert.ToBase64String(privateKeyParams.InverseQ);
                File.WriteAllText("E:/websocketchat.pem", publicKey);
                File.WriteAllText("E:/websocketchat.key", privateKey);
            }
        }
    }
}
