using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DPVision.Model.ROI
{


    #region Base

    [XmlInclude(typeof(RoiRectangle))]
    [XmlInclude(typeof(RoiCircle))]
    [XmlInclude(typeof(RoiEllipse))]
    [XmlInclude(typeof(RoiRectangleAffine))]
    [XmlInclude(typeof(RoiPoint))]
    [XmlInclude(typeof(RoiSegment))]
    public abstract class RoiBase
    {
        [XmlAttribute]
        public abstract string RoiTypeName { get; }

        public abstract XmlElement SaveToXmlNode(XmlDocument doc,string nodeName);
        

        public abstract void LoadFromXmlNode(XmlElement node);
 


    }
    #endregion


    #region ROI Types
    public class RoiRectangle : RoiBase
    {
        public override string RoiTypeName => "RoiRectangle";
        [XmlAttribute]
        public float X { get; set; }
        [XmlAttribute]
        public float Y { get; set; }
        [XmlAttribute]
        public float Width { get; set; }
        [XmlAttribute]
        public float Height { get; set; }

      
        public RoiRectangle(float x, float y, float width, float height)
        {
            X = x; Y = y; Width = width; Height = height;
        }

        public override XmlElement SaveToXmlNode(XmlDocument doc, string nodeName)
        {
            var node = doc.CreateElement(nodeName);
            node.SetAttribute("X", X.ToString());
            node.SetAttribute("Y", Y.ToString());
            node.SetAttribute("Width", Width.ToString());
            node.SetAttribute("Height", Height.ToString());
   
            return node;
        }

        public override void LoadFromXmlNode(XmlElement node)
        {
            if(node !=null)
            {
                var value=0f;
                if (float.TryParse(node.GetAttribute("X"), out value))
                {
                    this.X = value;
                }
                if (float.TryParse(node.GetAttribute("Y"), out value))
                {
                    this.Y = value;
                }
                if (float.TryParse(node.GetAttribute("Width"), out value))
                {
                    this.Width = value;
                }
                if (float.TryParse(node.GetAttribute("Height"), out value))
                {
                    this.Height = value;
                }
            }
         
        }

    }

    public class RoiRectangleAffine : RoiBase
    {
        public override string RoiTypeName => "RoiRectangleAffine";
        [XmlAttribute]
        public float X { get; set; }
        [XmlAttribute]
        public float Y { get; set; }
        [XmlAttribute]
        public float Width { get; set; }
        [XmlAttribute]
        public float Height { get; set; }
        [XmlAttribute]
        public float Angle { get; set; }
        public RoiRectangleAffine(float x, float y, float width, float height, float angle)
        {
            X = x; Y = y; Width = width; Height = height; Angle = angle;
        }
        public override XmlElement SaveToXmlNode(XmlDocument doc, string nodeName)
        {
            var node = doc.CreateElement(nodeName);
            node.SetAttribute("X", X.ToString());
            node.SetAttribute("Y", Y.ToString());
            node.SetAttribute("Width", Width.ToString());
            node.SetAttribute("Height", Height.ToString());
            node.SetAttribute("Angle", Angle.ToString());
            return node;
        }

        public override void LoadFromXmlNode(XmlElement node)
        {
            if (node != null)
            {
                var value = 0f;
                if (float.TryParse(node.GetAttribute("X"), out value))
                {
                    this.X = value;
                }
                if (float.TryParse(node.GetAttribute("Y"), out value))
                {
                    this.Y = value;
                }
                if (float.TryParse(node.GetAttribute("Width"), out value))
                {
                    this.Width = value;
                }
                if (float.TryParse(node.GetAttribute("Height"), out value))
                {
                    this.Height = value;
                }
                if (float.TryParse(node.GetAttribute("Angle"), out value))
                {
                    this.Angle = value;
                }
            }

        }
    }

    public class RoiCircle : RoiBase
    {
        public override string RoiTypeName => "RoiCircle";
       
        [XmlAttribute]
        public float X { get; set; }
        [XmlAttribute]
        public float Y { get; set; }
        [XmlAttribute]
        public float Radius { get; set; }
      
        public RoiCircle() { }
        public RoiCircle(float x, float y, float radius)
        {
            X = x; Y = y; Radius = radius;
        }
        public override XmlElement SaveToXmlNode(XmlDocument doc, string nodeName)
        {
            var node = doc.CreateElement(nodeName);
            node.SetAttribute("X", X.ToString());
            node.SetAttribute("Y", Y.ToString());
            node.SetAttribute("Radius", Radius.ToString());
          
            return node;
        }

        public override void LoadFromXmlNode(XmlElement node)
        {
            if (node != null)
            {
                var value = 0f;
                if (float.TryParse(node.GetAttribute("X"), out value))
                {
                    this.X = value;
                }
                if (float.TryParse(node.GetAttribute("Y"), out value))
                {
                    this.Y = value;
                }
                if (float.TryParse(node.GetAttribute("Radius"), out value))
                {
                    this.Radius = value;
                }
            }

        }

    }


    public class RoiPoint : RoiBase
    {
        public override string RoiTypeName => "RoiPoint";

        [XmlAttribute]
        public float X { get; set; }
        [XmlAttribute]
        public float Y { get; set; }
   
        public RoiPoint(float x, float y)
        {
            X = x; Y = y;
        }
        public override XmlElement SaveToXmlNode(XmlDocument doc, string nodeName)
        {
            var node = doc.CreateElement(nodeName);
            node.SetAttribute("X", X.ToString());
            node.SetAttribute("Y", Y.ToString());
      
            return node;
        }

        public override void LoadFromXmlNode(XmlElement node)
        {
            if (node != null)
            {
                var value = 0f;
                if (float.TryParse(node.GetAttribute("X"), out value))
                {
                    this.X = value;
                }
                if (float.TryParse(node.GetAttribute("Y"), out value))
                {
                    this.Y = value;
                }
               
            }

        }

    }


    public class RoiSegment : RoiBase
    {
        public override string RoiTypeName => "RoiSegment";
      
        [XmlAttribute]
        public float X1 { get; set; }
        [XmlAttribute]
        public float Y1{ get; set; }
        [XmlAttribute]
        public float X2 { get; set; }
        [XmlAttribute]
        public float Y2 { get; set; }
        RoiSegment(float x1, float y1,float x2, float y2)
        {
            X1 = x1; Y1 = y1; X2 = x2; Y2 = y2;
        }
        public override XmlElement SaveToXmlNode(XmlDocument doc, string nodeName)
        {
            var node = doc.CreateElement(nodeName);
            node.SetAttribute("X1", X1.ToString());
            node.SetAttribute("Y1", Y1.ToString());
            node.SetAttribute("X2", X2.ToString());
            node.SetAttribute("Y2", Y2.ToString());
          
            return node;
        }

        public override void LoadFromXmlNode(XmlElement node)
        {
            if (node != null)
            {
                var value = 0f;
                if (float.TryParse(node.GetAttribute("X1"), out value))
                {
                    this.X1 = value;
                }
                if (float.TryParse(node.GetAttribute("Y1"), out value))
                {
                    this.Y1 = value;
                }
                if (float.TryParse(node.GetAttribute("X2"), out value))
                {
                    this.X2 = value;
                }
                if (float.TryParse(node.GetAttribute("Y2"), out value))
                {
                    this.Y2 = value;
                }
              
            }
        }
        }

  
    public class RoiEllipse : RoiBase
    {
        public override string RoiTypeName => "RoiEllipse";
     
    [XmlAttribute]
    public float X { get; set; }
    [XmlAttribute]
    public float Y { get; set; }
    [XmlAttribute]
    public float Radius1 { get; set; }
    [XmlAttribute]
    public float Radius2 { get; set; }
    [XmlAttribute]
    public float Angle { get; set; }

        public RoiEllipse(float x, float y, float radius1, float radius2, float angle)
        {
            X = x; Y = y; Radius1 = radius1; Radius2 = radius2; Angle = angle;
        }

        public override XmlElement SaveToXmlNode(XmlDocument doc, string nodeName)
        {
            var node = doc.CreateElement(nodeName);
            node.SetAttribute("X", X.ToString());
            node.SetAttribute("Y", Y.ToString());
            node.SetAttribute("Radius1", Radius1.ToString());
            node.SetAttribute("Radius2", Radius2.ToString());
            node.SetAttribute("Angle", Angle.ToString());
            return node;
        }

        public override void LoadFromXmlNode(XmlElement node)
        {
            if (node != null)
            {
                var value = 0f;
                if (float.TryParse(node.GetAttribute("X"), out value))
                {
                    this.X = value;
                }
                if (float.TryParse(node.GetAttribute("Y"), out value))
                {
                    this.Y = value;
                }
                if (float.TryParse(node.GetAttribute("Radius1"), out value))
                {
                    this.Radius1 = value;
                }
                if (float.TryParse(node.GetAttribute("Radius2"), out value))
                {
                    this.Radius2 = value;
                }
                if (float.TryParse(node.GetAttribute("Angle"), out value))
                {
                    this.Angle = value;
                }
            }
        }
    }
    #endregion

 

    public class RoiText
    {
        public string Text { get; set; }
        public Point Position { get; set; }   // 文本左上角
        public double FontSize { get; set; } = 16;
        public Color Color { get; set; } = Color.Red;
        public string FontFamily { get; set; } = "Arial";
    }

}
