using System;
using TocTinyClient;

namespace TocTiny
{
    internal static class Program
    {
        [STAThread()]
        private static void Main()
        {
            App app = new App();
            FrameHost frameHost = new FrameHost();
            frameHost.Loaded += (sender, e) => frameHost.Frame.Navigate(new Login());
            app.Run(frameHost);
        }
        internal static void Navigate(object page)
        {
            (App.Current.MainWindow as FrameHost).Frame.Navigate(page);
        }
    }
}
