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

namespace DPVToolNPointCali
{

    public class ToolInputParams
    {

        #region [输入参数]
        public string mInputImageStr = string.Empty;//输入图像IO名字

        public Mat mTrainImage = null;//制作模板的使用的训练图像

        public Mat mTempImage = null;//训练后得到的模板图像

        public Mat mMaskImage = null;//掩膜图像
     
        public bool mReTrain = false;//重新训练标志
    

        #endregion

    }

 
}
