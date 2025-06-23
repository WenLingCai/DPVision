
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using Newtonsoft.Json.Linq;

namespace DPVision.Model.Camera
{
    public abstract class AbstractCamera
    {
        public AbstractCamera(string id)
        {
            if (id == string.Empty)
                throw new ArgumentException("传感器id不能为空", "ID");
            ID = id;
        }
        
        /// <summary>
        /// 相机ID
        /// </summary>
        public string ID { get; set; }

      
        /// <summary>
        /// 相机连接
        /// </summary>
        /// <returns></returns>
        public abstract int Connect();

        /// <summary>
        /// 相机断开
        /// </summary>
        /// <returns></returns>
        public abstract int Disconnect();

        /// <summary>
        /// 单次拍照
        /// </summary>
        /// <param name="imagedata"></param>
        /// <returns></returns>
        public abstract int TakePhoto(out VisionImage imagedata);

        /// <summary>
        /// 连续拍照
        /// </summary>
        /// <returns></returns>
        public abstract int GrapContinusImage();
        


        /// <summary>
        /// 停止相机采集
        /// </summary>
        /// <returns></returns>
        public abstract int Stop();

       
        /// <summary>
        /// 获取相机标定矩阵
        /// </summary>
        /// <returns></returns>
        public virtual VisionMatrix GetCaliMatrix()
        {
            return null;
        }
     
        /// <summary>
        /// 获取相机品牌
        /// </summary>
        /// <returns></returns>
        public abstract string GetCameraType();
      
        /// <summary>
        /// 获取相机状态
        /// </summary>
        /// <returns></returns>
        public abstract CameraState GetCameraState();

        /// <summary>
        /// 设置曝光
        /// </summary>
        public abstract bool SetExporse(float value);
        /// <summary>
        /// 设置增益
        /// </summary>
        public abstract bool SetGain(float value);
    

      
   
       
    }
}