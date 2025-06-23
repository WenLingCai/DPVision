
using DPVision.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPVision.Cameras._HK
{
    public class HKJob
    {
        //图像采集
        public float fExposureTime = 3000;
        public float fGain = 10;
        public TriggerMode CameraTriggerMode = TriggerMode.SoftWare;
        public bool bRotate = false;
        public RotateAangle CameraRotateAngle = RotateAangle.R90;
        public int iFrameTimeout = 3000;
        
        public string sCameraIP = "192.168.1.66";
        public AuxiliaryLineModel auxiliaryLineModel=new AuxiliaryLineModel();




        public JObject SaveFile()
        {
            JObject jObject = new JObject();

            jObject.Add("fGain", this.fGain);
            jObject.Add("fExposureTime", this.fExposureTime);
            jObject.Add("bRotate", this.bRotate);
            jObject.Add("CameraRotateAngle", (int)this.CameraRotateAngle);
            jObject.Add("sCameraIP", this.sCameraIP);
            jObject.Add("iFrameTimeout", this.iFrameTimeout);
            jObject.Add("CameraTriggerMode", (int)this.CameraTriggerMode);


            JObject obj = new JObject();
            obj.Add("bDrawCrossFlg", auxiliaryLineModel.bDrawCrossFlg);
            obj.Add("iCrossWidth", auxiliaryLineModel.iCrossWidth);
            obj.Add("CrossColor", ColorTranslator.ToHtml(auxiliaryLineModel.CrossColor));
            obj.Add("fPointSize", auxiliaryLineModel.fPointSize);
            obj.Add("fFontSize", auxiliaryLineModel.fFontSize);
            obj.Add("fMMInterval", auxiliaryLineModel.fMMInterval);
            obj.Add("iGetMMPerPixType", auxiliaryLineModel.iGetMMPerPixType);
            obj.Add("fMMPerPix", auxiliaryLineModel.fMMPerPix);
            obj.Add("bDrawCircleFlg", auxiliaryLineModel.bDrawCircleFlg);
            obj.Add("fCirR1", auxiliaryLineModel.fCirR1);
            obj.Add("fCirR2", auxiliaryLineModel.fCirR2);
            obj.Add("bDrawRectangleFlg", auxiliaryLineModel.bDrawRectangleFlg);
            obj.Add("fRecW", auxiliaryLineModel.fRecW);
            obj.Add("fRecH", auxiliaryLineModel.fRecH);

            jObject.Add("auxiliaryLineModel", obj);


            return jObject;
        }

        public void LoadFile(JObject jObject)
        {
            if (jObject == null)
            {
                return;
            }

            if (jObject["fGain"] != null)
            {
                this.fGain = (float)jObject["fGain"];
            }
            if (jObject["fExposureTime"] != null)
            {
                this.fExposureTime = (float)jObject["fExposureTime"];
            }
            if (jObject["bRotate"] != null)
            {
                this.bRotate = (bool)jObject["bRotate"];
            }
            if (jObject["CameraRotateAngle"] != null)
            {
                this.CameraRotateAngle = (RotateAangle)(int)jObject["CameraRotateAngle"];
            }
            if (jObject["sCameraIP"] != null)
            {
                this.sCameraIP = (string)jObject["sCameraIP"];
            }
            if (jObject["iFrameTimeout"] != null)
            {
                this.iFrameTimeout = (int)jObject["iFrameTimeout"];
            }
            if (jObject["CameraTriggerMode"] != null)
            {
                this.CameraTriggerMode = (TriggerMode)(int)jObject["CameraTriggerMode"];
            }

            if(jObject["auxiliaryLineModel"]!=null)
            {
                JObject obj = jObject["auxiliaryLineModel"] as JObject;
                if (obj["bDrawCrossFlg"] != null)
                {
                    auxiliaryLineModel.bDrawCrossFlg = (bool)obj["bDrawCrossFlg"];
                }
                if (obj["iCrossWidth"] != null)
                {
                    auxiliaryLineModel.iCrossWidth = (int)obj["iCrossWidth"];
                }
                if (obj["CrossColor"] != null)
                {
                    auxiliaryLineModel.CrossColor = ColorTranslator.FromHtml((string)obj["colorOK"]);
                }
                if (obj["fPointSize"] != null)
                {
                    auxiliaryLineModel.fPointSize = (float)obj["fPointSize"];
                }
                if (obj["fFontSize"] != null)
                {
                    auxiliaryLineModel.fFontSize = (float)obj["fFontSize"];
                }
                if (obj["fMMInterval"] != null)
                {
                    auxiliaryLineModel.fMMInterval = (float)obj["fMMInterval"];
                }
                if (obj["bDrawCircleFlg"] != null)
                {
                    auxiliaryLineModel.bDrawCircleFlg = (bool)obj["bDrawCircleFlg"];
                }
                if (obj["fCirR1"] != null)
                {
                    auxiliaryLineModel.fCirR1 = (float)obj["fCirR1"];
                }
                if (obj["fCirR2"] != null)
                {
                    auxiliaryLineModel.fCirR2 = (float)obj["fCirR2"];
                }
                if (obj["bDrawRectangleFlg"] != null)
                {
                    auxiliaryLineModel.bDrawRectangleFlg = (bool)obj["bDrawRectangleFlg"];
                }
                if (obj["fRecW"] != null)
                {
                    auxiliaryLineModel.fRecW = (float)obj["fRecW"];
                }
                if (obj["fRecH"] != null)
                {
                    auxiliaryLineModel.fRecH = (float)obj["fRecH"];
                }
            }
        }
    }
}
