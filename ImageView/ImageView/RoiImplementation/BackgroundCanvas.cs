using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImageView
{
    /// <summary>
    /// 始终绘制灰黑网格背景的Canvas
    /// </summary>
    public class BackgroundCanvas : Canvas
    {
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // 棋盘格参数
            double cellSize = 20; // 每个小格宽度
            Color color1 = Color.FromRgb(0x1C, 0x1C, 0x1C); // 黑
            Color color2 = Color.FromRgb(0x28, 0x28, 0x28); // 深灰

            int colCount = (int)(ActualWidth / cellSize) + 2;
            int rowCount = (int)(ActualHeight / cellSize) + 2;

            for (int y = 0; y < rowCount; y++)
            {
                for (int x = 0; x < colCount; x++)
                {
                    bool isDark = ((x + y) % 2 == 0);
                    var rect = new Rect(x * cellSize, y * cellSize, cellSize, cellSize);
                    dc.DrawRectangle(
                        new SolidColorBrush(isDark ? color1 : color2),
                        null,
                        rect);
                }
            }
        }
    }
}