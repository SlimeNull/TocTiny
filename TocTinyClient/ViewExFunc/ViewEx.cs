using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TocTiny.Client.ViewExFunc
{
    public static class ViewEx
    {
        public static MessageBoxResult ErrorMsg(string content, string title = "Error")
        {
            return MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static MessageBoxResult WarnMsg(string content, string title = "Warning")
        {
            return MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static MessageBoxResult AskMsg(string content, string title = "?")
        {
            return MessageBox.Show(content, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        }
    }
}
