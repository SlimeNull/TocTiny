using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using TocTiny.Public;
using Null.Library.EventedSocket;
using CHO.Json;

namespace TocTiny
{
    public class TocTinyServer
    {
        class ClientData
        {
            public MemoryStream Buffer;
            public DateTime LastSend;
        }

        SocketServer socketServer;

        int port, bufferSize, backlog, historyMaxCount;    // port, bufferSize, backlog, history max count; 启动信息
        TimeSpan btimeout;

        public double BufferTimeout { get => btimeout.TotalMilliseconds; set { btimeout = TimeSpan.FromMilliseconds(value); } }
        public double CleanInverval { get => bufferCleanner.Interval; set { bufferCleanner.Interval = value; } }
        public int HistoryMaxCount { get => historyMaxCount; set { historyMaxCount = value; } }

        readonly Dictionary<Socket, ClientData> clients;                // 客户端
        readonly Dictionary<string, (string, Socket)> clientRecords;    // guid : (name, socket)

        readonly List<Socket> clientToRemove;                           // 要被删去的服务端
        readonly List<byte[]> lastMessages;                             // 保存最近的几条消息


        readonly byte[] heartPackageData;

        readonly Timer bufferCleanner;


        //-------------------------------------------------------------------------------------------------------------------------------------------//

        public TocTinyServer()
        {
            port = 2020;                     // 默认端口
            backlog = 50;                    // 默认监听数
            bufferSize = 1 << 10 << 10;      // 默认缓冲区大小 1 byte << 10 << 10 = 1MB


            socketServer = new SocketServer();

            clients = new Dictionary<Socket, ClientData>();
            clientToRemove = new List<Socket>();
            lastMessages = new List<byte[]>();
            clientRecords = new Dictionary<string, (string, Socket)>();

            bufferCleanner = new Timer()
            {
                AutoReset = true
            };
            bufferCleanner.Elapsed += CleannerAction;
        }

        public void StartServer()
        {
            socketServer.ClientConnected += ClientConnectedController;
            socketServer.ClientDisconnected += ClientDisconnectedController;
            socketServer.RecvedClientMsg += RecvedClientMsgController;

            socketServer.Start(port, backlog, bufferSize);
            bufferCleanner.Start();
        }

        #region 主要的3个事件 {连接 接收 断开}
        private void RecvedClientMsgController(Socket socket, byte[] buffer, int size)
        {
            if (TryGetPackages(buffer, size, out TransPackage[] packages))
            {
                DealPackages(socket, packages, buffer, size);
                ClearPartBuffer(clients[socket]);
            }
            else
            {
                DealPartData(socket, buffer, size);
            }
        }
        private void ClientDisconnectedController(Socket socket)
        {
            if (ClientDisconnected != null)
                ClientDisconnected.BeginInvoke(this, new ClientDisconnectedEventArgs(socket), (_) => { }, null);

            SafeRemoveClient(socket);
        }
        private void ClientConnectedController(Socket socket)
        {
            if (ClientConnected != null)
                ClientConnected.BeginInvoke(this, new ClientConnectedEventArgs(socket), (_) => { }, null);

            SafeAddClient(socket);
            SafeSendHistoryData(socket);
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
        private void DealPackages(Socket sender, TransPackage[] packages, byte[] buffer, int size)
        {
            if (PackageReceived != null)
            {
                foreach (TransPackage package in packages)
                {
                    PackageReceivedEventArgs args = new PackageReceivedEventArgs(sender, package);
                    PackageReceived.Invoke(sender, args);
                    if (args.Boardcast)
                        SafeBoardcastData(buffer, 0, size);
                    if (args.Postback)
                        SafeSendData(sender, buffer, 0, size);
                    if (args.Record)
                        SafeAddHistoryData(buffer, size);
                }
            }
        }
        private bool DealPartData(Socket sender, byte[] part, int size)
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
        /// <param name="stream"></param>
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
        private void SafeSendHistoryData(Socket socket)
        {
            lock (lastMessages)
            {
                for (int i = 0; i < lastMessages.Count; i++)
                {
                    SafeSendData(socket, lastMessages[i]);
                }
            }
        }
        #endregion

        #region 客户端的管理函数
        private bool SafeAddClient(Socket socket)
        {
            lock (clients)
            {
                return clients.TryAdd(socket, new ClientData()
                {
                    Buffer = new MemoryStream()
                });
            }
        }
        private bool SafeRemoveClient(Socket socket)
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
                foreach(Socket i in clients.Keys)
                {
                    SafeSendData(i, data, offset, size);
                }
            }
        }
        public static void SafeSendData(Socket socket, byte[] data)
        {
            SafeSendData(socket, data, 0, data.Length);
        }
        public static void SafeSendData(Socket socket, byte[] data, int offset, int size)
        {
            socket.BeginSend(data, offset, size, SocketFlags.None, (_) => { }, null);
        }         // 异步发送数据
        #endregion

        public void StopServer()
        {
            socketServer.Stop();
            bufferCleanner.Stop();
        }

        public event PackageReceivedHandler PackageReceived;
        public event ClientConnectedHandler ClientConnected;
        public event ClientDisconnectedHandler ClientDisconnected;
    }
    public class PackageReceivedEventArgs : EventArgs
    {
        public Socket Sender;
        public TransPackage Package;

        public bool Boardcast = false;
        public bool Postback = false;
        public bool Record = false;

        public PackageReceivedEventArgs() { }
        public PackageReceivedEventArgs(Socket sender, TransPackage package)
        {
            this.Sender = sender;
            this.Package = package;
        }
    }
    public class ClientConnectedEventArgs : EventArgs
    {
        public Socket Client;

        public ClientConnectedEventArgs() { }
        public ClientConnectedEventArgs(Socket client)
        {
            this.Client = client;
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

    public delegate void PackageReceivedHandler(object sender, PackageReceivedEventArgs args);
    public delegate void ClientConnectedHandler(object sender, ClientConnectedEventArgs args);
    public delegate void ClientDisconnectedHandler(object sender, ClientDisconnectedEventArgs args);
}
