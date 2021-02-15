using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TocTiny.Client.ViewModel
{
    public class IntegerConverter : DependencyObject, IValueConverter
    {
        object Integer2String(int num)
        {
            if (num == default)
                return DependencyProperty.UnsetValue;

            return num.ToString();
        }
        object String2Integer(string str)
        {
            if (string.IsNullOrEmpty(str))
                return DependencyProperty.UnsetValue;

            if (int.TryParse(str, out int result))
                return result;
            else
                return DependencyProperty.UnsetValue;
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != null)
            {
                if (targetType == typeof(string) && value.GetType() == typeof(int))
                    return Integer2String((int)value);
                if (targetType == typeof(int) && value.GetType() == typeof(string))
                    return String2Integer((string)value);
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
    public class IPAddressConverter : DependencyObject, IValueConverter
    {
        object String2IPAddress(string str)
        {
            if (string.IsNullOrEmpty(str))
                return DependencyProperty.UnsetValue;

            IPAddress[] addresses = Dns.GetHostAddresses(str);

            if (addresses.Length > 0)
                return addresses[0];
            else
                return DependencyProperty.UnsetValue;
        }
        object IPAddress2String(IPAddress address)
        {
            if (address == null)
                return DependencyProperty.UnsetValue;

            return address.ToString();
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != null)
            {
                if (targetType == typeof(string) && value.GetType() == typeof(IPAddress))
                    return IPAddress2String((IPAddress)value);
                if (targetType == typeof(IPAddress) && value.GetType() == typeof(string))
                    return String2IPAddress((string)value);
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}
