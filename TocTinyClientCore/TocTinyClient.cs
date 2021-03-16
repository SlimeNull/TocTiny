using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using CHO.Json;
using NullLib.EventedSocket;
using TocTiny;
using TocTiny.Public;

namespace TocTiny.Core
{
    public class TocTinyClient
    {
        private EventedClient selfClient;

        private TimeSpan btimeout = TimeSpan.FromSeconds(2);

        private string userName;
        private string clientGuid;
        private Timer bufferCleaner;
        private DateTime lastRecv;
        private MemoryStream partBuffer;

        public string UserName { get => userName; set => userName = value; }
        public string ClientGuid { get => clientGuid; }
        public EventedClient SocketClient { get => selfClient; }
        public Socket Server { get => selfClient.BaseSocket; }
        public double BufferTimeout
        {
            get => btimeout.TotalMilliseconds;
            set
            {
                btimeout = TimeSpan.FromMilliseconds(value);
            }
        }
        public double CleanInterval
        {
            get => bufferCleaner.Interval;
            set
            {
                bufferCleaner.Interval = value;
            }
        }

        public TocTinyClient()
        {
            userName = Environment.UserName;
            clientGuid = Guid.NewGuid().ToString();
            lastRecv = DateTime.Now;
            bufferCleaner = new Timer();
            bufferCleaner.Elapsed += CleanAction;

            selfClient = new EventedClient();
            partBuffer = new MemoryStream();

            selfClient.DataReceived += SocketClient_ReceivedMsg;
            selfClient.Disconnected += SocketClient_Disconnected;
        }

