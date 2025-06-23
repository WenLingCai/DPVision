﻿﻿﻿using ImageView.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ImageView.RoiImplementation
{
    /// <summary>
    /// 点型ROI，表示图像上的一个点
    /// </summary>
    public class RoiPoint : Roi
    {
        /// <summary>
        /// 点的X坐标
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// 点的Y坐标
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// ROI类型标识
        /// </summary>
        public override string Type { get; protected set; } = "Point";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="x">点的X坐标</param>
        /// <param name="y">点的Y坐标</param>
        public RoiPoint(double x, double y)
        {
            X = x;
            Y = y;

            HasCreated = true;
        }

        /// <summary>
        /// 绘制点型ROI
        /// </summary>
        /// <summary>
        /// 绘制点型ROI
        /// </summary>
        public override void Draw()
        {
            if (!HasCreated) return;
            Pen pen = new Pen(this.Brush, Thickness / Scale);
            pen.Freeze();
            using (var dc = this.RenderOpen())
            {
                if (!Visible) return;//不显示,必须在using内部使用才会不显示
                dc.DrawLine(pen, new Point(X - 5, Y), new Point(X + 5, Y));
                dc.DrawLine(pen, new Point(X, Y - 5), new Point(X, Y + 5));
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
        /// 点型ROI的命中测试
        /// </summary>
        /// <param name="point">测试点</param>
        /// <returns>始终返回OnRoi，表示点击在ROI上</returns>
        public override RoiPart HitPointTest(Point point)
        {
            return RoiPart.OnRoi;
        }

        /// <summary>
        /// 移动点型ROI
        /// </summary>
        /// <param name="dx">X方向偏移量</param>
        /// <param name="dy">Y方向偏移量</param>
        public override void MoveOffset(double dx, double dy)
        {
            X += dx;
            Y += dy;
        }

        /// <summary>
        /// 点型ROI的操作点移动（点型ROI没有操作点，此方法为空实现）
        /// </summary>
        /// <param name="mousePoint">鼠标位置</param>
        /// <param name="dx">X方向偏移量</param>
        /// <param name="dy">Y方向偏移量</param>
        public override void MoveOffset_OperateItem(Point mousePoint, double dx, double dy)
        {
            // 点型ROI没有操作点，此方法为空实现
        }

        /// <summary>
        /// 更新操作点位置（点型ROI没有操作点，此方法为空实现）
        /// </summary>
        protected override void UpdataOperateItems()
        {
            // 点型ROI没有操作点，此方法为空实现
        }

        /// <summary>
        /// 获取点型ROI的像素信息
        /// </summary>
        /// <returns>包含点坐标的字典</returns>
        public override Dictionary<string, double> GetRoiPixelInfo()
        {
            Dictionary<string, double> pixelInfo = new Dictionary<string, double>
            {
                { "X", X },
                { "Y", Y }
            };
            return pixelInfo;
        }
    }
}