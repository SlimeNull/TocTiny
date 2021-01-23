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
            app.Exit += (s, e) => { Environment.Exit(0); };
            app.DispatcherUnhandledException +=
                (sender, e) =>
                {
                    TocErrorReport tocErrorReport = new TocErrorReport();
                    tocErrorReport.dzzz.Text = e.Exception.ToString();
                    frameHost.Frame.Navigate(tocErrorReport);
                    e.Handled = true;
                    return;
                };
            app.Run(frameHost);
        }
        internal static void Navigate(object page)
        {
            (App.Current.MainWindow as FrameHost).Frame.Navigate(page);
        }
    }
}
