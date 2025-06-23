﻿﻿﻿using ImageView.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ImageView.RoiImplementation
{
    /// <summary>
    /// 可旋转的矩形ROI实现类
    /// </summary>
    /// <summary>
    /// 可旋转的矩形ROI实现类
    /// </summary>
    public class RoiRectangleAffine : Roi
    {
        /// <summary>
        /// 矩形中心点X坐标
        /// </summary>
        public double CenterX { get; set; }

        /// <summary>
        /// 矩形中心点Y坐标
        /// </summary>
        public double CenterY { get; set; }

        /// <summary>
        /// 矩形宽度
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 矩形高度
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 旋转角度(度)，顺时针为正方向
        /// </summary>
        public double Angle { get; set; }

        /// <summary>
        /// ROI类型标识
        /// </summary>
        public override string Type { get ; protected set ; } = "RectangleAffine";

        public RoiRectangleAffine(double centerX, double centerY, double width, double height, double angle)
        {
            CenterX = centerX;
            CenterY = centerY;
            Width = width;
            Height = height;
            Angle = angle;

            for (int i = 0; i < 5; i++)
            {
                OperateItems.Add(new OperateItem(new Point()));
            }
            UpdataOperateItems();

            HasCreated = true;
        }
        public override void Draw()
        {
            if (!HasCreated) return;
            Pen pen = new Pen(this.Brush, Thickness / Scale);
            pen.Freeze();
            using (var dc = this.RenderOpen())
            {
                if (!Visible) return;//不显示,必须在using内部使用才会不显示
                var X = CenterX - Width / 2;
                var Y = CenterY - Height / 2;
                UpdataOperateItems();
                DrawRectangle(dc, pen);
                if (!Interactive) return;
                if (IsSelected)
                {
                    //绘制操作点
                    foreach (var item in OperateItems)
                    {
                        dc.DrawEllipse(Brushes.Blue, null, item.Center, item.Radius / Scale, item.Radius / Scale);
                    }
                    //绘制箭头
                    Point center = new Point(CenterX, CenterY);
                    var end = PointHelper.RotateFrom(new Point(X + Width + 20, Y + Height / 2), center, Angle);
                    DrawingHelper.DrawArraw(dc, pen, new Point(CenterX, CenterY), end);
                }
            }
        }
        private void DrawRectangle(DrawingContext dc, Pen pen)
        {
            var geometry = new StreamGeometry() { FillRule = FillRule.EvenOdd };
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(OperateItems[0].Center, true, true);
                ctx.LineTo(OperateItems[1].Center,true,true);
                ctx.LineTo(OperateItems[3].Center,true,true);
                ctx.LineTo(OperateItems[2].Center, true, true);
            }
            geometry.Freeze();
            dc.DrawGeometry(Brushes.Transparent, pen, geometry);
        }
        protected override void UpdataOperateItems()
        {
            var X = CenterX - Width / 2;
            var Y = CenterY - Height / 2;
            Point center = new Point(CenterX, CenterY);
            OperateItems[0].Center = PointHelper.RotateFrom(new Point(X, Y), center, Angle);
            OperateItems[1].Center = PointHelper.RotateFrom(new Point(X, Y + Height), center, Angle);
            OperateItems[2].Center = PointHelper.RotateFrom(new Point(X + Width, Y), center, Angle);
            OperateItems[3].Center = PointHelper.RotateFrom(new Point(X + Width, Y + Height), center, Angle);
            OperateItems[4].Center = PointHelper.RotateFrom(new Point(X + Width, Y + Height / 2), center, Angle);
        }
        private bool PointIsInRoi(Point point)
        {
            // 将点变换到矩形未旋转的状态
            Point transformedPoint = PointHelper.RotateFrom(point,new Point(CenterX, CenterY), -Angle);

            // 计算未旋转矩形的边界
            double left = CenterX - Width / 2;
            double right = CenterX + Width / 2;
            double top = CenterY - Height / 2;
            double bottom = CenterY + Height / 2;

            // 判断变换后的点是否在矩形内
            return transformedPoint.X >= left && transformedPoint.X <= right &&
                   transformedPoint.Y >= top && transformedPoint.Y <= bottom;
        }
        public override RoiPart HitPointTest(Point point)
        {
            foreach (var item in OperateItems)
            {
                if (OperateItem.IsInItem(point, item))
                {
                    return RoiPart.OnOperateItem;
                }
            }
            
            if (PointIsInRoi(point))
            {
                return RoiPart.OnRoi;
            }
            else
            {
                return RoiPart.None;
            }
        }

        public override void MoveOffset(double dx, double dy)
        {
            CenterX += dx;
            CenterY += dy;
        }
        private void GetNewCenterAndOffset(Point mousePoint, double dx, double dy,out Point newCenter,out double dx_new,out double dy_new)
        {
            var prePoint = new Point(mousePoint.X - dx, mousePoint.Y - dy);
            var noRotatePrePoint = PointHelper.RotateFrom(prePoint, new Point(CenterX, CenterY), -Angle);
            var noRotateMousePoint = PointHelper.RotateFrom(mousePoint, new Point(CenterX, CenterY), -Angle);
            dx_new = noRotateMousePoint.X - noRotatePrePoint.X;
            dy_new = noRotateMousePoint.Y - noRotatePrePoint.Y;
            var noRotateCenter = new Point(CenterX + dx_new / 2, CenterY + dy_new / 2);//中心变化
            newCenter = PointHelper.RotateFrom(noRotateCenter, new Point(CenterX, CenterY), Angle);
        }
        public override void MoveOffset_OperateItem(Point mousePoint, double dx, double dy)
        {
            switch (SelectedOperateItemIndex)
            {
                case 0:
                    GetNewCenterAndOffset(mousePoint, dx, dy, out Point newCenter, out double dx_new, out double dy_new);
                    CenterX = newCenter.X;
                    CenterY = newCenter.Y;
                    //宽高变化
                    if (Width - dx_new > 0) Width -= dx_new;
                    if (Height - dy_new > 0) Height -= dy_new;
                    break;
                case 1:
                    GetNewCenterAndOffset(mousePoint, dx, dy, out newCenter, out  dx_new, out dy_new);
                    CenterX = newCenter.X;
                    CenterY = newCenter.Y;
                    //宽高变化
                    if (Width - dx_new > 0) Width -= dx_new;
                    if (Height + dy_new > 0) Height += dy_new;
                    break;
                case 2:
                    GetNewCenterAndOffset(mousePoint, dx, dy, out newCenter, out  dx_new, out dy_new);
                    CenterX = newCenter.X;
                    CenterY = newCenter.Y;
                    //宽高变化
                    if (Width + dx_new > 0) Width += dx_new;
                    if (Height - dy_new > 0) Height -= dy_new;
                    break;
                case 3:
                    GetNewCenterAndOffset(mousePoint, dx, dy, out newCenter, out dx_new, out dy_new);
                    CenterX = newCenter.X;
                    CenterY = newCenter.Y;
                    //宽高变化
                    if (Width + dx_new > 0) Width += dx_new;
                    if (Height + dy_new > 0) Height += dy_new;
                    break;
                case 4:
                    Angle = PointHelper.GetAngle(new Point(CenterX, CenterY), mousePoint);
                    break;
                default:
                    break;
            }
        }

        public override Dictionary<string, double> GetRoiPixelInfo()
        {
            Dictionary<string, double> info = new Dictionary<string, double>
            {
                { "CenterX", CenterX },
                { "CenterY", CenterY },
                { "Height", Height },
                { "Width", Width },
                { "Angle", Angle }

            };
            return info;
        }
    }
}