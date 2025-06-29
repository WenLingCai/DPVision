
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using DPVision.Core;
using MvCameraControl;
using DPVision.Model.Camera;
using DPVision.Model;
using DPVisionLog;
using System.Windows.Media.Media3D;
using System.Windows;

namespace DPVision.Cameras._HK
{

    public class HKCamera : AbstractCamera
    {
        private const string CameraType = "HK";
        internal float frameRate = 10;
        internal int frameCount;
 
        public HKJob hk_job = new HKJob();
        public VisionMatrix matrixAffine2D;
        public HKCamera(string id) : base(id)
        {
            this.ID = id;
        }

        public override VisionMatrix GetCaliMatrix()
        {
            return matrixAffine2D;
        }


       

        public CameraState CameraState = CameraState.Disconnect;
        public override string GetCameraType()
        {
            return CameraType;
        }

        public override CameraState GetCameraState()
        {
            return CameraState;
        }

        /// <summary>
        /// 2d图像预处理
        /// </summary>
        /// <param name="image"></param>
        /// <param name="processImage"></param>
        public void preProcessImage()
        {
            if (this.hk_job.bRotate)
            {
                float angle = 0;
                RotateAangle rotateRangle = this.hk_job.CameraRotateAngle;
                switch (rotateRangle)
                {
                    case RotateAangle.R90:
                        angle = 90;
                        break;
                    case RotateAangle.R180:
                        angle = 180;
                        break;
                    case RotateAangle.R270:
                        angle = 270;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                
            }
         
        }

        MyCamera.MV_CC_DEVICE_INFO_LIST m_stDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
        MyCamera myCamera;
        //PixelFormat m_bitmapPixelFormat = PixelFormat.DontCare;
        IntPtr m_ConvertDstBuf = IntPtr.Zero;
        UInt32 m_nConvertDstBufLen = 0;
        MyCamera.MV_FRAME_OUT_INFO_EX m_stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
        private Object BufForDriverLock = new Object();
        private Action<VisionImage> CallbackFunc = null;

        #region 获取相机ip列表 此处只考虑网口
        public List<string> GetCameraDeivceList()
        {
            m_stDeviceList.nDeviceNum = 0;
            var cameras = new List<string>();
            int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_stDeviceList);
            if (nRet != MyCamera.MV_OK)
            {
                //LogHelp.Instance.Error($"Enumerate devices fail!  ErrorCode:{nRet}");
                return cameras;
            }
            // 遍历设备列表
            for (int i = 0; i < m_stDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                // 仅处理GigE（网口）设备
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    // 解析IP地址（32位整型转字符串）
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    uint ipValue = gigeInfo.nCurrentIp;
                    string deviceIp = $"{(ipValue & 0xFF)}.{(ipValue >> 8) & 0xFF}.{(ipValue >> 16) & 0xFF}.{(ipValue >> 24) & 0xFF}";
                    cameras.Add(deviceIp);
                  
                }
            }
            return cameras;
        }
        #endregion

