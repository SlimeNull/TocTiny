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

            Opacity = 0;

            clientSelf = new TocTinyClient();

            this.Loaded += (sender, e) => AppStartup();
            Whole.MouseLeftButtonDown += (sender, e) => DragMove();
            CancelButton.PreviewMouseUp += (sender, e) => AppShutdown();

            ConnectButton.Click += Connect_Click;
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

                clientSelf.ConnectTo(new IPEndPoint(addresses[0], port), 1048576);                       // 缓冲区大小: 1mb
                ViewModel.AcceptButtonContent = "Connect";
            }
            catch (ThreadAbortException)
            {
                // nothing to do.
            }
            catch
            {
                Dispatcher.Invoke(() => ViewEx.ErrorMsg("Connection failed."));
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
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (waitThread != null && waitThread.IsAlive)
                waitThread.Abort();

            if (connectThread != null && connectThread.IsAlive)
            {
                connectThread.Abort();
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
            DoubleAnimation opacityAnimation = new DoubleAnimation()
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                From = 0,
                To = 1,
            };

            this.BeginAnimation(OpacityProperty, opacityAnimation);
        }
        private void AppShutdown()
        {
            //Environment.Exit(0);
            Application.Current.Shutdown();
            return;
            Duration duration = new Duration(TimeSpan.FromMilliseconds(200));
            DoubleAnimation opacityAnimation = new DoubleAnimation(1, 0, duration, FillBehavior.HoldEnd);
            opacityAnimation.Completed += (sender, e) => Application.Current.Shutdown();
            
            this.ShowInTaskbar = false;
            this.BeginAnimation(OpacityProperty, opacityAnimation);
        }
    }
}
