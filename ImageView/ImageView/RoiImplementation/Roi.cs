﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace ImageView
{
    /// <summary>
    /// ROI(感兴趣区域)的抽象基类，定义了所有ROI的共同特性和行为
    /// </summary>
    public abstract class Roi : DrawingVisual
    {
        /// <summary>
        /// ROI的操作点列表，用于调整ROI的形状和位置
        /// </summary>
        internal List<OperateItem> OperateItems = new List<OperateItem>();
        
        /// <summary>
        /// 当前选中的操作点索引
        /// </summary>
        internal int SelectedOperateItemIndex = -1;

        /// <summary>
        /// 指示ROI是否已创建完成
        /// </summary>
        public bool HasCreated { get;protected set; } = false;
        
        /// <summary>
        /// ROI的唯一标识符
        /// </summary>
        public string ID { get;protected set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// ROI的类型名称
        /// </summary>
        public abstract string Type { get;protected set; }
        
        /// <summary>
        /// 指示ROI是否可交互
        /// </summary>
        public bool Interactive { get; set; } = true;
        
        /// <summary>
        /// ROI的绘制画笔颜色
        /// </summary>
        public Brush Brush { get; set; } = Brushes.Blue;
        
        /// <summary>
        /// ROI的线条粗细
        /// </summary>
        public double Thickness { get; set; } = 1.0;
        
        /// <summary>
        /// 所属的画布
        /// </summary>
        public RoiCanvas OwnerCanvas { get; set; }

        /// <summary>
        /// 捕获选中的操作点
        /// </summary>
        /// <param name="mousePoint">鼠标位置</param>
        public virtual void CatchSeletedOperateItem(Point mousePoint)
        {
            for (int i = 0; i < OperateItems.Count; i++)
            {
                if (OperateItem.IsInItem(mousePoint, OperateItems[i]))
                {
                    SelectedOperateItemIndex = i;
                    break;
                }
            }
        }
        
        /// <summary>
        /// 绘制ROI
        /// </summary>
        public abstract void Draw();
        
        /// <summary>
        /// 更新操作点位置
        /// </summary>
        protected abstract void UpdataOperateItems();
        
        /// <summary>
        /// ROI的命中测试，判断点在ROI的哪个部分
        /// </summary>
        /// <param name="point">测试点</param>
        /// <returns>点所在的ROI部位</returns>
        public abstract RoiPart HitPointTest(Point point);
        
        /// <summary>
        /// ROI的整体移动
        /// </summary>
        /// <param name="dx">X方向偏移量</param>
        /// <param name="dy">Y方向偏移量</param>
        public abstract void MoveOffset(double dx, double dy);
        
        /// <summary>
        /// ROI的操作点移动
        /// </summary>
        /// <param name="mousePoint">鼠标位置</param>
        /// <param name="dx">X方向偏移量</param>
        /// <param name="dy">Y方向偏移量</param>
        public abstract void MoveOffset_OperateItem(Point mousePoint, double dx, double dy);
        
        /// <summary>
        /// 获取ROI的像素信息
        /// </summary>
        /// <returns>包含ROI像素信息的字典</returns>
        public abstract Dictionary<string,double> GetRoiPixelInfo();

        /// <summary>
        /// 指示ROI是否被选中
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        /// <summary>
        /// IsSelected依赖属性
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected",
                typeof(bool),
                typeof(Roi),
                new PropertyMetadata(false,IsSelectedChanged));
                
        /// <summary>
        /// IsSelected属性变更处理方法
        /// </summary>
        private static void IsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Roi roi = d as Roi;
            if (!roi.Interactive) return;//不可交互的roi不响应
            roi.Draw();
        }

        /// <summary>
        /// ROI的缩放比例
        /// </summary>
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        
        /// <summary>
        /// Scale依赖属性
        /// </summary>
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(Roi), new PropertyMetadata(1.0, ScaleChanged));

        /// <summary>
        /// Scale属性变更处理方法
        /// </summary>
        private static void ScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Roi roi = d as Roi;
            roi.Draw();
        }

        /// <summary>
        /// 指示ROI是否可见
        /// </summary>
        public bool Visible
        {
            get { return (bool)GetValue(VisibleProperty); }
            set { SetValue(VisibleProperty, value); }
        }

        /// <summary>
        /// Visible依赖属性
        /// </summary>
        public static readonly DependencyProperty VisibleProperty =
            DependencyProperty.Register("Visible", typeof(bool), typeof(Roi), new PropertyMetadata(true, ScaleChanged));
    }
}