using DPVision.Model.ROI;
using System;
using System.Xml;

namespace DPVision.Model.Tool
{
    public interface ITool
    {
        string ToolType { get; }
        float ToolRunTime { get; }
        string ToolName { get;}
        ResultState ToolState { get;}
        event EventHandler ParametersChanged;
        bool GetParam(string keyName, ref string keyValue);
        bool SetParam(string keyName, string keyValue);
        object ExportParameters();
        void ImportParameters(object data);
        ResultState Run(IImageDisplay control=null);
    }

    public class ToolParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public string Type { get; set; }
        // 构造函数省略
        public ToolParameter(string name,object value,string type) 
        {
            Name=name;
            Value=value;
            Type=type;
        }
    }
}