        #region Connect
        public override int Connect()
        {
            try
            {
                if(CameraState != CameraState.Connect)
                {
                    List<string> cameras_ip = GetCameraDeivceList();
                    if (m_stDeviceList.nDeviceNum == 0)
                    {
                        //LogHelp.Instance.Error("No device, please select");
                        return ErrorType.Failure;
                    }

                    bool isFind = false;
                    for (int i = 0; i < cameras_ip.Count; i++)
                    {
                        if (cameras_ip[i].Equals(this.hk_job.sCameraIP))
                        {
                            isFind = true;
                            break;
                        }
                    }

                    if (!isFind)
                    {
                        //LogHelp.Instance.Error("No device, please select");
                        return ErrorType.Failure;
                    }

                    // ch:打开设备 | en:Open device
                    if (null == myCamera)
                    {
                        myCamera = new MyCamera();
                        if (null == myCamera)
                        {
                            //LogHelp.Instance.Error("Applying resource fail!");
                            return ErrorType.Failure;
                        }
                    }
                    MyCamera.MV_CC_DEVICE_INFO device = new MyCamera.MV_CC_DEVICE_INFO();
                    int nRet = myCamera.MV_CC_CreateDevice_NET(ref device);
                    if (MyCamera.MV_OK != nRet)
                    {
                        //LogHelp.Instance.Error("Create device fail!");
                        return ErrorType.Failure;
                    }

                    nRet = myCamera.MV_CC_OpenDevice_NET();
                    if (MyCamera.MV_OK != nRet)
                    {
                        myCamera.MV_CC_DestroyDevice_NET();
                        //LogHelp.Instance.Error("Device open fail!");
                        return ErrorType.Failure;
                    }

                    // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
                    if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                    {
                        int nPacketSize = myCamera.MV_CC_GetOptimalPacketSize_NET();
                        if (nPacketSize > 0)
                        {
                            nRet = myCamera.MV_CC_SetIntValueEx_NET("GevSCPSPacketSize", nPacketSize);
                            if (nRet != MyCamera.MV_OK)
                            {
                                //LogHelp.Instance.Error("Set Packet Size failed!");
                            }
                        }
                        else
                        {
                            // LogHelp.Instance.Error("Get Packet Size failed!");
                        }
                    }

                    //3-60s
                    if (MyCamera.MV_OK != myCamera.MV_CC_SetIntValue_NET("GevHeartbeatTimeout", (uint)this.hk_job.iFrameTimeout))
                    {
                        // LogHelp.Instance.Error($"Set GevHeartbeatTimeout failed:{nRet:x8}");
                    }

                    // ch:设置采集连续模式 | en:Set Continues Aquisition Mode
                    myCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
                    myCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
                    myCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);

                    // ch:前置配置 | en:pre-operation
                    nRet = NecessaryOperBeforeGrab();
                    if (MyCamera.MV_OK != nRet)
                    {
                        return ErrorType.Failure;
                    }
                    m_stFrameInfo.nFrameLen = 0;//取流之前先清除帧长度
                    m_stFrameInfo.enPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Undefined;


                    CameraState = CameraState.Connect;
                }
                return ErrorType.Success;

            }
            catch (Exception err)
            {
                CameraState = CameraState.Disconnect;
                return ErrorType.Failure;
            }
        }
        #endregion

        #region Disconnect
        public override int Disconnect()
        {
            try
            {
                //停止采集
                myCamera?.MV_CC_StopGrabbing_NET();
                // ch:关闭设备 | en:Close Device
                myCamera?.MV_CC_CloseDevice_NET();
                myCamera?.MV_CC_DestroyDevice_NET();
                CameraState = CameraState.Disconnect;
                return ErrorType.Success;
            }
            catch
            {
                return ErrorType.Failure;
            }

        }
        #endregion

        #region stop
        public override int Stop()
        {

            Thread.Sleep(20);
            this.CameraState = CameraState.Stop;

            return ErrorType.Success;

        }
        #endregion

        #region SetExporse
        public override bool SetExporse(float value)
        {
            try
            {
                myCamera.MV_CC_SetEnumValue_NET("ExposureAuto", 0);
                int nRet = myCamera.MV_CC_SetFloatValue_NET("ExposureTime", value);
                if (nRet != MyCamera.MV_OK)
                {
                    //LogHelp.Instance.Error("Set Exposure Time Fail!");
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }


        #endregion

        #region SetGain
        public override bool SetGain(float value)
        {
            try
            {
                myCamera.MV_CC_SetEnumValue_NET("GainAuto", 0);
                int nRet = myCamera.MV_CC_SetFloatValue_NET("Gain", value);
                if (nRet != MyCamera.MV_OK)
                {
                    //LogHelp.Instance.Error("Set Gain Fail!");
                    return false;
                }

                return true;
            }
            catch
            {

            }
            return true;
        }
        #endregion

        #region NecessaryOperBeforeGrab
        /// <summary>
        /// ch:取图前的必要操作步骤 | en:Necessary operation before grab
        /// </summary>
        private Int32 NecessaryOperBeforeGrab()
        {
            // ch:取图像宽 | en:Get Iamge Width
            MyCamera.MVCC_INTVALUE_EX stWidth = new MyCamera.MVCC_INTVALUE_EX();
            int nRet = myCamera.MV_CC_GetIntValueEx_NET("Width", ref stWidth);
            if (MyCamera.MV_OK != nRet)
            {
                //LogHelp.Instance.Error("Get Width Info Fail!");
                return nRet;
            }
            // ch:取图像高 | en:Get Iamge Height
            MyCamera.MVCC_INTVALUE_EX stHeight = new MyCamera.MVCC_INTVALUE_EX();
            nRet = myCamera.MV_CC_GetIntValueEx_NET("Height", ref stHeight);
            if (MyCamera.MV_OK != nRet)
            {
                //LogHelp.Instance.Error("Get Height Info Fail!");
                return nRet;
            }
            // ch:取像素格式 | en:Get Pixel Format
            MyCamera.MVCC_ENUMVALUE stPixelFormat = new MyCamera.MVCC_ENUMVALUE();
            nRet = myCamera.MV_CC_GetEnumValue_NET("PixelFormat", ref stPixelFormat);
            if (MyCamera.MV_OK != nRet)
            {
                //LogHelp.Instance.Error("Get Pixel Format Fail!");
                return nRet;
            }

            // ch:设置bitmap像素格式，申请相应大小内存 | en:Set Bitmap Pixel Format, alloc memory
            if ((Int32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Undefined == stPixelFormat.nCurValue)
            {
                //LogHelp.Instance.Error("Unknown Pixel Format!");
                return MyCamera.MV_E_UNKNOW;
            }
            else if (IsMono(stPixelFormat.nCurValue))
            {
                //m_bitmapPixelFormat = PixelFormat.Format8bppIndexed;

                if (IntPtr.Zero != m_ConvertDstBuf)
                {
                    //Marshal.Release(m_ConvertDstBuf);
                    m_ConvertDstBuf = IntPtr.Zero;
                }

                // Mono8为单通道
                m_nConvertDstBufLen = (UInt32)(stWidth.nCurValue * stHeight.nCurValue);
                m_ConvertDstBuf = Marshal.AllocHGlobal((Int32)m_nConvertDstBufLen);
                if (IntPtr.Zero == m_ConvertDstBuf)
                {
                    //LogHelp.Instance.Error("Malloc Memory Fail!");
                    return MyCamera.MV_E_RESOURCE;
                }
            }
            else
            {
                //m_bitmapPixelFormat = PixelFormat.Format24bppRgb;

                if (IntPtr.Zero != m_ConvertDstBuf)
                {
                    Marshal.FreeHGlobal(m_ConvertDstBuf);
                    m_ConvertDstBuf = IntPtr.Zero;
                }

                // RGB为三通道
                m_nConvertDstBufLen = (UInt32)(3 * stWidth.nCurValue * stHeight.nCurValue);
                m_ConvertDstBuf = Marshal.AllocHGlobal((Int32)m_nConvertDstBufLen);
                if (IntPtr.Zero == m_ConvertDstBuf)
                {
                    // LogHelp.Instance.Error("Malloc Memory Fail!");
                    return MyCamera.MV_E_RESOURCE;
                }
            }

            return MyCamera.MV_OK;
        }
        #endregion

        #region IsMono
        /// <summary>
        /// ch:像素类型是否为Mono格式 | en:If Pixel Type is Mono 
        /// </summary>
        private Boolean IsMono(UInt32 enPixelType)
        {
            switch (enPixelType)
            {
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono1p:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono2p:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono4p:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8_Signed:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono14:
                case (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono16:
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region GetPhotImageData
        private bool GetPhotImageData(out VisionImage data, int waiteTime = 1000, bool isGet = true)
        {
            data = new VisionImage();
            try
            {
                MyCamera.MV_FRAME_OUT stFrameInfo = new MyCamera.MV_FRAME_OUT();
                MyCamera.MV_PIXEL_CONVERT_PARAM stConvertInfo = new MyCamera.MV_PIXEL_CONVERT_PARAM();
                int nRet = MyCamera.MV_OK;
                nRet = myCamera.MV_CC_GetImageBuffer_NET(ref stFrameInfo, waiteTime);
                if (nRet != MyCamera.MV_OK)
                {
                    return false;
                }

                if (isGet
                    && stFrameInfo.stFrameInfo.nFrameLen > 0
                    && stFrameInfo.stFrameInfo.nFrameLen >= (stFrameInfo.stFrameInfo.nWidth * stFrameInfo.stFrameInfo.nHeight))
                {
                    stConvertInfo.nWidth = stFrameInfo.stFrameInfo.nWidth;
                    stConvertInfo.nHeight = stFrameInfo.stFrameInfo.nHeight;
                    stConvertInfo.enSrcPixelType = stFrameInfo.stFrameInfo.enPixelType;
                    stConvertInfo.pSrcData = stFrameInfo.pBufAddr;
                    stConvertInfo.nSrcDataLen = stFrameInfo.stFrameInfo.nFrameLen;
                    stConvertInfo.pDstBuffer = m_ConvertDstBuf;
                    stConvertInfo.nDstBufferSize = m_nConvertDstBufLen;
                    stConvertInfo.enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8;
                    myCamera.MV_CC_ConvertPixelType_NET(ref stConvertInfo);

                    data.ImageData = new byte[stConvertInfo.nWidth * stConvertInfo.nHeight];
                    Marshal.Copy(stConvertInfo.pSrcData, data.ImageData, 0, data.ImageData.Length);
                    data.Width = stConvertInfo.nWidth;
                    data.Height = stConvertInfo.nHeight;

                }
                myCamera.MV_CC_FreeImageBuffer_NET(ref stFrameInfo);

                return true;
            }
            catch (Exception ex)
            {
                //LogHelp.Instance.Exception(ex.Message);
            }

            return false;
        }
        #endregion

        #region TakePhoto
        public override int TakePhoto(out VisionImage data)
        {
            data = new VisionImage();
            // ch:触发命令 | en:Trigger command
            while (GetPhotImageData(out data, 100, false))
            {
                Thread.Sleep(100);
            }

            lock (BufForDriverLock)
            {
                int nRet = myCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
                if (MyCamera.MV_OK != nRet)
                {
                    //LogHelp.Instance.Error("Trigger Software Fail!");
                    myCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
                    return ErrorType.Failure;
                }
            }

            bool ret = GetPhotImageData(out data);
            if(ret)
            {
                return ErrorType.Success;
            }
            return ErrorType.Failure;
        }
        #endregion

        #region ContinusImage
        public override int GrapContinusImage()
        {
            try
            {

            }
            catch (Exception err)
            {

                Disconnect();
                return ErrorType.Failure;
            }
            return ErrorType.Success;

        }
        #endregion

        #region 带全局曝光增益拍照
        public bool GrapVisionImage(bool bSetParam, out VisionImage data)
        {
            data = new VisionImage();
            bool ret = false;
            if (CameraState == CameraState.Disconnect)
            {
                return false;
            }
            VisionImage cameraData = new VisionImage();
            if (bSetParam)
            {
                SetExporse(hk_job.fExposureTime);
                SetGain(hk_job.fGain);
            }
            TakePhoto(out cameraData);
            if (cameraData.ImageData == null)
            {
                DPLog.Instance.Info(this.GetType(), "ImageData=null");
                return false;
            }
            return ret;

        }
        #endregion
    }

    public enum TriggerMode
    {
        Off,
        SoftWare,
        Line
    }

    public enum RotateAangle
    {
        R90 = 0,
        R180 = 1,
        R270 = 2
    }
    public enum Polarity
    {
        Bright,
        Dark,
        Any
    }
}