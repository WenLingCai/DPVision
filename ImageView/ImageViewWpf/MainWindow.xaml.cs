using ImageView;
using ImageView.RoiImplementation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace ImageViewWpf
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void AddRoiBtn_Click(object sender, RoutedEventArgs e)
        {
            roiDisplay.AddRoi(new RoiRectangle(10, 10, 50, 50) { Interactive = true });
            roiDisplay.AddRoi(new RoiRectangleAffine(70, 70, 100, 100, 30) { Interactive = true });
            roiDisplay.AddRoi(new RoiCircle(100, 100, 50) { Interactive = true });
            roiDisplay.AddRoi(new RoiPoint(100, 100) { Interactive = true });
            roiDisplay.AddRoi(new RoiSegment(100, 100, 200, 200) { Interactive = true });
            roiDisplay.AddRoi(new RoiEllipse(100, 100, 50, 30,30) { Interactive = true });
        }

        private void DispyImageButton_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {   
                string path = "";
                path = @"C:\Users\Lenovo\Desktop\新建文件夹\1.png";   
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    roiDisplay.DisplayImage(new BitmapImage(new Uri(path)));
                }));
                
            });
        }

        private void ShowInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            var roi = roiDisplay.GetSelectedRoi();
            
            if(roi!= null)
            {
                string info = "";
                foreach(var i in roi.GetRoiPixelInfo())
                {
                    info += $"{i.Key}:{i.Value}\n";
                }
                MessageBox.Show(info);
            }
        }

        private void DeleteSelectedRoiBtn_Click(object sender, RoutedEventArgs e)
        {
            var roi = roiDisplay.GetSelectedRoi();
            if(roi!= null)
            {
                roiDisplay.RemoveRoi(roi);
            }
        }
        private void FitImage_Click(object sender, RoutedEventArgs e)
        {
            roiDisplay.FitImage();
        }
       
        private void ShowRoi_Click(object sender, RoutedEventArgs e)
        {
            roiDisplay.ShowRois();
        }
        private void HideRoi_Click(object sender, RoutedEventArgs e)
        {
            roiDisplay.HideRois();
        }
        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
           // roiDisplay.SaveSourceImage();
        }
        private void SaveScreenImage_Click(object sender, RoutedEventArgs e)
        {
           // roiDisplay.SaveCropImage();
        }
       
        private void SmearBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Content.ToString() == "开始涂抹")
            {
                roiDisplay.StartDrawMask();
                btn.Content = "结束涂抹";
            }
            else
            {
                roiDisplay.StopDrawMask();
                btn.Content = "开始涂抹";
            }
        }

        private void MaskBtn_Click(object sender, RoutedEventArgs e)
        {
            roiDisplay.SetMaskDrawingMode(false);
        }
        private void EraseBtn_Click(object sender, RoutedEventArgs e)
        {
            roiDisplay.SetMaskSize(20);
            roiDisplay.SetMaskDrawingMode(true);
        }
    }
}
