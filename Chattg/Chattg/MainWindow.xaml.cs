using Rulelx;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Chattg
{
    /// <summary>
    /// Chattg
    /// </summary>
    public partial class MainWindow : Window
    {

        ObservableCollection<DataGridMessage> DataGridMessages = new ObservableCollection<DataGridMessage>();
        SqlDataAdapter dataAdapter;
        DataSet dataSet;
        //记录昵称
        public string loginname;

        //默认不允许登录
        bool Online = true;

        //用于通信的Socket
        Socket clientSocket;

        //ip和port的正则表达式
        Regex reip = new Regex("^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");
        Regex report = new Regex("^\\d{1,5}$");

        public MainWindow()
        {
            InitializeComponent();
        }
        //----------------------------------------------------------------------------------------------------------------------登录窗口----------------
        //设置图标按钮
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (setting.Visibility == Visibility.Hidden)
            {
                setting.Visibility = Visibility.Visible;
            }
            else {
                setting.Visibility = Visibility.Hidden;
            }

        }

        //返回按钮（跳转）
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            setting.Visibility = Visibility.Hidden;
        }

        //登录按钮
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //登录管理员账户            
            if (loginid.Text == "admin")
            {
                //验证管理员密码是否正确
                if (loginpassword.Password == "admin") {
                    MessageBox.Show("管理员账户登录成功", "提示", MessageBoxButton.OK);
                    Loginwindow.Visibility = Visibility.Hidden;
                    Adminwindow.Visibility = Visibility.Visible;
                    mainest.Height = 430;
                    mainest.Width = 840;
                }
                else
                    MessageBox.Show("管理员账户密码错误！！！", "提示", MessageBoxButton.OK);
            }

            //非管理员
            else
            {
                //设置Socket
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //检查ip,端口格式是否正确，如果正确则连接服务端 
                if (reip.IsMatch(ipaddress.Text) && report.IsMatch(ipport.Text))
                {
                    string ip = ipaddress.Text;
                    int p = Convert.ToInt32(ipport.Text);
                    try
                    {
                        //连接服务端
                        clientSocket.Connect(ip, p);
                        MessageBox.Show("连接服务端成功", "提示", MessageBoxButton.OK);
                        login();
                    }
                    //连接服务端失败
                    catch
                    {
                        MessageBox.Show("连接服务端失败", "提示", MessageBoxButton.OK);
                    }
                }
                else {
                    MessageBox.Show("ip或端口错误:" + ipaddress.Text + ":" + ipport.Text, "提示", MessageBoxButton.OK);
                }
            }
        }

        //登录连接服务端方法
        public void login()
        {
            //使用CURD类直连数据库
            CURD s = new CURD();

            //判断账号是否存在
            if (s.SearchNumber(loginid.Text))
            {
                //验证账号对应的密码
                if (s.SearchPassword(loginid.Text, loginpassword.Password)) {
                    //获取昵称   
                    loginname = s.SearchName(loginid.Text);

                    //发送验证消息给服务端判断是否在线
                    OnlineOrOffline();

                    //不在线则直接登录
                    if (Online == false) {
                        usernametext.Text = loginname;
                        usernametext.IsReadOnly = true;
                        Loginwindow.Visibility = Visibility.Hidden;
                        Chatwindow.Visibility = Visibility.Visible;
                        setting.Visibility = Visibility.Hidden;
                        mainest.Height = 430;
                        mainest.Width = 840;

                        //开启接收消息监听
                        Thread threadacceptmsg = new Thread(ClientMessage);
                        threadacceptmsg.IsBackground = true;
                        threadacceptmsg.Start();
                    }
                    //用户已在线
                    else
                    {
                        MessageBox.Show("该用户已经在线！！！！！", "提示", MessageBoxButton.OK);
                    }
                }
                //密码错误
                else {
                    MessageBox.Show("密码错误！", "提示", MessageBoxButton.OK);
                }
            }
            //账号不存在
            else
            {
                MessageBox.Show("账号不存在！  " + loginid.Text + "||" + loginpassword.Password, "提示", MessageBoxButton.OK);
            }
        }

        //登录窗口的注册按钮（跳转）
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Loginwindow.Visibility = Visibility.Hidden;
            Registerwindow.Visibility = Visibility.Visible;
            mainest.Height = 320;
            mainest.Width = 380;
        }
        //-----------------------------------------------------------------------------------------------------------------------注册窗口--------------------------
        //返回按钮（跳转）
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Registerwindow.Visibility = Visibility.Hidden;
            Loginwindow.Visibility = Visibility.Visible;
            mainest.Height = 360;
            mainest.Width = 440;
        }

        //注册按钮
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //使用CURD类直连数据库
            CURD s = new CURD();

            //禁止使用管理员账户进行注册
            if (registerid.Text == "admin")
            {
                MessageBox.Show("禁止使用管理员账户！", "提示", MessageBoxButton.OK);
            }
            else
            {
                //判断账号是否存在
                if (s.SearchNumber(registerid.Text))
                {
                    MessageBox.Show("该用户名已经存在！", "提示", MessageBoxButton.OK);
                }
                else
                {
                    //账号不存在则验证重复密码是否正确
                    if (registerpassword1.Password == registerpassword2.Password)
                    {
                        //将数据注册到数据库
                        s.Addmessage(registerid.Text, nickname.Text, registerpassword1.Password);
                        MessageBox.Show("注册成功！" + registerid.Text + registerpassword1.Password, "提示", MessageBoxButton.OK);
                    }
                    else
                    {
                        MessageBox.Show("重复密码不正确！", "提示", MessageBoxButton.OK);
                    }
                }
            }
        }
        //----------------------------------------------------------------------------------------------------------------------群聊窗口--------------------------
        //发送聊天信息按钮
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //将消息打包成Message类型
            Message message = new Message
            {
                type = 1,
                model = 1,
                data = loginname + ":" + sentmessagebox.Text
            };

            //转化为字节流
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, message);
            byte[] msg = stream.GetBuffer();

            //发送
            clientSocket.Send(msg);

            //清空发送消息栏
            sentmessagebox.Text = "";
        }

        //----------------------------------------------------------------------------------------------------------------------管理员窗口--------------------------
        //查询信息按钮
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            try
            {
                string connStr = @"Data Source=114.115.159.221,55500\SQLEXPRESS;Initial Catalog=User.Message;user id=sa;password=admin";
                SqlConnection conn = new SqlConnection(connStr);
                conn.Open();
                string sql = "select Id, UserNumber, UserName, UserPassword,UserResignDate from Userinfoes";
                dataAdapter = new SqlDataAdapter(sql, conn);
                dataSet = new DataSet();
                dataAdapter.Fill(dataSet, "MyData");
                AdminmessageDataGrid.DataContext = dataSet.Tables[0];
                conn.Close();
                savebutton.IsEnabled = true;
            }
            catch {
                MessageBox.Show("查询失败！！！");
            }

        }

        //保存修改按钮
        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlCommandBuilder builder = new SqlCommandBuilder(dataAdapter);
                dataAdapter.Update(dataSet, "MyData");
                MessageBox.Show("保存成功！");
            } catch
            {
                MessageBox.Show("保存失败！");
                savebutton.IsEnabled = false;
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------其他方法---------------------
        //使打印的消息到达文字到底部  textBox.ScrollToEnd();
        public void texttoend()
        {
            messagebox.SelectionStart = messagebox.Text.Length;
            messagebox.SelectionLength = 0;
            messagebox.Focus();
        }

        //普通用户监听接收消息
        public void ClientMessage()
        {
            while (true)
            {
                try
                {
                    //将接受字节流的转化为自定义Message类型
                    byte[] msg = new byte[1024];
                    int msgLen = clientSocket.Receive(msg);
                    MemoryStream stream = new MemoryStream(msg, 0, msgLen);
                    BinaryFormatter formatter = new BinaryFormatter();
                    Message message = formatter.Deserialize(stream) as Message;

                    //判断接收到消息的类型
                    switch (message.type)
                    {
                        case 1:
                            switch (message.model)
                            {
                                //(1,1)接收到的消息为聊天信息
                                case 1:
                                    Dispatcher.BeginInvoke(new Action(delegate {
                                        messagebox.Text += message.data;
                                        texttoend();
                                    }));
                                    break;

                                //(1,2)接收到的消息为验证反馈消息
                                case 2:
                                    break;

                                //(1,3)接收到的消息为广播的在线用户
                                case 3:
                                    //修改在线用户
                                    Dispatcher.BeginInvoke(new Action(delegate { OnlineUsersBox.Text = "在线成员:\n" + message.data; }));
                                    break;
                            }
                            break;
                        case 2:
                            switch (message.model)
                            {
                                //(1,1)接收断开连接消息
                                case 1:
                                    clientSocket.Close();
                                    Sendbutton.IsEnabled = false;
                                    break;
                                case 2:
                                    break;
                                case 3:
                                    break;
                            }
                            break;
                        case 3:
                            break;
                    }
                }
                //异常断开
                catch
                {
                    Dispatcher.BeginInvoke(new Action(delegate {
                        messagebox.Text += "\n" + "服务端断开连接了啊啊啊啊啊啊啊啊";
                        Sendbutton.IsEnabled = false;
                    }));
                    break;
                }
            }
        }

        //移动窗口
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            // 获取鼠标相对标题栏位置  
            Point position = e.GetPosition(mainest);
            // 如果鼠标位置在标题栏内，允许拖动  
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (position.X >= 0 && position.X < mainest.ActualWidth && position.Y >= 0 && position.Y < mainest.ActualHeight)
                {
                    this.DragMove();
                }
            }
        }

        //关闭窗口图标按钮
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //发送验证消息方法
        public void OnlineOrOffline()
        {
            //将消息打包成Message类型
            Message message = new Message
            {
                type = 1,
                model = 2,
                data = loginname
            };

            //转化为字节流
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, message);
            byte[] msg = stream.GetBuffer();

            //发送消息
            clientSocket.Send(msg);

            try
            {
                byte[] waitmsg = new byte[1024];
                
                //接收服务端的反馈信息，会挂起当前线程
                int waitmsgLen = clientSocket.Receive(waitmsg);

                //转化为自定义Message类型
                MemoryStream waitstream = new MemoryStream(waitmsg, 0, waitmsgLen);
                BinaryFormatter waitformatter = new BinaryFormatter();
                Message waitmessage = waitformatter.Deserialize(waitstream) as Message;

                //Message消息若为反馈消息，则修改登录状态
                if (waitmessage.type == 1 && waitmessage.model == 2) {
                    Online = (bool)waitmessage.data;
                }
                else {
                    MessageBox.Show("服务端反馈的消息错误！！！", "提示", MessageBoxButton.OK);
                }
            }
            //异常断开
            catch
            {
                MessageBox.Show("等待服务端返回验证失败！！！", "提示", MessageBoxButton.OK);
                return;
            }
        }

        //踢出聊天室按钮
        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            //将消息打包成Message类型
            Message message = new Message
            {
                type = 3,
                model = 1,
                data = adminkickipport.Text
            };

            //转化为字节流

            byte[] msg = MessagetoByte(message);

            //发送
            clientSocket.Send(msg);

            adminkickipport.Text = "";
        }

        //检查服务端是否在线按钮
        private void Connectbutton_Click(object sender, RoutedEventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string ip = adminip.Text;
            int p = Convert.ToInt32(adminport.Text);
            if (reip.IsMatch(ipaddress.Text) && report.IsMatch(ipport.Text))
            {
                try
                {
                    //测试连接服务端
                    clientSocket.Connect(ip, p);
                    MessageBox.Show("服务端在线", "提示", MessageBoxButton.OK);
                    //开启监听
                    Thread thread = new Thread(AdminMessage);
                    //thread.IsBackground = true;
                    thread.Start();

                    connectbutton.IsEnabled = false;
                    adminguangbobutton.IsEnabled = true;
                    kickbutton.IsEnabled = true;

                    //发送请求返回在线用户
                    Message message = new Message();
                    message.type = 3;
                    message.model = 2;
                    byte[] s = MessagetoByte(message);
                    clientSocket.Send(s);
                }
                catch
                {
                    MessageBox.Show("连接失败！！", "提示", MessageBoxButton.OK);
                    adminguangbobutton.IsEnabled = false;
                    kickbutton.IsEnabled = false;
                }
            }
            else {
                MessageBox.Show("ip或端口格式不正确  "+ip+":"+p, "提示", MessageBoxButton.OK);
                adminguangbobutton.IsEnabled = false;
                kickbutton.IsEnabled = false;
            }
        }

        //管理员监听消息
        public void AdminMessage()
        {
            while (true)
            {
                try
                {
                    //将接受字节流的转化为自定义Message类型
                    byte[] msg = new byte[1024];
                    int msgLen = clientSocket.Receive(msg);
                    MemoryStream stream = new MemoryStream(msg, 0, msgLen);
                    BinaryFormatter formatter = new BinaryFormatter();
                    Message message = formatter.Deserialize(stream) as Message;

                    //判断接收到消息的类型
                    switch (message.type)
                    {
                        case 1:
                            switch (message.model)
                            {
                                //管理员接收用户聊天消息
                                case 1:
                                    Dispatcher.BeginInvoke(new Action(delegate {
                                        chatmessage.Text += message.data;
                                        chatmessage.ScrollToEnd();
                                    }));
                                    break;
                                case 2:
                                    break;
                                case 3:
                                    break;
                            }
                            break;
                        case 2:
                            break;
                        case 3:
                            switch (message.model)
                            {
                                //管理员接收用户在线消息
                                case 1:
                                    Dispatcher.BeginInvoke(new Action(delegate { AdminmessageTextBox.Text = "在线成员:\n" + message.data; }));
                                    break;
                                case 2:
                                    MessageBox.Show((string)message.data);
                                    break;
                                case 3:
                                    break;
                            }
                            break;
                    }
                }
                //异常断开
                catch
                {
                    Dispatcher.BeginInvoke(new Action(delegate {
                        AdminmessageTextBox.Text += "\n" + "服务端断开连接!!!";
                        kickbutton.IsEnabled = false;
                        adminguangbobutton.IsEnabled = false;
                    }));
                    break;
                }
            }
        }

        //转化为字节流方法
        public byte[] MessagetoByte(Message message)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, message);
            byte[] msg = stream.GetBuffer();
            return msg;
        }

        //管理员发送广播
        private void Adminguangbobutton_Click(object sender, RoutedEventArgs e)
        {
            Message message = new Message();
            message.type = 3;
            message.model = 3;
            message.data = adminguangbo.Text;

            byte[] mesg = MessagetoByte(message);

            clientSocket.Send(mesg);
            adminguangbo.Text = "";
        }
    }
}

//各窗口尺寸
//Height="360" Width="440" login
//Height="320" Width="380" resign
//Height="430" Width="840" chatwindow/Adminwindow