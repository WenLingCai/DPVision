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

namespace DPVToolFindCircle
{
    [Serializable]
    public class ToolRunParams
    {

        #region [运行参数]
        public int mTempLowThreshold = 10;//制作模板参数--低阈值
        public int mTempHighThreshold = 50;//制作模板参数--高阈值
        public int mMaxMatchNum = 1;//运行参数--最大匹配数量
        public float mMinScore = 0.75f; //运行参数--最低分数
        public float mMinAngle = -45f;//运行参数--最小角度
        public float mMaxAngle = 45f;//运行参数--最大角度
        public float mMinScale = 1.0f;//运行参数--最小缩放
        public float mMaxScale = 1.0f;//运行参数--最大缩放
        public float mMaxOverlap = 0F;//运行参数--最大重叠
        public float mGreediness = 1;//运行参数--贪婪度
        public float mFontSize = 0.5f;//显示字号

        public Rect mTempRect = new Rect(0, 0, 0, 0);//模板区域1
        public Rect mTempRect2 = new Rect(0, 0, 0, 0);//模板区域2
        public Rect mTempRect3 = new Rect(0, 0, 0, 0);//模板区域3
        public Rect mSearchRect = new Rect(0, 0, 0, 0);//搜索区域
        #endregion

    }
}
