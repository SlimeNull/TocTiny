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
        static ExecuteArgs Initialize(string[] args)
        {
            Arguments nargs = new Arguments(
                new FieldArgument("Port"),
                new FieldArgument("Backlog"),
                new FieldArgument("BufferTimeout"),
                new FieldArgument("CleanInterval"),
                new SwitchArgument("NoCmd"),
                new SwitchArgument("Help"),
                new SwitchArgument("H"));
            nargs.Parse(args);
            StartupArgs startupArgs = nargs.ToObject<StartupArgs>();
            return startupArgs.DeepParse();
        }
        static TocTinyServer tocTinyServer;
        static void DisplayHelpAndExit()
        {

            Environment.Exit(0);
        }

        static int onlineCount = 0;


        static void Main(string[] cargs)
        {
            ExecuteArgs args = Initialize(cargs);

            tocTinyServer = new TocTinyServer(args.Port)
            {
                CleanInverval = 2000
            };

            Console.WriteLine($"Server started at port {args.Port}");

            tocTinyServer.PackageReceived += TocTinyServer_PackageReceived;
            tocTinyServer.ClientConnected += TocTinyServer_ClientConnected;
            tocTinyServer.ClientDisconnected += TocTinyServer_ClientDisconnected;

            try
            {
                tocTinyServer.StartServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Binding failed. Please check if the port {args.Port} was listened by another application.");
                return;
            }

            while (true)
                Console.ReadKey(true);
        }

        private static void TocTinyServer_ClientDisconnected(object sender, ClientDisconnectedArgs args)
        {
            Console.WriteLine($"Client: {args.Client.BaseSocket.RemoteEndPoint} disconnected.");

            onlineCount--;
        }

        private static void TocTinyServer_ClientConnected(object sender, ClientConnectedArgs args)
        {
            Console.WriteLine($"Client: {args.Client.BaseSocket.RemoteEndPoint} connected.");

            onlineCount++;
        }

        private static void TocTinyServer_PackageReceived(object sender, PackageReceivedArgs args)
        {
            switch (args.Package.PackageType)
            {
                case ConstDef.NormalMessage:
                    Console.WriteLine($"{args.Package.Name}: {args.Package.Content}");
                    args.Boardcast = true;
                    return;
                case ConstDef.Verification:
                    args.Boardcast = true;
                    return;
                case ConstDef.ImageMessage:
                    Console.WriteLine($"{args.Package.Name}- Image (of size:{args.Package.Content.Length} base64 chars)");
                    args.Boardcast = true;
                    return;
                case ConstDef.DrawAttention:
                    Console.WriteLine($"{args.Package.Name}- Attention");
                    args.Boardcast = true;
                    return;
                case ConstDef.HeartPackage:
                    return;
                case ConstDef.ChangeChannelName:
                    //to do: deal this message
                    return;
                case ConstDef.ReportChannelOnline:
                    args.Sender.SendData(Encoding.UTF8.GetBytes(JsonData.ConvertToText(JsonData.Create(new TransPackage()
                    {
                        Name = "Server",
                        Content = $"Online: {onlineCount}; Your IP Address: {((IPEndPoint)args.Sender.BaseSocket.RemoteEndPoint).Address};",
                        ClientGuid = "Server",
                        PackageType = ConstDef.ReportChannelOnline
                    }))));
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
