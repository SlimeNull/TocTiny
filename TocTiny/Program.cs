using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Null.Library;
using Null.Library.ConsArgsParser;
using Null.Library.EventedSocket;
using CHO.Json;

namespace TocTiny
{
    class Program
    {
        static string channelName;

        static readonly Dictionary<Socket, MemoryStream> clients = new Dictionary<Socket, MemoryStream>();        // 客户端以及 部分包的缓冲区 DateTime是上一次将内容写入到缓冲区的时间.
        static readonly Dictionary<Socket, DateTime> clientBufferWroteTime = new Dictionary<Socket, DateTime>();  //
        static readonly List<Socket> clientToRemove = new List<Socket>();
        static readonly List<byte[]> lastMessages = new List<byte[]>();
        static readonly Dictionary<string, (string, Socket)> clientRecord = new Dictionary<string, (string, Socket)>();         // guid : (name, socket)
        static int maxSaveCount = 15;

        static int port, bufferSize, backlog, btimeout, cinterval;

        static DynamicScanner scanner;

        static byte[] heartPackageData;
        static bool nocolor, nocmd;

        class StartupArgs
        {
            public string PORT = "2020";           // port 
            public string BUFFER = "1024576";      // buffer
            public string BACKLOG = "50";          // backlog
            public string BTIMEOUT = "1000";       // buffer timeout : 缓冲区存活时间 (ms)
            public string CINTERVAL = "1000";      // cleaning interval : 内存清理间隔 (ms)

            public string NAME = "TOC Tiny Chat Room";

            public bool NOCOLOR = false;       // string color : 是否开启控制台的消息文本高亮
            public bool NOCMD = false;
        }
        static void SafeWriteLine(string content)
        {
            if (scanner != null && scanner.IsInputting)
            {
                scanner.ClearDisplayBuffer();
                Console.WriteLine(content);
                Console.ForegroundColor = ConsoleColor.Gray;
                scanner.DisplayTo(Console.CursorLeft, Console.CursorTop);
            }
            else
            {
                Console.WriteLine(content);
            }
        }
        static void DisplayHelpAndEnd()
        {
            SafeWriteLine("TOC Tiny : Server program for TOC Tiny\n\n  TocTiny[-Port port][-Buffer bufferSize][-Backlog backlog][/? | / Help]\n\n    port: Port number to be listened.Default value is 2020.\n    bufferSize: Buffer size for receive data. Default value is 1024576(B)\n    backlog    : Maximum length of the pending connections queue.Default value is 50.\n");
            Environment.Exit(0);
        }
        static void Initialize(string[] args)
        {
            ConsArgs consArgs = new ConsArgs(args);
            if (consArgs.Booleans.Contains("?") || consArgs.Booleans.Contains("HELP"))
            {
                DisplayHelpAndEnd();
            }

            scanner = new DynamicScanner()
            {
                PromptText = "\n>>> "
            };

            SafeWriteLine($"Initilizing...");
            StartupArgs startupArgs = consArgs.ToObject<StartupArgs>();

            channelName = startupArgs.NAME;

            if (!uint.TryParse(startupArgs.PORT, out uint uport))
            {
                SafeWriteLine($"Argument out of range, an integer is required. Argument name: 'Port'.");
                Environment.Exit(-1);
            }
            if (!uint.TryParse(startupArgs.BUFFER, out uint ubuffer))
            {
                SafeWriteLine($"Argument out of range, an integer is required. Argument name: 'Buffer'.");
                Environment.Exit(-1);
            }
            if (!uint.TryParse(startupArgs.BACKLOG, out uint ubacklog))
            {
                SafeWriteLine($"Argument out of range, an integer is required. Argument name: 'Backlog'.");
                Environment.Exit(-1);
            }
            if (!uint.TryParse(startupArgs.BTIMEOUT, out uint ubtimeout))
            {
                SafeWriteLine($"Argument out of range, an integer is required. Argument name: 'BTimeout'.");
                Environment.Exit(-1);
            }
            if (!uint.TryParse(startupArgs.CINTERVAL, out uint ucinterval))
            {
                SafeWriteLine($"Argument out of range, an integer is required. Argument name: 'CInterval'.");
                Environment.Exit(-1);
            }

            nocolor = startupArgs.NOCOLOR;
            nocmd = startupArgs.NOCMD;

            port = (int)uport;
            bufferSize = (int)ubuffer;
            backlog = (int)ubacklog;
            btimeout = (int)ubtimeout;
            cinterval = (int)ucinterval;

            heartPackageData = Encoding.UTF8.GetBytes(
                JsonData.ConvertToText(
                    JsonData.Create(
                        new TransPackage()
                        {
                            Name = "Server",
                            Content = null,
                            ClientGuid = "Server",
                            PackageType = ConstDef.HeartPackage
                        })));

            new Thread(MemoryCleaningLoop).Start();                     // 开启内存循环清理线程
        }
        static void Main(string[] args)
        {
            Initialize(args);

            SocketServer server = new SocketServer();
            try
            {
                server.Start((int)port, (int)backlog, (int)bufferSize);
                SafeWriteLine($"Server started. Port: {port}, Backlog: {backlog}, Buffer: {bufferSize}(B).");
                server.ClientConnected += Server_ClientConnected;
                server.ClientDisconnected += Server_ClientDisconnected;
                server.RecvedClientMsg += Server_RecvedClientMsg;
                if (nocmd)
                {
                    SafeWriteLine("Server command is unavailable now.");
                    while (server.Running)
                    {
                        Console.ReadKey(true);
                    }
                }
                else
                {
                    SafeWriteLine("Server command is available now. use '/help' for help.");
                    while (server.Running)
                    {
                        string cmd = scanner.ReadLine();
                        DealCommand(cmd);
                    }
                }
            }
            catch
            {
                SafeWriteLine($"Start failed. check if the port {port} is being listened.");
                Environment.Exit(-2);
            }
        }