        /// <summary>
        /// 清理 PartBuffer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CleanAction(object sender, ElapsedEventArgs e)
        {
            if (partBuffer.Length > 0 && DateTime.Now - lastRecv > btimeout)
            {
                partBuffer.SetLength(0);
            }
        }
        /// <summary>
        /// 连接到服务端
        /// </summary>
        /// <param name="point"></param>
        public void ConnectTo(IPEndPoint point)
        {
            selfClient.Connect(point.Address, point.Port);
            selfClient.StartReceiveData();
            bufferCleaner.Start();
        }
        /// <summary>
        /// 断开连接时, 引发事件, 并停止 BufferCleaner 线程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SocketClient_Disconnected(object sender, NullLib.EventedSocket.ClientDisconnectedEventArgs args)
        {
            OnConnectionLost(Server);                 // 客户端断开连接, 引发Disconnected事件并停止BufferCleaner.
            bufferCleaner.Stop();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SocketClient_ReceivedMsg(object sender, ClientDataReceivedEventArgs args)
        {
            byte[] buffer = args.Buffer;
            int size = args.Size;

            if (TryGetPackages(buffer, size, out TransPackage[] packages))
            {
                DealPackages(packages, buffer, size);
                ClearPartBuffer();
            }
            else
            {
                DealPartData(buffer, size);
            }
        }


        private static bool TryGetPackages(byte[] data, out TransPackage[] packages)
        {
            return TryGetPackages(data, data.Length, out packages);
        }
        private static bool TryGetPackages(byte[] data, int size, out TransPackage[] packages)
        {
            try
            {
                string jsonText = Encoding.UTF8.GetString(data, 0, size);
                JsonData[] jsons = JsonData.ParseStream(jsonText);

                var tmp = jsons.Select((json) => JsonData.ConvertToInstance<TransPackage>(json));
                packages = tmp.ToArray();
                return true;
            }
            catch
            {
                packages = null;
                return false;
            }
        }
        private void DealPackages(TransPackage[] packages, byte[] buffer, int size)
        {
            if (PackageReceived != null)
            {
                foreach (TransPackage package in packages)
                {
                    OnPackageReceived(package, out bool postback);
                    if (postback)                              // OnPackageReceived 返回的 bool 值表示是否Postback
                        selfClient.BeginSendData(buffer, 0, size);
                }
            }
        }
        private void DealPartData(byte[] data, int size)
        {
            if (partBuffer.Length == 0)
                UpdatePartBuffer();

            WritePartBuffer(data, size);
            byte[] bytes = partBuffer.ToArray();
            if (TryGetPackages(bytes, out TransPackage[] packages))
            {
                DealPackages(packages, bytes, bytes.Length);
                ClearPartBuffer();
            }
        }


        private void WritePartBuffer(byte[] data, int size)
        {
            partBuffer.Write(data, 0, size);
        }
        /// <summary>
        /// 更新 PartBuffer 的写入时间标识 (lastRecv)
        /// </summary>
        private void UpdatePartBuffer()
        {
            lastRecv = DateTime.Now;
        }
        /// <summary>
        /// 清空 PartBuffer
        /// </summary>
        private void ClearPartBuffer()
        {
            partBuffer.SetLength(0);
        }

        /// <summary>
        /// 发送文本
        /// </summary>
        /// <param name="text"></param>
        public void SendText(string text)
        {
            SendPackage(new TransPackage()
            {
                Name = userName,
                Content = text,
                ClientGuid = clientGuid,

                PackageType = ConstDef.NormalMessage
            });
        }

        /// <summary>
        /// 发送图片
        /// </summary>
        /// <param name="image"></param>
        public void SendImage(Image image)
        {
            MemoryStream tempStream = new MemoryStream();
            image.Save(tempStream, ImageFormat.Jpeg);
            string base64 = Convert.ToBase64String(tempStream.ToArray());

            tempStream.Dispose();

            SendPackage(new TransPackage()
            {
                Name = userName,
                Content = base64,
                ClientGuid = clientGuid,

                PackageType = ConstDef.ImageMessage
            });
        }

        /// <summary>
        /// 吸引注意力
        /// </summary>
        public void DrawAttention()
        {
            SendPackage(new TransPackage()
            {
                Name = userName,
                Content = null,
                ClientGuid = clientGuid,

                PackageType = ConstDef.DrawAttention
            });
        }

        /// <summary>
        /// 请求在线信息
        /// </summary>
        public void RequestOnlineInfo()
        {
            SendPackage(new TransPackage()
            {
                Name = userName,
                Content = null,
                ClientGuid = clientGuid,

                PackageType = ConstDef.ReportChannelOnline
            });
        }

        public bool TrySendText(string text)
        {
            return ExFunc.TryDo(() => SendText(text));
        }
        public bool TrySendImage(Image image)
        {
            return ExFunc.TryDo(() => SendImage(image));
        }
        public bool TryDrawAttention()
        {
            return ExFunc.TryDo(() => DrawAttention());
        }
        public bool TryRequestOnlineInfo()
        {
            return ExFunc.TryDo(() => RequestOnlineInfo());
        }
        public void SendPackage(TransPackage package)
        {
            selfClient.BeginSendData(
                Encoding.UTF8.GetBytes(
                    JsonData.ConvertToText(
                        JsonData.Create(package))));
        }
        public bool TrySendPackage(TransPackage package)
        {
            return ExFunc.TryDo(() => SendPackage(package));
        }


        public event EventHandler<PackageReceivedEventArgs> PackageReceived;
        public event EventHandler<ClientDisconnectedEventArgs> ConnectionLost;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        /// <returns>是否Postback</returns>
        private void OnPackageReceived(TransPackage package, out bool postback)
        {
            postback = false;

            if (PackageReceived != null)
            {
                PackageReceivedEventArgs e = new PackageReceivedEventArgs(package);
                PackageReceived.Invoke(this, e);
                postback = e.Postback;
            }
        }
        private void OnConnectionLost(Socket client)
        {
            if (ConnectionLost != null)
                ConnectionLost.Invoke(this, new ClientDisconnectedEventArgs(client));
        }
    }
    public class PackageReceivedEventArgs : EventArgs
    {
        public TransPackage Package;
        
        public bool Postback = false;

        public PackageReceivedEventArgs() { }
        public PackageReceivedEventArgs(TransPackage package)
        {
            this.Package = package;
        }
    }
    public class ClientDisconnectedEventArgs : EventArgs
    {
        public Socket Client;
        
        public ClientDisconnectedEventArgs() { }
        public ClientDisconnectedEventArgs(Socket client)
        {
            this.Client = client;
        }
    }
}
