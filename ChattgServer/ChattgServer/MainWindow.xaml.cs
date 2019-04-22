using Rulelx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace ChattgServer
{
    /// <summary>
    /// ChattgServer
    /// </summary>
    public partial class MainWindow : Window
    {
        //用于通信的Socket
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //用于记录管理员
        List<Socket> AdminList = new List<Socket>();
        //用于记录在线用户
        Dictionary<Socket, Member> clientList = new Dictionary<Socket, Member>();

        public MainWindow()
        {
            InitializeComponent();
        }

        //设定端口并且开启服务端
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Regex re = new Regex("^\\d{1,5}$");//判断输入的格式是否正确
            if (re.IsMatch(setport.Text))
            {
                int p = Convert.ToInt32(setport.Text);
                try
                {
                    //开启服务端
                    Server(p);

                    selectwindow.Visibility = Visibility.Hidden;
                    mainwindow.Visibility = Visibility.Visible;
                    mainest.Height = 460;
                    mainest.Width = 800;
                    duankou.Text += p;
                }
                catch
                {
                    MessageBox.Show("该端口被使用！！", "提示", MessageBoxButton.OK);
                }
             }
            else {
                MessageBox.Show("请输入正确的端口！！", "提示", MessageBoxButton.OK);
            }
        }

        //开启服务端方法
        public void Server(int portset)
        {
            //绑定IP地址和端口号
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, portset));

            //开始监听:设置最大可以同时连接多少个请求
            serverSocket.Listen(10);

            //提示
            Dispatcher.BeginInvoke(new Action(delegate { show.Text += "\n启动服务器成功 ";}));
            MessageBox.Show("启动服务器成功", "提示", MessageBoxButton.OK);

            //创建线程接收客户端连接
            Thread threadaccept = new Thread(Accept);
            threadaccept.IsBackground = true;
            threadaccept.Start();
        }

        //监听接收登录的客户端
        public void Accept()
        {
            //接收客户端的方法，会挂起当前线程
            Socket client = serverSocket.Accept();

            //记录ip地址
            IPEndPoint point = client.RemoteEndPoint as IPEndPoint;

            //打印消息
            Dispatcher.BeginInvoke(new Action(delegate {
                messageRichTextBox.Text += "\n" + point.Address + ":" + point.Port + " 请求连接 ";
                texttoend();
            }));

            //创建线程监听接听消息
            Thread threadacceptmsg = new Thread(SendMessage);
            threadacceptmsg.IsBackground = true;
            threadacceptmsg.Start(client);

            Accept();
        }

        //监听消息
        public void SendMessage(object obj)
        {
            //记录客户端
            Socket client = obj as Socket;
            IPEndPoint point = client.RemoteEndPoint as IPEndPoint;
            byte[] mesg = new byte[1024];

            //通过昵称判断该用户是否已经在线
            try
            {
                //转化为自定义Message类型
                int mesgLen = client.Receive(mesg);
                MemoryStream stream = new MemoryStream(mesg, 0, mesgLen);
                BinaryFormatter formatter = new BinaryFormatter();
                Message message = formatter.Deserialize(stream) as Message;

                //提示
                Dispatcher.BeginInvoke(new Action(delegate {
                    messageRichTextBox.Text += "\n" + "Server接收到消息message=" + message.type + "," + message.model + "," + (string)message.data;
                    texttoend();
                }));

                //判断接收到消息的类型
                switch (message.type)
                {
                    case 1:
                        switch (message.model)
                        {
                            //(1,1)接收到的消息为需要广播的消息
                            case 1:
                                BroadcastMessage("\n" +DateTime.Now.ToString()+"\n"+ message.data);
                                Dispatcher.BeginInvoke(new Action(delegate {
                                    messageRichTextBox.Text += "\n" + "Server执行（1,1）广播消息";
                                    texttoend();
                                }));
                        break;
                            //(1,2)接收到的消息为验证消息
                            case 2:                        
                                Dispatcher.BeginInvoke(new Action(delegate {
                                    messageRichTextBox.Text += "\n" + "Server即将执行（1,2）返回验证消息";
                                    texttoend();
                                }));

                                //返回验证消息给客户端
                                UserOnline(client, message);                                
                                break;
                            case 3:
                                break;
                        }
                        break;
                    case 2:
                        switch (message.model)
                        {
                            case 1://(2,1)响应时间
                                break;
                            case 2:
                                break;
                            case 3:
                                break;
                        }
                        break;
                    case 3:
                        switch (message.model)
                        {
                            //（3，1）管理员踢出用户
                            case 1:
                                AdminKickUser(client,message);
                                break;
                            //（3，1）管理员请求在线用户
                            case 2:
                                AdminRequesUser(client);
                                break;
                                //管理员发送广播
                            case 3:
                                BroadcastMessage("\n============="+DateTime.Now.ToString()+ "来自管理员广播：=============\n" + message.data+ "\n================================================");
                                break;
                        }
                        break;
                }
                SendMessage(obj);
            }
            catch
            {
                foreach (KeyValuePair<Socket, Member> item in clientList)
                {
                    if (item.Key==client)
                    {
                        //将用户改为离线
                        clientList.Remove(client);
                        //client.Close();
                        //AdminList.Remove(client);
                        //广播用户断开连接消息
                        BroadcastMessage("\n---------------------------------------------" + item.Value.name + "断开连接");
                        //广播目前在线的用户
                        BroadcastOnlinemenber();
                    }
                }
            }
        }

        //查询该用户是否已经登录
        public void UserOnline(Socket getclient,Message getmessage)
        {
            int n=0;
            //遍历查询是否在线
            foreach (Member k in clientList.Values)
            {
                if (k.name == (string)getmessage.data)
                { n++; }
                Dispatcher.BeginInvoke(new Action(delegate {
                    messageRichTextBox.Text += "\n对比结果：  " + k.name + "--" + getmessage.data;
                    texttoend();
                }));
            }

            //封装反馈Message
            Message message = new Message
            {
                type = 1,
                model = 2
            };
            if (n == 0) {
                //该用户尚不在线
                //记录登录用户信息
                message.data = false;
                Member member = new Member();
                member.name = (string)getmessage.data;
                IPEndPoint point = getclient.RemoteEndPoint as IPEndPoint;
                member.ip = point.ToString();//格式为  183.6.46.9:47715

                //添加在线用户
                clientList.Add(getclient, member);

                //转化为字节流
                byte[] msg = MessagetoByte(message);

                //发送反馈消息
                getclient.Send(msg);

                //广播在线成员
                BroadcastOnlinemenber();

                //广播用户连接登录消息
                BroadcastMessage("\n----------------------------------------------" + getmessage.data + "连接登录");
            }
            else {
                //该用户已经在线
                message.data = true;

                //转化为字节流
                byte[] msg = MessagetoByte(message);

                //发送反馈消息
                getclient.Send(msg);
            }            
            //提示
            Dispatcher.BeginInvoke(new Action(delegate {
                messageRichTextBox.Text += "\n" + "返回验证消息(1,2)message=" + message.type + "," + message.model + "," + message.data;
                texttoend();
            }));
        }

        //广播消息方法
        public void BroadcastMessage(object mesg)
        {
            //将需要广播的消息打包成Message类型
            Message message = new Message
            {
                type = 1,
                model = 1,
                data = mesg
            };

            //转化为字节流
            byte[] msg = MessagetoByte(message);

            //提示
            Dispatcher.BeginInvoke(new Action(delegate {
                messageRichTextBox.Text += "\n" + "广播消息(1,1)message=" + message.type + "," + message.model + "," + message.data;
                texttoend();
            }));

            //广播给用户
            try
            {
                foreach (var client in clientList.Keys)
                {
                    client.Send(msg);
                }
            }
            catch
            {
                Dispatcher.BeginInvoke(new Action(delegate {
                    messageRichTextBox.Text += "\n无人在线";
                    texttoend();
                }));
            }

            try
            {
                foreach (var adminclient in AdminList)
                {
                    adminclient.Send(msg);
                }
            }
            catch
            {
                Dispatcher.BeginInvoke(new Action(delegate {
                    messageRichTextBox.Text += "\n无管理员在线";
                    texttoend();
                }));
            }
        }

        //广播目前在线的用户(1,3)//广播给管理员为（3,1）
        public void BroadcastOnlinemenber() 
            {
            //将需要广播的消息打包成Message类型
            Message message = new Message
            {
                type = 1,
                model = 3
            };
            string Onlinemenber = "";
                foreach (var user in clientList.Values)
                {
                    Onlinemenber += user.ip+ "||" + user.name + "\n";
                };
                Dispatcher.BeginInvoke(new Action(delegate { show.Text = "在线用户：\n"+Onlinemenber; }));
                message.data = Onlinemenber;

            //转化为字节流
            byte[] msg = MessagetoByte(message);

            //提示
                Dispatcher.BeginInvoke(new Action(delegate {
                messageRichTextBox.Text += "\n广播给用户在线用户消息";
                texttoend();
            }));

            //发送广播给每个在线用户
            try
            {        
                foreach (var client in clientList.Keys)
                {
                    client.Send(msg);
                }
            }
            catch
            {
                Dispatcher.BeginInvoke(new Action(delegate { messageRichTextBox.Text += "\n无用户在线"; }));
            }
            try
            {
                foreach (var adminclient in AdminList)
                {
                    AdminRequesUser(adminclient);
                    Dispatcher.BeginInvoke(new Action(delegate { messageRichTextBox.Text += "\n广播给管理员（在线用户消息）"; }));
                }
            }
            catch
            {
                Dispatcher.BeginInvoke(new Action(delegate { messageRichTextBox.Text += "\n广播给管理员（在线用户消息）时，无管理员在线"; }));
            }
        }

        //刷新服务端在线用户按钮
        private void Fefesh_Click(object sender, RoutedEventArgs e)
        {
            string Onlinemenber = "";
            foreach (var user in clientList)
            {
                Onlinemenber += user.Key.RemoteEndPoint.AddressFamily+"||"+user.Value + "\n";
            };
            Dispatcher.BeginInvoke(new Action(delegate {show.Text = "在线用户：\n" + Onlinemenber;}));
        }

        //向客户端发送广播按钮
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BroadcastMessage("\n------------------------------------------------广播：" + guangbo.Text);
            guangbo.Text = "";
        }

        //使打印的消息到达文字到底部
        public void texttoend()
        {
            messageRichTextBox.SelectionStart = messageRichTextBox.Text.Length;
            messageRichTextBox.SelectionLength = 0;
            messageRichTextBox.Focus();
        }

        //转化为字节流方法
        public byte[] MessagetoByte(Message message) {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, message);
            byte[] msg = stream.GetBuffer();
            return msg;
        }

        //定义Member作为clientlist的value,用于储存ip和昵称
        public class Member {
            public string ip;
            public string name;
        }
        
        //管理员请求踢出用户
        public void AdminKickUser(Socket adminclient, Message messageip) {

            Message message = new Message();
            message.type = 3;
            message.model = 2;
            try
            {
                var firstKey = clientList.FirstOrDefault(q => q.Value.ip == (string)messageip.data).Key;  //通过Value的ip 找 key
                var firstvalue= clientList.FirstOrDefault(q => q.Key== firstKey);  //get Value

                Message close = new Message();
                close.type = 2;
                close.model = 1;
                close.data = "";
                byte[] ss = MessagetoByte(close);
                firstKey.Send(ss);
                firstKey.Close();
                //广播断开连接
                BroadcastMessage("\n---------------------------------------------" + firstvalue.Value.name + "断开连接");
                clientList.Remove(firstKey);
                message.data = "踢出成功！！！";
                BroadcastOnlinemenber();
            }
            catch {
                message.data = "踢出失败！！！";
                BroadcastOnlinemenber();
            }
            byte[] s = MessagetoByte(message);
            adminclient.Send(s);
        }

        //给管理员反馈在线用户消息（3,1）
        public void AdminRequesUser(Socket adminclient)
        {
            //将在线用户消息打包成Message类型
            Message message = new Message
            {
                type = 3,
                model = 1
            };
            string Onlinemenber = "";
                foreach (var user in clientList.Values)
                {
                    Onlinemenber += user.ip + "||" + user.name + "\n";
                };

            if (Onlinemenber == "")
                Onlinemenber = "无人在线";

            Dispatcher.BeginInvoke(new Action(delegate {
                messageRichTextBox.Text += "\n" + "给管理员反馈在线用户消息(3,1)message=" + message.type + "," + message.model + "," + message.data;
                texttoend();
            }));

            message.data = Onlinemenber;
            AdminList.Add(adminclient);

            //转化为字节流
            byte[] msg = MessagetoByte(message);

            adminclient.Send(msg);
        }

    }
}
//各窗口尺寸
//Height="460" Width="800"
//Height="360" Width="460"