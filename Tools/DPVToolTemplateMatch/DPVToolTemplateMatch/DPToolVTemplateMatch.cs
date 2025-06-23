
using DPVision.Common.FileFormat;
using DPVision.Core;
using DPVision.Model;
using DPVision.Model.ROI;
using DPVision.Model.Tool;
using DPVisionLog;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;


namespace DPVToolTemplateMatch
{
    public enum TemplateMode
    {
        Edge = 0,
        Gray
    }


    public class DPToolVTemplateMatch : ITool
    {
        public IImageDisplay imageDisplay;
        public string Name 
        {
            get;
            set;
        }
        public string ToolType => "DPVToolTemplateMatch";
   
        public event EventHandler ParametersChanged;
        public float runTime { get; private set; }
    
        public DPToolVTemplateMatch()
        {
           
        }
        public ToolInputParams inputParams { get; private set; }
        public ToolRunParams toolRunParams { get; private set; }
        public  ToolResultParams resultParams { get; private set; }

   
        #region [临时变量]
        bool bRes = false;
        [NonSerialized]
        string ErrMsg = "";
        [NonSerialized]
        Mat pResMat = new Mat();
        [NonSerialized]
        int nTrueFindNum = 0;
        #endregion
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

        //
        // 获取参数
        public bool GetParam(string keyName, ref string keyValue)
        {
            // 获取
            PropertyInfo? propertyInfo = this.GetType().GetProperty(keyName);
            if (propertyInfo!=null)
            {
                object obj= propertyInfo.GetValue(this);
                if (obj != null)
                {
                    keyValue= obj.ToString();
                    return true;
                }
            }
            return false;
        }

        // 设置参数
        public bool SetParam(string keyName, string keyValue)
        {
            // 设置
            PropertyInfo? propertyInfo = this.GetType().GetProperty(keyName);
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(this, keyValue);
                return true;
            }

            return false;
        }

        public XmlElement SaveToXmlNode(XmlDocument doc, string nodeName)
        {
            ToolRunParams param = toolRunParams as ToolRunParams;
            var node = doc.CreateElement(nodeName);
            node.SetAttribute("MinScore", param.MinScore.ToString());
            node.SetAttribute("PyramidLevel", param.PyramidLevel.ToString());
            node.SetAttribute("AngleStart", param.AngleStart.ToString());
            node.SetAttribute("AngleEnd", param.AngleEnd.ToString());

            if (param.FeatureRoi1 != null)
            {
                node.AppendChild(param.FeatureRoi1.SaveToXmlNode(doc, "FeatureRoi1"));
            }
            if (param.FeatureRoi2 != null)
            {
                node.AppendChild(param.FeatureRoi2.SaveToXmlNode(doc, "FeatureRoi2"));
            }
            if (param.FeatureRoi3 != null)
            {
                node.AppendChild(param.FeatureRoi3.SaveToXmlNode(doc, "FeatureRoi3"));
            }
            return node;
        }

        public void LoadFromXmlNode(XmlElement node)
        {
            ToolRunParams param = toolRunParams as ToolRunParams;
            if (node != null)
            {
                param.MinScore = DPVisionCore.Instance.ReadFloatAttr(node, "MinScore");
                param.PyramidLevel = DPVisionCore.Instance.ReadIntAttr(node, "PyramidLevel");
                param.AngleStart = DPVisionCore.Instance.ReadFloatAttr(node, "AngleStart");
                param.AngleEnd = DPVisionCore.Instance.ReadFloatAttr(node, "AngleEnd");

                DPVisionCore.Instance.LoadRoi(node, "FeatureRoi1", param.FeatureRoi1);
                DPVisionCore.Instance.LoadRoi(node, "FeatureRoi2", param.FeatureRoi2);
                DPVisionCore.Instance.LoadRoi(node, "FeatureRoi3", param.FeatureRoi3);
            }

        }
        public ResultState Run(IImageDisplay image)
        {
            
            return ResultState.OK;
        }
        public float GetAlgRunTime()
        {
            return runTime;
        }
    }
}