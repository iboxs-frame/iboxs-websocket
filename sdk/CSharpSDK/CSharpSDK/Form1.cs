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

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "链接")
            {
                bool res = this.webSocket.Connect(textBox1.Text.Trim());
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
    }
}
