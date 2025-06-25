using DPVision.Core;
using DPVision.Model;
using DPVision.Model.Flow;
using DPVision.Model.ROI;
using DPVision.Model.Tool;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace DPVFlowMark
{
 
    public class DPVFlowMark : IFlowBase
    {
        public IImageDisplay imageDisplay;
        public string FlowName
        {
            get;
            set;
        }
        public string FlowType => "DPVFlowMark";
      
        public float runTime { get; private set; }
        
        public DPVFlowMark()
        {
            
        }
        public List<ITool> toolList
        {
            get;
            set;
        }
        public List<IToolUI> toolUIList
        {
            get;
            set;
        }

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
            if (!File.Exists(path))
            {

            }
          
                var proxy = new FlowConfigProxy
                {
                    FlowType = this.GetType().AssemblyQualifiedName,
                    FlowName = this.FlowName,
                };
                foreach (var tool in this.toolList)
                {
                    var toolType = tool.GetType();
                    proxy.Tools.Add(new ToolConfigProxy
                    {
                        ToolType = toolType.AssemblyQualifiedName,
                        ToolName = tool.ToolName,
                        Parameters = tool.ExportParameters().ToString()
                    });
                }
                var serializer = new XmlSerializer(typeof(FlowConfigProxy));
                using (var sw = new StreamWriter(path))
                {
                    serializer.Serialize(sw, proxy);
                }
            
        }

        public void Load(string path)
        {
            if (File.Exists(path))
            {
                var serializer = new XmlSerializer(typeof(FlowConfigProxy));
                FlowConfigProxy proxy;
                using (var sr = new StreamReader(path))
                    proxy = (FlowConfigProxy)serializer.Deserialize(sr);

                this.FlowName = proxy.FlowName;
                this.toolList.Clear();
                this.toolUIList.Clear();
                foreach (var toolProxy in proxy.Tools)
                {
                    var tool = ToolFactory.CreateTool(toolProxy.ToolType);
               
                    tool.ImportParameters(toolProxy.Parameters);
                    this.toolList.Add(tool);
                }
            }
            else
            {
                this.toolList.Clear();
                var tool_TemplateMatch = ToolFactory.CreateTool("DPToolVTemplateMatch");
                var tool_FindCicle = ToolFactory.CreateTool("DPVToolFindCicle");

                this.toolList.Add(tool_TemplateMatch);
                this.toolList.Add(tool_FindCicle);



                this.toolUIList.Clear();
                var tool_TemplateMatchUI = ToolFactory.CreateToolUI("DPToolVTemplateMatch");
                var tool_FindCicleUI = ToolFactory.CreateToolUI("DPVToolFindCicle");

                this.toolUIList.Add(tool_TemplateMatchUI);
                this.toolUIList.Add(tool_FindCicleUI);
            }
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
