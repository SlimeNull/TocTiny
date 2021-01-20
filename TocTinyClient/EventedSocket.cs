using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;

namespace Null.Library.EventedSocket
{
    public delegate void SocketConnectedHandler(Socket socket);
    public delegate void SocketDisconnectedHandler(Socket socket);
    public delegate void SocketRecvMsgHandler(Socket socket, byte[] buffer, int size);

    public class SocketServer
    {
        Socket server;                                               // 用来接受连接, 接收数据, 转发数据的套接字
        Dictionary<Socket, byte[]> clientBufferPairs;                // 缓冲区
        int bufferSize;

        public Socket BaseSocket
        {
            get => server;
        }
        public bool Running
        {
            get
            {
                return server.IsBound;
            }
        }
        public int ConnectedCount
        {
            get
            {
                return clientBufferPairs.Count;
            }
        }
        public void Start(int port, int backlog, int bufferSize)
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientBufferPairs = new Dictionary<Socket, byte[]>();
            this.bufferSize = bufferSize;
            server.Bind(new IPEndPoint(IPAddress.Any, port));
            server.Listen(backlog);
            server.BeginAccept(AcceptAction, null);
        }
        public void Stop()
        {
            server.Close();
        }

        public event SocketConnectedHandler ClientConnected;
        public event SocketDisconnectedHandler ClientDisconnected;
        public event SocketRecvMsgHandler RecvedClientMsg;

        void AcceptAction(IAsyncResult ar)
        {
            Socket client = server.EndAccept(ar);
            if (ClientConnected != null) ClientConnected.Invoke(client);

            clientBufferPairs[client] = new byte[bufferSize];
            StateObject state = new StateObject
            {
                workSocket = client
            };
            client.BeginReceive(clientBufferPairs[client], 0, bufferSize, SocketFlags.None, ReceiveAction, state);

            server.BeginAccept(AcceptAction, null);
        }
        void ReceiveAction(IAsyncResult ar)
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
                if (ClientDisconnected != null) ClientDisconnected.Invoke(client);
                client.Close();
                return;
            }

            if (size == 0)
            {
                clientBufferPairs.Remove(client);
                if (ClientDisconnected != null) ClientDisconnected.Invoke(client);
                client.Close();
            }
            else
            {
                if (RecvedClientMsg != null) RecvedClientMsg.Invoke(client, clientBufferPairs[client], size);

                client.BeginReceive(clientBufferPairs[client], 0, 4096, SocketFlags.None, new AsyncCallback(ReceiveAction), ar.AsyncState);
            }
        }
    }
    public class SocketClient
    {
        Socket server;
        byte[] buffer;

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
        public Socket Server
        {
            get
            {
                return server;
            }
        }

        public void ConnectTo(EndPoint address, int bufferSize)
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Connect(address);
            this.buffer = new byte[bufferSize];

            try
            {
                server.BeginReceive(buffer, 0, this.buffer.Length, SocketFlags.None, ReceiveAction, null);
            }
            catch
            {
                if (Disconnected != null)
                {
                    Disconnected.Invoke(server);
                }
            }
        }

        public event SocketRecvMsgHandler ReceivedMsg;
        public event SocketDisconnectedHandler Disconnected;

        void ReceiveAction(IAsyncResult ar)
        {
            int size = 0;
            try
            {
                size = server.EndReceive(ar);
            }
            catch
            {
                if (Disconnected != null) Disconnected.Invoke(server);
                server.Close();
                return;
            }

            if (size == 0)
            {
                if (Disconnected != null) Disconnected.Invoke(server);
                server.Close();
            }
            else
            {
                if (ReceivedMsg != null) ReceivedMsg.Invoke(Server, buffer, size);

                try
                {
                    server.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveAction), null);
                }
                catch
                {
                    if (Disconnected != null)
                    {
                        Disconnected.Invoke(server);
                    }
                }
            }
        }
    }
    public class StateObject
    {
        public Socket workSocket;
    }
}
