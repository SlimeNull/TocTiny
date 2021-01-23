using System.Windows;
using System.Windows.Controls;
using TocTiny;

namespace TocTinyClient
{
    /// <summary>
    /// PictureView.xaml 的交互逻辑
    /// </summary>
    public partial class PictureView : UserControl
    {
        public PictureView()
        {
            InitializeComponent();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if ((SV.ActualWidth == 0) | (SV.ActualHeight == 0)) { return; }
                double oldWidth = 0; double oldHeight = Picture.ActualHeight;
                if (Picture.Width == double.NaN) { oldWidth = Picture.ActualWidth; } else { oldWidth = Picture.Width; }
                Picture.Width = (SV.ActualWidth - 18) * e.NewValue / 100;
                Picture.Height = (SV.ActualHeight - 18) * e.NewValue / 100;
                SV.ScrollToHorizontalOffset(SV.HorizontalOffset + (-oldWidth + Picture.Width) / 2);
                SV.ScrollToVerticalOffset(SV.VerticalOffset + (-oldHeight + Picture.Height) / 2);
            }
            catch
            { 
            
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ImgRotate.Angle -= 90;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ImgRotate.Angle += 90;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            (App.Current.MainWindow as FrameHost).Frame.GoBack();
        }

        private void ScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SV.ScrollToHorizontalOffset(e.NewValue);
        }

        private void ScrollBar_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SV.ScrollToVerticalOffset(e.NewValue);
        }
    }
}
