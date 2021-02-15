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

namespace TocTiny.View
{
    public partial class MainChat : Window
    {
        public readonly Login WindowParent;
        public TocTinyClient ClientSelf { get => WindowParent.ClientSelf; }

        private bool forceClose = false;
        private bool keepMain = false;

        public MainChat(Login loginWindow)
        {
            InitializeComponent();

            loginWindow.MouseDown += (sender, e) => DragMove();   // 拖动移动
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = WindowParent.Left + (WindowParent.Width - this.Width) / 2;
            this.Top = WindowParent.Top + (WindowParent.Height - this.Height) / 2;
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
                        {
                            AppendTextMessage(recvPackage.Name, recvPackage.Content, HorizontalAlignment.Right);
                        }
                        else
                        {
                            AppendTextMessage(recvPackage.Name, recvPackage.Content, HorizontalAlignment.Left);
                        }
                        Dispatcher.Invoke(() => { ChatScroller.ScrollToBottom(); });
                        break;
                    case ConstDef.Verification:
                        AppendAnnouncement(recvPackage.Content);
                        Dispatcher.Invoke(() => { ChatScroller.ScrollToBottom(); });
                        break;
                    case ConstDef.ImageMessage:
                        //暂时不处理, 保留
                        if (recvPackage.ClientGuid == WindowParent.ClientSelf.ClientGuid)
                        {
                            AppendImageMessage(recvPackage.Name, recvPackage.Content, HorizontalAlignment.Right);
                        }
                        else
                        {
                            AppendImageMessage(recvPackage.Name, recvPackage.Content, HorizontalAlignment.Left);
                        }
                        break;
                    case ConstDef.DrawAttention:
                        if (recvPackage.ClientGuid == WindowParent.ClientSelf.ClientGuid)
                        {
                            AppendAnnouncement($"Your try to draw others' attention.");
                        }
                        else
                        {
                            AppendAnnouncement($"{recvPackage.Name} try to draw your attention.");
                        }
                        break;
                    case ConstDef.ChangeChannelName:
                        SetCurrentChannelName(recvPackage.Content);
                        Dispatcher.Invoke(() => { ChatScroller.ScrollToBottom(); });
                        break;
                    case ConstDef.ReportChannelOnline:
                        AppendAnnouncement(recvPackage.Content);
                        Dispatcher.Invoke(() => { ChatScroller.ScrollToBottom(); });
                        break;
                    default:
                        // ( 在规定之外的消息 ) 以后再弄详细处理
                        break;
                }
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
            //Grid.SetRow(control, ChatMsgContainer.RowDefinitions.Count);
            //ChatMsgContainer.RowDefinitions.Add(new RowDefinition()
            //{
            //    Height = GridLength.Auto
            //});
            //ChatMsgContainer.Children.Add(control);
            ChatMsgContainer.Children.Add(control);
        }
        private void AppendTextMessage(string name, string content, HorizontalAlignment align)
        {
            this.Dispatcher.Invoke(() =>
            {
                Grid newMsg = MainChatEx.GenTextMsgGrid(name, content, align);
                AppendMsgControl(newMsg);
            });
        }
        private void AppendImageMessage(string name, string baseImg, HorizontalAlignment align)
        {
            this.Dispatcher.Invoke(() =>
            {
                Grid newMsg = MainChatEx.GenImageMsgGrid(name, baseImg, align);
                AppendMsgControl(newMsg);
            });
        }
        private void AppendAnnouncement(string content)
        {
            this.Dispatcher.Invoke(() =>
            {
                Label newMsg = new Label()
                {
                    Content = content,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                AppendMsgControl(newMsg);
            });
        }
        private void SetCurrentChannelName(string name)
        {
            ChannelName.Content = name;
            this.Title = $"{name} - TOC Tiny";
        }

        private void Image_DragLeave(object sender, DragEventArgs e)
        {

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
                WindowParent.Close();
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

        OpenFileDialog ofd;
        private void SendPicture_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ofd == null)
            {
                ofd = new OpenFileDialog();
                ofd.Title = "Open a image file for sending";
                ofd.Filter = "Normal image format|*.jpg;*.jpeg;*.png;*.gif;*.bmp|JPEG|*.jpg;*.jpeg|PNG|*.png|GIF|*.gif|Bitmap|*.bmp|Other|*.*";
                ofd.CheckFileExists = true;
                ofd.Multiselect = false;
            }
            if (ofd.ShowDialog().GetValueOrDefault(false))
            {
                System.Drawing.Image imgForSending = null;
                try
                {
                    imgForSending = System.Drawing.Image.FromFile(ofd.FileName);
                    SendImage(imgForSending);
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
            if (e.Data.GetDataPresent(DataFormats.Bitmap) || e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                e.Effects = DragDropEffects.Copy;
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
                }
                catch
                {
                    MessageBox.Show("Not.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
