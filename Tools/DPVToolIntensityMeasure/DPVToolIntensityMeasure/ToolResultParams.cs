using DPVision.Model.ROI;
using DPVision.Model.Tool;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DPVToolIntensityMeasure
{
    public class ToolResultParams
    {
        #region[输出参数]
        [NonSerialized]
        internal int ModelID = -1;//模型ID
        [NonSerialized]
        internal IntPtr pMatches = IntPtr.Zero;
        [NonSerialized]
        internal int MatchNum;//匹配数量
        [NonSerialized]
        internal Point2d[] MatchCenter = null;//匹配中心
        [NonSerialized]
        internal double[] MatchR = null;//匹配角度A
        [NonSerialized]
        internal double[] MatchScore = null;//匹配分数
        [NonSerialized]
        internal double[] MatchScale = null;//匹配缩放
        #endregion
    }
}
