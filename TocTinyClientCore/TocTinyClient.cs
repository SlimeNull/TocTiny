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
using Null.Library.EventedSocket;
using TocTiny;
using TocTiny.Public;

namespace TocTiny.Core
{
    public class TocTinyClient
    {
        SocketClient socketClient;

        TimeSpan btimeout = TimeSpan.FromSeconds(2);
        double BufferTimeout
        {
            get => btimeout.TotalMilliseconds;
            set
            {
                btimeout = TimeSpan.FromMilliseconds(value);
            }
        }
        double CleanInterval
        {
            get => bufferCleanner.Interval;
            set
            {
                bufferCleanner.Interval = value;
            }
        }

        string userName;
        string clientGuid;
        Timer bufferCleanner;
        DateTime lastRecv;
        MemoryStream partBuffer;

        public string UserName { get => userName; set => userName = value; }
        public string ClientGuid { get => clientGuid; }
        public SocketClient SocketClient { get => socketClient; }
        public Socket Server { get => socketClient.Server; }

        public TocTinyClient()
        {
            userName = Environment.UserName;
            clientGuid = Guid.NewGuid().ToString();
            lastRecv = DateTime.Now;
            bufferCleanner = new Timer();
            bufferCleanner.Elapsed += CleanAction;

            socketClient = new SocketClient();
            partBuffer = new MemoryStream();

            socketClient.ReceivedMsg += SocketClient_ReceivedMsg;
            socketClient.Disconnected += SocketClient_Disconnected;
        }

        private void CleanAction(object sender, ElapsedEventArgs e)
        {
            if (partBuffer.Length > 0 && DateTime.Now - lastRecv > btimeout)
            {
                partBuffer.SetLength(0);
            }
        }

        public void ConnectTo(IPEndPoint point, int bufferSize)
        {
            socketClient.ConnectTo(point, bufferSize);
        }
        private void SocketClient_Disconnected(Socket socket)
        {
            if (Disconnected != null)
                Disconnected.Invoke(this, EventArgs.Empty);

            bufferCleanner.Stop();
        }
        private void SocketClient_ReceivedMsg(Socket socket, byte[] buffer, int size)
        {
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
                    PackageReceivedEventArgs args = new PackageReceivedEventArgs(package);
                    PackageReceived.Invoke(this, args);

                    if (args.Postback)
                        ExFunc.SafeAsyncSendData(Server, buffer, 0, size);
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
        private void UpdatePartBuffer()
        {
            lastRecv = DateTime.Now;
        }
        private void ClearPartBuffer()
        {
            partBuffer.SetLength(0);
        }

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

                PackageType = ConstDef.NormalMessage
            });
        }
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
            ExFunc.AsyncSendData(socketClient.Server, 
                Encoding.UTF8.GetBytes(
                    JsonData.ConvertToText(
                        JsonData.Create(package))));
        }
        public bool TrySendPackage(TransPackage package)
        {
            return ExFunc.TryDo(() => SendPackage(package));
        }


        public event EventHandler<PackageReceivedEventArgs> PackageReceived;
        public event EventHandler Disconnected;
    }
    public class PackageReceivedEventArgs
    {
        public TransPackage Package;
        
        public bool Postback = false;

        public PackageReceivedEventArgs() { }
        public PackageReceivedEventArgs(TransPackage package)
        {
            this.Package = package;
        }
    }
}
