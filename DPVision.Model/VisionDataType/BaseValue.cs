
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DPVision.Model
{
    public abstract class BaseValue
    {
        public string sID = "";
        public int labelNo = 0;
        public abstract VisionDataType GetDataType();

        public BaseValue()
        {

        }
        public BaseValue(string floatName)
        {
            this.sID = floatName;
            this.labelNo = 0;
        }

        public BaseValue(BaseValue goFloat)
        {
            this.sID = goFloat.sID;
            this.labelNo = 0;
        }

        public abstract XmlElement SaveToXmlNode(XmlDocument doc, string nodeName);

        public abstract void LoadFromXmlNode(XmlElement node);

        
    }


    public enum VisionDataType
    {
        Undefine,
        Image,    //图像
        Matrix,   //矩阵

        DataBool,
        DataDouble,
        DataFloat,
        DataInt,
        DataLong,
        DataShort,
        DataString,

        FeatureCoordinate,
        FeatureLine2D,
        FeatureLine3D,
        FeaturePlane,
        FeaturePoint2D,
        FeaturePoint3D,
        FeatureRectangle,
        FeatureRegion,
        FeatureSegment2D,
        FeatureSegment3D,
        FeatureArc,
        FeatureCircle,
        FeaturePath,

        DataBoolArray,
        DataDoubleArray,
        DataFloatArray,
        DataIntArray,
        DataLongArray,
        DataShortArray,
        DataStringArray,

        FeatureCoordinateArray,
        FeatureLine2DArray,
        FeatureLine3DArray,
        FeaturePlaneArray,
        FeaturePoint2DArray,
        FeaturePoint3DArray,
        FeatureRectangleArray,
        FeatureRegionArray,
        FeatureSegment2DArray,
        FeatureSegment3DArray,
        FeatureArcArray,
        FeatureCircleArray,
        FeaturePathArray,
        ResultState,

    }
    /// <summary>
    /// 自定义变量数据类型
    /// </summary>
    public enum VisionDataGroup
    {
        Single = 0,           ///单个变量
        Array,               ///数组类型
    }

    public enum PhotoMode
    {
        OneTime,
        Online,
        Cmd
    }
    public enum CameraState
    {
        Connect,
        Disconnect,
        Start,
        Stop
    }


 
}
