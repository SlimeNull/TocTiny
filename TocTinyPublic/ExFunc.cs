using CHO.Json;
using Null.Library.EventedSocket;
using System;
using System.Collections;
using System.Collections.Generic;
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
