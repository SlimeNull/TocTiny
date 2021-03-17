using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TocTiny.Client.View
{
    /// <summary>
    /// MessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class MsgBox : UserControl
    {
        public MsgBox()
        {
            InitializeComponent();
        }
        public string Title { get => BoxTitle.Content.ToString(); set => BoxTitle.Content = value; }
        public string TextContent { get => BoxText.Content.ToString(); set => BoxText.Content = value; }
        public ImageSource ImageContent { get => BoxImage.Source; set => BoxImage.Source = value; }

        private void BoxImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            new ImageViewer(Window.GetWindow(this)) { Source = BoxImage.Source as BitmapImage }.Show();
        }
    }
    [ValueConversion(typeof(HorizontalAlignment), typeof(CornerRadius))]
    public class CornerRadiusConverter : TypeConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((HorizontalAlignment)value)
            {
                case HorizontalAlignment.Left:
                    return new CornerRadius(0, 3, 3, 3);
                case HorizontalAlignment.Right:
                    return new CornerRadius(3, 0, 3, 3);
                default:
                    return new CornerRadius(0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
    [ValueConversion(typeof(HorizontalAlignment), typeof(Thickness))]
    public class BorderThicknessConverter : TypeConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((HorizontalAlignment)value)
            {
                case HorizontalAlignment.Left:
                case HorizontalAlignment.Right:
                    return new Thickness(1);
                default:
                    return new Thickness(0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
