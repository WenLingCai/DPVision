﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ImageView.RoiImplementation
{
    /// <summary>
    /// 矩形ROI，表示图像上的一个矩形区域
    /// </summary>
    public class RoiRectangle : Roi
    {
        /// <summary>
        /// 矩形左上角X坐标
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// 矩形左上角Y坐标
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// 矩形宽度
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 矩形高度
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// ROI类型标识
        /// </summary>
        public override string Type { get; protected set; } = "Rectangle";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="x">矩形左上角X坐标</param>
        /// <param name="y">矩形左上角Y坐标</param>
        /// <param name="width">矩形宽度</param>
        /// <param name="height">矩形高度</param>
        public RoiRectangle(double x, double y, double width, double height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;

            // 添加四个操作点，分别位于矩形的四个角
            for (int i = 0; i < 4; i++)
            {
                OperateItems.Add(new OperateItem(new Point()));
            }
            UpdataOperateItems();
            HasCreated = true;
        }
        /// <summary>
        /// 更新操作点位置
        /// </summary>
        protected override void UpdataOperateItems()
        {
            // 更新四个角的操作点位置
            OperateItems[0].Center = new Point(X, Y);                     // 左上角
            OperateItems[1].Center = new Point(X, Y + Height);           // 左下角
            OperateItems[2].Center = new Point(X + Width, Y);           // 右上角
            OperateItems[3].Center = new Point(X + Width, Y + Height); // 右下角
        }

        /// <summary>
        /// 绘制矩形ROI
        /// </summary>
        public override void Draw()
        {
            if (!HasCreated) return;
            Pen pen = new Pen(this.Brush, Thickness / Scale);
            pen.Freeze();
            using (var dc = this.RenderOpen())
            {
                if (!Visible) return;//不显示,必须在using内部使用才会不显示
                // 绘制矩形主体
                dc.DrawRectangle(Brushes.Transparent, pen, new Rect(X, Y, Width, Height));
                if (!Interactive) return;
                if (IsSelected)
                {
                    UpdataOperateItems();
                    //绘制操作点
                    foreach (var item in OperateItems)
                    {
                        dc.DrawEllipse(Brushes.Blue, null, item.Center, item.Radius / Scale, item.Radius / Scale);
                    }
                }
            }
        }

        /// <summary>
        /// 矩形ROI的命中测试
        /// </summary>
        /// <param name="point">测试点</param>
        /// <returns>点所在的ROI部位</returns>
        public override RoiPart HitPointTest(Point point)
        {
            // 首先检查是否点击在操作点上
            foreach(var item in OperateItems)
            {
                if(OperateItem.IsInItem(point, item))
                {
                    return RoiPart.OnOperateItem;
                }
            }
            // 检查点是否在矩形内
            if(point.X>=X && point.X<=X+Width && point.Y>=Y && point.Y <= Y + Height)
            {
                return RoiPart.OnRoi;
            }
            else
            {
                return RoiPart.None;
            }
        }

        /// <summary>
        /// 移动矩形ROI
        /// </summary>
        /// <param name="dx">X方向偏移量</param>
        /// <param name="dy">Y方向偏移量</param>
        public override void MoveOffset(double dx, double dy)
        {
            X += dx;
            Y += dy;
        }

        /// <summary>
        /// 矩形ROI的操作点移动，用于调整矩形大小
        /// </summary>
        /// <param name="mousePoint">鼠标位置</param>
        /// <param name="dx">X方向偏移量</param>
        /// <param name="dy">Y方向偏移量</param>
        public override void MoveOffset_OperateItem(Point mousePoint,double dx, double dy)
        {
            switch (SelectedOperateItemIndex)
            {
                case 0://左上角
                    if (Width - dx > 0) Width -= dx;
                    if (Height - dy > 0) Height -= dy;
                    X += dx;
                    Y += dy;
                    break;
                case 1://左下角
                    if (Width - dx > 0) Width -= dx;
                    X += dx;
                    if (Height + dy > 0) Height += dy;
                    break;
                case 2://右上角
                    if (Height - dy > 0) Height -= dy;
                    Y += dy;
                    if(Width + dx > 0) Width += dx;
                    break;
                case 3://右下角
                    if (Width + dx > 0)  Width += dx;
                    if (Height + dy > 0) Height += dy;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 获取矩形ROI的像素信息
        /// </summary>
        /// <returns>包含位置和尺寸信息的字典</returns>
        public override Dictionary<string, double> GetRoiPixelInfo()
        {
            Dictionary<string, double> info = new Dictionary<string, double>
            {
                { "X", X },
                { "Y", Y },
                { "Width", Width },
                { "Height", Height }
            };
            return info;
        }
    }
}