        private static void MemoryCleaningLoop()
        {
            while (true)
            {
                Thread.Sleep(cinterval);
                lock (clients)
                {
                    foreach (Socket i in clients.Keys)
                    {
                        if (clients[i] != null && DateTime.Now - clientBufferWroteTime[i] > TimeSpan.FromMilliseconds(btimeout))
                        {
                            DisposePartsBuffer(i);
                        }
                    }
                }
            }
        }
        private static void DisposePartsBuffer(Socket socket)
        {
            clients[socket].Dispose();
            clients[socket] = null;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            SafeWriteLine($"PartsBuffer Disposed: {socket.RemoteEndPoint}");
        }
        private static void DealCommand(string cmd)
        {
            if (cmd.StartsWith("/"))
            {

            }
            else
            {
                TransPackage msg = new TransPackage()
                {
                    Name = "Server",
                    Content = cmd,
                    ClientGuid = "Server",
                    PackageType = ConstDef.Verification
                };

                BoardcastPackage(msg);
            }
        }                                               // 解释指令
        private static void BoardcastPackage(TransPackage package)
        {
            JsonData jsonData = JsonData.Create(package);
            string jsonText = JsonData.ConvertToText(jsonData);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonText);
            BoardcastData(bytes, bytes.Length);
        }                                // 广播包
        private static bool TrySendData(Socket socket, byte[] data, int size, bool autoRemove = false)
        {
            try
            {
                if (size <= bufferSize)
                {
                    socket.Send(data, size, SocketFlags.None);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                if (autoRemove)
                {
                    lock (clients)
                    {
                        clients.Remove(socket);
                    }
                }
                return false;
            }
        }    // 尝试发送数据
        private static void BoardcastData(byte[] data, int size)
        {
            foreach (Socket i in clients.Keys)
            {
                try
                {
                    i.Send(data, size, SocketFlags.None);
                }
                catch
                {
                    clientToRemove.Add(i);
                }
            }

            lock (clients)
            {
                foreach (Socket i in clientToRemove)
                {
                    clients.Remove(i);
                    SafeWriteLine($"Removed a disconnected socket which address is {i.RemoteEndPoint}");
                }
            }

            clientToRemove.Clear();
        }                                  // 广播数据
        private static void DrawAttention(string senderName, string senderGuid)
        {
            byte[] attentionData = Encoding.UTF8.GetBytes(
                JsonData.ConvertToText(
                    JsonData.Create(new TransPackage()
                    {
                        Name = senderName,
                        Content = null,
                        ClientGuid = senderGuid,
                        PackageType = ConstDef.DrawAttention
                    })));

            BoardcastData(attentionData, attentionData.Length);
        }                   // 发送吸引注意力
        private static void Server_RecvedClientMsg(Socket socket, byte[] buffer, int size)
        {
            try
            {
                string recvStr = Encoding.UTF8.GetString(buffer, 0, size);
                JsonData[] recvJsons = JsonData.ParseStream(recvStr);
                foreach (JsonData recvJson in recvJsons)
                {
                    TransPackage recvPackage = JsonData.ConvertToInstance<TransPackage>(recvJson);
                    DealRecvPackage(recvPackage, ref socket, ref buffer, size);
                }
            }
            catch
            {
                if (clients[socket] == null)
                {
                    clients[socket] = new MemoryStream();
                    clients[socket].Write(buffer, 0, size);
                }
                else
                {
                    lock (clients[socket])
                    {
                        clients[socket].Write(buffer, 0, size);
                        try
                        {
                            byte[] totalBuffer = clients[socket].ToArray();
                            string bufferStr = Encoding.UTF8.GetString(clients[socket].ToArray());
                            if (JsonData.TryParseStream(bufferStr, out JsonData[] jsons))
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                SafeWriteLine($"PartsBuffer parse succeed from {socket.RemoteEndPoint}, size: {clients[socket].Length}.");
                                foreach (JsonData dealJson in jsons)
                                {
                                    TransPackage recvPackage = JsonData.ConvertToInstance<TransPackage>(dealJson);
                                    DealRecvPackage(recvPackage, ref socket, ref totalBuffer, totalBuffer.Length);
                                }
                            }
                        }
                        catch { }
                    }
                }

                clientBufferWroteTime[socket] = DateTime.Now;                       // 记录写入时间

                Console.ForegroundColor = ConsoleColor.DarkGray;
                SafeWriteLine($"Recieved data from {socket.RemoteEndPoint}, size: {size}, Wrote to PartsBuffer.");
            }
        }        // 当收到客户端消息
        private static void Server_ClientDisconnected(Socket socket)
        {
            SafeWriteLine($"Removed a disconnected socket which address is {socket.RemoteEndPoint}");

            lock (clients)
            {
                clients.Remove(socket);
            }
        }                              // 当客户端断开
        private static void Server_ClientConnected(Socket socket)
        {
            SafeWriteLine($"News socket connected, address: {socket.RemoteEndPoint}");

            lock (clients)
            {
                clients[socket] = null;
                clientBufferWroteTime[socket] = DateTime.Now;
            }

            new Thread(() =>
            {
                foreach (byte[] msg in lastMessages)
                {
                    TrySendData(socket, msg, msg.Length);
                }
            }).Start();
        }              // 当客户端连接
        private static void AddMessageRecord(byte[] buffer, int size)
        {
            byte[] newRecord = buffer.Take(size).ToArray();
            lastMessages.Add(newRecord);
            if (lastMessages.Count > maxSaveCount)
            {
                lastMessages.RemoveAt(0);
            }
        }                             // 添加消息记录
        private static void DealRecvPackage(TransPackage recvPackage, ref Socket socket, ref byte[] buffer, int size)
        {
            if (recvPackage.PackageType != ConstDef.HeartPackage)
            {
                switch (recvPackage.PackageType)
                {
                    case ConstDef.NormalMessage:
                        BoardcastData(buffer, size);
                        AddMessageRecord(buffer, size);
                        Console.ForegroundColor = ConsoleColor.Green;
                        SafeWriteLine($"{recvPackage.Name}: {recvPackage.Content}");
                        break;
                    case ConstDef.Verification:
                        clientRecord[recvPackage.ClientGuid] = (recvPackage.Name, socket);
                        BoardcastData(buffer, size);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        SafeWriteLine($"Verification: {recvPackage.Name} - {recvPackage.Content}");
                        break;
                    case ConstDef.ImageMessage:
                        BoardcastData(buffer, size);
                        AddMessageRecord(buffer, size);
                        Console.ForegroundColor = ConsoleColor.Blue;
                        SafeWriteLine($"Image: {recvPackage.Name} - Base string length: {recvPackage.Content.Length}");
                        break;
                    case ConstDef.DrawAttention:
                        DrawAttention(recvPackage.Name, recvPackage.ClientGuid);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        SafeWriteLine($"Draw Attention: Sender: {recvPackage.Name}");
                        break;
                    case ConstDef.HeartPackage:
                        TrySendData(socket, heartPackageData, heartPackageData.Length, true);
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        SafeWriteLine($"Heart Beat: Sender: {recvPackage.Name}");
                        socket.Send(heartPackageData);
                        break;
                    case ConstDef.ChangeChannelName:
                        socket.Send(
                            Encoding.UTF8.GetBytes(
                                JsonData.ConvertToText(
                                    JsonData.Create(new TransPackage()
                                    {
                                        Name = "Server",
                                        Content = channelName,
                                        ClientGuid = "Server",
                                        PackageType = ConstDef.ChangeChannelName
                                    }))));
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        SafeWriteLine($"Channel Name Req: Sender: {recvPackage.Name}");
                        break;
                    case ConstDef.ReportChannelOnline:
                        socket.Send(
                            Encoding.UTF8.GetBytes(
                                JsonData.ConvertToText(
                                    JsonData.Create(new TransPackage()
                                    {
                                        Name = "Server",
                                        Content = $"Online: {clients.Count}, Your IP Address: {((IPEndPoint)socket.RemoteEndPoint).Address}",
                                        ClientGuid = "Server",
                                        PackageType = ConstDef.ReportChannelOnline
                                    }))));
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        SafeWriteLine($"Online Info Req: Sender: {recvPackage.Name}");
                        break;
                    default:
                        SafeWriteLine($"Recieved data from {socket.RemoteEndPoint}, size: {size}, but cannot be processed.");
                        break;
                }

                if (clients[socket] != null)
                {
                    DisposePartsBuffer(socket);
                }
            }
        }      // 处理消息 (主函数
        private static TransPackage CreateChangeChannelPackage(string channelName)
        {
            return new TransPackage()
            {
                Name = "Server",
                Content = channelName,
                ClientGuid = "Server",
                PackageType = ConstDef.ChangeChannelName
            };
        }
    }
}
