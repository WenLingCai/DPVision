using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace DPVision.Model.Tool
{
    

    public class ToolNode
    {
        [XmlAttribute] 
        public string ToolType { get; set; }
        [XmlElement("Params")]
        public ToolParamsBase Params { get; set; }
    }
}
