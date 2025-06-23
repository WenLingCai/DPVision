﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using Microsoft.Win32;
using ImageView.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xaml;
using static System.Net.Mime.MediaTypeNames;
using DPVision.Model.ROI;
using ImageView.RoiImplementation;
using DPVision.Model;
using System.Xml.Linq;
namespace ImageView
{
    /// <summary>
    /// ROI显示主控类，负责协调画布和ROI的交互
    /// </summary>
    public class ImageView : Control, IImageDisplay
    {
        /// <summary>
        /// 当前ROI数量
        /// </summary>
        public int RoiCount => roiCanvas.Rois.Count;

        /// <summary>
        /// 当前显示的图像
        /// </summary>
        public BitmapImage CurrentImage { get; private set; } = null;

        /// <summary>
        /// 鼠标移动状态枚举
        /// </summary>
        private enum MouseMoveStatus
        {
            None,
            ImageMoving,
            RoiMoving,
            RoiOperateItemMoving,
        }
        private MouseMoveStatus mouseMoveStatus = MouseMoveStatus.None;
        private RoiCanvas roiCanvas;
        private TransformGroup canvas_transformGroup = new TransformGroup();
        private Point MouseDownPoint;
        private Point CanvasMouseMovePoint;
        /// <summary>
        /// 静态构造函数，设置默认样式键
        /// </summary>
        static ImageView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageView), new FrameworkPropertyMetadata(typeof(ImageView)));
        }

        /// <summary>
        /// 构造函数，初始化事件处理程序和右键菜单
        /// </summary>
        public ImageView()
        {
            Loaded += RoiDisplay_Loaded;
            MouseLeftButtonDown += RoiDisplay_MouseLeftButtonDown;
            MouseLeftButtonUp += RoiDisplay_MouseLeftButtonUp;
            MouseMove += RoiDisplay_MouseMove;
            MouseWheel += RoiDisplay_MouseWheel;
           
            Background = Brushes.Transparent;
        }

    
    
        /// <summary>
        /// 鼠标滚轮事件处理，用于图像缩放
        /// </summary>
        private void RoiDisplay_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.CurrentImage is null) return;
            var curPoint = e.GetPosition(this);
            ZoomObject(canvas_transformGroup, e.Delta, curPoint);
        }

        /// <summary>
        /// 缩放对象
        /// </summary>
        /// <param name="transformGroup">变换组</param>
        /// <param name="e_delta">滚轮增量</param>
        /// <param name="curPoint">当前鼠标位置</param>
        public void ZoomObject(TransformGroup transformGroup, int e_delta, Point curPoint)
        {
            var objectCurPoint = TranslatePoint(curPoint, roiCanvas);//转换成canvas控件的坐标
            var translate = transformGroup.Children[1] as TranslateTransform;
            var scale = transformGroup.Children[0] as ScaleTransform;
            double delta = e_delta > 0 ? scale.ScaleX * 0.1 : scale.ScaleX * -0.1;
            // 限制最大、最小缩放倍数
            if (scale.ScaleX + delta < 0.1 || scale.ScaleX + delta > 30) return;

            scale.ScaleX += delta;
            scale.ScaleY += delta;

            translate.X -= objectCurPoint.X * delta;
            translate.Y -= objectCurPoint.Y * delta;

            foreach (var roi in roiCanvas.Rois)
            {
                roi.Scale = scale.ScaleX;
            }
        }

        /// <summary>
        /// 鼠标移动事件处理
        /// </summary>
        private void RoiDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.CurrentImage is null) return;
            var currentPoint = e.GetPosition(this);
            var currentCanvasPoint = TranslatePoint(currentPoint, roiCanvas);
            //超出范围不可移动
            if (currentPoint.X > ActualWidth - 1 || currentPoint.Y > ActualHeight - 1 ||
                currentPoint.X < 0 || currentPoint.Y < 0)
            {
                mouseMoveStatus = MouseMoveStatus.None;
            }

            var seleceted_roi = roiCanvas.GetSelectedRoi();
            //非鼠标左键按下不可移动
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //掩膜绘制
                if(roiCanvas.IsDrawMask && mouseMoveStatus != MouseMoveStatus.None)
                {
                    if(roiCanvas.Mask.DrawingMode == DrawingMode.Draw)
                    {
                        roiCanvas.DrawMask(currentCanvasPoint);
                    }
                    else if(roiCanvas.Mask.DrawingMode == DrawingMode.Erase)
                    {
                        roiCanvas.EraseMask(currentCanvasPoint);
                    }
                    goto UpdatePoint;
                }

                double dx = currentCanvasPoint.X - CanvasMouseMovePoint.X;
                double dy = currentCanvasPoint.Y - CanvasMouseMovePoint.Y;

                switch (mouseMoveStatus)
                {
                    case MouseMoveStatus.ImageMoving:
                        var translate_canvas = canvas_transformGroup.Children[1] as TranslateTransform;
                        translate_canvas.X += currentPoint.X - MouseDownPoint.X;
                        translate_canvas.Y += currentPoint.Y - MouseDownPoint.Y;
                        MouseDownPoint = currentPoint;
                        break;
                    case MouseMoveStatus.RoiMoving:
                        seleceted_roi?.MoveOffset(dx, dy);
                        seleceted_roi?.Draw();
                        break;
                    case MouseMoveStatus.RoiOperateItemMoving:
                        seleceted_roi?.MoveOffset_OperateItem(currentCanvasPoint, dx, dy);
                        seleceted_roi?.Draw();
                        break;
                    case MouseMoveStatus.None:
                        break;
                }

                UpdatePoint:
                CanvasMouseMovePoint = currentCanvasPoint;
            }
            else
            {
                mouseMoveStatus = MouseMoveStatus.None;
                CanvasMouseMovePoint = currentCanvasPoint;
                //非移动状态，也可以有Roi上的鼠标样式改变
                VisualTreeHelper.HitTest(this, null, new HitTestResultCallback(MouseMoveHitTest), new PointHitTestParameters(currentPoint));
            }


            //显示当前鼠标位置的像素坐标,只有鼠标在图片上才显示       
            if (currentCanvasPoint.X >= 0 && currentCanvasPoint.X < roiCanvas.ImageWidth && currentCanvasPoint.Y >= 0 && currentCanvasPoint.Y < roiCanvas.ImageHeight)
            {
                var x = currentCanvasPoint.X;
                var y = currentCanvasPoint.Y;
                x = Math.Round(x, 3);
                y = Math.Round(y, 3);
                SetImagePixelPosText(x, y);
                //获取鼠标所在位置的图像的灰度值
                try
                {
                    var pixel = CurrentImage.GetPixelGray((int)x, (int)y);
                    SetImageGrayText(pixel[1], pixel[2], pixel[3]);
                }
                catch
                {

                }
            }
        }

        private void SetImagePixelPosText(double x, double y)
        {
            var txt = Template.FindName("TxtImagePixelPos", this) as TextBlock;
            txt.Text = $"[X,Y]({x},{y})";
        }
        private void SetImageGrayText(int r, int g, int b)
        {
            var txt = Template.FindName("TxtImageGray", this) as TextBlock;
            txt.Text = $"[RGB]({r},{g},{b})";
        }
        private void RoiDisplay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void RoiDisplay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pt = e.GetPosition(this);
            MouseDownPoint = pt;
            CanvasMouseMovePoint = TranslatePoint(pt, roiCanvas);
            //命中测试，点击到Roi时触发MouseDownHitTest事件
            VisualTreeHelper.HitTest(this, null, new HitTestResultCallback(MouseDownHitTest), new PointHitTestParameters(pt));
        }
        /// <summary>
        /// 鼠标按下时的命中测试
        /// </summary>
        /// <param name="result">命中测试结果</param>
        /// <returns>命中测试行为</returns>
        private HitTestResultBehavior MouseDownHitTest(HitTestResult result)
        {
            bool ImageCanMove = true;
            //roi移动流畅的关键：这个设定好移动状态后，MouseMove中仅通过左键是否按压判断来修改移动状态，而不是通过鼠标位置是否在Roi上判断
            if (result.VisualHit.GetType().IsSubclassOf(typeof(Roi)))
            {
                var roi = result.VisualHit as Roi;
                if (!roi.Interactive)
                {
                    ImageCanMove = true;
                }
                else
                {
                    roi.IsSelected = true;                   
                    var type = roi.HitPointTest(TranslatePoint(MouseDownPoint, roiCanvas));
                    if (type == RoiPart.OnOperateItem)
                    {
                        mouseMoveStatus = MouseMoveStatus.RoiOperateItemMoving;
                        var canvasMouseDownPoint = TranslatePoint(MouseDownPoint, roiCanvas);
                        roi.CatchSeletedOperateItem(canvasMouseDownPoint);
                    }
                    else
                    {
                        mouseMoveStatus = MouseMoveStatus.RoiMoving;                        
                    }
                    
                    //其他的roi取消选中
                    foreach (var r in roiCanvas.Rois)
                    {
                        if (r.ID == roi.ID) continue;
                        r.IsSelected = false;
                    }
                    ImageCanMove = false;
                }
            }

            if(ImageCanMove)
            {
                //roi取消选中
                foreach (var r in roiCanvas.Rois)
                {
                    r.IsSelected = false;
                }
                mouseMoveStatus = MouseMoveStatus.ImageMoving;
            }
            return HitTestResultBehavior.Stop;
        }

        /// <summary>
        /// 鼠标移动时的命中测试
        /// </summary>
        /// <param name="result">命中测试结果</param>
        /// <returns>命中测试行为</returns>
        private HitTestResultBehavior MouseMoveHitTest(HitTestResult result)
        {
            if (result.VisualHit.GetType().IsSubclassOf(typeof(Roi)))
            {
                var roi = result.VisualHit as Roi;
                var pointInRoiPart = roi.HitPointTest(CanvasMouseMovePoint);
                if (pointInRoiPart == RoiPart.OnOperateItem)
                {
                    Cursor = Cursors.Hand;
                }
                else
                {
                    Cursor = Cursors.SizeAll;
                }
            }
            else
            {
                Cursor = Cursors.Arrow;
            }
            return HitTestResultBehavior.Stop;
        }

        private void RoiDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            roiCanvas = Template.FindName("canvas", this) as RoiCanvas;

            canvas_transformGroup.Children.Add(new ScaleTransform());
            canvas_transformGroup.Children.Add(new TranslateTransform());
        }

        /// <summary>
        /// 添加ROI到画布
        /// </summary>
        /// <param name="roi">要添加的ROI对象</param>
        public void AddRoi(Roi roi)
        {
            if (CurrentImage is null) return;
            roiCanvas.AddRoi(roi);
        }

        /// <summary>
        /// 从画布移除ROI
        /// </summary>
        /// <param name="roi">要移除的ROI对象</param>
        public void RemoveRoi(Roi roi)
        {
            roiCanvas.RemoveRoi(roi);
        }

        /// <summary>
        /// 清除所有ROI
        /// </summary>
        public void ClearRois()
        {
            for(int i = RoiCount - 1; i>=0 ;i--)
            {
                var roi  = roiCanvas.Rois[i];
                RemoveRoi(roi);
            }
        }

        /// <summary>
        /// 通过索引获取ROI
        /// </summary>
        /// <param name="index">ROI索引</param>
        /// <returns>ROI对象</returns>
        public Roi GetRoi(int index)
        {
            return roiCanvas.Rois[index];
        }

        /// <summary>
        /// 通过ID获取ROI
        /// </summary>
        /// <param name="ID">ROI的唯一标识符</param>
        /// <returns>ROI对象</returns>
        public Roi GetRoi(string ID)
        {
            return roiCanvas.GetRoi(ID);
        }

        /// <summary>
        /// 获取当前选中的ROI
        /// </summary>
        /// <returns>选中的ROI对象，如果没有选中则返回null</returns>
        public Roi GetSelectedRoi()
        {
            return roiCanvas.GetSelectedRoi();
        }
        /// <summary>
        /// 显示图像
        /// </summary>
        /// <param name="image">要显示的图像</param>
        /// <param name="fitImage">是否自动调整图像大小以适应控件</param>
        public void DisplayImage(BitmapImage image,bool fitImage = false)
        {
            if (image is null) return;
            ClearAll();
            this.CurrentImage = image;
            var background = new ImageBrush(image)
            {
                Stretch = Stretch.Fill,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top
            };
            RenderOptions.SetBitmapScalingMode(background, BitmapScalingMode.NearestNeighbor);
            roiCanvas.Width = image.PixelWidth;
            roiCanvas.Height = image.PixelHeight;
            roiCanvas.Background = background;
            roiCanvas.ImageHeight = image.PixelHeight;
            roiCanvas.ImageWidth = image.PixelWidth;
            roiCanvas.RenderTransform = canvas_transformGroup;
            var txt = Template.FindName("TxtImageSize", this) as TextBlock;
            txt.Text = $"[W,H]({image.PixelWidth},{image.PixelHeight})";
            if (fitImage) FitImage();
        }

        /// <summary>
        /// 调整图像大小以适应控件尺寸
        /// </summary>
        public void FitImage()
        {
            if (CurrentImage is null) return;
            //根据宽高比设置Canvas的大小
            var wh = ActualWidth / ActualHeight;
            var image_wh = roiCanvas.ImageWidth / (double)roiCanvas.ImageHeight;
            double scale = 1;
            if (image_wh > wh)
            {
                scale = ActualWidth / roiCanvas.ImageWidth;
            }
            else
            {
                scale = ActualHeight / roiCanvas.ImageHeight;
            }
            var scaleTrans = canvas_transformGroup.Children[0] as ScaleTransform;
            var translate = canvas_transformGroup.Children[1] as TranslateTransform;

            scaleTrans.ScaleX = scale;
            scaleTrans.ScaleY = scale;
            translate.X = (ActualWidth - roiCanvas.ImageWidth * scale) / 2;
            translate.Y = (ActualHeight - roiCanvas.ImageHeight * scale) / 2;

            foreach (var roi in roiCanvas.Rois)
            {
                roi.Scale = scaleTrans.ScaleX;
            }
        }

        /// <summary>
        /// 清除所有内容(图像、ROI和遮罩)
        /// </summary>
        public void ClearAll()
        {
            //清除所有roi
            ClearRois();
            //清除图像
            roiCanvas.Background = Brushes.Transparent;
            CurrentImage = null;
            roiCanvas.ImageHeight = -1;
            roiCanvas.ImageWidth = -1;

            var txt_size = Template.FindName("TxtImageSize", this) as TextBlock;
            txt_size.Text = "[W,H](--,--)";
            var txt_pixel = Template.FindName("TxtImagePixelPos", this) as TextBlock;
            txt_pixel.Text = "[X,Y](--,--)";
            var txt_gray = Template.FindName("TxtImageGray", this) as TextBlock;
            txt_gray.Text = "[RGB](---,---,---)";
        }

        /// <summary>
        /// 保存原始图像
        /// </summary>
        /// <param name="path">保存路径</param>
        public void SaveSourceImage(string path)
        {
            if (CurrentImage is null) return;
            CurrentImage.Save(path, System.IO.Path.GetExtension(path));
        }

        /// <summary>
        /// 保存当前选中ROI的截图
        /// </summary>
        /// <param name="filePath">保存路径</param>
        /// <param name="quality">JPEG质量(1-100)</param>
        public void SaveCropImage(string filePath, int quality = 70)
        {
            if (CurrentImage is null) return;

            // 创建渲染位图（带 DPI 设置）
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)roiCanvas.ActualWidth,
                (int)roiCanvas.ActualHeight,
                96d,  // DPI X
                96d,  // DPI Y
                PixelFormats.Pbgra32);
            //设置Canvas的变换为初始状态
            var  transform = new TransformGroup();
            transform.Children.Add(new ScaleTransform());
            transform.Children.Add(new TranslateTransform());
            roiCanvas.RenderTransform = transform;
            // 确保Canvas完成布局更新
            roiCanvas.Measure(new Size(roiCanvas.ActualWidth, roiCanvas.ActualHeight));
            roiCanvas.Arrange(new Rect(new Size(roiCanvas.ActualWidth, roiCanvas.ActualHeight)));

            // 绘制到位图
            rtb.Render(roiCanvas);
            BitmapEncoder encoder = new JpegBitmapEncoder() {QualityLevel = quality};
            // 添加帧并保存
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fs);
            }
            // 还原Canvas的变换
            roiCanvas.RenderTransform = canvas_transformGroup;
        }
        /// <summary>
        /// 隐藏所有ROI
        /// </summary>
        public void HideRois()
        {
            roiCanvas.Rois.ForEach(r => r.Visible = false);
        }

        /// <summary>
        /// 显示所有ROI
        /// </summary>
        public void ShowRois()
        {
            roiCanvas.Rois.ForEach(r => r.Visible = true);
        }
        /// <summary>
        /// 设置遮罩画笔大小
        /// </summary>
        /// <param name="size">画笔大小(像素)</param>
        public void SetMaskSize(int size)
        {
            if (size <= 0) return;
            roiCanvas.Mask.Size = size;
        }

        /// <summary>
        /// 开始绘制遮罩
        /// </summary>
        public void StartDrawMask()
        {
            if (CurrentImage is null) return;
            roiCanvas.IsDrawMask = true;
            roiCanvas.Mask.DrawingMode = DrawingMode.Draw;
        }

        /// <summary>
        /// 设置遮罩绘制模式
        /// </summary>
        /// <param name="isEraser">true表示擦除模式，false表示绘制模式</param>
        public void SetMaskDrawingMode(bool isEraser)
        {
            if (!roiCanvas.IsDrawMask) return;
            roiCanvas.Mask.DrawingMode = isEraser ? DrawingMode.Erase : DrawingMode.Draw;
        }

        /// <summary>
        /// 停止绘制遮罩
        /// </summary>
        public void StopDrawMask()
        {
            if (CurrentImage is null) return;
            roiCanvas.IsDrawMask = false;
            roiCanvas.Mask.DrawingMode = DrawingMode.Draw;
        }

        /// <summary>
        /// 获取当前遮罩绘制模式
        /// </summary>
        /// <returns>"Draw"表示绘制模式，"Erase"表示擦除模式</returns>
        public string GetMaskDrawingMode()
        {
            return roiCanvas.Mask.DrawingMode == DrawingMode.Draw? "Draw" : "Erase";
        }

        /// <summary>
        /// 获取遮罩几何数据
        /// </summary>
        /// <returns>包含绘制矩形和擦除矩形的字典</returns>
        public Dictionary<string,List<Rect>> GetMaskGeometry()
        {
            Dictionary<string, List<Rect>> result = new Dictionary<string, List<Rect>>
            {
                { "DrawRects", roiCanvas.Mask.DrawRects },
                { "EraseRects", roiCanvas.Mask.EraseRects }
            };
            return result;
        }
        internal void AddVisual(Visual visual)
        {
            base.AddVisualChild(visual);
            base.AddLogicalChild(visual);
        }

        #region 封装roi方法
        public BitmapImage ByteArrayToBitmapImage(byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // 重要，避免流关闭后失效
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze(); // 线程安全
                return bitmap;
            }
        }

        public byte[] BitmapImageToByteArray(BitmapImage bitmapImage)
        {
            byte[] data;
            var encoder = new PngBitmapEncoder(); // 可根据需要改为JpegBitmapEncoder等
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }
            return data;
        }
        /// <summary>
        /// 绘制直线
        /// </summary>
        public void DrawLine(System.Drawing.PointF pointStar, System.Drawing.PointF pointEnd, bool bInteractive = false)
        {
            this.AddRoi(new RoiSegment(pointStar.X, pointStar.Y, pointEnd.X, pointEnd.Y) { Interactive = bInteractive });
        }

        /// <summary>
        /// 绘制矩形
        /// </summary>
        public void DrawRectangle(System.Drawing.PointF point, float width, float height, bool bInteractive = false)
        {
            this.AddRoi(new RoiRectangle(point.X, point.Y, width, height) { Interactive = bInteractive });
        }

        /// <summary>
        /// 绘制旋转矩形
        /// </summary>
        public void DrawRectangleAffine(System.Drawing.PointF point, float width, float height,float angle, bool bInteractive = false)
        {
            this.AddRoi(new RoiRectangleAffine(point.X, point.Y, width, height, angle) { Interactive = bInteractive });
        }

        /// <summary>
        /// 绘制圆
        /// </summary>
        public void DrawCircle(System.Drawing.PointF point, float r, bool bInteractive = false)
        {
            this.AddRoi(new RoiCircle(point.X, point.Y, r) { Interactive = bInteractive });
        }

        /// <summary>
        /// 绘制椭圆
        /// </summary>
        public void DrawEllipse(System.Drawing.PointF point,float r1,float r2,float angle, bool bInteractive = false)
        {
            this.AddRoi(new RoiEllipse(point.X, point.Y, r1, r2, angle) { Interactive = bInteractive });
        }

        /// <summary>
        /// 绘制点
        /// </summary>
        public void DrawPoint(System.Drawing.PointF pt, bool bInteractive = false)
        {
            this.AddRoi(new RoiPoint(pt.X, pt.Y) { Interactive = bInteractive });
        }


        /// <summary>
        /// 显示图片
        /// </summary>
        public void DispalyImage(byte[] imageData)
        {
            if (imageData.Length<=0) return;
            ClearAll();
            this.CurrentImage = ByteArrayToBitmapImage(imageData);
            var background = new ImageBrush(this.CurrentImage)
            {
                Stretch = Stretch.Fill,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top
            };
            RenderOptions.SetBitmapScalingMode(background, BitmapScalingMode.NearestNeighbor);
            roiCanvas.Width = CurrentImage.Width;
            roiCanvas.Height = CurrentImage.Height;
            roiCanvas.Background = background;
            roiCanvas.ImageHeight = (int)CurrentImage.Height;
            roiCanvas.ImageWidth = (int)CurrentImage.Width;
            roiCanvas.RenderTransform = canvas_transformGroup;
            var txt = Template.FindName("TxtImageSize", this) as TextBlock;
            txt.Text = $"[W,H]({CurrentImage.Width},{CurrentImage.Height})";
           
        }

        /// <summary>
        /// 自适应显示图片
        /// </summary>
        public void AutoFitImage(byte[] image)
        {
            DispalyImage(image);
            FitImage();
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        public void SaveImage(string path)
        {
            SaveSourceImage(path);
        }

        /// <summary>
        /// 保存截图
        /// </summary>
        public void SaveScreenImage(string path)
        {
            SaveCropImage(path);
        }

        /// <summary>
        /// 显示辅助线
        /// </summary>
        public void DispalyCrossLine(System.Drawing.PointF point,float fontSize)
        {
          
        }

        /// <summary>
        /// 绘制掩膜
        /// </summary>
        public void DrawMask(List<System.Drawing.RectangleF> rect, List<System.Drawing.RectangleF> EraseRects, int pointSize)
        {
           
        }

        /// <summary>
        /// 绘制文字
        /// </summary>
        public void DrawText(string text, float fontSize,System.Drawing.Color color)
        {
            TextBlock textblock = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(DrawingHelper.DrawingColorToMediaColor(color)),
                FontSize = fontSize,
                FontFamily = new FontFamily("Arial")
            };
            Canvas.SetLeft(textblock, 100);
            Canvas.SetTop(textblock, 150);
            roiCanvas.Children.Add(textblock);
           
        }


        /// <summary>
        /// 获取roi信息
        /// </summary>
        public Dictionary<string, double> GetRoiInfo()
        {
            return new Dictionary<string, double>();
        }

        /// <summary>
        /// 获取mask信息
        /// </summary>
        public Dictionary<string, List<System.Drawing.RectangleF>> GetMaskInfo()
        {
            List<System.Drawing.RectangleF> roi= new List<System.Drawing.RectangleF>();
            
            for(int i = 0; i < roiCanvas.Mask.DrawRects.Count;i++)
            {
                System.Drawing.RectangleF rect =new System.Drawing.RectangleF((float)roiCanvas.Mask.DrawRects[i].X, (float)roiCanvas.Mask.DrawRects[i].Y, (float)roiCanvas.Mask.DrawRects[i].Width, (float)roiCanvas.Mask.DrawRects[i].Height);
                roi.Add(rect);
            }
            List<System.Drawing.RectangleF> EraseRects = new List<System.Drawing.RectangleF>();
            for (int i = 0; i < roiCanvas.Mask.DrawRects.Count; i++)
            {
                System.Drawing.RectangleF rect = new System.Drawing.RectangleF((float)roiCanvas.Mask.EraseRects[i].X, (float)roiCanvas.Mask.EraseRects[i].Y, (float)roiCanvas.Mask.EraseRects[i].Width, (float)roiCanvas.Mask.EraseRects[i].Height);
                EraseRects.Add(rect);
            }
            Dictionary<string, List<System.Drawing.RectangleF>> result = new Dictionary<string, List<System.Drawing.RectangleF>>
            {
                { "DrawRects", roi},
                { "EraseRects", EraseRects}
            };
            return result;
        }
        #endregion
    }
}