﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ImageView
{
    /// <summary>
    /// ROI画布类，负责管理和绘制所有ROI元素
    /// </summary>
    public class RoiCanvas : Canvas
    {
        /// <summary>
        /// 遮罩对象
        /// </summary>
        public Mask Mask { get; set; }

        /// <summary>
        /// ROI对象列表
        /// </summary>
        public readonly List<Roi> Rois = new List<Roi>();

        /// <summary>
        /// 是否正在绘制遮罩
        /// </summary>
        public bool IsDrawMask { get; set; } = false;

        /// <summary>
        /// 可视化子元素数量（ROI数量+1个遮罩）
        /// </summary>
        protected override int VisualChildrenCount => Rois.Count + 1;

        /// <summary>
        /// 图像宽度
        /// </summary>
        internal int ImageWidth { get; set; } = -1;

        /// <summary>
        /// 图像高度
        /// </summary>
        internal int ImageHeight { get; set; } = -1;

        /// <summary>
        /// 构造函数，初始化遮罩
        /// </summary>
        public RoiCanvas()
        {
            Mask = new Mask
            {
                OwnerCanvas = this
            };
            AddLogicalChild(Mask);
            AddVisualChild(Mask);
            // 保证Canvas没有不透明背景，不会遮盖OnRender的内容
            Background = Brushes.Transparent;
        }

        /// <summary>
        /// 在指定位置绘制遮罩
        /// </summary>
        /// <param name="point">绘制点的坐标</param>
        public void DrawMask(Point point)
        {
            Mask.DrawRects.Add(new Rect(point.X - Mask.Size / 2, point.Y - Mask.Size / 2, Mask.Size, Mask.Size));
            Mask.Draw();
        }

        /// <summary>
        /// 在指定位置擦除遮罩
        /// </summary>
        /// <param name="point">擦除点的坐标</param>
        public void EraseMask(Point point)
        {
            Mask.EraseRects.Add(new Rect(point.X - Mask.Size / 2, point.Y - Mask.Size / 2, Mask.Size, Mask.Size));
            Mask.Draw();
        }

        /// <summary>
        /// 清除所有遮罩
        /// </summary>
        public void ClearMask()
        {
            Mask.Clear();
            Mask.Draw();
        }

        /// <summary>
        /// 获取指定索引的可视化子元素
        /// </summary>
        /// <param name="index">索引(0=遮罩，1~n=ROI)</param>
        /// <returns>可视化元素</returns>
        protected override Visual GetVisualChild(int index)
        {
            if(index == 0)
            {
                return Mask;
            }
            else
            {
                return Rois[index - 1];
            }
        }

        /// <summary>
        /// 通过索引获取ROI
        /// </summary>
        /// <param name="index">ROI索引</param>
        /// <returns>ROI对象</returns>
        public Roi GetRoi(int index)
        {
            return Rois[index];
        }

        /// <summary>
        /// 通过ID获取ROI
        /// </summary>
        /// <param name="ID">ROI的唯一标识符</param>
        /// <returns>ROI对象，未找到返回null</returns>
        public Roi GetRoi(string ID)
        {
            return Rois.Find(x => x.ID == ID);
        }

        /// <summary>
        /// 获取当前选中的ROI
        /// </summary>
        /// <returns>选中的ROI对象，未选中返回null</returns>
        public Roi GetSelectedRoi()
        {
            return Rois.Find(x => x.IsSelected);
        }

        /// <summary>
        /// 添加ROI到画布
        /// </summary>
        /// <param name="roi">要添加的ROI对象</param>
        public void AddRoi(Roi roi)
        {
            Rois.Add(roi);
            roi.OwnerCanvas = this;
            AddLogicalChild(roi);
            AddVisualChild(roi);
            roi.Draw();
        }

        /// <summary>
        /// 从画布移除ROI
        /// </summary>
        /// <param name="roi">要移除的ROI对象</param>
        public void RemoveRoi(Roi roi)
        {
            roi.OwnerCanvas = null;
            Rois.Remove(roi);
            RemoveLogicalChild(roi);
            RemoveVisualChild(roi);
        }
    }
}