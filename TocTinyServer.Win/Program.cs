using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CHO.Json;
using TocTiny.Public;
using TocTiny.Server.Core;
using NullLib.ArgsParser;
using NullLib.EventedSocket;
using NullLib.DynamicScanner;

namespace TocTiny
{
    class Program
    {
        static int onlineCount = 0;
        static ExecuteArgs Initialize(string[] args)
        {
            Arguments nargs = new Arguments(
                new FieldArgument("Port") { Alias = "P" },
                new FieldArgument("Backlog") { Alias = "B" },
                new FieldArgument("BufferTimeout") { Alias = "BT" },
                new FieldArgument("CleanInterval") { Alias = "CI" },
                new SwitchArgument("NoCmd") { Alias = "NC" },
                new SwitchArgument("Help") { Alias = "H" });
            nargs.Parse(args);
            StartupArgs startupArgs = nargs.ToObject<StartupArgs>();
            ExecuteArgs exeArgs = startupArgs.DeepParse();
            if (exeArgs.Help)
                DisplayHelpAndExit();
            return exeArgs;
        }
        static TocTinyServer tocTinyServer;
        static void DisplayHelpAndExit()
        {
            SafePrintText(string.Join("\n",
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
        static void SafePrintText(object obj, bool newline = true)
        {
            if (DynamicScanner.IsInputting)
            {
                DynamicScanner.ClearDisplayBuffer();
                Console.SetCursorPosition(DynamicScanner.StartLeft, DynamicScanner.StartTop);
                Console.Write(obj);
                if (newline)
                    Console.WriteLine();
                DynamicScanner.SetInputStart(Console.CursorLeft, Console.CursorTop, false);
            }
            else
            {
                Console.Write(obj);
                if (newline)
                    Console.WriteLine();
            }
        }

        static void Main(string[] cargs)
        {
            Console.WriteLine("Null: TOC Tiny. Copyright 2021 Null, All rights reserved.\n");
            ExecuteArgs args = Initialize(cargs);

            tocTinyServer = new TocTinyServer(args.Port)
            {
                BufferTimeout = args.BufferTimeout,
                CleanInverval = args.CleanInterval,
                Backlog = args.Backlog,
                HistoryMaxCount = 20,
            };

            SafePrintText($"Server started at port {args.Port}", true);

            tocTinyServer.PackageReceived += TocTinyServer_PackageReceived;
            tocTinyServer.ClientConnected += TocTinyServer_ClientConnected;
            tocTinyServer.ClientDisconnected += TocTinyServer_ClientDisconnected;

            try
            {
                tocTinyServer.StartServer();
            }
            catch (Exception ex)
            {
                SafePrintText($"Binding failed. Please check if the port {args.Port} was listened by another application. Exception:{ex.Message}", true);
                return;
            }

            while (true)
                DynamicScanner.ReadLine();
        }

        private static void TocTinyServer_ClientDisconnected(object sender, ClientDisconnectedArgs args)
        {
            //SafePrintText($"Client: {args.Client.BaseSocket.RemoteEndPoint} disconnected.", true);
            onlineCount--;
        }

        private static void TocTinyServer_ClientConnected(object sender, ClientConnectedArgs args)
        {
            SafePrintText($"Client: {args.Client.BaseSocket.RemoteEndPoint} connected.", true);
            onlineCount++;
        }

        private static void TocTinyServer_PackageReceived(object sender, PackageReceivedArgs args)
        {
            switch (args.Package.PackageType)
            {
                case ConstDef.NormalMessage:
                    args.Record = true;
                    break;
            }
            switch (args.Package.PackageType)
            {
                case ConstDef.NormalMessage:
                case ConstDef.Verification:
                case ConstDef.ImageMessage:
                case ConstDef.DrawAttention:
                    args.Boardcast = true;
                    break;
            }
            switch (args.Package.PackageType)
            {
                case ConstDef.NormalMessage:
                    SafePrintText($"{args.Package.Name}: {args.Package.Content}", true);
                    break;
                case ConstDef.ImageMessage:
                    SafePrintText($"{args.Package.Name}- Image (of size:{args.Package.Content.Length} base64 chars)", true);
                    break;
                case ConstDef.DrawAttention:
                    SafePrintText($"{args.Package.Name}- Attention", true);
                    break;
                case ConstDef.ReportChannelOnline:
                    args.Sender.BeginSendData(Encoding.UTF8.GetBytes(JsonData.ConvertToText(JsonData.Create(new TransPackage()
                    {
                        Name = "Server",
                        Content = $"Online: {onlineCount}; Your IP Address: {((IPEndPoint)args.Sender.BaseSocket.RemoteEndPoint).Address};",
                        ClientGuid = "Server",
                        PackageType = ConstDef.ReportChannelOnline
                    }))));
                    break;
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
