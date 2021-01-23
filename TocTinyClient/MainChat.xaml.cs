using CHO.Json;
using Microsoft.Win32;
using Null.Library.EventedSocket;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Drawing;

namespace TocTiny
{
    /// <summary>
    /// MainChat.xaml 的交互逻辑
    /// </summary>
    public partial class MainChat
    {
        public readonly SocketClient SelfClient;
        public readonly Login WindowParent;
        private readonly byte[] heartPackageData;
        private readonly byte[] getOnlineInfoPackageData;

        private bool forceClose = false;
        private bool keepMain = false;

        private readonly int btimeout = 1000, cinterval = 1000;
        private DateTime wroteTime;

        private MemoryStream partsBuffer;

        public MainChat(Login loginWindow)
        {
            InitializeComponent();

            WindowParent = loginWindow;
            SelfClient = loginWindow.SelfClient;

            heartPackageData = Encoding.UTF8.GetBytes(
                JsonData.ConvertToText(
                    JsonData.Create(new TransPackage()
                    {
                        Name = WindowParent.NickName,
                        Content = null,
                        ClientGuid = WindowParent.ClientGuid,
                        PackageType = ConstDef.HeartPackage
                    })));

            getOnlineInfoPackageData = Encoding.UTF8.GetBytes(
                JsonData.ConvertToText(
                    JsonData.Create(new TransPackage()
                    {
                        Name = WindowParent.NickName,
                        Content = null,
                        ClientGuid = WindowParent.ClientGuid,
                        PackageType = ConstDef.ReportChannelOnline
                    })));

            DataObject.AddPastingHandler(InputBox, InputBox_OnPaste);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SendVerification($"{WindowParent.NickName} entered.");
            new Thread(HeartBeatLoop).Start();         // 开启心跳包发送循环
            new Thread(MemoryCleaningLoop).Start();    // 开启内存清理循环
        }
        private void HeartBeatLoop()
        {
            try
            {
                while (SelfClient.Connected)
                {
                    SelfClient.SocketToServer.Send(heartPackageData);
                    Thread.Sleep(120000);
                }
            }
            catch { }
        }


        /* -------------------------------------------------------------------------------------------------------------------------- */

        #region UI Core
        private Grid GenBaseMsgGrid(string name, UIElement content, HorizontalAlignment align)
        {
            Grid rstGrid = new Grid()
            {
                VerticalAlignment = VerticalAlignment.Top
            };
            rstGrid.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(25)
            });
            rstGrid.RowDefinitions.Add(new RowDefinition());

