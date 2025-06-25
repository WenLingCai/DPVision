using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DPVision.Model.ROI
{


    #region Base

    public abstract class RoiBase
    {
        /// <summary>
        /// 返回当前ROI类型名，等于节点名
        /// </summary>
        public virtual string RoiType
        {
            get { return GetType().Name; }
        }

        /// <summary>
        /// 序列化为XmlElement。节点名就是类型名（如RoiCircle）。
        /// </summary>
        public abstract XmlElement ToXml(XmlDocument doc);

        /// <summary>
        /// 统一反序列化工厂。根据节点名自动构造对应Roi对象。
        /// </summary>
        public static RoiBase FromXml(XmlElement elem)
        {
            if (elem == null) throw new ArgumentNullException("elem");
            string type = elem.Name;
            if (type == "RoiRectangle")
                return RoiRectangle.FromXml(elem);
            else if (type == "RoiRectangleAffine")
                return RoiRectangleAffine.FromXml(elem);
            else if (type == "RoiCircle")
                return RoiCircle.FromXml(elem);
            else if (type == "RoiPoint")
                return RoiPoint.FromXml(elem);
            else if (type == "RoiSegment")
                return RoiSegment.FromXml(elem);
            else if (type == "RoiEllipse")
                return RoiEllipse.FromXml(elem);
            else
                throw new NotSupportedException("Unknown ROI type: " + type);
        }
    }
    #endregion


    #region ROI Types
    public class RoiRectangle : RoiBase
    {
        public override string RoiType => "RoiRectangle";

        public float X { get; set; }
    
        public float Y { get; set; }
  
        public float Width { get; set; }
    
        public float Height { get; set; }

        public RoiRectangle()
        {

        }
        public RoiRectangle(float x, float y, float width, float height)
        {
            X = x; Y = y; Width = width; Height = height;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement("RoiRectangle");
            node.SetAttribute("X", X.ToString());
            node.SetAttribute("Y", Y.ToString());
            node.SetAttribute("Width", Width.ToString());
            node.SetAttribute("Height", Height.ToString());
            return node;
        }

        public static RoiRectangle FromXml(XmlElement elem)
        {
            RoiRectangle roi = new RoiRectangle();
            float value;
            if (float.TryParse(elem.GetAttribute("X"), out value)) roi.X = value;
            if (float.TryParse(elem.GetAttribute("Y"), out value)) roi.Y = value;
            if (float.TryParse(elem.GetAttribute("Width"), out value)) roi.Width = value;
            if (float.TryParse(elem.GetAttribute("Height"), out value)) roi.Height = value;
            return roi;
        }

    }

    public class RoiRectangleAffine : RoiBase
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Angle { get; set; }

        public RoiRectangleAffine() { }
        public RoiRectangleAffine(float x, float y, float width, float height, float angle)
        {
            X = x; Y = y; Width = width; Height = height; Angle = angle;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement("RoiRectangleAffine");
            node.SetAttribute("X", X.ToString());
            node.SetAttribute("Y", Y.ToString());
            node.SetAttribute("Width", Width.ToString());
            node.SetAttribute("Height", Height.ToString());
            node.SetAttribute("Angle", Angle.ToString());
            return node;
        }

        public static RoiRectangleAffine FromXml(XmlElement elem)
        {
            RoiRectangleAffine roi = new RoiRectangleAffine();
            float value;
            if (float.TryParse(elem.GetAttribute("X"), out value)) roi.X = value;
            if (float.TryParse(elem.GetAttribute("Y"), out value)) roi.Y = value;
            if (float.TryParse(elem.GetAttribute("Width"), out value)) roi.Width = value;
            if (float.TryParse(elem.GetAttribute("Height"), out value)) roi.Height = value;
            if (float.TryParse(elem.GetAttribute("Angle"), out value)) roi.Angle = value;
            return roi;
        }
    }

    public class RoiCircle : RoiBase
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Radius { get; set; }

        public RoiCircle() { }
        public RoiCircle(float x, float y, float radius)
        {
            X = x; Y = y; Radius = radius;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement("RoiCircle");
            node.SetAttribute("X", X.ToString());
            node.SetAttribute("Y", Y.ToString());
            node.SetAttribute("Radius", Radius.ToString());
            return node;
        }

        public static RoiCircle FromXml(XmlElement elem)
        {
            RoiCircle roi = new RoiCircle();
            float value;
            if (float.TryParse(elem.GetAttribute("X"), out value)) roi.X = value;
            if (float.TryParse(elem.GetAttribute("Y"), out value)) roi.Y = value;
            if (float.TryParse(elem.GetAttribute("Radius"), out value)) roi.Radius = value;
            return roi;
        }
    }

    public class RoiPoint : RoiBase
    {
        public float X { get; set; }
        public float Y { get; set; }

        public RoiPoint() { }
        public RoiPoint(float x, float y)
        {
            X = x; Y = y;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement("RoiPoint");
            node.SetAttribute("X", X.ToString());
            node.SetAttribute("Y", Y.ToString());
            return node;
        }

        public static RoiPoint FromXml(XmlElement elem)
        {
            RoiPoint roi = new RoiPoint();
            float value;
            if (float.TryParse(elem.GetAttribute("X"), out value)) roi.X = value;
            if (float.TryParse(elem.GetAttribute("Y"), out value)) roi.Y = value;
            return roi;
        }
    }

    public class RoiSegment : RoiBase
    {
        public float X1 { get; set; }
        public float Y1 { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }

        public RoiSegment() { }
        public RoiSegment(float x1, float y1, float x2, float y2)
        {
            X1 = x1; Y1 = y1; X2 = x2; Y2 = y2;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement("RoiSegment");
            node.SetAttribute("X1", X1.ToString());
            node.SetAttribute("Y1", Y1.ToString());
            node.SetAttribute("X2", X2.ToString());
            node.SetAttribute("Y2", Y2.ToString());
            return node;
        }

        public static RoiSegment FromXml(XmlElement elem)
        {
            RoiSegment roi = new RoiSegment();
            float value;
            if (float.TryParse(elem.GetAttribute("X1"), out value)) roi.X1 = value;
            if (float.TryParse(elem.GetAttribute("Y1"), out value)) roi.Y1 = value;
            if (float.TryParse(elem.GetAttribute("X2"), out value)) roi.X2 = value;
            if (float.TryParse(elem.GetAttribute("Y2"), out value)) roi.Y2 = value;
            return roi;
        }
    }


    public class RoiEllipse : RoiBase
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Radius1 { get; set; }
        public float Radius2 { get; set; }
        public float Angle { get; set; }

        public RoiEllipse() { }
        public RoiEllipse(float x, float y, float radius1, float radius2, float angle)
        {
            X = x; Y = y; Radius1 = radius1; Radius2 = radius2; Angle = angle;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement("RoiEllipse");
            node.SetAttribute("X", X.ToString());
            node.SetAttribute("Y", Y.ToString());
            node.SetAttribute("Radius1", Radius1.ToString());
            node.SetAttribute("Radius2", Radius2.ToString());
            node.SetAttribute("Angle", Angle.ToString());
            return node;
        }

        public static RoiEllipse FromXml(XmlElement elem)
        {
            RoiEllipse roi = new RoiEllipse();
            float value;
            if (float.TryParse(elem.GetAttribute("X"), out value)) roi.X = value;
            if (float.TryParse(elem.GetAttribute("Y"), out value)) roi.Y = value;
            if (float.TryParse(elem.GetAttribute("Radius1"), out value)) roi.Radius1 = value;
            if (float.TryParse(elem.GetAttribute("Radius2"), out value)) roi.Radius2 = value;
            if (float.TryParse(elem.GetAttribute("Angle"), out value)) roi.Angle = value;
            return roi;
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
