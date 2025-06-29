using DPVision.Core;
using DPVision.Model.Flow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Xml.Serialization;




namespace DPVision.Flow.ViewModels
{
    public class DPVFlowViewModel : BindableBase
    {
       
        public DPVFlowViewModel()
        {
          
        }
       


        public string GetFlowPath(string flowName)
        {
            string path = AppContext.BaseDirectory + VisionConstVar.ProjectFile + VisionConstVar.measurePath + flowName + VisionConstVar.measurePath + VisionConstVar.FileSuffix;
            return path;
        }

        //public IFlowBase LoadFlow(FlowType flowType, string flowName)
        //{
        //    IFlowBase flow;
        //    switch (flowType)
        //    {
        //        case FlowType.Cali:
        //            flow = FlowFactory.CreateFlow("DPVFlowCali", flowName);
        //            break;
        //        case FlowType.Mark:
        //            flow = FlowFactory.CreateFlow("DPVFlowMark", flowName);
        //            break;
        //        default:
        //            flow = FlowFactory.CreateFlow("DPVFlowMark", flowName);
        //            break;
        //    }
        //    if (flow != null)
        //    {
        //        string path = GetFlowPath(flowName);
        //        flow.Load(path);
        //    }
        //    return flow;
        //}
        //public void Save(string path)
        //{
        //    if (!File.Exists(path))
        //    {

        //    }

        //    var proxy = new FlowConfigProxy
        //    {
        //        FlowType = this.GetType().AssemblyQualifiedName,
        //        FlowName = this.FlowName,
        //    };
        //    foreach (var tool in this.toolList)
        //    {
        //        var toolType = tool.GetType();
        //        proxy.Tools.Add(new ToolConfigProxy
        //        {
        //            ToolType = toolType.AssemblyQualifiedName,
        //            ToolName = tool.ToolName,
        //            Parameters = tool.ExportParameters().ToString()
        //        });
        //    }
        //    var serializer = new XmlSerializer(typeof(FlowConfigProxy));
        //    using (var sw = new StreamWriter(path))
        //    {
        //        serializer.Serialize(sw, proxy);
        //    }

        //}

        //public void Load(string path)
        //{
        //    if (File.Exists(path))
        //    {
        //        var serializer = new XmlSerializer(typeof(FlowConfigProxy));
        //        FlowConfigProxy proxy;
        //        using (var sr = new StreamReader(path))
        //            proxy = (FlowConfigProxy)serializer.Deserialize(sr);

        //        this.FlowName = proxy.FlowName;
        //        this.toolList.Clear();
        //        this.toolUIList.Clear();
        //        foreach (var toolProxy in proxy.Tools)
        //        {
        //            var tool = ToolFactory.CreateTool(toolProxy.ToolType);

        //            tool.ImportParameters(toolProxy.Parameters);
        //            this.toolList.Add(tool);
        //        }
        //    }
        //    else
        //    {
        //        this.toolList.Clear();
        //        var tool_TemplateMatch = ToolFactory.CreateTool("DPToolVTemplateMatch");
        //        var tool_FindCicle = ToolFactory.CreateTool("DPVToolFindCicle");

        //        this.toolList.Add(tool_TemplateMatch);
        //        this.toolList.Add(tool_FindCicle);



        //        this.toolUIList.Clear();
        //        var tool_TemplateMatchUI = ToolFactory.CreateToolUI("DPToolVTemplateMatch");
        //        var tool_FindCicleUI = ToolFactory.CreateToolUI("DPVToolFindCicle");

        //        this.toolUIList.Add(tool_TemplateMatchUI);
        //        this.toolUIList.Add(tool_FindCicleUI);
        //    }
        //}
    }
}
