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
    /// 圆形ROI，表示图像上的一个圆
    /// </summary>
    public class RoiCircle : Roi
    {
        /// <summary>
        /// 圆心X坐标
        /// </summary>
        public double CenterX { get; set; }
        
        /// <summary>
        /// 圆心Y坐标
        /// </summary>
        public double CenterY { get; set; }
        
        /// <summary>
        /// 圆的半径
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// ROI类型标识
        /// </summary>
        public override string Type { get; protected set; } = "Circle";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="centerX">圆心X坐标</param>
        /// <param name="centerY">圆心Y坐标</param>
        /// <param name="radius">圆的半径</param>
        public RoiCircle(double centerX, double centerY, double radius)
        {
            CenterX = centerX;
            CenterY = centerY;
            Radius = radius;
            
            // 添加一个操作点用于调整半径
            OperateItems.Add(new OperateItem(new Point()));
            
            UpdataOperateItems();
            HasCreated = true;
        }
        /// <summary>
        /// 绘制圆形ROI
        /// </summary>
        public override void Draw()
        {
            if (!HasCreated) return;
            Pen pen = new Pen(this.Brush, Thickness / Scale);
            pen.Freeze();
            using (var dc = this.RenderOpen())
            {
                if (!Visible) return;//不显示,必须在using内部使用才会不显示
                dc.DrawEllipse(Brushes.Transparent, pen,new Point(CenterX, CenterY), Radius, Radius);
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
        /// 圆形ROI的命中测试
        /// </summary>
        /// <param name="point">测试点</param>
        /// <returns>点所在的ROI部位</returns>
        public override RoiPart HitPointTest(Point point)
        {
            // 首先检查是否点击在操作点上
            foreach (var item in OperateItems)
            {
                if (OperateItem.IsInItem(point, item))
                {
                    return RoiPart.OnOperateItem;
                }
            }
            // 检查是否点击在圆内
            if (PointHelper.GetDistance(point, new Point(CenterX, CenterY)) <= Radius)
            {
                return RoiPart.OnRoi;
            }
            else
            {
                return RoiPart.None;
            }
        }

        /// <summary>
        /// 移动圆形ROI
        /// </summary>
        /// <param name="dx">X方向偏移量</param>
        /// <param name="dy">Y方向偏移量</param>
        public override void MoveOffset(double dx, double dy)
        {
            CenterX += dx;
            CenterY += dy;
        }

        /// <summary>
        /// 圆形ROI的操作点移动，用于调整半径
        /// </summary>
        /// <param name="mousePoint">鼠标位置</param>
        /// <param name="dx">X方向偏移量</param>
        /// <param name="dy">Y方向偏移量</param>
        public override void MoveOffset_OperateItem(Point mousePoint, double dx, double dy)
        {
            switch (SelectedOperateItemIndex)
            {
                case 0:
                    // 调整半径，确保半径始终为正值
                    if( Radius + dx > 0) Radius += dx;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 更新操作点位置
        /// </summary>
        protected override void UpdataOperateItems()
        {
            // 操作点位于圆的右侧，用于调整半径
            OperateItems[0].Center = new Point(CenterX + Radius, CenterY);
        }

        /// <summary>
        /// 获取圆形ROI的像素信息
        /// </summary>
        /// <returns>包含圆心坐标和半径的字典</returns>
        public override Dictionary<string, double> GetRoiPixelInfo()
        {
            Dictionary<string, double> info = new Dictionary<string, double>()
            {
                {"CenterX",CenterX },
                {"CenterY" ,CenterY },
                {"Radius",Radius }
            };
            return info;
        }
    }
}