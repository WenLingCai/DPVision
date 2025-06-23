using DPVision.Model.ROI;
using DPVision.Model.Tool;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DPVToolTemplateMatch
{

    public class ToolInputParams
    {

        #region [输入参数]
        internal string mInputImageStr = string.Empty;//输入图像IO名字
        [NonSerialized]
        internal Mat mTrainImage = null;//制作模板的使用的训练图像
        [NonSerialized]
        internal Mat mTempImage = null;//训练后得到的模板图像
        [NonSerialized]
        internal Mat mMaskImage = null;//掩膜图像
        internal Rect mTempRect = new Rect(0, 0, 0, 0);//模板区域
        internal Rect mSearchRect = new Rect(0, 0, 0, 0);//搜索区域
        [NonSerialized]
        internal RoiBase mTempRoi = null;//模版框
        [NonSerialized]
        internal RoiBase mSearchRoi = null;//搜索框
        [NonSerialized]
        internal RoiBase mAimRoi = null;//目标框
        [NonSerialized]
        internal bool mReTrain = false;//重新训练标志

        RoiBase FeatureRoi1;
        RoiBase FeatureRoi2;
        RoiBase FeatureRoi3;
        #endregion

    }
}
