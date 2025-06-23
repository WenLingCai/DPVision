﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace ImageView
{
    /// <summary>
    /// 绘制模式枚举
    /// </summary>
    internal enum DrawingMode
    { 
        /// <summary>
        /// 绘制模式
        /// </summary>
        Draw,
        
        /// <summary>
        /// 擦除模式
        /// </summary>
        Erase
    }

    /// <summary>
    /// ROI遮罩类，用于绘制和擦除操作
    /// </summary>
    public class Mask : DrawingVisual
    {
        private readonly Brush brush = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
        private Geometry geometry = new PathGeometry();

        /// <summary>
        /// 当前绘制模式
        /// </summary>
        internal DrawingMode DrawingMode { get; set; } = DrawingMode.Draw;

        /// <summary>
        /// 画笔大小
        /// </summary>
        public double Size { get; set; }

        /// <summary>
        /// 绘制矩形区域列表
        /// </summary>
        public List<Rect> DrawRects { get; set; } = new List<Rect>();

        /// <summary>
        /// 擦除矩形区域列表
        /// </summary>
        public List<Rect> EraseRects { get; set; } = new List<Rect>();

        /// <summary>
        /// 所属画布
        /// </summary>
        public RoiCanvas OwnerCanvas { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Mask()
        {
            Size = 8;
            brush.Freeze();
        }

        /// <summary>
        /// 执行绘制操作
        /// </summary>
        internal void Draw()
        {
            using (DrawingContext dc = RenderOpen())
            {           
                if(DrawingMode == DrawingMode.Draw && DrawRects.Count > 0)
                {
                    geometry = Geometry.Combine(geometry, new RectangleGeometry(DrawRects.Last()), GeometryCombineMode.Union, null);
                }
                else if(DrawingMode == DrawingMode.Erase && EraseRects.Count > 0 && geometry.GetArea() > 0)
                {
                    geometry = Geometry.Combine(geometry, new RectangleGeometry(EraseRects.Last()), GeometryCombineMode.Exclude, null);
                }
                dc.DrawGeometry(brush, null, geometry);
            }
        }

        /// <summary>
        /// 清除遮罩
        /// </summary>
        public void Clear()
        {
            geometry = new PathGeometry();
            DrawRects.Clear();
            EraseRects.Clear();
        }
    }
}