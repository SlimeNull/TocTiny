using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TocTinyClient
{
    /// <summary>
    /// TocErrorReport.xaml 的交互逻辑
    /// </summary>
    public partial class TocErrorReport
    {
        public TocErrorReport()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Thread.Sleep(300);
            Environment.Exit(0);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Environment.Exit(-1);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            dzzz.Visibility = Visibility.Visible;
        }
    }
}
