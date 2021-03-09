using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TocTiny;

namespace TocTiny.Client.ViewExFunc
{
    public static class MainChatEx
    {
        #region UI Core
        public static Grid GenBasicMsgGrid(string name, UIElement content, HorizontalAlignment align)
        {
            Grid rstGrid = new Grid()
            {
                VerticalAlignment = VerticalAlignment.Top
            };
            rstGrid.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(25)
            });
            rstGrid.RowDefinitions.Add(new RowDefinition());

            Label nameLabel = new Label
            {
                Content = name,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = align
            };
            Grid.SetRow(nameLabel, 0);
            Border contentBorder = new Border()
            {
                Padding = new Thickness(3),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = align,
            };
            contentBorder.Child = content;
            Grid.SetRow(contentBorder, 1);
            rstGrid.Children.Add(nameLabel);
            rstGrid.Children.Add(contentBorder);
            return rstGrid;
        }
        public static Grid GenTextMsgGrid(string name, string content, HorizontalAlignment align)
        {
            UIElement contentBox = GenTextMsgContent(content, align);
            Grid msgBox = GenBasicMsgGrid(name, contentBox, align);
            return msgBox;
        }
        public static Grid GenImageMsgGrid(string name, string baseImage, HorizontalAlignment align)
        {
            UIElement contentBox = GenImageMsgContent(baseImage, align);
            Grid msgBox = GenBasicMsgGrid(name, contentBox, align);
            return msgBox;
        }
        public static UIElement GenTextMsgContent(string content, HorizontalAlignment align)
        {
            TextBox contentBox = new TextBox()
            {
                Text = content,
                IsReadOnly = true,
                IsReadOnlyCaretVisible = false,
                MaxLines = int.MaxValue,
                TextWrapping = TextWrapping.Wrap,
                BorderThickness = new Thickness(0),
                BorderBrush = new SolidColorBrush(Colors.Transparent),
                HorizontalAlignment = align,
                Cursor = Cursors.Arrow
            };
            return contentBox;
        }
        public static UIElement GenImageMsgContent(string baseImage, HorizontalAlignment align)
        {
            byte[] imgData;
            UIElement rst;
            try
            {
                imgData = Convert.FromBase64String(baseImage);
                MemoryStream stream = new MemoryStream(imgData);
                BitmapImage imgSrc = new BitmapImage();
                imgSrc.BeginInit();
                imgSrc.CacheOption = BitmapCacheOption.OnLoad;
                imgSrc.StreamSource = stream;
                imgSrc.EndInit();
                imgSrc.Freeze();

                stream.Dispose();

                rst = new Image()
                {
                    Source = imgSrc,
                    MaxWidth = 260,
                    AllowDrop = true,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = align,
                };

                return rst;
            }
            catch
            {
                rst = new TextBox()
                {
                    Text = "图像加载失败",
                    Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                    IsReadOnly = true,
                    IsReadOnlyCaretVisible = false,
                    MaxLines = int.MaxValue,
                    TextWrapping = TextWrapping.Wrap,
                    BorderThickness = new Thickness(0),
                    BorderBrush = new SolidColorBrush(Colors.Transparent),
                    Cursor = Cursors.Arrow
                };

                return rst;
            }
        }

        #endregion
    }
}
