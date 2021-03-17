using CHO.Json;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Windows.Shapes;
using TocTiny.Public;
using TocTiny.Core;
using TocTiny.Client.ViewExFunc;
using System.Windows.Media.Animation;
using TocTiny.Client.View;

namespace TocTiny.View
{
    public partial class MainChat : Window
    {
        public readonly Login WindowParent;
        public TocTinyClient ClientSelf;

        private bool forceClose = false;
        private bool keepMain = false;

        public MainChat(Login loginWindow)
        {
            InitializeComponent();

            WindowParent = loginWindow;
            ClientSelf = loginWindow.ClientSelf;
            loginWindow.MouseLeftButtonDown += (sender, e) => DragMove();   // 拖动移动

            ClientSelf.PackageReceived += ClientSelf_PackageReceived;
            ClientSelf.ConnectionLost += ClientSelf_Disconnected;

            this.Left = WindowParent.Left + (WindowParent.Width - this.Width) / 2;
            this.Top = WindowParent.Top + (WindowParent.Height - this.Height) / 2;
        }

        private void ClientSelf_Disconnected(object sender, EventArgs e)
        {
            MessageBox.Show("Connection lost.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Dispatcher.Invoke(() =>
            {
                WindowParent.Show();
                CloseSelf();
            });
        }

        private void ClientSelf_PackageReceived(object sender, PackageReceivedEventArgs e)
        {
            DealPackage(e.Package);
        }


        /* -------------------------------------------------------------------------------------------------------------------------- */