            Label nameLabel = new Label
            {
                Content = name,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = align
            };
            Grid.SetRow(nameLabel, 0);
            Border contentBorder = new Border()
            {
                Padding = new Thickness(3),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = align,
            };
            contentBorder.Child = content;
            Grid.SetRow(contentBorder, 1);
            rstGrid.Children.Add(nameLabel);
            rstGrid.Children.Add(contentBorder);
            return rstGrid;
        }
        private Grid GenTextMsgGrid(string name, string content, HorizontalAlignment align)
        {
            UIElement contentBox = GenTextMsgContent(content, align);
            Grid msgBox = GenBaseMsgGrid(name, contentBox, align);
            return msgBox;
        }
        private Grid GenImageMsgGrid(string name, string baseImage, HorizontalAlignment align)
        {
            UIElement contentBox = GenImageMsgContent(baseImage, align);
            Grid msgBox = GenBaseMsgGrid(name, contentBox, align);
            return msgBox;
        }
        private UIElement GenTextMsgContent(string content, HorizontalAlignment align)
        {
            TextBox contentBox = new TextBox()
            {
                Text = content,
                IsReadOnly = true,
                IsReadOnlyCaretVisible = false,
                MaxLines = int.MaxValue,
                TextWrapping = TextWrapping.Wrap,
                BorderThickness = new Thickness(0),
                BorderBrush = new SolidColorBrush(Colors.Transparent),
                HorizontalAlignment = align,
                Cursor = Cursors.Arrow
            };
            return contentBox;
        }
        private UIElement GenImageMsgContent(string baseImage, HorizontalAlignment align)
        {
            byte[] imgData;
            UIElement rst;
            try
            {
                imgData = Convert.FromBase64String(baseImage);
                MemoryStream stream = new MemoryStream(imgData);
                BitmapImage imgSrc = new BitmapImage();
                imgSrc.BeginInit();
                imgSrc.StreamSource = stream;
                imgSrc.EndInit();

                rst = new Image()
                {
                    Source = imgSrc,
                    MaxWidth = 260,
                    AllowDrop = true,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = align,
                };

                ((Image)rst).DragLeave += Image_DragLeave;

                return rst;
            }
            catch
            {
                rst = new TextBox()
                {
                    Text = "图像加载失败",
                    Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                    IsReadOnly = true,
                    IsReadOnlyCaretVisible = false,
                    MaxLines = int.MaxValue,
                    TextWrapping = TextWrapping.Wrap,
                    BorderThickness = new Thickness(0),
                    BorderBrush = new SolidColorBrush(Colors.Transparent),
                    Cursor = Cursors.Arrow
                };

                return rst;
            }
        }

        #endregion

        #region Message Factory
        private void DealPackage(TransPackage recvPackage)
        {
            if (recvPackage.PackageType != ConstDef.HeartPackage)
            {
                switch (recvPackage.PackageType)
                {
                    case ConstDef.NormalMessage:
                        if (recvPackage.ClientGuid == WindowParent.ClientGuid)
                        {
                            AppendTextMessage(recvPackage.Name, recvPackage.Content, HorizontalAlignment.Right);
                        }
                        else
                        {
                            AppendTextMessage(recvPackage.Name, recvPackage.Content, HorizontalAlignment.Left);
                        }
                        App.Current.Dispatcher.Invoke(() => { ChatScroller.ScrollToBottom(); });
                        break;
                    case ConstDef.Verification:
                        AppendAnnouncement(recvPackage.Content);
                        App.Current.Dispatcher.Invoke(() => { ChatScroller.ScrollToBottom(); });
                        break;
                    case ConstDef.ImageMessage:
                        if (recvPackage.ClientGuid == WindowParent.ClientGuid)
                        {
                            AppendImageMessage(recvPackage.Name, recvPackage.Content, HorizontalAlignment.Right);
                        }
                        else
                        {
                            AppendImageMessage(recvPackage.Name, recvPackage.Content, HorizontalAlignment.Left);
                        }
                        break;
                    case ConstDef.DrawAttention:
                        if (recvPackage.ClientGuid == WindowParent.ClientGuid)
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
                        App.Current.Dispatcher.Invoke(() => { ChatScroller.ScrollToBottom(); });
                        break;
                    case ConstDef.ReportChannelOnline:
                        AppendAnnouncement(recvPackage.Content);
                        App.Current.Dispatcher.Invoke(() => { ChatScroller.ScrollToBottom(); });
                        break;
                    default:
                        // ( 在规定之外的消息 ) 以后再弄详细处理
                        break;
                }

                if (partsBuffer != null)
                {
                    DisposePartsBuffer();
                }
            }
        }
        private bool TrySendPackage(TransPackage package)
        {
            JsonData jsonToSend = JsonData.Create(package);
            string textToSend = JsonData.ConvertToText(jsonToSend);
            byte[] bytesToSend = Encoding.UTF8.GetBytes(textToSend);
            return TrySendData(bytesToSend);
        }
        private bool TrySendData(byte[] data)
        {
            try
            {
                if (data.Length <= WindowParent.BufferSize)
                {
                    SelfClient.SocketToServer.Send(data);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        private bool TrySendTextMsg(string text)
        {
            TransPackage msg = new TransPackage()
            {
                Name = WindowParent.NickName,
                Content = text,
                ClientGuid = WindowParent.ClientGuid,
                PackageType = ConstDef.NormalMessage
            };

            return TrySendPackage(msg);
        }
        private bool TrySendImageMsg(System.Drawing.Image img)
        {
            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream();
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                TransPackage msg = new TransPackage()
                {
                    Name = WindowParent.NickName,
                    Content = Convert.ToBase64String(ms.ToArray()),
                    ClientGuid = WindowParent.ClientGuid,
                    PackageType = ConstDef.ImageMessage
                };

                return TrySendPackage(msg);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (ms != null)
                {
                    ms.Dispose();
                }
            }
        }
        private void SendInputtedText()
        {
            if (TrySendTextMsg(InputBox.Text))
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
            if (TrySendImageMsg(img))
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
        private void SendVerification(string content)
        {
            TransPackage package = new TransPackage()
            {
                Name = WindowParent.NickName,
                Content = content,
                ClientGuid = WindowParent.ClientGuid,
                PackageType = ConstDef.Verification
            };

            TrySendPackage(package);
        }
        private void AppendMsgControl(FrameworkElement control)
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
            App.Current.Dispatcher.Invoke(() =>
            {
                Grid newMsg = GenTextMsgGrid(name, content, align);
                AppendMsgControl(newMsg);
            });
        }
        private void AppendImageMessage(string name, string baseImg, HorizontalAlignment align)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Grid newMsg = GenImageMsgGrid(name, baseImg, align);
                AppendMsgControl(newMsg);
            });
        }
        private void AppendAnnouncement(string content)
        {
            App.Current.Dispatcher.Invoke(() =>
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
        }

        private void Image_DragLeave(object sender, DragEventArgs e)
        {

        }
        #endregion

        #region Socket Core
        private void MemoryCleaningLoop()
        {
            while (true)
            {
                Thread.Sleep(cinterval);
                if (partsBuffer != null && DateTime.Now - wroteTime > TimeSpan.FromMilliseconds(btimeout))
                {
                    //DisposePartsBuffer();
                }
            }
        }
        private void DisposePartsBuffer()
        {
            partsBuffer.Dispose();
            partsBuffer = null;
        }
        public void SelfClient_ReceivedMsg(object sender, System.Net.Sockets.Socket socket, byte[] buffer, int size)
        {
            MainChat sender1 = (MainChat)((SocketClient)sender).Tag;
            try
            {
                string recvText = Encoding.UTF8.GetString(buffer, 0, size);
                JsonData[] recvJsons = JsonData.ParseStream(recvText);

                foreach (JsonData per in recvJsons)
                {
                    TransPackage recvPackage = JsonData.ConvertToInstance<TransPackage>(per);
                    sender1.DealPackage(recvPackage);
                }

                return;
            }
            catch
            {
                if (partsBuffer == null)
                {
                    partsBuffer = new MemoryStream();
                    partsBuffer.Write(buffer, 0, size);
                }
                else
                {
                    partsBuffer.Write(buffer, 0, size);

                    try
                    {
                        string bufferStr = Encoding.UTF8.GetString(partsBuffer.ToArray());
                        if (JsonData.TryParseStream(bufferStr, out JsonData[] jsons))
                        {
                            foreach (JsonData per in jsons)
                            {
                                TransPackage perPackage = JsonData.ConvertToInstance<TransPackage>(per);
                                sender1.DealPackage(perPackage);
                            }
                        }
                    }
                    catch { }
                }

                wroteTime = DateTime.Now;

                //MessageBox.Show("Received wrong data which can't be decoded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void SelfClient_Disconnected(object sender, System.Net.Sockets.Socket socket)
        {
            MessageBox.Show("Disconnected from server.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            App.Current.Dispatcher.Invoke(() =>
            {
                Program.Navigate(new Login());
                forceClose = true;
                keepMain = true;
            });
        }
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
            TrySendPackage(new TransPackage()
            {
                Name = WindowParent.NickName,
                Content = null,
                ClientGuid = WindowParent.ClientGuid,
                PackageType = ConstDef.DrawAttention
            });
        }
        private void OnlineInfo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TrySendData(getOnlineInfoPackageData);
        }                    // 

        private OpenFileDialog ofd;
        private void SendPicture_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ofd == null)
            {
                ofd = new OpenFileDialog
                {
                    Title = "Open a image file for sending",
                    Filter = "Normal image format|*.jpg;*.jpeg;*.png;*.gif;*.bmp|JPEG|*.jpg;*.jpeg|PNG|*.png|GIF|*.gif|Bitmap|*.bmp|Other|*.*",
                    CheckFileExists = true,
                    Multiselect = false
                };
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
