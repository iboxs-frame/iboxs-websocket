using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpSDK.msgHandle
{
    class Connected
    {
        private JObject sendAesKey()
        {
            string aesKey = Common.GenerateAes(32);
            Common.chatData.chatAesKey = aesKey;
            string publickey = Common.chatData.publicKey;
            string aes = encrypt.RsaHelper.EncryptWithPublicKey(aesKey, publickey);

            JObject me = new JObject();
            me["type"] = "device";
            me["license"] = "5555555"; //设备授权码

            JObject data = new JObject();
            data["aesKey"] = aes;
            data["me"] = me;
            return data;
        }

        public void publicKey(JObject jObject,JObject msg)
        {
            string publickey = (string)jObject["publickey"];
            Common.chatData.publicKey = publickey;
            JObject data=sendAesKey();
            JObject header = new JObject();
            Common.sendMsg("connected", "aeskey", (string)msg["msg_id"], data, header, false);
        }

        public void confirm(JObject data)
        {
            string userID = (string)data["user_id"];
            Thread timerThread = new Thread(() =>
            {
                Thread.Sleep(2*60*60*1000);
                JObject mdata = sendAesKey();
                JObject header = new JObject();
                Common.sendMsg("connected", "aeskey", "", mdata, header, false);
            });
            // 启动线程
            timerThread.Start();
        }
    }
}
