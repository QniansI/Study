using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        //创建1个客户端套接字和1个负责监听服务端请求的线程  
        static Thread ThreadClient = null;
        static Socket SocketClient = null;
        //服务器ip
        IPAddress ipadrSer ;
        //服务器端口
        int portSer = 8080;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string s = textBox3.Text.ToString().Trim();
            ClientSendMsg(s);
        }

        private void btnConn_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox2.Text.ToString().Trim()))
            {
                ipadrSer = IPAddress.Loopback;
            }
            else
            {
                try
                {
                    ipadrSer = IPAddress.Parse(textBox2.Text.ToString().Trim());
                }
                catch (Exception ep)
                {
                    MessageBox.Show("请输入正确的IP后重试");
                }
            }
            IPEndPoint ipe = new IPEndPoint(ipadrSer, portSer);
            SocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                //客户端套接字连接到网络节点上，用的是Connect  
                SocketClient.Connect(ipe);
            }
            catch (Exception)
            {
                Console.WriteLine("连接失败！\r\n");
                Console.ReadLine();
                return;
            }
            label2.Text = "本机通讯地址：" + SocketClient.LocalEndPoint.ToString();
            ThreadClient = new Thread(RevSer);
            ThreadClient.IsBackground = true;
            ThreadClient.Start();

        }
        //接受服务器消息
        private void RevSer()
        {
            while (true)
            {
                //用于临时性存储接收到的消息
                Byte[] bytesFrom = new Byte[4096];
                try
                {
                    int len = SocketClient.Receive(bytesFrom);    //获取客户端发来的信息
                    String tmp = Encoding.UTF8.GetString(bytesFrom, 0, len);  //将字节流转换成字符串
                    textBox1.Text+= tmp + "\r\n";
                }
                catch (Exception ep)
                {
                    
                }
            }
        }

        //发送字符信息到服务端的方法  
        public static void ClientSendMsg(string sendMsg)
        {
            //将输入的内容字符串转换为机器可以识别的字节数组     
            byte[] arrClientSendMsg = Encoding.UTF8.GetBytes(sendMsg);
            //调用客户端套接字发送字节数组    
            SocketClient.Send(arrClientSendMsg);
        }    
    }
}