        #region Message Factory
        private void DealPackage(TransPackage recvPackage)
        {
            if (recvPackage.PackageType != ConstDef.HeartPackage)
            {
                switch (recvPackage.PackageType)
                {
                    case ConstDef.NormalMessage:
                        if (recvPackage.ClientGuid == ClientSelf.ClientGuid)
                            AppendMessage(recvPackage.Name, recvPackage.Content, null, HorizontalAlignment.Right);
                        else
                            AppendMessage(recvPackage.Name, recvPackage.Content, null, HorizontalAlignment.Left);
                        break;
                    case ConstDef.Verification:
                        AppendMessage(null, recvPackage.Content, null, HorizontalAlignment.Center);
                        break;
                    case ConstDef.ImageMessage:
                        if (MainChatEx.TryGetImageSource(recvPackage.Content, out BitmapImage outsrc))
                            if (recvPackage.ClientGuid == WindowParent.ClientSelf.ClientGuid)
                                AppendMessage(recvPackage.Name, null, outsrc, HorizontalAlignment.Right);
                            else
                                AppendMessage(recvPackage.Name, null, outsrc, HorizontalAlignment.Left);
                        else
                            AppendMessage(recvPackage.Name, "图片消息解析失败", null, HorizontalAlignment.Center);
                        break;
                    case ConstDef.DrawAttention:
                        if (recvPackage.ClientGuid == WindowParent.ClientSelf.ClientGuid)
                            AppendMessage(null, "Your try to draw others' attention.", null, HorizontalAlignment.Center);
                        else
                            AppendMessage(null, $"{recvPackage.Name} try to draw your attention.", null, HorizontalAlignment.Center);
                        break;
                    case ConstDef.ChangeChannelName:
                        AppendMessage(null, recvPackage.Content, null, HorizontalAlignment.Center);
                        break;
                    case ConstDef.ReportChannelOnline:
                        AppendMessage(null, recvPackage.Content, null, HorizontalAlignment.Center);
                        break;
                    default:
                        // ( 在规定之外的消息 ) 以后再弄详细处理
                        break;
                }
                Dispatcher.Invoke(() => { ChatScroller.ScrollToBottom(); });
            }
        }
        private void SendInputtedText()
        {
            if (ClientSelf.TrySendText(InputBox.Text))
            {
                InputBox.Clear();
            }
            else
            {
                MessageBox.Show("Message send failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SendImage(System.Drawing.Image img)
        {
            if (ClientSelf.TrySendImage(img))
            {
                InputBox.Clear();
            }
            else
            {
                MessageBox.Show("Message send failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Functions
        private void AppendMsgControl(UIElement control)
        {
            Dispatcher.Invoke(()=>
            {
                ChatMsgContainer.Children.Add(control);
            });
        }
        private void AppendMessage(string title, string text, ImageSource image, HorizontalAlignment align)
        {
            Dispatcher.Invoke(() =>
            {
                ChatMsgContainer.Children.Add(new MsgBox()
                {
                    Title = title,
                    TextContent = text,
                    ImageContent = image,
                    HorizontalAlignment = align,
                    Margin = new Thickness(0, 5, 0, 5)
                });
            });
        }
        private void AppendTextMessage(string name, string content, HorizontalAlignment align)
        {
            Grid newMsg = MainChatEx.GenTextMsgGrid(name, content, align);
            AppendMsgControl(newMsg);
        }
        private void AppendImageMessage(string name, string baseImg, HorizontalAlignment align)
        {
            Grid newMsg = MainChatEx.GenImageMsgGrid(name, baseImg, align);
            AppendMsgControl(newMsg);
        }
        private void AppendAnnouncement(string content)
        {
            Label newMsg = new Label()
            {
                Content = content,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            AppendMsgControl(newMsg);
        }
        private void SetCurrentChannelName(string name)
        {
            ChannelName.Content = name;
            this.Title = $"{name} - TOC Tiny";
        }
        #endregion

        #region Socket Core

        #endregion

        #region Window UI Events
        private void Send_Click(object sender, RoutedEventArgs e)
        {
            SendInputtedText();
        }
        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.KeyboardDevice.IsKeyDown(Key.LeftCtrl))
            {
                SendInputtedText();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!forceClose && MessageBox.Show("Exit?", "Tips", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }

            if (!keepMain)
            {
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
        }
        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Stickers.IsMouseOver)
            {
                Stickers.Visibility = Visibility.Collapsed;
            }
        }
        private void OpenSticker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Stickers.Visibility == Visibility.Collapsed)
            {
                Stickers.Visibility = Visibility.Visible;
            }
            else
            {
                Stickers.Visibility = Visibility.Collapsed;
            }
        }
        private void Sticker_Selected(object sender, SelectionChangedEventArgs e)
        {
            ListView source = (ListView)sender;

            if (source.SelectedIndex > 0)
            {
                ListViewItem item = (ListViewItem)source.SelectedItem;
                InputBox.SelectedText = (item.Content.ToString());
                Stickers.Visibility = Visibility.Collapsed;

                InputBox.Focus();
                InputBox.SelectionStart += InputBox.SelectionLength;
                InputBox.SelectionLength = 0;

                source.SelectedIndex = -1;
            }
        }
        private void DrawAttention_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ClientSelf.TryDrawAttention();
        }
        private void OnlineInfo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ClientSelf.TryRequestOnlineInfo();
        }                    // 

        OpenFileDialog ofd = new OpenFileDialog()
        {
            Title = "Open a image file for sending",
            Filter = "Normal image format|*.jpg;*.jpeg;*.png;*.gif;*.bmp|JPEG|*.jpg;*.jpeg|PNG|*.png|GIF|*.gif|Bitmap|*.bmp|Other|*.*",
            CheckFileExists = true,
            Multiselect = false,
        };
        private void SendPicture_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ofd.ShowDialog().GetValueOrDefault(true))
            {
                System.Drawing.Image imgForSending = null;
                try
                {
                    imgForSending = System.Drawing.Image.FromFile(ofd.FileName);
                    SendImage(imgForSending);
                    imgForSending.Dispose();
                }
                finally
                {
                    if (imgForSending != null)
                    {
                        imgForSending.Dispose();
                    }
                }
            }

        }                   // 发送图片 按钮按下    (打开文件框)
        private void InputBox_DragEnter(object sender, DragEventArgs e)
        {
            var data = e.Data;
            if (e.Data.GetDataPresent(DataFormats.Bitmap) || e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }                             // 拖拽进入输入框事件
        private void InputBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                object bmp = e.Data.GetData(DataFormats.Bitmap);
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                Array arr = (Array)e.Data.GetData(DataFormats.FileDrop);

                string[] files = new string[arr.Length];
                arr.CopyTo(files, 0);

                System.Drawing.Image img = null;
                try
                {
                    img = System.Drawing.Image.FromFile(files[0]);
                    SendImage(img);
                    img.Dispose();
                }
                catch
                {
                    MessageBox.Show("Not a valid image file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }                                  // 输入框拖拽结束事件
        private void InputBox_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Bitmap))
            {
                object bmp = e.DataObject;
            }
        }                  // 输入框粘贴事件

        #endregion

        public void CloseSelf()
        {
            forceClose = true;
            keepMain = true;
            AppShutdownAnimation((s, e) => this.Close());
        }
        public void CloseAll()
        {
            forceClose = true;
            keepMain = false;
            AppShutdownAnimation((s, e) => Application.Current.Shutdown());
        }
        private void AppStartupAnimation(EventHandler callback)
        {
            Duration duration = new Duration(TimeSpan.FromMilliseconds(100));
            DoubleAnimation opacityAnimation = new DoubleAnimation(0.0, 1.0, duration);
            opacityAnimation.Completed += callback;

            this.BeginAnimation(OpacityProperty, opacityAnimation);
        }
        private void AppShutdownAnimation(EventHandler callback)
        {
            Duration duration = new Duration(TimeSpan.FromMilliseconds(100));
            DoubleAnimation opacityAnimation = new DoubleAnimation(1.0, 0.0, duration, FillBehavior.HoldEnd);
            opacityAnimation.Completed += callback;

            this.BeginAnimation(OpacityProperty, opacityAnimation);
        }
    }
}
