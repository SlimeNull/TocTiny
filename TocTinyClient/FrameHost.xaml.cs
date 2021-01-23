using System;
using System.Windows;

namespace TocTinyClient
{
    /// <summary>
    /// FrameHost.xaml 的交互逻辑
    /// </summary>
    public partial class FrameHost
    {
        public FrameHost()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            throw new Exception("测试异常");
        }
    }
}
