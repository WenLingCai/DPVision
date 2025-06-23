﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ImageView
{
    /// <summary>
    /// ROI操作点类，用于调整ROI的形状和位置
    /// </summary>
    public class OperateItem
    {
        /// <summary>
        /// 操作点的中心位置
        /// </summary>
        public Point Center { get; set; }

        /// <summary>
        /// 操作点的半径（默认5像素）
        /// </summary>
        public double Radius { get; set; } = 5;

        /// <summary>
        /// 操作点的填充颜色（默认蓝色）
        /// </summary>
        public Brush FillBrush { get; set; } = Brushes.Blue;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="center">操作点的中心位置</param>
        public OperateItem(Point center)
        {
            Center = center;
        }

        /// <summary>
        /// 判断点是否在操作点范围内
        /// </summary>
        /// <param name="point">测试点</param>
        /// <param name="item">操作点</param>
        /// <returns>如果点在操作点范围内返回true，否则返回false</returns>
        public static bool IsInItem(Point point, OperateItem item)
        {
            var distance = Math.Sqrt(Math.Pow(point.X - item.Center.X, 2) + Math.Pow(point.Y - item.Center.Y, 2));
            return distance <= item.Radius;
        }
    }
}