using CHO.Json;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace TocTiny.Public
{
    public static class ExFunc
    {
        public static void ErrorExit(string desc, int errCode)
        {
            Console.WriteLine(desc);
            Environment.Exit(errCode);
        }
        public static bool SafeAsyncSendData(Socket socket, byte[] data)
        {
            return SafeAsyncSendData(socket, data, 0, data.Length);
        }
        public static bool SafeAsyncSendData(Socket socket, byte[] data, int offset, int size)
        {
            return TryDo(() => socket.BeginSend(data, offset, size, SocketFlags.None, (_) => { }, null));
        }        // 异步发送数据
        public static void AsyncSendData(Socket socket, byte[] data)
        {
            AsyncSendData(socket, data, 0, data.Length);
        }
        public static void AsyncSendData(Socket socket, byte[] data, int offset, int size)
        {
            socket.BeginSend(data, offset, size, SocketFlags.None, (_) => { }, null);
        }
        public static bool TryGetPackages(byte[] data, out TransPackage[] packages)
        {
            return TryGetPackages(data, data.Length, out packages);
        }
        public static bool TryGetPackages(byte[] data, int size, out TransPackage[] packages)
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
        public static bool TryDo(Action action)
        {
            try
            {
                action.Invoke();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
