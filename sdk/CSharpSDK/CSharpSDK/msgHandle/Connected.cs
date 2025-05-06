using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpSDK.msgHandle
{
    class Connected
    {
        public void publicKey(JObject jObject,JObject msg)
        {
            string publickey = (string)jObject["publickey"];
            string aesKey = Common.Generate(16);
            Common.chatData.chatAesKey = aesKey;
            string aes = encrypt.RsaHelper.EncryptWithPublicKey(aesKey, publickey);
            MessageBox.Show("加密秘钥"+aes);

            JObject me = new JObject();
            me["type"] = "device";
            me["license"] = "5555555"; //设备授权码

            JObject data = new JObject();
            data["aesKey"] = aes;
            data["me"]=me;
            JObject header = new JObject();
            Common.sendMsg("connected", "aeskey", (string)msg["msg_id"], data, header, false);
        }
    }
}
