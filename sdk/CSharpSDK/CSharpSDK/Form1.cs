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
        WebSocket webSocket;
        public Form1()
        {
            InitializeComponent();
            this.webSocket = new WebSocket();
        }

        void OnMessageReceived(string message)
        {
            listBox1.Items.Add("接收到消息:" + message);
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "链接")
            {
                bool res = this.webSocket.Connect(textBox1.Text.Trim(), OnMessageReceived);
                if (res)
                {
                    button1.Text = "断开";
                }
            }
            else
            {
                this.webSocket.Close();
                button1.Text = "链接";
            }
        }

        public static void receiveMsg(string msg)
        {
            MessageBox.Show("接受到消息", msg);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string msg = textBox2.Text;
            if (msg.Length < 1)
            {
                return;
            }
            listBox1.Items.Add("发送消息：" + msg);
            this.webSocket.SendMsg(msg);
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
