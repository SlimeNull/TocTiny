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
using TocTiny.Public;
using CHO.Json;
using TocTiny.Server.Core;

namespace TocTiny
{
    class Program
    {
        static ExecuteArgs Initialize(string[] args)
        {
            scanner = new DynamicScanner();

            ConsArgs consArgs = new ConsArgs(args);
            StartupArgs startupArgs = consArgs.ToObject<StartupArgs>();
            return startupArgs.DeepParse();
        }
        static DynamicScanner scanner;
        static void DisplayHelpAndEnd()
        {
            App.SafeWriteLine(scanner, string.Join('\n',
                "TOC Tiny : TOC Tiny 的服务端程序",
                "",
                "  TocTiny [-Port 端口] [-Backlog 监听数量] /[? | Help]",
                "",
                "    port       : 将要被监听的端口号.",
                "    bufferSize : 接收消息的缓冲区大小. 默认是1024576B(1MB)",
                "    backlog    : 等待连接的队列大小. 默认是50.",
                "    ",
                ""));

            Environment.Exit(0);
        }


        static void Main(string[] cargs)
        {
            ExecuteArgs args = Initialize(cargs);

            TocTinyServer tocTinyServer = new TocTinyServer()
            {
                CleanInverval = 2000
            };

            Console.WriteLine($"Server started at port {args.Port}");

            tocTinyServer.PackageReceived += TocTinyServer_PackageReceived;
            tocTinyServer.ClientConnected += TocTinyServer_ClientConnected;
            tocTinyServer.ClientDisconnected += TocTinyServer_ClientDisconnected;
            tocTinyServer.StartServer();

            while (true)
                Console.ReadKey(true);
        }

        private static void TocTinyServer_ClientDisconnected(object sender, ClientDisconnectedArgs args)
        {
            Console.WriteLine($"{args.Client.RemoteEndPoint} disconnected.");
        }

        private static void TocTinyServer_ClientConnected(object sender, ClientConnectedArgs args)
        {
            Console.WriteLine($"{args.Client.RemoteEndPoint} connected.");
        }

        private static void TocTinyServer_PackageReceived(object sender, PackageReceivedArgs args)
        {
            switch(args.Package.PackageType)
            {
                case ConstDef.NormalMessage:
                    args.Boardcast = true;
                    return;
                case ConstDef.Verification:
                    args.Boardcast = true;
                    return;
                case ConstDef.ImageMessage:
                    args.Boardcast = true;
                    return;
                case ConstDef.DrawAttention:
                    args.Boardcast = true;
                    return;
                case ConstDef.HeartPackage:
                    return;
                case ConstDef.ChangeChannelName:
                    // to do: 返回错误
                    return;
                case ConstDef.ReportChannelOnline:
                    // to do: 处理消息
                    return;
            }
        }
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
