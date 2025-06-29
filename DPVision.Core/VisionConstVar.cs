using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Specialized;

namespace DPVision.Core
{
    public class VisionConstVar
    {

        #region 常用常量
        public const string WinGrapName = "WinGrap.cfg";
        public const string CamerasPath = "\\Cameras\\";
        public const string FileSuffix = ".xml";
        public const string sToolsFileHead = "SYS-MeasureTools";
        public const string ImageSource = "ImageSource";



        public const string measurePath = "\\Vision\\";//在机种路径下存放视觉文件夹，xml文件名称由调用方提供
        public const string ProjectFile = "\\Software\\";//最终替换为上位机的机种路径保存

        #endregion
    }
}
