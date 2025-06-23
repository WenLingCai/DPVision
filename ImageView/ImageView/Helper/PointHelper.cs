﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImageView.Helper
{
    /// <summary>
    /// 提供二维点相关的几何计算辅助方法
    /// </summary>
    public class PointHelper
    {
        /// <summary>
        /// 将点绕指定中心旋转指定角度
        /// </summary>
        /// <param name="point">要旋转的点坐标</param>
        /// <param name="center">旋转中心点坐标</param>
        /// <param name="angle">旋转角度(度)，顺时针为正方向</param>
        /// <returns>旋转后的新点坐标</returns>
        public static Point RotateFrom(Point point,Point center,double angle)
        {
            double radians = angle * (Math.PI / 180);
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            double dx = point.X - center.X;
            double dy = point.Y - center.Y;
            double newX = center.X + (cos * dx - sin * dy);
            double newY = center.Y + (sin * dx + cos * dy);
            return new Point(newX, newY);
        }
        /// <summary>
        /// 计算两点连线的角度（相对于X轴正方向）
        /// </summary>
        /// <param name="startPoint">起点坐标</param>
        /// <param name="endPoint">终点坐标</param>
        /// <returns>两点连线的角度(度)，范围[-180,180]，X轴正方向为0度，顺时针为正</returns>
        /// <exception cref="ArgumentException">当两点重合时抛出</exception>
        public static double GetAngle(Point startPoint, Point endPoint)
        {
            // 计算坐标差值
            double deltaX = endPoint.X - startPoint.X;
            double deltaY = endPoint.Y - startPoint.Y;

            if(deltaX == 0 && deltaY == 0) // 两点重合
            {
                throw new ArgumentException("两点出现重合");
            }
            // 使用反正切计算弧度
            double radians = Math.Atan2(deltaY, deltaX);

            // 转换为角度
            double degrees = radians * 180 / Math.PI;

            // 将角度规范化为-180到180范围
            // 计算模360后的余数（可能为负数）
            double remainder = degrees % 360;

            // 调整余数到 [0, 360) 区间
            if (remainder < 0)
                remainder += 360;

            // 将超过180度的部分转换为负数
            if (remainder >= 180)
                remainder -= 360;

            return remainder;
        }
        /// <summary>
        /// 计算两点之间的欧几里得距离
        /// </summary>
        /// <param name="point0">第一个点</param>
        /// <param name="Point1">第二个点</param>
        /// <returns>两点之间的直线距离</returns>
        public static double GetDistance(Point point0,Point Point1)
        {
            return Math.Sqrt(Math.Pow(point0.X - Point1.X, 2) + Math.Pow(point0.Y - Point1.Y, 2));
        }
        public static bool IsPointOnLineSegment(Point segmentStart, Point segmentEnd, Point point)
        {
            // 首先检查P是否在AB延长线上
            double crossProduct = (point.Y - segmentStart.Y) * (segmentEnd.X - segmentStart.X) - (point.X - segmentStart.X) * (segmentEnd.Y - segmentStart.Y);
            if (Math.Abs(crossProduct) > 1e-10) return false; // 不在同一条直线上

            // 然后检查P是否在A和B之间
            // 检查x的范围
            if (Math.Min(segmentStart.X, segmentEnd.X) <= point.X && point.X <= Math.Max(segmentStart.X, segmentEnd.X) &&
                // 检查y的范围
                Math.Min(segmentStart.Y, segmentEnd.Y) <= point.Y && point.Y <= Math.Max(segmentStart.Y, segmentEnd.Y))
            {
                return true;
            }

            return false;
        }
    }
}