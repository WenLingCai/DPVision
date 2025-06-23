﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ImageView.Helper
{
    /// <summary>
    /// 提供绘图相关的辅助方法
    /// </summary>
    public class DrawingHelper
    {
        /// <summary>
        /// 绘制带箭头的线段
        /// </summary>
        /// <param name="dc">绘图上下文</param>
        /// <param name="pen">画笔</param>
        /// <param name="point0">起点</param>
        /// <param name="point1">终点</param>
        public static void DrawArraw(DrawingContext dc,Pen pen, Point point0,Point point1)
        {
            dc.DrawLine(pen, point0, point1);
            var angle = PointHelper.GetAngle(point0, point1);
            var u = angle * Math.PI / 180;
            Point[] points = new Point[2];
            // 计算两侧点坐标
            points[0] = new Point(
                point1.X - 10 * (float)Math.Cos(u - Math.PI / 6),
                point1.Y - 10 * (float)Math.Sin(u - Math.PI / 6));

            points[1] = new Point(
                point1.X - 10 * (float)Math.Cos(u + Math.PI / 6),
                point1.Y - 10 * (float)Math.Sin(u + Math.PI / 6));

            dc.DrawLine(pen, point1, points[0]);
            dc.DrawLine(pen, point1, points[1]);
        }

        public static void DrawCrossLine(DrawingContext dc, Pen pen, float width,float height,float fontSize)
        {
            Point pCenter = new Point(width * 0.5, height * 0.5);
            dc.DrawLine(pen, new Point(0, pCenter.Y), new Point(width, pCenter.Y));
            dc.DrawLine(pen, new Point(pCenter.X, 0), new Point(pCenter.X, height));
        }
        public static System.Windows.Media.Color DrawingColorToMediaColor(System.Drawing.Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
        public static void DrawText(DrawingContext dc, string szText, Point position, int fontSize,System.Drawing.Color color)
        {
            FontFamily fontFamily = new FontFamily("Arial");
            FormattedText formattedText = new FormattedText(szText, System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, new Typeface(fontFamily.Source), fontSize, new SolidColorBrush(DrawingColorToMediaColor(color)), 1.25);
        
            dc.DrawText(formattedText, position);

        }

    }

   
}