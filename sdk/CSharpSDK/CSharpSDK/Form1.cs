using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpSDK
{
    public partial class Form1 : Form
    {
        public static WebSocket webSocket;
        public Form1()
        {
            InitializeComponent();
            webSocket = new WebSocket();
        }

        void OnMessageReceived(string message)
        {
            this.receiveMsg(message);
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "链接")
            {
                bool res = webSocket.Connect(textBox1.Text.Trim(), OnMessageReceived);
                if (res)
                {
                    button1.Text = "断开";
                }
            }
            else
            {
                webSocket.Close();
                button1.Text = "链接";
            }
        }

        public void receiveMsg(string msg)
        {
            JObject jobj = MsgHandle.parseJson(msg);
            bool encrpt = (bool)jobj["encrypt"];

            string type = (string)jobj["type"];
            string sub_type = (string)jobj["sub_type"];
            JObject data = (JObject)jobj["data"];
            if (encrpt) //消息加密
            {

            }
            switch (type)
            {
                case "connected":
                    msgHandle.Connected connected = new msgHandle.Connected();
                    switch (sub_type){
                        case "publickey":
                            connected.publicKey(data,jobj);
                            break;
                    }
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string msg = textBox2.Text;
            if (msg.Length < 1)
            {
                return;
            }
            listBox1.Items.Add("发送消息：" + msg);
            webSocket.SendMsg(msg);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            encrypt.RSAHandle.createRSAKey();
        }

        private void Form1_Close(object sender, EventArgs e)
        {
            webSocket.Close();
            Application.Exit();
        }
    }
}
