using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using TocTiny.Public;
using NullLib.EventedSocket;
using CHO.Json;
using System.Net;

namespace TocTiny
{
    public class TocTinyServer
    {
        struct ClientData
        {
            public MemoryStream Buffer;
            public DateTime LastSend;
        }

        EventedListener clientListener;

        int port, backlog, historyMaxCount;    // port, bufferSize, backlog, history max count; 启动信息
        TimeSpan btimeout;

        public double BufferTimeout { get => btimeout.TotalMilliseconds; set { btimeout = TimeSpan.FromMilliseconds(value); } }
        public double CleanInverval { get => bufferCleanner.Interval; set { bufferCleanner.Interval = value; } }
        public int HistoryMaxCount { get => historyMaxCount; set { historyMaxCount = value; } }
        public int Backlog { get => backlog; set => backlog = value; }
        public int Port { get => port; }

        readonly Dictionary<EventedClient, ClientData> clients;                // 客户端

        readonly List<byte[]> lastMessages;                             // 保存最近的几条消息
        readonly Timer bufferCleanner;


        //-------------------------------------------------------------------------------------------------------------------------------------------//

        public TocTinyServer(int port)
        {
            this.port = port;                     // 默认端口
            backlog = 50;                    // 默认监听数

            clientListener = new EventedListener(IPAddress.Any, port);

            clients = new Dictionary<EventedClient, ClientData>();
            lastMessages = new List<byte[]>();

            bufferCleanner = new Timer()
            {
                AutoReset = true
            };
            bufferCleanner.Elapsed += CleannerAction;
        }

        public void StartServer()
        {
            clientListener.ClientConnected += ClientConnectedController;

            clientListener.Start();
            clientListener.StartAcceptClient();
            bufferCleanner.Start();
        }

        #region 主要的3个事件 {连接 接收 断开}
        private void RecvedClientMsgController(object sender, ClientDataReceivedEventArgs e)
        {
            EventedClient client = e.Client;
            byte[] buffer = e.Buffer;
            int size = e.Size;

            if (TryGetPackages(buffer, size, out TransPackage[] packages))
            {
                DealPackages(client, packages, buffer, size);
                ClearPartBuffer(clients[client]);
            }
            else
            {
                DealPartData(client, buffer, size);
            }
        }
        private void ClientDisconnectedController(object sender, ClientDisconnectedEventArgs e)
        {
            EventedClient client = e.Client;
            SafeRemoveClient(client);

            OnClientDisconnected(client);
        }
        private void ClientConnectedController(object sender, ClientConnectedEventArgs e)
        {
            EventedClient client = e.Client;
            client.DataReceived += RecvedClientMsgController;
            client.Disconnected += ClientDisconnectedController;
            client.StartReceiveData();

            SafeAddClient(client);

            if (clients.Count >= Backlog)
                clientListener.StopAcceptClient();

            SafeSendHistoryData(client);

            OnClientConnected(client);
        }
        #endregion

