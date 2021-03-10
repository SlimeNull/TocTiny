using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TocTiny.Client.ViewExFunc;
using TocTiny.Core;

namespace TocTiny.View
{
    public partial class Login : Window
    {
        MainChat chatWindow;
        TocTinyClient clientSelf;

        public MainChat ChatWindow { get => chatWindow; }
        public TocTinyClient ClientSelf { get => clientSelf; }

        public Login()
        {
            InitializeComponent();

            clientSelf = new TocTinyClient();

            Opacity = 0;
            this.Loaded += (sender, e) => AppStartup();
            Whole.MouseLeftButtonDown += (sender, e) => DragMove();
            CancelButton.PreviewMouseUp += (sender, e) => AppShutdown();

            ConnectButton.Click += ConnectButton_Click;
        }

        Thread connectThread;
        Thread waitThread;
        private void ConnectAction()
        {
            try
            {
                if (!int.TryParse(ViewModel.Port, out int port))
                {
                    Dispatcher.Invoke(() => ViewEx.ErrorMsg("Remote port must be a number"));
                    goto ExitConnection;
                }
                IPAddress[] addresses = Dns.GetHostAddresses(ViewModel.IPAddress);
                if (addresses.Length == 0)
                {
                    Dispatcher.Invoke(() => ViewEx.ErrorMsg("Server address is not available"));
                    goto ExitConnection;
                }

                clientSelf.UserName = ViewModel.Nickname;
                clientSelf.ConnectTo(new IPEndPoint(addresses[0], port));                       // 缓冲区大小: 1mb
                ViewModel.AcceptButtonContent = "Connect";

                Dispatcher.Invoke(() =>
                {
                    if (chatWindow != null)
                        chatWindow.Close();

                    this.Hide();
                    chatWindow = new MainChat(this);
                    chatWindow.Show();
                });
            }
            catch (ThreadAbortException)
            {
                // nothing to do.
            }
            catch (Exception e)
            {
                Dispatcher.Invoke(() => ViewEx.ErrorMsg($"Connection failed. {e.Message}"));
            }

            ExitConnection:
            if (waitThread.IsAlive)
                waitThread.Abort();
        }
        private void WaitConnectAction()
        {
            try
            {
                Thread.Sleep(5000);

                if (connectThread.IsAlive)
                {
                    connectThread.Abort();
                    connectThread = null;
                    ViewModel.AcceptButtonContent = "Connect";
                    Dispatcher.Invoke(() => ViewEx.ErrorMsg("Connection timeout."));
                }
            }
            catch { }
            finally
            {

                ViewModel.AcceptButtonContent = "Connect";
            }
        }
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (waitThread != null && waitThread.IsAlive)
            {
                waitThread.Abort();
                waitThread = null;
            }

            if (connectThread != null && connectThread.IsAlive)
            {
                connectThread.Abort();
                connectThread = null;
                ViewModel.AcceptButtonContent = "Connect";
            }
            else
            {
                waitThread = new Thread(WaitConnectAction);
                connectThread = new Thread(ConnectAction);

                waitThread.Start();
                connectThread.Start();

                ViewModel.AcceptButtonContent = "Abort";
            }
        }

        private void AppStartup()
        {
            Duration duration = new Duration(TimeSpan.FromMilliseconds(100));
            DoubleAnimation opacityAnimation = new DoubleAnimation(0.0, 1.0, duration);

            this.BeginAnimation(OpacityProperty, opacityAnimation);
        }
        private void AppShutdown()
        {
            Duration duration = new Duration(TimeSpan.FromMilliseconds(100));
            DoubleAnimation opacityAnimation = new DoubleAnimation(1.0, 0.0, duration, FillBehavior.HoldEnd);
            opacityAnimation.Completed += (sender, e) => Application.Current.Shutdown();
            
            this.BeginAnimation(OpacityProperty, opacityAnimation);
        }
    }
}
