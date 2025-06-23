using DPVision.Model;
using DPVision.Model.Flow;
using DPVision.Model.ROI;
using DPVision.Model.Tool;
using System.Reflection;
using System.Xml;

namespace DPVFlowDoubleMark1
{
 
    public class DPVFlowDoubleMark1 : IFlowBase
    {
        public IImageDisplay imageDisplay;
        public string Name
        {
            get;
            set;
        }
        public string FlowType => "DPVToolTemplateMatch";
      
        public float runTime { get; private set; }
        
        public DPVFlowDoubleMark1()
        {
            toolList.Add()
        }
        public List<ITool> toolList=new List<ITool>();
        public List<IToolUI> toolUIList = new List<IToolUI>();
        //临时参数
        private VisionImage inputImage;
        /// <summary>
        ///  初始化图片，设置输入图像
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="keyValue"></param>
        public void InitImage(VisionImage imagedata)
        {
            inputImage = imagedata;
        }

      

        public void Save(string path)
        {
          
        }

        public void Load(string path)
        {
           

        }
        public ResultState Run(IImageDisplay image)
        {
            
            return ResultState.OK;
        }
        public float GetFlowRunTime()
        {
            return runTime;
        }
    }
}
