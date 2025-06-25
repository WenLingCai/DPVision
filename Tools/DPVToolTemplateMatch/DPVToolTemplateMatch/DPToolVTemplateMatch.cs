
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
       
        public string ToolName 
        {
            get;
            set;
        }
        public string ToolType => "DPVToolTemplateMatch";
   
        public event EventHandler ParametersChanged;
       
        public float ToolRunTime { get; private set; }
        public ResultState ToolState { get; private set; }
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

        // 获取参数
        public bool GetParam(string keyName, ref string keyValue)
        {
            var propertyInfo = this.toolRunParams.GetType().GetProperty(keyName);
            if (propertyInfo != null)
            {
                object obj = propertyInfo.GetValue(this.toolRunParams);
                if (obj != null)
                {
                    keyValue = obj.ToString();
                    return true;
                }
            }
            return false;
        }

        // 设置参数（支持常见类型转换，含bool、enum、nullable）
        public bool SetParam(string keyName, string keyValue)
        {
            var propertyInfo = this.toolRunParams.GetType().GetProperty(keyName);
            if (propertyInfo != null)
            {
                try
                {
                    Type propType = propertyInfo.PropertyType;

                    object value = null;
                    if (propType.IsEnum)
                    {
                        value = Enum.Parse(propType, keyValue);
                    }
                    else if (propType == typeof(bool))
                    {
                        value = (keyValue == "1" || keyValue.ToLower() == "true");
                    }
                    else if (Nullable.GetUnderlyingType(propType) != null)
                    {
                        // 处理可空类型
                        var underlyingType = Nullable.GetUnderlyingType(propType);
                        value = string.IsNullOrEmpty(keyValue) ? null : Convert.ChangeType(keyValue, underlyingType);
                    }
                    else
                    {
                        value = Convert.ChangeType(keyValue, propType);
                    }

                    propertyInfo.SetValue(this.toolRunParams, value);
                    return true;
                }
                catch
                {
                    // 类型转换失败
                    return false;
                }
            }
            return false;
        }

        public object ExportParameters()
        {
            var serializer = new XmlSerializer(typeof(ToolRunParams));
            using (var sw = new StringWriter())
            {
                serializer.Serialize(sw, toolRunParams);
                return sw.ToString(); // 返回XML格式字符串
            }
        }

        public void ImportParameters(object data)
        {
            if (data == null) return;
            var xml = data.ToString();
            var serializer = new XmlSerializer(typeof(ToolRunParams));
            using (var sr = new StringReader(xml))
            {
                toolRunParams = (ToolRunParams)serializer.Deserialize(sr);
            }
        }

        public ResultState Run(IImageDisplay image)
        {
            
            return ResultState.OK;
        }
       
    }
}