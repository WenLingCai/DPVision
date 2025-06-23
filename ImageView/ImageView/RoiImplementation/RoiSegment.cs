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
    /// 线段ROI，表示图像上的一条线段
    /// </summary>
    public class RoiSegment : Roi
    {
        /// <summary>
        /// 起点X坐标
        /// </summary>
        public double StartX { get; set; }
        
        /// <summary>
        /// 起点Y坐标
        /// </summary>
        public double StartY { get; set; }
        
        /// <summary>
        /// 终点X坐标
        /// </summary>
        public double EndX { get; set; }
        
        /// <summary>
        /// 终点Y坐标
        /// </summary>
        public double EndY { get; set; }

        /// <summary>
        /// ROI类型标识
        /// </summary>
        public override string Type { get; protected set; } = "Segment";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="startX">起点X坐标</param>
        /// <param name="startY">起点Y坐标</param>
        /// <param name="endX">终点X坐标</param>
        /// <param name="endY">终点Y坐标</param>
        public RoiSegment(double startX, double startY, double endX, double endY)
        {
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;

            // 添加两个操作点，分别用于调整线段的起点和终点
            for(int i = 0; i < 2; i++)
            {
                OperateItems.Add(new OperateItem(new Point()));
            }
            HasCreated = true;
        }
        /// <summary>
        /// 绘制线段ROI
        /// </summary>
        public override void Draw()
        {
            if (!HasCreated) return;
            Pen pen = new Pen(this.Brush, Thickness / Scale);
            pen.Freeze();
            using (var dc = this.RenderOpen())
            {
                if (!Visible) return;//不显示,必须在using内部使用才会不显示
                // 绘制带箭头的线段
                DrawingHelper.DrawArraw(dc, pen, new Point(StartX, StartY), new Point(EndX, EndY));
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
        /// 获取线段ROI的像素信息
        /// </summary>
        /// <returns>包含起点和终点坐标的字典</returns>
        public override Dictionary<string, double> GetRoiPixelInfo()
        {
            Dictionary<string, double> info = new Dictionary<string, double>()
            {
                {"StartX",StartX },
                {"StartY",StartY },
                {"EndX",EndX },
                {"EndY",EndY },
            };
            return info;
        }

        /// <summary>
        /// 线段ROI的命中测试
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
            // 检查点是否在线段上
            if (PointHelper.IsPointOnLineSegment( new Point(StartX, StartY), new Point(EndX, EndY),point))
            {
                return RoiPart.OnRoi;
            }
            else
            {
                return RoiPart.None;
            }
        }

        /// <summary>
        /// 移动线段ROI
        /// </summary>
        /// <param name="dx">X方向偏移量</param>
        /// <param name="dy">Y方向偏移量</param>
        public override void MoveOffset(double dx, double dy)
        {
            StartX += dx;
            StartY += dy;
            EndX += dx;
            EndY += dy;
        }

        /// <summary>
        /// 线段ROI的操作点移动，用于调整起点或终点位置
        /// </summary>
        /// <param name="mousePoint">鼠标位置</param>
        /// <param name="dx">X方向偏移量</param>
        /// <param name="dy">Y方向偏移量</param>
        public override void MoveOffset_OperateItem(Point mousePoint, double dx, double dy)
        {
            switch (SelectedOperateItemIndex)
            {
                case 0: // 移动起点
                    StartX += dx;
                    StartY += dy;
                    break;
                case 1: // 移动终点
                    EndX += dx;
                    EndY += dy;
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
            // 更新起点和终点的操作点位置
            OperateItems[0].Center = new Point(StartX, StartY);
            OperateItems[1].Center = new Point(EndX, EndY);
        }
    }
}