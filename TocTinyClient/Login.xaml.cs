using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Null.Library.EventedSocket;

namespace TocTiny
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        public MainChat ChatWindow;
        private SocketClient selfClient;
        private readonly string clientGuid;
        private readonly int bufferSize = 1048576;

        public string NickName => NickNameBox.Text;
        public string ClientGuid => clientGuid;
        public int BufferSize => bufferSize;

        public SocketClient SelfClient { get => selfClient; }

        public Login()
        {
            InitializeComponent();
            clientGuid = Guid.NewGuid().ToString();
            this.MouseDown += Login_MouseDown;
            NickNameBox.Text = Environment.UserName;
            AddressBox.Text = "chonet.xyz";
            PortBox.Text = "2020";
        }
        private void Login_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        Thread loginThread;
        Thread loginWaitThread;
        struct LoginThreadFuncParam
        {
            public IPAddress IPAddress;
            public int Port;
            public int BufferSize;
        }
        private void LoginThreadFunc(object parameters)
        {
            LoginThreadFuncParam param = (LoginThreadFuncParam)parameters;

            try
            {
                SelfClient.ConnectTo(new IPEndPoint(param.IPAddress, param.Port), param.BufferSize);
                SelfClient.ReceivedMsg += ChatWindow.SelfClient_ReceivedMsg;
                SelfClient.Disconnected += ChatWindow.SelfClient_Disconnected;

                Dispatcher.Invoke(() =>
                {
                    ChatWindow.Show();
                    this.Hide();
                });
            }
            catch
            {
                Dispatcher.Invoke(() =>
                {
                    loginWaitThread.Abort();

                    loginThread = null;
                    ConnectButton.IsEnabled = true;
                    ConnectButton.Content = "Connect";
                    MessageBox.Show("Communicate failed, please check your network connection.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }
        private void LoginWaitThreadFunc()
        {
            Thread.Sleep(5000);

            Dispatcher.Invoke(() =>
            {
                ConnectButton.IsEnabled = true;
                ConnectButton.Content = "Connect";
            });

            if (loginThread != null && loginThread.IsAlive)
            {
                loginThread.Abort();
                MessageBox.Show("Connection timeout, please check your network connection.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoginClick(object sender, RoutedEventArgs e)
        {
            if (loginThread == null || !loginThread.IsAlive)
            {
                selfClient = new SocketClient();
                ChatWindow = new MainChat(this);

                if (int.TryParse(PortBox.Text, out int port))
                {
                    IPAddress[] addresses = Dns.GetHostAddresses(AddressBox.Text);
                    if (addresses.Length > 0)
                    {
                        try
                        {
                            loginThread = new Thread(LoginThreadFunc);
                            loginThread.Start(new LoginThreadFuncParam()
                            {
                                IPAddress = addresses[0],
                                Port = port,
                                BufferSize = BufferSize
                            });

                            loginWaitThread = new Thread(LoginWaitThreadFunc);
                            loginWaitThread.Start();

                            ConnectButton.IsEnabled = false;
                            ConnectButton.Content = "Connecting";
                        }
                        catch
                        {
                            MessageBox.Show("Communicate failed, please check your network connection.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please input a correct network address!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please input a correct number as a port!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }
        private void CancelLoginClick(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
