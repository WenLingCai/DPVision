using DPVision.Model.ROI;
using DPVision.Model.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DPVToolTemplateMatch
{

    public class ToolRunParams
    {

        #region [运行参数]
        internal int mTempLowThreshold = 10;//制作模板参数--低阈值
        internal int mTempHighThreshold = 50;//制作模板参数--高阈值
        internal int mMaxMatchNum = 1;//运行参数--最大匹配数量
        internal float mMinScore = 0.75f; //运行参数--最低分数
        internal float mMinAngle = -45f;//运行参数--最小角度
        internal float mMaxAngle = 45f;//运行参数--最大角度
        internal float mMinScale = 1.0f;//运行参数--最小缩放
        internal float mMaxScale = 1.0f;//运行参数--最大缩放
        internal float mMaxOverlap = 0F;//运行参数--最大重叠
        internal float mGreediness = 1;//运行参数--贪婪度
        internal float mFontSize = 0.5f;//显示字号
        #endregion

    }
}
