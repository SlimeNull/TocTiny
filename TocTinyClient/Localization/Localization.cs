using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xaml;

namespace TocTinyClient.Localization
{
    static class Localization
    {
        internal static ResourceDictionary resourceDictionary = new ResourceDictionary();
        static Localization()
        {
            if (Thread.CurrentThread.CurrentCulture.DisplayName.Contains("中文"))
            {
                SetLanguage("zh-cn");
            }
            else
            {
                SetLanguage("en-us");
            }
        }
        /// <summary>
        /// 设定本地化语言
        /// </summary>
        /// <param name="name"></param>
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
        internal static Dictionary<string, string> LocalizationCache = new Dictionary<string, string>();
        public static string GetLocalizationString(string key)
        {
            if (LocalizationCache.ContainsKey(key)) { return LocalizationCache[key]; }
            if (resourceDictionary.Keys.Cast<string>().Contains(key)) 
            {
                string localizationString = ((LocalizationItem)resourceDictionary[key]).LocalizationString;
                LocalizationCache.Add(key, localizationString);//缓存本地化
                return localizationString; 
            };
            return $"Fall to get localization string\"{key}\"";
        }
    }
}
