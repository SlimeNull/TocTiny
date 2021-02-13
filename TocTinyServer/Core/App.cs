using System;
using Null.Library;

namespace TocTiny.Server.Core
{
    public static class App
    {
        public static void SafeWriteLine(DynamicScanner scanner, string content)
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
    }
}
