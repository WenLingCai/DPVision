
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace DPVision.Model
{
    public class VisionImage: BaseValue
    {
        public override VisionDataType GetDataType()
        {
            return VisionDataType.Image;
        }

     
        public byte[] ImageData;
        public float Width = 0;
        public float Height = 0;
        public float xOffset =0;
        public float yOffset =0;
        public float fScale = 1f;

        public VisionImage()
        {
            this.sID = "";
            this.labelNo = 0;
            this.ImageData = null;
        }

        public void SetImage(byte[] image)
        {
            this.ImageData = image;
        }
      
        public float GetScale()
        {
            return this.fScale;
        }

        public float GetXOffset()
        {
            return this.xOffset;
        }
        public float GetYOffset()
        {
            return this.yOffset;
        }
        public void SetScale(float scale)
        {
            this.fScale = scale;
        }

        public void SetID(string id)
        {
            this.sID = id;
        }
        public void SetLabelNo(int labelNo)
        {
            this.labelNo = labelNo;
        }
        public bool HasData()
        {
            return this.ImageData != null;
        }
        public override XmlElement SaveToXmlNode(XmlDocument doc, string nodeName)
        {

            return null;
        }

        public override void LoadFromXmlNode(XmlElement node)
        {
           
        }
    }

  
 

   

}