
using System;
using System.Collections.Generic;
using System.Text;

namespace DPVision.Model
{
    public delegate void IMMessageHandler(object sender, IMMessageSet messages);
    public sealed class IMMessageSet
    {
        public IMMessageSet(string id, IMMessage[] messages)
        {
            Msgs = messages;
            ID = id;

        }
        private IMMessage[] Msgs = null;
        public string ID { get; private set; }
        public int Count
        {
            get
            {
                if (Msgs == null)
                    return 0;
                else
                    return Msgs.Length;
            }
        }
        public IMMessage Get(int nIndex)
        {
            if (Msgs == null || nIndex < 0 || nIndex >= Msgs.Length)
                return null;
            return Msgs[nIndex];

        }
    }
    public enum IMMessageType
    {
        Surface,
        Image,
        Profile,
        Point
    }
    public abstract class IMMessage
    {
        public IMMessage()
        {

        }
        public IMMessage(string id)
        {
            ID = id;
        }
        public string ID { get; protected set; }
        public IMMessageType MessageType { get; protected set; }
    }


    public class IMImageMessage : IMMessage
    {

        public IMImageMessage(ContextBase context) : base()
        {
            Context = context;
            MessageType = IMMessageType.Image;
            SetIDImage(context);
        }
        public IMImageMessage(VisionImage IDimage) : base()
        {
            this.IDImage = IDimage;
            MessageType = IMMessageType.Image;
        }
        public IMImageMessage(VisionImage IDimage, ContextBase context) : base()
        {
            this.IDImage=IDimage;
            Context = context;
            MessageType = IMMessageType.Image;
        }
        private VisionImage IDImage;

        public VisionImage GetIDImage()
        {
            return IDImage;
        }
      
        private ContextBase Context;


        public ContextBase GetContext()
        {
            return Context;
        }
        public void SetIDImage(ContextBase inContext)
        {
            IDImage = new VisionImage();
            this.IDImage.SetImage(inContext.dataIntensity);
            this.IDImage.xOffset = (float)inContext.xOffset;
            this.IDImage.yOffset = (float)inContext.yOffset;
            this.IDImage.fScale = (float)inContext.dScale;
          
        }
    }


    public class ContextBase
    {

        public byte[] dataIntensity = null;
        public int iImageWidth;
        public int iImageHeight;
        public int iImagePointCount;

        public int Width
        {
            get
            {
 
                    return iImageWidth;
          
            }
            set
            {
          
                    iImageWidth = value;
        
            }
        }

        public int Height
        {
            get
            {
      
                    return iImageHeight;
    
            }
            set
            {

                    iImageHeight = value;
       
            }
        }


        public int PointCount
        {
            get
            {
 
                    return iImagePointCount;
                
              
            }
            set
            {

                    iImagePointCount = value;
          
            }
        }
        public double xResolution;
        public double yResolution;

        public double xOffset;
        public double yOffset;

        public double dScale = 1;


        public float point3D = 0;
        public float GetPointData()
        {
            return point3D;
        }
        public float[] dataProfile = null;
        public float[] GetProfileData()
        {
            return dataProfile;
        }
        public byte[] GetIntensityData()
        {
            return dataIntensity;
        }
     
        public ContextBase()
        {
          
        }

        public ContextBase(ContextBase item)
        {
           
            this.dataIntensity = item.dataIntensity;
            this.iImageWidth = item.iImageWidth;
            this.iImageHeight = item.iImageHeight;
            this.iImagePointCount = item.iImagePointCount;
            this.xResolution = item.xResolution;
            this.yResolution = item.yResolution;

            this.xOffset = item.xOffset;
            this.yOffset = item.yOffset;
     
      
        }
    }
    public enum ResultState
    {
        None,
        OK,
        NG,
        Error
    }

    public enum ConfigState
    {
        OK,
        Cancel
    }


    
}
