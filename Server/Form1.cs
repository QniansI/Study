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

namespace Server
{
  
    public partial class Form1 : Form
    {
        //保存多个客户端的通信套接字
        public static Dictionary<String, Socket> clientList = new Dictionary<string, Socket>();
        //申明一个监听套接字 
        Socket serverSocket = null;
        //默认一个主机监听的IP
        IPAddress ipadr;
        //监听端口
        int port = 8080;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox2.Text.ToString().Trim()))
            {
                ipadr = IPAddress.Loopback;
            }
            else
            {
                try
                {
                    ipadr = IPAddress.Parse(textBox2.Text.ToString().Trim());
                }
                catch (Exception ep)
                {
                    MessageBox.Show("请输入正确的IP后重试");
                }
            }
            //将IP地址和端口号绑定到网络节点point上 
            IPEndPoint ipe = new IPEndPoint(ipadr, port);
            //实例监听套接字
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //监听绑定的网络节点  
            serverSocket.Bind(ipe);
            //设置最大连接数
            serverSocket.Listen(100);

            //负责监听客户端的线程:创建一个监听线程  
            Thread threadwatch = new Thread(ListenConnection);
            //将窗体线程设置为与后台同步，随着主线程结束而结束  
            threadwatch.IsBackground = true;
            //启动线程     
            threadwatch.Start();
            label2.Text = "监听IP为：" + ipadr.ToString();
        }
        //监听
        private void ListenConnection()
        {
            Socket connection = null;
            while (true)
            {
                try
                {
                    connection = serverSocket.Accept();
                }
                catch (Exception ex)
                {
                    //提示套接字监听异常     
                    Console.WriteLine(ex.Message);
                    break;
                }
                //客户端网络结点号  
                string remoteEndPoint = connection.RemoteEndPoint.ToString();
                //添加客户端信息  
                clientList.Add(remoteEndPoint, connection);

                textBox1.BeginInvoke(new Action(() =>
                {
                    textBox1.Text+= remoteEndPoint.ToString()+ "已连接\r\n";
                }));
                string sendmsg = remoteEndPoint.ToString() + "已连接\r\n";
                SendMsg(sendmsg);
                //创建一个通信线程      
                Thread thread = new Thread(revMsg);
                //设置为后台线程，随着主线程退出而退出 
                thread.IsBackground = true;
                //启动线程     
                thread.Start(connection);
            }
        }
        //发送消息
        private void SendMsg(string s)
        {
            byte[] arrSendMsg = Encoding.UTF8.GetBytes(s);
            foreach (var i in clientList)
            {
                i.Value.Send(arrSendMsg);
            }
        }
        //接受消息并发送
        private void revMsg(object socketclientpara)
        {
            Socket socketServer = socketclientpara as Socket;
            while (true)
            {
                //用于临时性存储接收到的消息
                Byte[] bytesFrom = new Byte[4096];
                try
                {
                    int len = socketServer.Receive(bytesFrom);    //获取客户端发来的信息
                    String tmp = Encoding.UTF8.GetString(bytesFrom, 0, len);  //将字节流转换成字符串
                    string c = socketServer.RemoteEndPoint.ToString();
                    string s = c+":"+tmp;
                    SendMsg(s);
                }
                catch (Exception ep)
                {

                }
            }
        }
    }
}
