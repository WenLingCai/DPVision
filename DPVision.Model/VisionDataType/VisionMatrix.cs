
using Newtonsoft.Json.Linq;
using System;
using System.Xml;


namespace DPVision.Model
{
   
    public class VisionMatrix : BaseValue
    {
        public override VisionDataType GetDataType()
        {
            return VisionDataType.Matrix;
        }

        public double xx;
        public double xy;
        public double xz;
        public double yx;
        public double yy;
        public double yz;
        public double zx;
        public double zy;
        public double zz;


        public VisionMatrix(string id="", int lable = 0)
        {
            this.sID = id;
            this.labelNo = lable;
        }
        public void SetMatrix(double[,] data)
        {
            this.xx = data[0, 0];
            this.xy = data[0, 1];
            this.xz = data[0, 2];

            this.yx = data[1, 0];
            this.yy = data[1, 1];
            this.yz = data[1, 2];

            this.zx = data[2, 0];
            this.zy = data[2, 1];
            this.zz = data[2, 2];
        }


        public override XmlElement SaveToXmlNode(XmlDocument doc, string nodeName)
        {
            var node = doc.CreateElement(nodeName);
            node.SetAttribute("xx", xx.ToString());
            node.SetAttribute("xy", xy.ToString());
            node.SetAttribute("xz", xz.ToString());
            node.SetAttribute("yx", yx.ToString());
            node.SetAttribute("yy", yy.ToString());
            node.SetAttribute("yz", yz.ToString());
            node.SetAttribute("zx", zx.ToString());
            node.SetAttribute("zy", zy.ToString());
            node.SetAttribute("zz", zz.ToString());
            return node;
        }

        public override void LoadFromXmlNode(XmlElement node)
        {
            if (node != null)
            {
                var value = 0d;
                if (double.TryParse(node.GetAttribute("xx"), out value))
                {
                    this.xx = value;
                }
                if (double.TryParse(node.GetAttribute("xy"), out value))
                {
                    this.xy = value;
                }
                if (double.TryParse(node.GetAttribute("xz"), out value))
                {
                    this.xz = value;
                }
                if (double.TryParse(node.GetAttribute("yx"), out value))
                {
                    this.yx = value;
                }
                if (double.TryParse(node.GetAttribute("yy"), out value))
                {
                    this.yy = value;
                }
                if (double.TryParse(node.GetAttribute("yz"), out value))
                {
                    this.yz = value;
                }
                if (double.TryParse(node.GetAttribute("zx"), out value))
                {
                    this.zx = value;
                }
                if (double.TryParse(node.GetAttribute("zy"), out value))
                {
                    this.zy = value;
                }
                if (double.TryParse(node.GetAttribute("zz"), out value))
                {
                    this.zz = value;
                }
            }

        }


    }



  
}