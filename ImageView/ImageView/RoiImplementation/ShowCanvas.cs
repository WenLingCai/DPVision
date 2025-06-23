using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImageView
{
    /// <summary>
    /// 始终绘制灰黑网格背景的Canvas
    /// </summary>
    public class ShowCanvas : Canvas
    {
        public Point? CrossCenter { get; set; } // null表示不画十字

        // 可选：十字颜色/大小可属性化
        public double CrossSize { get; set; } = 16;
        public Brush CrossBrush { get; set; } = Brushes.Red;
        public double CrossThickness { get; set; } = 1.0;
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            // -------画十字线-------
            if (CrossCenter.HasValue)
            {
                var p = CrossCenter.Value;
                double s = CrossSize / 2;
                var pen = new Pen(CrossBrush, CrossThickness);

                // 横线
                dc.DrawLine(pen, new Point(p.X - s, p.Y), new Point(p.X + s, p.Y));
                // 竖线
                dc.DrawLine(pen, new Point(p.X, p.Y - s), new Point(p.X, p.Y + s));
            }
        }
        // 设置十字中心并刷新
        public void SetCross(Point? center, double crossSize)
        {
            this.CrossCenter = center;
            this.CrossSize = crossSize;
            this.InvalidateVisual();
        }
    }
}