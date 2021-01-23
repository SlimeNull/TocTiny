using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xaml;

namespace TocTinyClient.Localization
{
    static class Localization
    {
        internal static ResourceDictionary resourceDictionary;
        public static void SetLanguage(string name)
        {
            switch (name)
            {
                case "en-us":
                    resourceDictionary = new ResourceDictionary() { Source = new Uri("pack://Application:,,,/Localization/内置美国英语.xaml") };
                    break;
                case "zh-cn":
                    resourceDictionary = new ResourceDictionary() { Source= new Uri("pack://Application:,,,/Localization/内置简体中文.xaml") };
                    break;
                default:
                    throw new ArgumentException(nameof(name));
            }
        }
        public static void LoadExtendLocalization(string path)
        {
            XamlObjectReader xr = new XamlObjectReader(path);
            resourceDictionary = (ResourceDictionary)xr.Instance;
        }
        public static string GetLocalizationString(string key)
        {

            return $"Fall to get localization string\"{key}\"";
        }
    }
}
