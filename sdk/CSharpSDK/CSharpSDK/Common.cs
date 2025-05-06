using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSDK
{
    class Common
    {
        public class chatData
        {
            public static string chatAesKey = "";
            public static string chatAesIV = "IBOXSADMIN8414sh";
            public static string chatKey = "adsadasdwdasxsa"; //通信秘钥
        }

        public static string Generate(int length)
        {
            string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random Random = new Random();
            StringBuilder result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int index = Random.Next(Characters.Length);
                result.Append(Characters[index]);
            }

            return result.ToString();
        }

        public static bool sendMsg(string type,string sub_type,string reply_id,JObject data,JObject header,bool encrypt=true)
        {
            JObject obj = new JObject();
            obj["type"] = type;
            obj["sub_type"] = sub_type;
            obj["reply_id"] = reply_id;
            //obj["data"] = data;
            obj["header"] = header;
            obj["encrypt"] = encrypt;
            if (encrypt)
            {
                string dataJson = MsgHandle.toJson(data);
                string dataAes =CSharpSDK.encrypt.AesHandle.Encrypt(dataJson,Common.chatData.chatAesKey,Common.chatData.chatAesIV);
                obj["data"] = dataAes;
            }
            else
            {
                obj["data"] = data;
            }
            string msgID = Common.Generate(15);
            obj["msg_id"] = msgID;
            string sign = "";
            string now = Common.UnixTime().ToString();
            if (encrypt)
            {
                string d = (string)obj["data"];
                if (d.Length > 100)
                {
                    d = d.Substring(0, 100);
                }
                sign = Common.MD5(d + now + Common.MD5(Common.chatData.chatAesKey + Common.chatData.chatKey));
            }
            else
            {
                sign = Common.MD5(now + Common.MD5(Common.chatData.chatKey));
            }
            obj["sign"] = sign;
            obj["now"] = now;
            string message = MsgHandle.toJson(obj);
            return Form1.webSocket.SendMsg(message);
        }

        public static long UnixTime()
        {
            long unixTimestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            return unixTimestamp;
        }

        public static string MD5(string input)
        {
            using (MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
