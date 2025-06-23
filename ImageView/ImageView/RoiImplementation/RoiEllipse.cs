﻿﻿﻿﻿using ImageView.Helper;
﻿using System;
﻿using System.Collections.Generic;
﻿using System.Linq;
﻿using System.Reflection.Emit;
﻿using System.Text;
﻿using System.Threading.Tasks;
﻿using System.Windows;
﻿using System.Windows.Media;
﻿using System.Windows.Media.Media3D;

﻿namespace ImageView.RoiImplementation
﻿{
﻿    /// <summary>
﻿    /// 椭圆ROI，表示图像上的一个可旋转的椭圆区域
﻿    /// </summary>
﻿    public class RoiEllipse : Roi
﻿    {
﻿        /// <summary>
﻿        /// 椭圆中心X坐标（未旋转状态）
﻿        /// </summary>
﻿        public double CenterX { get; set; }

﻿        /// <summary>
﻿        /// 椭圆中心Y坐标（未旋转状态）
﻿        /// </summary>
﻿        public double CenterY { get; set; }

﻿        /// <summary>
﻿        /// 椭圆X方向半径
﻿        /// </summary>
﻿        public double Radius1 { get; set; }

﻿        /// <summary>
﻿        /// 椭圆Y方向半径
﻿        /// </summary>
﻿        public double Radius2 { get; set; }

﻿        /// <summary>
﻿        /// 椭圆旋转角度（单位：度）
﻿        /// </summary>
﻿        public double Angle { get; set; }

﻿        /// <summary>
﻿        /// ROI类型标识
﻿        /// </summary>
﻿        public override string Type { get; protected set; } = "Ellipse";

﻿        /// <summary>
﻿        /// 获取旋转后的椭圆中心X坐标（像素坐标）
﻿        /// </summary>
﻿        public double PixelCenterX 
﻿        {
﻿            get
﻿            {
﻿                var newCenter = transform.Transform(new Point(CenterX, CenterY));
﻿                return newCenter.X;
﻿            }
﻿        }

﻿        /// <summary>
﻿        /// 获取旋转后的椭圆中心Y坐标（像素坐标）
﻿        /// </summary>
﻿        public double PixelCenterY
﻿        {
﻿            get
﻿            {
﻿                var newCenter = transform.Transform(new Point(CenterX, CenterY));
﻿                return newCenter.Y;
﻿            }
﻿        }

﻿        /// <summary>
﻿        /// 变换组，包含旋转和平移变换
﻿        /// </summary>
﻿        private TransformGroup transform = new TransformGroup();

﻿        /// <summary>
﻿        /// 构造函数
﻿        /// </summary>
﻿        /// <param name="centerX">椭圆中心X坐标</param>
﻿        /// <param name="centerY">椭圆中心Y坐标</param>
﻿        /// <param name="radiusX">椭圆X方向半径</param>
﻿        /// <param name="radiusY">椭圆Y方向半径</param>
﻿        /// <param name="angle">椭圆旋转角度（单位：度）</param>
﻿        public RoiEllipse(double centerX,double centerY,double radiusX,double radiusY,double angle)
﻿        {
﻿            CenterX = centerX;
﻿            CenterY = centerY;
﻿            Radius1 = radiusX;
﻿            Radius2 = radiusY;
﻿            Angle = angle;

﻿            // 创建旋转变换
﻿            var rotateTransform = new RotateTransform
﻿            {
﻿                Angle= Angle,
﻿                CenterX = CenterX,
﻿                CenterY = CenterY
﻿            };
﻿            // 创建平移变换
﻿            var translateTransform = new TranslateTransform();
﻿            // 将变换添加到变换组
﻿            transform.Children.Add(rotateTransform);
﻿            transform.Children.Add(translateTransform);
            
﻿            // 添加三个操作点，分别用于调整X半径、Y半径和旋转角度
﻿            for (int i = 0; i < 3; i++)
﻿            {
﻿                OperateItems.Add(new OperateItem(new Point()));
﻿            }
﻿            UpdataOperateItems();
﻿            HasCreated = true;
﻿        }

﻿        /// <summary>
﻿        /// 绘制椭圆ROI
﻿        /// </summary>
﻿        public override void Draw()
﻿        {
﻿            if (!HasCreated) return;
﻿            Pen pen = new Pen(this.Brush, Thickness / Scale);
﻿            pen.Freeze();
﻿            using (var dc = this.RenderOpen())
﻿            {
﻿                // 应用变换（旋转和平移）
﻿                dc.PushTransform(transform);
﻿                if (!Visible) return;//不显示,必须在using内部使用才会不显示
﻿                // 绘制椭圆主体
﻿                dc.DrawEllipse(Brushes.Transparent, pen, new Point(CenterX, CenterY), Radius1, Radius2);
﻿                if (!Interactive) return;
﻿                if (IsSelected)
﻿                {
﻿                    UpdataOperateItems();
﻿                    //绘制操作点
﻿                    foreach (var item in OperateItems)
﻿                    {
﻿                        dc.DrawEllipse(Brushes.Blue, null, item.Center, item.Radius / Scale, item.Radius / Scale);
﻿                    }
﻿                    // 绘制指示椭圆方向的箭头
﻿                    var end  = new Point(CenterX + Radius1 + 20, CenterY);
﻿                    DrawingHelper.DrawArraw(dc, pen, new Point(CenterX, CenterY), end);
﻿                }
﻿            }
﻿        }

        /// <summary>
        /// 获取椭圆ROI的像素信息
        /// </summary>
        /// <returns>包含中心点坐标、半径和角度的字典</returns>
        public override Dictionary<string, double> GetRoiPixelInfo()
        {
            Dictionary<string, double> pixelInfo = new Dictionary<string, double>
            {
                { "CenterX", PixelCenterX },
                { "CenterY", PixelCenterY },
                { "Radius1", Radius1 },
                { "Radius2", Radius2 },
                { "Angle", Angle }
            };
            return pixelInfo;
        }

        /// <summary>
        /// 椭圆ROI的命中测试
        /// </summary>
        /// <param name="point">测试点</param>
        /// <returns>点所在的ROI部位</returns>
        public override RoiPart HitPointTest(Point point)
        {
            // 检查是否点击在操作点上
            foreach (var item in OperateItems)
            {
                var itemCenter = transform.Transform(item.Center);
                var distance = Math.Sqrt(Math.Pow(point.X - itemCenter.X, 2) + Math.Pow(point.Y - itemCenter.Y, 2));
                if (distance <= item.Radius)
                {
                    return RoiPart.OnOperateItem;
                }
            }
            return RoiPart.OnRoi;
        }

        /// <summary>
        /// 移动椭圆ROI
        /// </summary>
        /// <param name="dx">X方向偏移量</param>
        /// <param name="dy">Y方向偏移量</param>
        public override void MoveOffset(double dx, double dy)
        {
            // 通过平移变换移动椭圆
            var translateTransform = transform.Children[1] as TranslateTransform;
            translateTransform.X += dx;
            translateTransform.Y += dy;
        }

        /// <summary>
        /// 计算旋转坐标系中的新偏移量
        /// </summary>
        /// <param name="mousePoint">当前鼠标位置</param>
        /// <param name="dx">原始X方向偏移量</param>
        /// <param name="dy">原始Y方向偏移量</param>
        /// <param name="dx_new">旋转坐标系中的X方向偏移量</param>
        /// <param name="dy_new">旋转坐标系中的Y方向偏移量</param>
        private void GetNewOffset(Point mousePoint, double dx, double dy,out double dx_new, out double dy_new)
        {
            // 计算前一个点的位置
            var prePoint = new Point(mousePoint.X - dx, mousePoint.Y - dy);
            // 将前一个点旋转到椭圆的局部坐标系
            var noRotatePrePoint = PointHelper.RotateFrom(prePoint, new Point(CenterX, CenterY), -Angle);
            // 将当前鼠标点旋转到椭圆的局部坐标系
            var noRotateMousePoint = PointHelper.RotateFrom(mousePoint, new Point(CenterX, CenterY), -Angle);
            // 计算局部坐标系中的偏移量
            dx_new = noRotateMousePoint.X - noRotatePrePoint.X;
            dy_new = noRotateMousePoint.Y - noRotatePrePoint.Y;
        }

        /// <summary>
        /// 椭圆ROI的操作点移动，用于调整椭圆大小和旋转角度
        /// </summary>
        /// <param name="mousePoint">鼠标位置</param>
        /// <param name="dx">X方向偏移量</param>
        /// <param name="dy">Y方向偏移量</param>
        public override void MoveOffset_OperateItem(Point mousePoint, double dx, double dy)
        {
            switch (SelectedOperateItemIndex)
            {
                case 0: // X半径操作点
                    GetNewOffset(mousePoint, dx, dy, out double dx_new, out double dy_new);
                    if(Radius1 + dx_new > 0) Radius1 += dx_new;
                    break;
                case 1: // Y半径操作点
                    GetNewOffset(mousePoint, dx, dy, out dx_new, out dy_new);
                    if (Radius2 + dy_new > 0) Radius2 += dy_new;
                    break;
                case 2: // 旋转角度操作点
                    // 获取变换后的中心点
                    var newCenter = transform.Transform(new Point(CenterX, CenterY));
                    // 计算鼠标点与中心点的角度
                    Angle = PointHelper.GetAngle(newCenter, mousePoint);                  
                    // 更新旋转变换的角度
                    var rotate = transform.Children[0] as RotateTransform;
                    rotate.Angle = Angle;                  
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 捕获被选中的操作点
        /// </summary>
        /// <param name="mousePoint">鼠标位置</param>
        public override void CatchSeletedOperateItem(Point mousePoint)
        {
            // 遍历所有操作点，找到距离鼠标最近的一个
            for(int i = 0; i < OperateItems.Count; i++)
            {
                var item = OperateItems[i];
                var itemCenter = transform.Transform(item.Center);
                var distance = Math.Sqrt(Math.Pow(mousePoint.X - itemCenter.X, 2) + Math.Pow(mousePoint.Y - itemCenter.Y, 2));
                if (distance <= item.Radius)
                {
                    SelectedOperateItemIndex = i;
                    break;
                }
            }
        }

        /// <summary>
        /// 更新操作点位置
        /// </summary>
        protected override void UpdataOperateItems()
        {
            OperateItems[0].Center = new Point(CenterX + Radius1, CenterY);                                      // 右侧X半径操作点
            OperateItems[1].Center = new Point(CenterX, CenterY + Radius2);                                      // 下侧Y半径操作点
            OperateItems[2].Center = new Point((CenterX + OperateItems[0].Center.X)/2, (CenterY + OperateItems[0].Center.Y)/2); // 旋转操作点
        }
    }
}