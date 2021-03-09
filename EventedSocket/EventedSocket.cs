using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Null.Library.EventedSocket
{
    public class SocketConnectedArgs : EventArgs
    {
        public Socket Socket;
        public SocketConnectedArgs(Socket socket)
        {
            this.Socket = socket;
        }
    }
    public class SocketDisconnectedArgs : EventArgs
    {
        public Socket Socket;
        public SocketDisconnectedArgs(Socket socket)
        {
            this.Socket = socket;
        }
    }
    public class SocketReceivedDataArgs : EventArgs
    {
        public Socket Socket;
        public byte[] Buffer;
        public int Size;
        public SocketReceivedDataArgs(Socket socket, byte[] buffer, int size)
        {
            this.Socket = socket;
            this.Buffer = buffer;
            this.Size = size;
        }
    }

    public static class EventedSocket
    {
        public static void SendData(Socket target, byte[] data, int offset, int length)
        {
            target.Send(BitConverter.GetBytes(length));
            target.Send(data, offset, length, SocketFlags.None);
        }
        public static void SendData(Socket target, byte[] data, int offset)
        {
            target.Send(BitConverter.GetBytes(data.Length - offset));
            target.Send(data, offset, SocketFlags.None);
        }
        public static void SendData(Socket target, byte[] data)
        {
            target.Send(BitConverter.GetBytes(data.Length));
            target.Send(data);
        }
        public static void BeginSendData(Socket target, byte[] data, int offset, int length, AsyncCallback callback, object state)
        {
            target.Send(BitConverter.GetBytes(length));
            target.BeginSend(data, offset, length, SocketFlags.None, callback, state);
        }
        public static void BeginSendData(Socket target, byte[] data, int offset, AsyncCallback callback, object state)
        {
            int length = data.Length - offset;
            target.Send(BitConverter.GetBytes(length));
            target.BeginSend(data, offset, length, SocketFlags.None, callback, state);
        }
        public static void BeginSendData(Socket target, byte[] data, AsyncCallback callback, object state)
        {
            int length = data.Length;
            target.Send(BitConverter.GetBytes(length));
            target.BeginSend(data, 0, length, SocketFlags.None, callback, state);
        }
        public static void BeginSendData(Socket target, byte[] data, int offset, int length)
        {
            target.Send(BitConverter.GetBytes(length));
            target.BeginSend(data, offset, length, SocketFlags.None, (_) => target.EndSend(_), null);
        }
        public static void BeginSendData(Socket target, byte[] data, int offset)
        {
            int length = data.Length - offset;
            target.Send(BitConverter.GetBytes(length));
            target.BeginSend(data, 0, length, SocketFlags.None, (_) => target.EndSend(_), null);
        }
        public static void BeginSendData(Socket target, byte[] data)
        {
            int length = data.Length;
            target.Send(BitConverter.GetBytes(length));
            target.BeginSend(data, 0, length, SocketFlags.None, (_) => target.EndSend(_), null);
        }

        public static void BoardcastData(IEnumerable<Socket> targets, byte[] data, int offset, int length)
        {
            foreach (Socket socket in targets)
                EventedSocket.SendData(socket, data, offset, length);
        }
        public static void BoardcastData(IEnumerable<Socket> targets, byte[] data, int offset)
        {
            foreach (Socket socket in targets)
                EventedSocket.SendData(socket, data, offset);
        }
        public static void BoardcastData(IEnumerable<Socket> targets, byte[] data)
        {
            foreach (Socket socket in targets)
                SendData(socket, data);
        }
        public static void BeginBoardcastData(IEnumerable<Socket> targets, byte[] data, int offset, int length, AsyncCallback callback, object state)
        {
            foreach (Socket socket in targets)
                BeginSendData(socket, data, offset, length, callback, state);
        }
        public static void BeginBoardcastData(IEnumerable<Socket> targets, byte[] data, int offset, AsyncCallback callback, object state)
        {
            foreach (Socket socket in targets)
                BeginSendData(socket, data, offset, callback, state);
        }
        public static void BeginBoardcastData(IEnumerable<Socket> targets, byte[] data, AsyncCallback callback, object state)
        {
            foreach (Socket socket in targets)
                BeginSendData(socket, data, callback, state);
        }
        public static void BeginBoardcastData(IEnumerable<Socket> targets, byte[] data, int offset, int length)
        {
            foreach (Socket socket in targets)
                BeginSendData(socket, data, offset, length);
        }
        public static void BeginBoardcastData(IEnumerable<Socket> targets, byte[] data, int offset)
        {
            foreach (Socket socket in targets)
                BeginSendData(socket, data, offset);
        }
        public static void BeginBoardcastData(IEnumerable<Socket> targets, byte[] data)
        {
            foreach (Socket socket in targets)
                BeginSendData(socket, data);
        }

        public static bool TrySendData(Socket target, byte[] data, int offset, int length)
        {
            try { SendData(target, data, offset, length); return true; } catch { return false; }
        }
        public static bool TrySendData(Socket target, byte[] data, int offset)
        {
            try { SendData(target, data, offset); return true; } catch { return false; }
        }
        public static bool TrySendData(Socket target, byte[] data)
        {
            try { SendData(target, data); return true; } catch { return false; }
        }
        public static bool TryBeginSendData(Socket target, byte[] data, int offset, int length, AsyncCallback callback, object state)
        {
            try { BeginSendData(target, data, offset, length, callback, state); return true; } catch { return false; }
        }
        public static bool TryBeginSendData(Socket target, byte[] data, int offset, AsyncCallback callback, object state)
        {
            try { BeginSendData(target, data, offset, callback, state); return true; } catch { return false; }
        }
        public static bool TryBeginSendData(Socket target, byte[] data, AsyncCallback callback, object state)
        {
            try { BeginSendData(target, data, callback, state); return true; } catch { return false; }
        }
        public static bool TryBeginSendData(Socket target, byte[] data, int offset, int length)
        {
            try { BeginSendData(target, data, offset, length); return true; } catch { return false; }
        }
        public static bool TryBeginSendData(Socket target, byte[] data, int offset)
        {
            try { BeginSendData(target, data, offset); return true; } catch { return false; }
        }
        public static bool TryBeginSendData(Socket target, byte[] data)
        {
            try { BeginSendData(target, data); return true; } catch { return false; }
        }
                      
        public static bool TryBoardcastData(IEnumerable<Socket> targets, byte[] data, int offset, int length)
        {
            try { BeginBoardcastData(targets, data, offset, length); return true; } catch { return false; }
        }
        public static bool TryBoardcastData(IEnumerable<Socket> targets, byte[] data, int offset)
        {
            try { BeginBoardcastData(targets, data, offset); return true; } catch { return false; }
        }
        public static bool TryBoardcastData(IEnumerable<Socket> targets, byte[] data)
        {
            try { BeginBoardcastData(targets, data); return true; } catch { return false; }
        }
        public static bool TryBeginBoardcastData(IEnumerable<Socket> targets, byte[] data, int offset, int length, AsyncCallback callback, object state)
        {
            try { BeginBoardcastData(targets, data, offset, length, callback, state); return true; } catch { return false; }
        }
        public static bool TryBeginBoardcastData(IEnumerable<Socket> targets, byte[] data, int offset, AsyncCallback callback, object state)
        {
            try { BeginBoardcastData(targets, data, offset, callback, state); return true; } catch { return false; }
        }
        public static bool TryBeginBoardcastData(IEnumerable<Socket> targets, byte[] data, AsyncCallback callback, object state)
        {
            try { BeginBoardcastData(targets, data, callback, state); return true; } catch { return false; }
        }
        public static bool TryBeginBoardcastData(IEnumerable<Socket> targets, byte[] data, int offset, int length)
        {
            try { BeginBoardcastData(targets, data, offset, length); return true; } catch { return false; }
        }
        public static bool TryBeginBoardcastData(IEnumerable<Socket> targets, byte[] data, int offset)
        {
            try { BeginBoardcastData(targets, data, offset); return true; } catch { return false; }
        }
        public static bool TryBeginBoardcastData(IEnumerable<Socket> targets, byte[] data)
        {
            try { BeginBoardcastData(targets, data); return true; } catch { return false; }
        }
    }

    public class SocketServer
    {
        Socket server;                                               // 用来接受连接, 接收数据, 转发数据的套接字
        Dictionary<Socket, SocketStateObject> clientStates;                // 缓冲区

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
                return clientStates.Count;
            }
        }
        public void Start(int port, int backlog)
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientStates = new Dictionary<Socket, SocketStateObject>();
            server.Bind(new IPEndPoint(IPAddress.Any, port));
            server.Listen(backlog);
            server.BeginAccept(AcceptAction, null);
        }
        public void Stop()
        {
            server.Close();
            clientStates = null;
        }

        public event EventHandler<SocketConnectedArgs> ClientConnected;
        public event EventHandler<SocketReceivedDataArgs> ReceivedClientData;
        public event EventHandler<SocketDisconnectedArgs> ClientDisconnected;

        public void BoardcastData(byte[] data, int offset, int length)
        {
            EventedSocket.BoardcastData(clientStates.Keys, data, offset, length);
        }
        public void BoardcastData(byte[] data, int offset)
        {
            EventedSocket.BoardcastData(clientStates.Keys, data, offset);
        }
        public void BoardcastData(byte[] data)
        {
            EventedSocket.BoardcastData(clientStates.Keys, data);
        }
        public void BeginBoardcastData(byte[] data, int offset, int length, AsyncCallback callback, object state)
        {
            EventedSocket.BeginBoardcastData(clientStates.Keys, data, offset, length, callback, state);
        }
        public void BeginBoardcastData(byte[] data, int offset, AsyncCallback callback, object state)
        {
            EventedSocket.BeginBoardcastData(clientStates.Keys, data, offset, callback, state);
        }
        public void BeginBoardcastData(byte[] data, AsyncCallback callback, object state)
        {
            EventedSocket.BeginBoardcastData(clientStates.Keys, data, callback, state);
        }
        public void BeginBoardcastData(byte[] data, int offset, int length)
        {
            EventedSocket.BeginBoardcastData(clientStates.Keys, data, offset, length);
        }
        public void BeginBoardcastData(byte[] data, int offset)
        {
            EventedSocket.BeginBoardcastData(clientStates.Keys, data, offset);
        }
        public void BeginBoardcastData(byte[] data)
        {
            EventedSocket.BeginBoardcastData(clientStates.Keys, data);
        }

        void AcceptAction(IAsyncResult ar)
        {
            Socket client = server.EndAccept(ar);
            OnSocketConnected(client);

            SocketStateObject state = new SocketStateObject();
            clientStates[client] = state;
            state.WorkSocket = client;
            client.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveAction, state);

            server.BeginAccept(AcceptAction, null);
        }
        void ReceiveAction(IAsyncResult ar)
        {
            SocketStateObject state = (SocketStateObject)ar.AsyncState;
            Socket client = state.WorkSocket;
            int size = 0;
            try
            {
                size = client.EndReceive(ar);

                if (state.IsLength)
                {
                    if (size == 4)
                    {
                        int bodySize = BitConverter.ToInt32(state.Buffer, 0);
                        state.Buffer = new byte[bodySize];
                    }
                    else
                    {
                        client.Close();
                        OnSocketDisconnected(client);
                    }
                }
                else
                {
                    OnSocketReceivedData(client, state.Buffer, size);
                    state.Buffer = new byte[4];
                }

                if (size == 0)
                {
                    clientStates.Remove(client);
                    OnSocketDisconnected(client);
                    client.Close();
                }
                else
                {
                    state.IsLength = !state.IsLength;
                    client.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveAction, clientStates[client]);
                }
            }
            catch
            {
                clientStates.Remove(client);
                OnSocketDisconnected(client);
                client.Close();
                return;
            }
        }

        void OnSocketReceivedData(Socket socket, byte[] buffer, int size)
        {
            if (ReceivedClientData != null)
                ReceivedClientData.Invoke(this,
                    new SocketReceivedDataArgs(socket, buffer, size));
        }
        void OnSocketDisconnected(Socket socket)
        {
            if (ClientDisconnected != null)
                ClientDisconnected.Invoke(this, new SocketDisconnectedArgs(socket));
        }
        void OnSocketConnected(Socket socket)
        {
            if (ClientConnected != null)
                ClientConnected.Invoke(this, new SocketConnectedArgs(socket));
        }
    }
    public class SocketClient
    {
        SocketStateObject socketState = new SocketStateObject();

        public bool Connected { get => socketState.WorkSocket != null ? socketState.WorkSocket.Connected : false; }
        public Socket Server { get => socketState.WorkSocket; }

        public void ConnectTo(EndPoint address)
        {
            socketState.WorkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketState.WorkSocket.Connect(address);

            try
            {
                socketState.WorkSocket.BeginReceive(socketState.Buffer, 0, socketState.Buffer.Length, SocketFlags.None, ReceiveAction, null);
            }
            catch
            {
                OnSocketDisconnected(socketState.WorkSocket);
            }
        }

        public event EventHandler<SocketReceivedDataArgs> ReceivedData;
        public event EventHandler<SocketDisconnectedArgs> Disconnected;

        public void SendData(byte[] data, int offset, int length)
        {
            Socket target = socketState.WorkSocket;
            EventedSocket.SendData(target, data, offset, length);
        }
        public void SendData(byte[] data, int offset)
        {
            Socket target = socketState.WorkSocket;
            EventedSocket.SendData(target, data, offset);
        }
        public void SendData(byte[] data)
        {
            Socket target = socketState.WorkSocket;
            EventedSocket.SendData(target, data);
        }
        public void BeginSendData(byte[] data, int offset, int length, AsyncCallback callback, object state)
        {
            Socket target = socketState.WorkSocket;
            EventedSocket.BeginSendData(target, data, offset, length, callback, state);
        }
        public void BeginSendData(byte[] data, int offset, AsyncCallback callback, object state)
        {
            Socket target = socketState.WorkSocket;
            EventedSocket.BeginSendData(target, data, offset, callback, state);
        }
        public void BeginSendData(byte[] data, AsyncCallback callback, object state)
        {
            Socket target = socketState.WorkSocket;
            EventedSocket.BeginSendData(target, data, callback, state);
        }
        public void BeginSendData(byte[] data, int offset, int length)
        {
            Socket target = socketState.WorkSocket;
            EventedSocket.BeginSendData(target, data, offset, length);
        }
        public void BeginSendData(byte[] data, int offset)
        {
            Socket target = socketState.WorkSocket;
            EventedSocket.BeginSendData(target, data, offset);
        }
        public void BeginSendData(byte[] data)
        {
            Socket target = socketState.WorkSocket;
            EventedSocket.BeginSendData(target, data);
        }

        void ReceiveAction(IAsyncResult ar)
        {
            int size = 0;
            try
            {
                size = socketState.WorkSocket.EndReceive(ar);
                if (size == 0)
                {
                    socketState.WorkSocket.Close();
                    OnSocketDisconnected(socketState.WorkSocket);
                }
                else
                {
                    try
                    {
                        if (socketState.IsLength)
                        {
                            if (size == 4)
                            {
                                int newSize = BitConverter.ToInt32(socketState.Buffer, 0);
                                socketState.Buffer = new byte[newSize];
                            }
                            else
                            {
                                socketState.WorkSocket.Close();
                                OnSocketDisconnected(socketState.WorkSocket);
                            }
                        }
                        else
                        {
                            OnSocketReceivedData(socketState.WorkSocket, socketState.Buffer, size);
                            socketState.Buffer = new byte[4];
                        }

                        socketState.IsLength = !socketState.IsLength;
                        socketState.WorkSocket.BeginReceive(
                            socketState.Buffer, 0,
                            socketState.Buffer.Length,
                            SocketFlags.None,
                            ReceiveAction, null);
                    }
                    catch
                    {
                        OnSocketDisconnected(socketState.WorkSocket);
                    }
                }
            }
            catch
            {
                socketState.WorkSocket.Close();
                OnSocketDisconnected(socketState.WorkSocket);
                return;
            }
        }

        void OnSocketReceivedData(Socket socket, byte[] buffer, int size)
        {
            if (ReceivedData != null)
                ReceivedData.Invoke(this,
                    new SocketReceivedDataArgs(socket, buffer, size));
        }
        void OnSocketDisconnected(Socket socket)
        {
            if (Disconnected != null)
                Disconnected.Invoke(this, new SocketDisconnectedArgs(socket));
        }
    }
    public class SocketStateObject
    {
        public Socket WorkSocket = null;
        public bool IsLength = true;
        public byte[] Buffer = new byte[4];

        public SocketStateObject()
        {
            WorkSocket = null;
            IsLength = true;
            Buffer = new byte[4];
        }
    }
}
