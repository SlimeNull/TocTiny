using System;
using NullLib.DynamicScanner;

namespace TocTiny.Server.Core
{
    public static class App
    {
        public static void SafeWriteLine(string content)
        {
            if (DynamicScanner.IsInputting)
            {
                DynamicScanner.ClearDisplayBuffer();
                Console.WriteLine(content);
                Console.ForegroundColor = ConsoleColor.Gray;
                DynamicScanner.DisplayTo(Console.CursorLeft, Console.CursorTop);
            }
            else
            {
                Console.WriteLine(content);
            }
        }
    }
}