        #region 消息管理函数 发送包 处理包
        private bool TryGetPackages(byte[] data, out TransPackage[] packages)
        {
            return TryGetPackages(data, data.Length, out packages);
        }
        private bool TryGetPackages(byte[] data, int size, out TransPackage[] packages)
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
        private void DealPackages(EventedClient sender, TransPackage[] packages, byte[] buffer, int size)
        {
            foreach (TransPackage package in packages)
            {
                OnPackageReceived(sender, package, out bool boardcast, out bool postback, out bool record);
                if (boardcast)
                    foreach (var i in clients.Keys)
                        i.SendData(buffer, 0, size);
                if (postback)
                    sender.SendData(buffer, 0, size);
                if (record)
                    SafeAddHistoryData(buffer, size);
            }
        }
        private bool DealPartData(EventedClient sender, byte[] part, int size)
        {
            if (clients.TryGetValue(sender, out ClientData cdata))
            {
                if (cdata.Buffer.Length == 0)
                {
                    UpdatePartBuffer(cdata);
                }

                WritePartBuffer(cdata, part, size);
                byte[] bytes = cdata.Buffer.ToArray();
                if (TryGetPackages(bytes, out TransPackage[] packages))
                {
                    DealPackages(sender, packages, bytes, size);
                    ClearPartBuffer(cdata);                           // 清空缓冲区
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 将数据写入到缓冲区
        /// </summary>
        /// <param name="cdata"></param>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns>是否是第一次写入缓冲区</returns>
        private void WritePartBuffer(ClientData cdata, byte[] data, int size)
        {
            cdata.Buffer.Write(data, 0, size);
        }
        private void UpdatePartBuffer(ClientData cdata)
        {
            cdata.LastSend = DateTime.Now;
        }
        private void ClearPartBuffer(ClientData cdata)
        {
            cdata.Buffer.SetLength(0);
        }
        #endregion

        #region 客户端独立缓冲区管理
        private void CleannerAction(object sender, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;
            foreach (var cdata in clients.Values)
            {
                if (now - cdata.LastSend > btimeout)
                    cdata.Buffer.SetLength(0);
            }
        }
        #endregion

        #region 较高层次的管理函数
        private void SafeAddHistoryData(byte[] data, int count)
        {
            lock (lastMessages)
            {
                byte[] segment = new byte[count];
                data.CopyTo(segment, 0);
                lastMessages.Add(segment);

                while (lastMessages.Count > historyMaxCount)
                    lastMessages.RemoveAt(0);
            }
        }
        private void SafeSendHistoryData(EventedClient socket)
        {
            lock (lastMessages)
            {
                for (int i = 0; i < lastMessages.Count; i++)
                {
                    socket.SendData(lastMessages[i]);
                }
            }
        }
        #endregion

        #region 客户端的管理函数
        private void SafeAddClient(EventedClient socket)
        {
            lock (clients)
            {
                clients[socket] = new ClientData()
                {
                    Buffer = new MemoryStream()
                };
            }
        }
        private bool SafeRemoveClient(EventedClient socket)
        {
            lock (clients)
            {
                if (clients.TryGetValue(socket, out ClientData data))
                {
                    data.Buffer.Dispose();
                    clients.Remove(socket);
                    socket.Dispose();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public void SafeBoardcastData(byte[] data)
        {
            SafeBoardcastData(data, 0, data.Length);
        }
        public void SafeBoardcastData(byte[] data, int offset, int size)
        {
            lock(clients)
            {
                foreach (var i in clients.Keys)
                    i.SendData(data, offset, size);
            }
        }
        #endregion

        public void StopServer()
        {
            clientListener.Stop();
            bufferCleanner.Stop();
        }

        public event EventHandler<PackageReceivedArgs> PackageReceived;
        public event EventHandler<ClientConnectedArgs> ClientConnected;
        public event EventHandler<ClientDisconnectedArgs> ClientDisconnected;

        private void OnPackageReceived(EventedClient sender, TransPackage package, out bool boardcast, out bool postback, out bool record)
        {
            boardcast = false;
            postback = false;
            record = false;

            if (PackageReceived != null)
            {
                PackageReceivedArgs e = new PackageReceivedArgs(sender, package);
                PackageReceived.Invoke(this, e);
                boardcast = e.Boardcast;
                postback = e.Postback;
                record = e.Record;
            }
        }
        private void OnClientConnected(EventedClient client)
        {
            if (ClientConnected != null)
            {
                ClientConnected.Invoke(this, new ClientConnectedArgs(client));
            }
        }
        private void OnClientDisconnected(EventedClient client)
        {
            if (ClientDisconnected != null)
            {
                ClientDisconnected.Invoke(this, new ClientDisconnectedArgs(client));
            }
        }
    }
    public class PackageReceivedArgs : EventArgs
    {
        public EventedClient Sender;
        public TransPackage Package;

        public bool Boardcast = false;
        public bool Postback = false;
        public bool Record = false;

        public PackageReceivedArgs() { }
        public PackageReceivedArgs(EventedClient sender, TransPackage package)
        {
            this.Sender = sender;
            this.Package = package;
        }
    }
    public class ClientConnectedArgs : EventArgs
    {
        public EventedClient Client;

        public ClientConnectedArgs() { }
        public ClientConnectedArgs(EventedClient client)
        {
            this.Client = client;
        }
    }
    public class ClientDisconnectedArgs : EventArgs
    {
        public EventedClient Client;

        public ClientDisconnectedArgs() { }
        public ClientDisconnectedArgs(EventedClient client)
        {
            this.Client = client;
        }
    }
}
