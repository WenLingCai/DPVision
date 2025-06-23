﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageView
{
    /// <summary>
    /// ROI的部位类型，用于标识鼠标点击的位置
    /// </summary>
    public enum RoiPart
    {
        /// <summary>
        /// 在ROI主体上
        /// </summary>
        OnRoi,
        
        /// <summary>
        /// 在ROI的操作点上
        /// </summary>
        OnOperateItem,
        
        /// <summary>
        /// 不在ROI上
        /// </summary>
        None
    }
}