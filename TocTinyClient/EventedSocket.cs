using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Null.Library.EventedSocket
{
    public delegate void SocketConnectedHandler(object sender,Socket socket);
    public delegate void SocketDisconnectedHandler(object sender,Socket socket);
    public delegate void SocketRecvMsgHandler(object sender,Socket socket, byte[] buffer, int size);
    public class SocketServer
    {
        private Socket server;                                               // 用来接受连接, 接收数据, 转发数据的套接字
        private Dictionary<Socket, byte[]> clientBufferPairs;                // 缓冲区
        private int bufferSize;
        /// <summary>
        /// 基础Socket
        /// </summary>
        public Socket Socket => server;
        /// <summary>
        /// 是否在运行
        /// </summary>
        public bool Running => server.IsBound;
        /// <summary>
        /// 连接数
        /// </summary>
        public int ConnectedCount => clientBufferPairs.Count;
        /// <summary>
        /// 启动监听
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="backlog">连接列队限制</param>
        /// <param name="bufferSize">缓冲区大小</param>
        public void Start(ushort port, int backlog, int bufferSize)
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientBufferPairs = new Dictionary<Socket, byte[]>();
            this.bufferSize = bufferSize;
            server.Bind(new IPEndPoint(IPAddress.Any, port));
            server.Listen(backlog);
            server.BeginAccept(AcceptAction, null);
        }
        /// <summary>
        /// 停止监听
        /// </summary>
        public void Stop()
        {
            server.Close();
        }

        public event SocketConnectedHandler ClientConnected;
        public event SocketDisconnectedHandler ClientDisconnected;
        public event SocketRecvMsgHandler RecvedClientMsg;

        /// <summary>
        /// 接受连接操作
        /// </summary>
        /// <param name="ar">Sokcet连接异步结果</param>
        private void AcceptAction(IAsyncResult ar)
        {
            Socket client = server.EndAccept(ar);
            if (ClientConnected != null)
            {
                ClientConnected.Invoke(this,client);
            }

            clientBufferPairs[client] = new byte[bufferSize];
            StateObject state = new StateObject
            {
                workSocket = client
            };
            client.BeginReceive(clientBufferPairs[client], 0, bufferSize, SocketFlags.None, ReceiveAction, state);

            server.BeginAccept(AcceptAction, null);
        }

        /// <summary>
        /// 接受数据操作
        /// </summary>
        /// <param name="ar">Sokcet连接异步结果</param>
        private void ReceiveAction(IAsyncResult ar)
        {
            Socket client = ((StateObject)ar.AsyncState).workSocket;
            int size = 0;
            try
            {
                size = client.EndReceive(ar);
            }
            catch
            {
                clientBufferPairs.Remove(client);
                if (ClientDisconnected != null)
                {
                    ClientDisconnected.Invoke(this,client);
                }

                client.Close();
                return;
            }

            if (size == 0)
            {
                clientBufferPairs.Remove(client);
                if (ClientDisconnected != null)
                {
                    ClientDisconnected.Invoke(this,client);
                }

                client.Close();
            }
            else
            {
                if (RecvedClientMsg != null)
                {
                    RecvedClientMsg.Invoke(this,client, clientBufferPairs[client], size);
                }

                client.BeginReceive(clientBufferPairs[client], 0, 4096, SocketFlags.None, new AsyncCallback(ReceiveAction), ar.AsyncState);
            }
        }
    }
    public class SocketClient
    {
        private Socket server;
        private byte[] buffer;
        /// <summary>
        /// 获取一个值，该值指示 System.Net.Sockets.Socket 是在上次 Overload:System.Net.Sockets.Socket.Send
        /// 还是 Overload:System.Net.Sockets.Socket.Receive 操作时连接到远程主机。
        /// </summary>
        public bool Connected
        {
            get
            {
                if (server != null)
                {
                    return server.Connected;
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// 获得连接到服务器的Sokcet
        /// </summary>
        public Socket SocketToServer => server;
        /// <summary>
        /// 连接到目标主机
        /// </summary>
        /// <param name="address">主机地址</param>
        /// <param name="bufferSize">缓冲区大小</param>
        public void ConnectTo(IPEndPoint address, int bufferSize)
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Connect(address);
            buffer = new byte[bufferSize];

            try
            {
                server.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveAction, null);
            }
            catch
            {
                if (Disconnected != null)
                {
                    Disconnected.Invoke(this,server);
                }
            }
        }
        public object Tag { get; set; }
        public event SocketRecvMsgHandler ReceivedMsg;
        public event SocketDisconnectedHandler Disconnected;

        /// <summary>
        /// 接受操作
        /// </summary>
        /// <param name="ar">Sokcet连接异步结果</param>
        private void ReceiveAction(IAsyncResult ar)
        {
            int size = 0;
            try
            {
                size = server.EndReceive(ar);
            }
            catch
            {
                if (Disconnected != null)
                {
                    Disconnected.Invoke(this,server);
                }

                server.Close();
                return;
            }

            if (size == 0)
            {
                if (Disconnected != null)
                {
                    Disconnected.Invoke(this,server);
                }

                server.Close();
            }
            else
            {
                if (ReceivedMsg != null)
                {
                    ReceivedMsg.Invoke(this,SocketToServer, buffer, size);
                }

                try
                {
                    server.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveAction), null);
                }
                catch
                {
                    if (Disconnected != null)
                    {
                        Disconnected.Invoke(this,server);
                    }
                }
            }
        }
    }
    public class StateObject
    {
        /// <summary>
        /// 工作Socket
        /// </summary>
        public Socket workSocket;
    }
}
