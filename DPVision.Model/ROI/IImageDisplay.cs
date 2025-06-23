using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DPVision.Model.ROI
{
    public interface IImageDisplay
    {
        /// <summary>
        /// 绘制直线
        /// </summary>
        void DrawLine(System.Drawing.PointF pointStar, System.Drawing.PointF pointEnd, bool bInteractive = false);


        /// <summary>
        /// 绘制矩形
        /// </summary>
        void DrawRectangle(System.Drawing.PointF point,float width,float height, bool bInteractive = false);


        /// <summary>
        /// 绘制旋转矩形
        /// </summary>
        void DrawRectangleAffine(System.Drawing.PointF point,float width,float height,float angle, bool bInteractive = false);


        /// <summary>
        /// 绘制圆
        /// </summary>
        void DrawCircle(System.Drawing.PointF point,float r, bool bInteractive = false);


        /// <summary>
        /// 绘制椭圆
        /// </summary>
        void DrawEllipse(System.Drawing.PointF point,float r1, float r2,float angle, bool bInteractive = false);


        /// <summary>
        /// 绘制点
        /// </summary>
        void DrawPoint(System.Drawing.PointF point, bool bInteractive = false);

        /// <summary>
        /// 显示图片
        /// </summary>
        void DispalyImage(byte[] image);

        /// <summary>
        /// 自适应显示图片
        /// </summary>
        void AutoFitImage(byte[] image);

        /// <summary>
        /// 保存图片
        /// </summary>
         void SaveImage(string path);
     

        /// <summary>
        /// 保存截图
        /// </summary>
         void SaveScreenImage(string path);
    

        /// <summary>
        /// 显示辅助线
        /// </summary>
         void DispalyCrossLine(System.Drawing.PointF point, float fontSize);
       

        /// <summary>
        /// 绘制掩膜
        /// </summary>
         void DrawMask(List<System.Drawing.RectangleF> rect, List<System.Drawing.RectangleF> EraseRects, int pointSize);
     

        /// <summary>
        /// 绘制文字
        /// </summary>
         void DrawText(string text, float fontSize, System.Drawing.Color color);
     


        /// <summary>
        /// 获取roi信息
        /// </summary>
         Dictionary<string, double> GetRoiInfo();
     

        /// <summary>
        /// 获取mask信息
        /// </summary>
         Dictionary<string, List<System.Drawing.RectangleF>> GetMaskInfo();

    }
}
