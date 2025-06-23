using DPVision.Model.ROI;
using System;
using System.Xml;

namespace DPVision.Model.Tool
{
    public interface ITool
    {
        string ToolType { get; }
        float runTime { get; }
        string Name { get; set; }
     
        event EventHandler ParametersChanged; // 通知UI参数有变
        bool GetParam(string keyName, ref string keyValue);
        bool SetParam(string keyName, string keyValue);
        XmlElement SaveToXmlNode(XmlDocument doc, string nodeName);
        void LoadFromXmlNode(XmlElement node);
        ResultState Run(IImageDisplay control=null);
        float GetAlgRunTime();
    }
}