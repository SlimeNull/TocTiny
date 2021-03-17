using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace TocTiny.Client.View
{
    /// <summary>
    /// ImageViewer.xaml 的交互逻辑
    /// </summary>
    public partial class ImageViewer : Window
    {
        private RotateTransform imageRotate;
        private bool isMouseDown = false;
        private Point lastPos;

        private double imgBoxX { get => Canvas.GetLeft(ImgBox); set => Canvas.SetLeft(ImgBox, value); }
        private double imgBoxY { get => Canvas.GetTop(ImgBox); set => Canvas.SetTop(ImgBox, value); }
        private double imgBoxWidth
        { 
            get => ImgBox.Width;
            set
            {
                ImgBox.Width = value;
                imageRotate.CenterX = value / 2;
            }
        }
        private double imgBoxHeight
        { 
            get => ImgBox.Height; 
            set
            {
                ImgBox.Height = value;
                imageRotate.CenterY = value / 2;
            }
        }

        private double wholeWidth => Width;
        private double wholeHeight => Height;
        private double imageWidth;
        private double imageHeight;

        private double scaleRatio = 1;

        public ImageViewer()
        {
            InitializeComponent();

            imageRotate = new RotateTransform();
            ImgBox.RenderTransform = imageRotate;

            MouseUp += (s, e) => isMouseDown = false;
            MouseLeave += (s, e) => isMouseDown = false;
            MouseDown += (s, e) =>
            {
                isMouseDown = true;
                lastPos = e.GetPosition(s as IInputElement);
            };

            MouseMove += Window_MouseMove;
            MouseWheel += Window_MouseWheel;
        }

        public ImageViewer(Window parent)
        {
            InitializeComponent();

            imageRotate = new RotateTransform();
            ImgBox.RenderTransform = imageRotate;

            MouseUp += (s, e) => isMouseDown = false;
            MouseLeave += (s, e) => isMouseDown = false;
            MouseDown += (s, e) =>
            {
                isMouseDown = true;
                lastPos = e.GetPosition(s as IInputElement);
            };

            MouseMove += Window_MouseMove;
            MouseWheel += Window_MouseWheel;

            this.Left = (parent.Width - Width) / 2 + parent.Left;
            this.Top = (parent.Height - Height) / 2 + parent.Top;
        }

        public BitmapImage Source
        { 
            get => ImgBox.Source as BitmapImage;
            set
            {
                ImgBox.Source = value;
                imageWidth = value.PixelWidth;
                imageHeight = value.PixelHeight;

                scaleRatio = wholeWidth / imageWidth;
                if (imageHeight * scaleRatio > wholeHeight)
                    scaleRatio = wholeHeight / imageHeight;

                imgBoxWidth = imageWidth * scaleRatio;
                imgBoxHeight = imageHeight * scaleRatio;

                imgBoxX = (wholeWidth - imgBoxWidth) / 2;
                imgBoxY = (wholeHeight - imgBoxHeight) / 2;
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point posNow = e.GetPosition(sender as IInputElement);
                imgBoxX += posNow.X - lastPos.X;
                imgBoxY += posNow.Y - lastPos.Y;
                lastPos = posNow;
            }
        }
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScaleImage(e.Delta, e.GetPosition(Whole));
        }
        private void ScaleImage(int delta, Point relativePoint)
        {
            double assignBy = Math.Pow(10, delta / 2000d);
            scaleRatio *= assignBy;
            imgBoxWidth = imageWidth * scaleRatio;
            imgBoxHeight = imageHeight * scaleRatio;
            imgBoxX -= (relativePoint.X - imgBoxX) * (assignBy - 1);
            imgBoxY -= (relativePoint.Y - imgBoxY) * (assignBy - 1);
        }
        private void RotateImage(int deg)
        {
            imageRotate.Angle += deg;
        }

        private void ScalePlus(object sender, RoutedEventArgs e)
        {
            ScaleImage(120, new Point(imgBoxX + imgBoxWidth / 2, imgBoxY + imgBoxHeight / 2));
        }

        private void ScaleReduse(object sender, RoutedEventArgs e)
        {
            ScaleImage(-120, new Point(imgBoxX + imgBoxWidth / 2, imgBoxY + imgBoxHeight / 2));
        }

        private void RotateLeft(object sender, RoutedEventArgs e)
        {
            RotateImage(-90);
        }

        private void RotateRight(object sender, RoutedEventArgs e)
        {
            RotateImage(90);
        }

        SaveFileDialog sfd = new SaveFileDialog()
        {
            Filter = "JPEG|*.jpg;*.jpeg|PNG|*.png|BMP|*.bmp|GIF|*.gif|TIFF|*.tiff|All|*.*",
            CheckPathExists = true,
        };
        private void SaveImage(object sender, RoutedEventArgs e)
        {
            if (sfd.ShowDialog().GetValueOrDefault(true))
            {
                BitmapImage bmpimg = ImgBox.Source as BitmapImage;
                if (bmpimg != null)
                {
                    string filename = sfd.FileName;
                    string extension = System.IO.Path.GetExtension(filename);
                    BitmapEncoder encoder = extension.ToUpper() switch
                    {
                        ".JPEG" => new JpegBitmapEncoder(),
                        ".JPG" => new JpegBitmapEncoder(),
                        ".PNG" => new PngBitmapEncoder(),
                        ".GIF" => new GifBitmapEncoder(),
                        ".TIFF" => new TiffBitmapEncoder(),
                        _ => new BmpBitmapEncoder()
                    };
                    try
                    {
                        encoder.Frames.Add(BitmapFrame.Create(bmpimg));
                        using (System.IO.FileStream fs = System.IO.File.OpenWrite(filename))
                        {
                            encoder.Save(fs);
                        }
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show($"{ex.GetType()}:{ex.Message}", "Save failed.", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"Empty image!", "Save failed.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
