using DPVision.Model.ROI;
using DPVision.Model.Tool;
using HalconDotNet;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DPVision.Core
{
    public class DPVisionCore
    {

        public DPVisionCore()
        {
            ToolRegistry.LoadTools("./Plugins/Tools/");
        }
        private static DPVisionCore instance = new DPVisionCore();
        public static DPVisionCore Instance
        {
            get
            {
                return DPVisionCore.instance;
            }
        }
        #region 创建掩膜

        public Mat CreateMask(Size maskSize, List<Rect> drawRects, List<Rect> eraseRects)
        {
            // 1. 创建全黑的mask
            Mat mask = new Mat(maskSize, MatType.CV_8UC1, Scalar.All(0));

            // 2. 先绘制DrawRects区域为255
            foreach (var rect in drawRects)
            {
                Cv2.Rectangle(mask, rect, Scalar.All(255), -1); // -1填充
            }

            // 3. 再擦除EraseRects区域为0
            foreach (var rect in eraseRects)
            {
                Cv2.Rectangle(mask, rect, Scalar.All(0), -1);
            }

            return mask;
        }
        #endregion

        #region 

        // 1. 读取float属性的通用函数
      public float ReadFloatAttr(XmlElement node, string attrName, float defaultValue = 0)
        {
            if (node == null) return defaultValue;
            float value;
            return float.TryParse(node.GetAttribute(attrName), out value) ? value : defaultValue;
        }

        // 2. 读取int属性的通用函数
       public int ReadIntAttr(XmlElement node, string attrName, int defaultValue = 0)
        {
            if (node == null) return defaultValue;
            int value;
            return int.TryParse(node.GetAttribute(attrName), out value) ? value : defaultValue;
        }

        // 3. 读取并加载Roi子节点的通用函数
        public void LoadRoi(XmlElement node, string roiName, RoiBase roiObj)
        {
            var roiNode = node.SelectSingleNode(roiName) as XmlElement;
            if (roiNode != null && roiObj != null)
                roiObj.LoadFromXmlNode(roiNode);
        }


        #endregion
        #region /****静态变量(全局公用变量)****/ 



        public static bool bModeType = false;//传递模板是否用XLD创建

    
      
        //(0)声明事件型委托：“发送测试流程的拍照指令回调”(在运动控制程序中注册，其他任意地方执行)
        public delegate int DelegSendTestAcqCmd(int CcdId, int CapCmd, out string strErrMsg);
        public static event DelegSendTestAcqCmd DelegSendTestAcqCmdEvent;
        /*******功能： 发送用于测试图像处理流程的拍照指令
          输入参数：
          * 参1：哪个相机拍照。（0、1、2...）
          * 参2：拍照指令,第几次拍照=拍照视野Id+1（1、2、3...)
          * 参3：错误返回错误信息描述
          * 返回值： 成功返回0，异常出错返回-1     
          最近更改日期:2020-6-9
       ************************************************/
        public int ExeDelegSendAcqCmdEvent(int CcdId, int CapCmd, out string strErrMsg)
        {
            strErrMsg = null;
            int retureResult = 0;
            if (DelegSendTestAcqCmdEvent != null)
            {
                retureResult = DelegSendTestAcqCmdEvent(CcdId, CapCmd, out strErrMsg);
            }
            else
            {
                strErrMsg = "Error, unregistered function";
                retureResult = -1;
            }
            return retureResult;
        }

        //(1)声明一个适用于所有相机抓图完成后的事件型委托(图像预处理回调)--在图像预处理工具中注册,图像采集工具中执行
        public delegate int DelegImgPreproCallback(int ImgPreproIndex, HObject ho_OldImg, out HObject ho_NewImg, HTuple hv_WindowHandle);//定义类型
        public static event DelegImgPreproCallback DelegImgPreproCallbackEvent;//定义变量,因为不是在注册方和调用方，所以必须定义为静态，起"桥梁"作用
        /********功能： 执行“图像预处理”委托********
         输入参数：
         * 参1 输入输出要处理的图像变量
         * 参2：输入显示图像的窗体句柄
         * 参3：图像预处理对象索引
         * 返回值： 空 
         最近更改日期:2019-9-4
       ************************************************/
        public int ExeDelegImgPreproCallbackEvent(int ImgPreproIndex, HObject ho_OldImg, out HObject ho_NewImg, HTuple hv_WindowHandle)
        {
            int retureResult = 0;
            HOperatorSet.GenEmptyObj(out ho_NewImg);

            if (DelegImgPreproCallbackEvent != null)
            {
                //执行注册的方法,可以多次注册成多播委托链
                ho_NewImg.Dispose();
                retureResult = DelegImgPreproCallbackEvent(ImgPreproIndex, ho_OldImg, out ho_NewImg, hv_WindowHandle);
            }
            else
            {
                //"未注册相机采集回调方法！";
                retureResult = -1;
            }
            return retureResult;
        }
        //(2)声明一个适用于所有相机抓图完成后的事件型委托(在显示窗口上绘制图形信息并显示)--在图像绘制工具中注册，图像采集工具中执行
        public delegate void DelegWinGrapCallback(int WinGrapIndex, HObject ho_Image, HTuple hv_WindowHandle);
        public static event DelegWinGrapCallback DelegWinGrapCallbackEvent;
        /********功能： 执行“窗口上绘制图形”委托********
         输入参数：
         * 参1 输入要处理的图像变量
         * 参2：输入窗体句柄
         * 输3：窗体绘图工具对象的索引
         * 返回值： 空
         最近更改日期:2019-9-4
       ************************************************/
        public void ExeDelegWinGrapCallbackEvent(int WinGrapIndex, HObject ho_Image, HTuple hv_WindowHandle)
        {
            if (DelegWinGrapCallbackEvent != null)
            {
                DelegWinGrapCallbackEvent(WinGrapIndex, ho_Image, hv_WindowHandle);//执行注册的方法,可以多次注册成多播委托链
            }
        }

        //(3)声明一个适用于所有相机抓图完成后的事件型委托(图像处理回调)--在图像处理主流程中注册，图像采集工具中执行
        public delegate void DelegImgProCallback(int CCDIndex, HObject ho_Image, HTuple hv_WindowHandle, ref int iGrabOKNG, ref string strErrMsg);
        public static event DelegImgProCallback DelegImgProCallbackEvent;
        /********功能： 执行“图像处理”委托********
         输入参数：
         * 参1 输入要处理的图像变量
         * 参2：输入显示图像的窗体句柄
         * 参3：输入输出图像采集及图像处理是否OK，0：OK，-1：NG
         * 参4：输入相机对象ID，目的：当多个相机同时调用该公用方法时，要知道是哪个相机调用的。
         * 返回值： 空
         最近更改日期:2019-12-18
       ************************************************/
        public void ExeDelegImgProCallbackEvent(int CCDIndex, HObject ho_Image, HTuple hv_WindowHandle, ref int iGrabOKNG, ref string strErrMsg)
        {
            if (DelegImgProCallbackEvent != null)
            {
                DelegImgProCallbackEvent(CCDIndex, ho_Image, hv_WindowHandle, ref iGrabOKNG, ref strErrMsg);
            }
            else
            {
                iGrabOKNG = -1;
                strErrMsg = "未注册方法！";
            }
        }
        //(4)声明一个适用于所有窗体执行，单次“取像”事件型委托(单次抓拍回调)--在图像采集工具中注册，其他任意地方执行
        public delegate void DelegOnceAcqImgCallback(int CCDIndex, int ExposureGainIndex, bool isCCDCallBackProImgFlg, ref int iGrabOKNG, ref string strErrMsg);
        public static event DelegOnceAcqImgCallback DelegOnceAcqImgCallbackEvent;
        /*****  功能： 执行"单次拍照"委托
         输入参数：
         * 参1： 输入相机索引，哪个相机对象软触发拍照
         * 参2：输入多曝光值索引
         * 参3：取像完成后是否执行图像处理回调函数，true:执行
         * 参4：返回是否成功采集图像；0：OK，-1：ng
         最近更改日期:2019-12-18
       ************************************************/
        public void ExeDelegOnceAcqImgCallbackEvent(int CCDIndex, int ExposureGainIndex, bool isCCDCallBackProImgFlg, ref int iGrabOKNG, ref string strErrMsg)
        {
            if (DelegOnceAcqImgCallbackEvent != null)
            {
                DelegOnceAcqImgCallbackEvent(CCDIndex, ExposureGainIndex, isCCDCallBackProImgFlg, ref iGrabOKNG, ref strErrMsg);//执行注册的方法(多播委托)
            }
            else
            {
                iGrabOKNG = -1;
                strErrMsg = "未注册方法！";
            }
        }

        //(5)声明适用于所有窗体获取指定机械坐标系坐标的异步事件型委托(获取机械坐标回调)--在运动控制程序中注册，其他任意地方执行
        public delegate int DelegGetMachineCoord(int CoordIndex, float[] PosXYZA, out string strErrMsg);
        public static event DelegGetMachineCoord DelegGetMachineCoordEvent;
        /*******功能： 执行"获取机械坐标"委托
          输入参数：
          * 参1 输入机械坐标系(工位)索引，从哪个机械坐标系(工位)获取坐标
          * 参2：输出获取的机械坐标值(数组不需要ref输出)，通常定义元素[0]=X,[1]=Y,[2]=Z,[3]=A,可其他自定义
          * 参3：错误返回错误信息描述
          * 返回值： 成功返回0，异常出错返回-1     
          最近更改日期:2019-6-14
       ************************************************/
        public int ExeDelegGetMachineCoordEvent(int CoordIndex, float[] PosXYZA, out string strErrMsg)
        {
            strErrMsg = null;
            int retureResult = 0;
            if (DelegGetMachineCoordEvent != null)
            {
                retureResult = DelegGetMachineCoordEvent(CoordIndex, PosXYZA, out strErrMsg);
            }
            else
            {
                strErrMsg = "未注册获取机械坐标的回调方法！";
                retureResult = -1;
            }
            return retureResult;
        }

        //(6)声明指定(工位)坐标系运动到指定位置的异步事件型委托(轴运动指定坐标回调)--在运动控制程序中注册，其他任意地方执行
        public delegate int DelegSetGotoMachineCoord(int CoordIndex, float[] PosXYZA, out string strErrMsg);
        public static event DelegSetGotoMachineCoord DelegSetGotoMachineCoordEvent;
        /*******功能： 执行"运动到指定位置"委托
          输入参数：
          * 参1：输入机械坐标系(工位)索引，运动哪个机械坐标系。
          * 参2：输入获取的机械坐标值，通常定义元素[0]=X,[1]=Y,[2]=Z,[3]=A,可其他自定义
          * 参3：错误返回错误信息描述
          * 返回值： 成功返回0，异常出错返回-1     
          最近更改日期:2019-6-14
       ************************************************/
        public int ExeDelegSetGotoMachineCoordEvent(int CoordIndex, float[] PosXYZA, out string strErrMsg)
        {
            strErrMsg = null;
            int retureResult = 0;
            if (DelegSetGotoMachineCoordEvent != null)
            {
                retureResult = DelegSetGotoMachineCoordEvent(CoordIndex, PosXYZA, out strErrMsg);
            }
            else
            {
                strErrMsg = "未注册轴运动到指定位置的回调方法！";
                retureResult = -1;
            }
            return retureResult;
        }

        //(7)声明适用于运动控制来切换触发模式的设置方法
        public delegate void DelegSetTrigger();
        public static event DelegSetTrigger DelegSetTriggerEvent;
        /*******功能： 执行"运动控制来切换触发模式"委托
          输入参数：
          * 参1：
          * 参2：
          * 参3：错误返回错误信息描述
          * 返回值： 成功返回0，异常出错返回-1     
          最近更改日期:2020-7-17
       ************************************************/
        public int ExeDelegSetTriggerExent()
        {
            int retureResult = 0;
            if (DelegSetTriggerEvent != null)
            {
                DelegSetTriggerEvent();

            }
            else
            {
                retureResult = -1;
            }
            return retureResult;
        }

        #endregion




        #region /******HDevelop12算法*********/


        //打印字符串消息(内有短暂改写setPart不可高频使用):拓展了disp_message方法，Row、Col、hv_String元素数一致可以为多元素，
        //hv_Color可为单或|Row|个元素,单元素时对每个字符有效，多元素时对应字符有效
        public void my_disp_message(HTuple hv_WindowHandle, HTuple hv_String, HTuple hv_CoordSystem,
       HTuple hv_Row, HTuple hv_Column, HTuple hv_Color, HTuple hv_Box)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_Red = null, hv_Green = null, hv_Blue = null;
            HTuple hv_Row1Part = null, hv_Column1Part = null, hv_Row2Part = null;
            HTuple hv_Column2Part = null, hv_RowWin = null, hv_ColumnWin = null;
            HTuple hv_WidthWin = null, hv_HeightWin = null, hv_MaxAscent = null;
            HTuple hv_MaxDescent = null, hv_MaxWidth = null, hv_MaxHeight = null;
            HTuple hv_R1 = new HTuple(), hv_C1 = new HTuple(), hv_FactorRow = new HTuple();
            HTuple hv_FactorColumn = new HTuple(), hv_UseShadow = null;
            HTuple hv_ShadowColor = null, hv_Exception = new HTuple();
            HTuple hv_Width = new HTuple(), hv_Index = new HTuple();
            HTuple hv_Ascent = new HTuple(), hv_Descent = new HTuple();
            HTuple hv_W = new HTuple(), hv_H = new HTuple(), hv_FrameHeight = new HTuple();
            HTuple hv_FrameWidth = new HTuple(), hv_R2 = new HTuple();
            HTuple hv_C2 = new HTuple(), hv_DrawMode = new HTuple();
            HTuple hv_CurrentColor = new HTuple();
            HTuple hv_Box_COPY_INP_TMP = hv_Box.Clone();
            HTuple hv_Color_COPY_INP_TMP = hv_Color.Clone();
            HTuple hv_Column_COPY_INP_TMP = hv_Column.Clone();
            HTuple hv_Row_COPY_INP_TMP = hv_Row.Clone();
            HTuple hv_String_COPY_INP_TMP = hv_String.Clone();

            // Initialize local and output iconic variables 
            //This procedure displays text in a graphics window.
            //
            //Input parameters:
            //WindowHandle: The WindowHandle of the graphics window, where
            //   the message should be displayed
            //String: A tuple of strings containing the text message to be displayed
            //CoordSystem: If set to 'window', the text position is given
            //   with respect to the window coordinate system.
            //   If set to 'image', image coordinates are used.
            //   (This may be useful in zoomed images.)
            //Row: The row coordinate of the desired text position
            //   If set to -1, a default value of 12 is used.
            //Column: The column coordinate of the desired text position
            //   If set to -1, a default value of 12 is used.
            //Color: defines the color of the text as string.
            //   If set to [], '' or 'auto' the currently set color is used.
            //   If a tuple of strings is passed, the colors are used cyclically
            //   for each new textline.
            //Box: If Box[0] is set to 'true', the text is written within an orange box.
            //     If set to' false', no box is displayed.
            //     If set to a color string (e.g. 'white', '#FF00CC', etc.),
            //       the text is written in a box of that color.
            //     An optional second value for Box (Box[1]) controls if a shadow is displayed:
            //       'true' -> display a shadow in a default color
            //       'false' -> display no shadow (same as if no second value is given)
            //       otherwise -> use given string as color string for the shadow color
            //
            //Prepare window
            HOperatorSet.GetRgb(hv_WindowHandle, out hv_Red, out hv_Green, out hv_Blue);
            HOperatorSet.GetPart(hv_WindowHandle, out hv_Row1Part, out hv_Column1Part, out hv_Row2Part,
                out hv_Column2Part);
            HOperatorSet.GetWindowExtents(hv_WindowHandle, out hv_RowWin, out hv_ColumnWin,
                out hv_WidthWin, out hv_HeightWin);
            HOperatorSet.SetPart(hv_WindowHandle, 0, 0, hv_HeightWin - 1, hv_WidthWin - 1);
            //
            //default settings
            if ((int)(new HTuple(hv_Row_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Row_COPY_INP_TMP = 12;
            }
            if ((int)(new HTuple(hv_Column_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Column_COPY_INP_TMP = 12;
            }

            if ((int)(new HTuple(hv_Color_COPY_INP_TMP.TupleEqual(new HTuple()))) != 0)
            {
                hv_Color_COPY_INP_TMP = "";
            }

            hv_String_COPY_INP_TMP = ((("" + hv_String_COPY_INP_TMP) + "")).TupleSplit("\n");
            //
            //Estimate extentions of text depending on font size.
            HOperatorSet.GetFontExtents(hv_WindowHandle, out hv_MaxAscent, out hv_MaxDescent,
                out hv_MaxWidth, out hv_MaxHeight);
            if ((int)(new HTuple(hv_CoordSystem.TupleEqual("window"))) != 0)
            {
                hv_R1 = hv_Row_COPY_INP_TMP.Clone();
                hv_C1 = hv_Column_COPY_INP_TMP.Clone();
            }
            else
            {
                //Transform image to window coordinates
                hv_FactorRow = (1.0 * hv_HeightWin) / ((hv_Row2Part - hv_Row1Part) + 1);
                hv_FactorColumn = (1.0 * hv_WidthWin) / ((hv_Column2Part - hv_Column1Part) + 1);
                hv_R1 = ((hv_Row_COPY_INP_TMP - hv_Row1Part) + 0.5) * hv_FactorRow;
                hv_C1 = ((hv_Column_COPY_INP_TMP - hv_Column1Part) + 0.5) * hv_FactorColumn;
            }
            //
            //Display text box depending on text size
            hv_UseShadow = 1;
            hv_ShadowColor = "gray";
            if ((int)(new HTuple(((hv_Box_COPY_INP_TMP.TupleSelect(0))).TupleEqual("true"))) != 0)
            {
                if (hv_Box_COPY_INP_TMP == null)
                    hv_Box_COPY_INP_TMP = new HTuple();
                hv_Box_COPY_INP_TMP[0] = "#fce9d4";
                hv_ShadowColor = "#f28d26";
            }
            if ((int)(new HTuple((new HTuple(hv_Box_COPY_INP_TMP.TupleLength())).TupleGreater(
                1))) != 0)
            {
                if ((int)(new HTuple(((hv_Box_COPY_INP_TMP.TupleSelect(1))).TupleEqual("true"))) != 0)
                {
                    //Use default ShadowColor set above
                }
                else if ((int)(new HTuple(((hv_Box_COPY_INP_TMP.TupleSelect(1))).TupleEqual(
                    "false"))) != 0)
                {
                    hv_UseShadow = 0;
                }
                else
                {
                    hv_ShadowColor = hv_Box_COPY_INP_TMP[1];
                    //Valid color?
                    try
                    {
                        HOperatorSet.SetColor(hv_WindowHandle, hv_Box_COPY_INP_TMP.TupleSelect(
                            1));
                    }
                    // catch (Exception) 
                    catch (HalconException HDevExpDefaultException1)
                    {
                        HDevExpDefaultException1.ToHTuple(out hv_Exception);
                        hv_Exception = "Wrong value of control parameter Box[1] (must be a 'true', 'false', or a valid color string)";
                        throw new HalconException(hv_Exception);
                    }
                }
            }
            if ((int)(new HTuple(((hv_Box_COPY_INP_TMP.TupleSelect(0))).TupleNotEqual("false"))) != 0)
            {
                //Valid color?
                try
                {
                    HOperatorSet.SetColor(hv_WindowHandle, hv_Box_COPY_INP_TMP.TupleSelect(0));
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    hv_Exception = "Wrong value of control parameter Box[0] (must be a 'true', 'false', or a valid color string)";
                    throw new HalconException(hv_Exception);
                }
                //Calculate box extents
                hv_String_COPY_INP_TMP = (" " + hv_String_COPY_INP_TMP) + " ";
                hv_Width = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    HOperatorSet.GetStringExtents(hv_WindowHandle, hv_String_COPY_INP_TMP.TupleSelect(
                        hv_Index), out hv_Ascent, out hv_Descent, out hv_W, out hv_H);
                    hv_Width = hv_Width.TupleConcat(hv_W);
                }
                hv_FrameHeight = hv_MaxHeight * (new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                    ));
                hv_FrameWidth = (((new HTuple(0)).TupleConcat(hv_Width))).TupleMax();
                hv_R2 = hv_R1 + hv_FrameHeight;
                hv_C2 = hv_C1 + hv_FrameWidth;
                //Display rectangles
                HOperatorSet.GetDraw(hv_WindowHandle, out hv_DrawMode);
                HOperatorSet.SetDraw(hv_WindowHandle, "fill");
                //Set shadow color
                HOperatorSet.SetColor(hv_WindowHandle, hv_ShadowColor);
                if ((int)(hv_UseShadow) != 0)
                {
                    HOperatorSet.DispRectangle1(hv_WindowHandle, hv_R1 + 1, hv_C1 + 1, hv_R2 + 1, hv_C2 + 1);
                }
                //Set box color
                HOperatorSet.SetColor(hv_WindowHandle, hv_Box_COPY_INP_TMP.TupleSelect(0));
                HOperatorSet.DispRectangle1(hv_WindowHandle, hv_R1, hv_C1, hv_R2, hv_C2);
                HOperatorSet.SetDraw(hv_WindowHandle, hv_DrawMode);
            }
            //Write text.
            for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                )) - 1); hv_Index = (int)hv_Index + 1)
            {
                hv_CurrentColor = hv_Color_COPY_INP_TMP.TupleSelect(hv_Index % (new HTuple(hv_Color_COPY_INP_TMP.TupleLength()
                    )));
                if ((int)((new HTuple(hv_CurrentColor.TupleNotEqual(""))).TupleAnd(new HTuple(hv_CurrentColor.TupleNotEqual(
                    "auto")))) != 0)
                {
                    HOperatorSet.SetColor(hv_WindowHandle, hv_CurrentColor);
                }
                else
                {
                    HOperatorSet.SetRgb(hv_WindowHandle, hv_Red, hv_Green, hv_Blue);
                }

                //拓展：Row、Col可以为多元素
                if ((int)(new HTuple((new HTuple(hv_String_COPY_INP_TMP.TupleLength())).TupleEqual(
                    new HTuple(hv_Row_COPY_INP_TMP.TupleLength())))) != 0)
                {
                    HOperatorSet.SetTposition(hv_WindowHandle, hv_R1.TupleSelect(hv_Index), hv_C1.TupleSelect(
                        hv_Index));
                    HOperatorSet.WriteString(hv_WindowHandle, hv_String_COPY_INP_TMP.TupleSelect(
                        hv_Index));
                }
                else
                {
                    hv_Row_COPY_INP_TMP = hv_R1 + (hv_MaxHeight * hv_Index);
                    HOperatorSet.SetTposition(hv_WindowHandle, hv_Row_COPY_INP_TMP, hv_C1);
                    HOperatorSet.WriteString(hv_WindowHandle, hv_String_COPY_INP_TMP.TupleSelect(
                        hv_Index));
                }
            }
            //Reset changed window settings
            HOperatorSet.SetRgb(hv_WindowHandle, hv_Red, hv_Green, hv_Blue);
            HOperatorSet.SetPart(hv_WindowHandle, hv_Row1Part, hv_Column1Part, hv_Row2Part,
                hv_Column2Part);

            return;
        }

        //打印字符串消息(内有短暂改写setPart不可高频使用)：disp_message中定义string可以是数组，Row,Col只能是一个值，
        //Color可以是数组或一个值(如果string是数组则一个值适用所有string)
        public void disp_message(HTuple hv_WindowHandle, HTuple hv_String, HTuple hv_CoordSystem,
            HTuple hv_Row, HTuple hv_Column, HTuple hv_Color, HTuple hv_Box)
        {
            // Local iconic variables 

            // Local control variables 

            HTuple hv_Red = null, hv_Green = null, hv_Blue = null;
            HTuple hv_Row1Part = null, hv_Column1Part = null, hv_Row2Part = null;
            HTuple hv_Column2Part = null, hv_RowWin = null, hv_ColumnWin = null;
            HTuple hv_WidthWin = null, hv_HeightWin = null, hv_MaxAscent = null;
            HTuple hv_MaxDescent = null, hv_MaxWidth = null, hv_MaxHeight = null;
            HTuple hv_R1 = new HTuple(), hv_C1 = new HTuple(), hv_FactorRow = new HTuple();
            HTuple hv_FactorColumn = new HTuple(), hv_UseShadow = null;
            HTuple hv_ShadowColor = null, hv_Exception = new HTuple();
            HTuple hv_Width = new HTuple(), hv_Index = new HTuple();
            HTuple hv_Ascent = new HTuple(), hv_Descent = new HTuple();
            HTuple hv_W = new HTuple(), hv_H = new HTuple(), hv_FrameHeight = new HTuple();
            HTuple hv_FrameWidth = new HTuple(), hv_R2 = new HTuple();
            HTuple hv_C2 = new HTuple(), hv_DrawMode = new HTuple();
            HTuple hv_CurrentColor = new HTuple();
            HTuple hv_Box_COPY_INP_TMP = hv_Box.Clone();
            HTuple hv_Color_COPY_INP_TMP = hv_Color.Clone();
            HTuple hv_Column_COPY_INP_TMP = hv_Column.Clone();
            HTuple hv_Row_COPY_INP_TMP = hv_Row.Clone();
            HTuple hv_String_COPY_INP_TMP = hv_String.Clone();

            // Initialize local and output iconic variables 
            //This procedure displays text in a graphics window.
            //
            //Input parameters:
            //WindowHandle: The WindowHandle of the graphics window, where
            //   the message should be displayed
            //String: A tuple of strings containing the text message to be displayed
            //CoordSystem: If set to 'window', the text position is given
            //   with respect to the window coordinate system.
            //   If set to 'image', image coordinates are used.
            //   (This may be useful in zoomed images.)
            //Row: The row coordinate of the desired text position
            //   If set to -1, a default value of 12 is used.
            //Column: The column coordinate of the desired text position
            //   If set to -1, a default value of 12 is used.
            //Color: defines the color of the text as string.
            //   If set to [], '' or 'auto' the currently set color is used.
            //   If a tuple of strings is passed, the colors are used cyclically
            //   for each new textline.
            //Box: If Box[0] is set to 'true', the text is written within an orange box.
            //     If set to' false', no box is displayed.
            //     If set to a color string (e.g. 'white', '#FF00CC', etc.),
            //       the text is written in a box of that color.
            //     An optional second value for Box (Box[1]) controls if a shadow is displayed:
            //       'true' -> display a shadow in a default color
            //       'false' -> display no shadow (same as if no second value is given)
            //       otherwise -> use given string as color string for the shadow color
            //
            //Prepare window
            HOperatorSet.GetRgb(hv_WindowHandle, out hv_Red, out hv_Green, out hv_Blue);
            HOperatorSet.GetPart(hv_WindowHandle, out hv_Row1Part, out hv_Column1Part, out hv_Row2Part,
                out hv_Column2Part);
            HOperatorSet.GetWindowExtents(hv_WindowHandle, out hv_RowWin, out hv_ColumnWin,
                out hv_WidthWin, out hv_HeightWin);
            HOperatorSet.SetPart(hv_WindowHandle, 0, 0, hv_HeightWin - 1, hv_WidthWin - 1);
            //
            //default settings
            if ((int)(new HTuple(hv_Row_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Row_COPY_INP_TMP = 12;
            }
            if ((int)(new HTuple(hv_Column_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Column_COPY_INP_TMP = 12;
            }
            if ((int)(new HTuple(hv_Color_COPY_INP_TMP.TupleEqual(new HTuple()))) != 0)
            {
                hv_Color_COPY_INP_TMP = "";
            }
            //
            hv_String_COPY_INP_TMP = ((("" + hv_String_COPY_INP_TMP) + "")).TupleSplit("\n");
            //
            //Estimate extentions of text depending on font size.
            HOperatorSet.GetFontExtents(hv_WindowHandle, out hv_MaxAscent, out hv_MaxDescent,
                out hv_MaxWidth, out hv_MaxHeight);
            if ((int)(new HTuple(hv_CoordSystem.TupleEqual("window"))) != 0)
            {
                hv_R1 = hv_Row_COPY_INP_TMP.Clone();
                hv_C1 = hv_Column_COPY_INP_TMP.Clone();
            }
            else
            {
                //Transform image to window coordinates
                hv_FactorRow = (1.0 * hv_HeightWin) / ((hv_Row2Part - hv_Row1Part) + 1);
                hv_FactorColumn = (1.0 * hv_WidthWin) / ((hv_Column2Part - hv_Column1Part) + 1);
                hv_R1 = ((hv_Row_COPY_INP_TMP - hv_Row1Part) + 0.5) * hv_FactorRow;
                hv_C1 = ((hv_Column_COPY_INP_TMP - hv_Column1Part) + 0.5) * hv_FactorColumn;
            }
            //
            //Display text box depending on text size
            hv_UseShadow = 1;
            hv_ShadowColor = "gray";
            if ((int)(new HTuple(((hv_Box_COPY_INP_TMP.TupleSelect(0))).TupleEqual("true"))) != 0)
            {
                if (hv_Box_COPY_INP_TMP == null)
                    hv_Box_COPY_INP_TMP = new HTuple();
                hv_Box_COPY_INP_TMP[0] = "#fce9d4";
                hv_ShadowColor = "#f28d26";
            }
            if ((int)(new HTuple((new HTuple(hv_Box_COPY_INP_TMP.TupleLength())).TupleGreater(
                1))) != 0)
            {
                if ((int)(new HTuple(((hv_Box_COPY_INP_TMP.TupleSelect(1))).TupleEqual("true"))) != 0)
                {
                    //Use default ShadowColor set above
                }
                else if ((int)(new HTuple(((hv_Box_COPY_INP_TMP.TupleSelect(1))).TupleEqual(
                    "false"))) != 0)
                {
                    hv_UseShadow = 0;
                }
                else
                {
                    hv_ShadowColor = hv_Box_COPY_INP_TMP[1];
                    //Valid color?
                    try
                    {
                        HOperatorSet.SetColor(hv_WindowHandle, hv_Box_COPY_INP_TMP.TupleSelect(
                            1));
                    }
                    // catch (Exception) 
                    catch (HalconException HDevExpDefaultException1)
                    {
                        HDevExpDefaultException1.ToHTuple(out hv_Exception);
                        hv_Exception = "Wrong value of control parameter Box[1] (must be a 'true', 'false', or a valid color string)";
                        throw new HalconException(hv_Exception);
                    }
                }
            }
            if ((int)(new HTuple(((hv_Box_COPY_INP_TMP.TupleSelect(0))).TupleNotEqual("false"))) != 0)
            {
                //Valid color?
                try
                {
                    HOperatorSet.SetColor(hv_WindowHandle, hv_Box_COPY_INP_TMP.TupleSelect(0));
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    hv_Exception = "Wrong value of control parameter Box[0] (must be a 'true', 'false', or a valid color string)";
                    throw new HalconException(hv_Exception);
                }
                //Calculate box extents
                hv_String_COPY_INP_TMP = (" " + hv_String_COPY_INP_TMP) + " ";
                hv_Width = new HTuple();
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                    )) - 1); hv_Index = (int)hv_Index + 1)
                {
                    HOperatorSet.GetStringExtents(hv_WindowHandle, hv_String_COPY_INP_TMP.TupleSelect(
                        hv_Index), out hv_Ascent, out hv_Descent, out hv_W, out hv_H);
                    hv_Width = hv_Width.TupleConcat(hv_W);
                }
                hv_FrameHeight = hv_MaxHeight * (new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                    ));
                hv_FrameWidth = (((new HTuple(0)).TupleConcat(hv_Width))).TupleMax();
                hv_R2 = hv_R1 + hv_FrameHeight;
                hv_C2 = hv_C1 + hv_FrameWidth;
                //Display rectangles
                HOperatorSet.GetDraw(hv_WindowHandle, out hv_DrawMode);
                HOperatorSet.SetDraw(hv_WindowHandle, "fill");
                //Set shadow color
                HOperatorSet.SetColor(hv_WindowHandle, hv_ShadowColor);
                if ((int)(hv_UseShadow) != 0)
                {
                    HOperatorSet.DispRectangle1(hv_WindowHandle, hv_R1 + 1, hv_C1 + 1, hv_R2 + 1, hv_C2 + 1);
                }
                //Set box color
                HOperatorSet.SetColor(hv_WindowHandle, hv_Box_COPY_INP_TMP.TupleSelect(0));
                HOperatorSet.DispRectangle1(hv_WindowHandle, hv_R1, hv_C1, hv_R2, hv_C2);
                HOperatorSet.SetDraw(hv_WindowHandle, hv_DrawMode);
            }
            //Write text.
            for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_String_COPY_INP_TMP.TupleLength()
                )) - 1); hv_Index = (int)hv_Index + 1)
            {
                hv_CurrentColor = hv_Color_COPY_INP_TMP.TupleSelect(hv_Index % (new HTuple(hv_Color_COPY_INP_TMP.TupleLength()
                    )));
                if ((int)((new HTuple(hv_CurrentColor.TupleNotEqual(""))).TupleAnd(new HTuple(hv_CurrentColor.TupleNotEqual(
                    "auto")))) != 0)
                {
                    HOperatorSet.SetColor(hv_WindowHandle, hv_CurrentColor);
                }
                else
                {
                    HOperatorSet.SetRgb(hv_WindowHandle, hv_Red, hv_Green, hv_Blue);
                }
                hv_Row_COPY_INP_TMP = hv_R1 + (hv_MaxHeight * hv_Index);
                HOperatorSet.SetTposition(hv_WindowHandle, hv_Row_COPY_INP_TMP, hv_C1);
                HOperatorSet.WriteString(hv_WindowHandle, hv_String_COPY_INP_TMP.TupleSelect(
                    hv_Index));
            }
            //Reset changed window settings
            HOperatorSet.SetRgb(hv_WindowHandle, hv_Red, hv_Green, hv_Blue);
            HOperatorSet.SetPart(hv_WindowHandle, hv_Row1Part, hv_Column1Part, hv_Row2Part,
                hv_Column2Part);

            return;
        }

        //设置打印字符串字体
        public void set_display_font(HTuple hv_WindowHandle, HTuple hv_Size, HTuple hv_Font,
            HTuple hv_Bold, HTuple hv_Slant)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_OS = null, hv_BufferWindowHandle = new HTuple();
            HTuple hv_Ascent = new HTuple(), hv_Descent = new HTuple();
            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_Scale = new HTuple(), hv_Exception = new HTuple();
            HTuple hv_SubFamily = new HTuple(), hv_Fonts = new HTuple();
            HTuple hv_SystemFonts = new HTuple(), hv_Guess = new HTuple();
            HTuple hv_I = new HTuple(), hv_Index = new HTuple(), hv_AllowedFontSizes = new HTuple();
            HTuple hv_Distances = new HTuple(), hv_Indices = new HTuple();
            HTuple hv_FontSelRegexp = new HTuple(), hv_FontsCourier = new HTuple();
            HTuple hv_Bold_COPY_INP_TMP = hv_Bold.Clone();
            HTuple hv_Font_COPY_INP_TMP = hv_Font.Clone();
            HTuple hv_Size_COPY_INP_TMP = hv_Size.Clone();
            HTuple hv_Slant_COPY_INP_TMP = hv_Slant.Clone();

            // Initialize local and output iconic variables 
            //This procedure sets the text font of the current window with
            //the specified attributes.
            //It is assumed that following fonts are installed on the system:
            //Windows: Courier New, Arial Times New Roman
            //Mac OS X: CourierNewPS, Arial, TimesNewRomanPS
            //Linux: courier, helvetica, times
            //Because fonts are displayed smaller on Linux than on Windows,
            //a scaling factor of 1.25 is used the get comparable results.
            //For Linux, only a limited number of font sizes is supported,
            //to get comparable results, it is recommended to use one of the
            //following sizes: 9, 11, 14, 16, 20, 27
            //(which will be mapped internally on Linux systems to 11, 14, 17, 20, 25, 34)
            //
            //Input parameters:
            //WindowHandle: The graphics window for which the font will be set
            //Size: The font size. If Size=-1, the default of 16 is used.
            //Bold: If set to 'true', a bold font is used
            //Slant: If set to 'true', a slanted font is used
            //
            HOperatorSet.GetSystem("operating_system", out hv_OS);
            // dev_get_preferences(...); only in hdevelop
            // dev_set_preferences(...); only in hdevelop
            if ((int)((new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleOr(
                new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(-1)))) != 0)
            {
                hv_Size_COPY_INP_TMP = 16;
            }
            if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Win"))) != 0)
            {
                //Set font on Windows systems
                try
                {
                    //Check, if font scaling is switched on
                    HOperatorSet.OpenWindow(0, 0, 256, 256, 0, "buffer", "", out hv_BufferWindowHandle);
                    HOperatorSet.SetFont(hv_BufferWindowHandle, "-Consolas-16-*-0-*-*-1-");
                    HOperatorSet.GetStringExtents(hv_BufferWindowHandle, "test_string", out hv_Ascent,
                        out hv_Descent, out hv_Width, out hv_Height);
                    //Expected width is 110
                    hv_Scale = 110.0 / hv_Width;
                    hv_Size_COPY_INP_TMP = ((hv_Size_COPY_INP_TMP * hv_Scale)).TupleInt();
                    HOperatorSet.CloseWindow(hv_BufferWindowHandle);
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    //throw (Exception)
                }
                if ((int)((new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("Courier"))).TupleOr(
                    new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("courier")))) != 0)
                {
                    hv_Font_COPY_INP_TMP = "Courier New";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("mono"))) != 0)
                {
                    hv_Font_COPY_INP_TMP = "Consolas";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("sans"))) != 0)
                {
                    hv_Font_COPY_INP_TMP = "Arial";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("serif"))) != 0)
                {
                    hv_Font_COPY_INP_TMP = "Times New Roman";
                }
                if ((int)(new HTuple(hv_Bold_COPY_INP_TMP.TupleEqual("true"))) != 0)
                {
                    hv_Bold_COPY_INP_TMP = 1;
                }
                else if ((int)(new HTuple(hv_Bold_COPY_INP_TMP.TupleEqual("false"))) != 0)
                {
                    hv_Bold_COPY_INP_TMP = 0;
                }
                else
                {
                    hv_Exception = "Wrong value of control parameter Bold";
                    throw new HalconException(hv_Exception);
                }
                if ((int)(new HTuple(hv_Slant_COPY_INP_TMP.TupleEqual("true"))) != 0)
                {
                    hv_Slant_COPY_INP_TMP = 1;
                }
                else if ((int)(new HTuple(hv_Slant_COPY_INP_TMP.TupleEqual("false"))) != 0)
                {
                    hv_Slant_COPY_INP_TMP = 0;
                }
                else
                {
                    hv_Exception = "Wrong value of control parameter Slant";
                    throw new HalconException(hv_Exception);
                }
                try
                {
                    HOperatorSet.SetFont(hv_WindowHandle, ((((((("-" + hv_Font_COPY_INP_TMP) + "-") + hv_Size_COPY_INP_TMP) + "-*-") + hv_Slant_COPY_INP_TMP) + "-*-*-") + hv_Bold_COPY_INP_TMP) + "-");
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    //throw (Exception)
                }
            }
            else if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Dar"))) != 0)
            {
                //Set font on Mac OS X systems. Since OS X does not have a strict naming
                //scheme for font attributes, we use tables to determine the correct font
                //name.
                hv_SubFamily = 0;
                if ((int)(new HTuple(hv_Slant_COPY_INP_TMP.TupleEqual("true"))) != 0)
                {
                    hv_SubFamily = hv_SubFamily.TupleBor(1);
                }
                else if ((int)(new HTuple(hv_Slant_COPY_INP_TMP.TupleNotEqual("false"))) != 0)
                {
                    hv_Exception = "Wrong value of control parameter Slant";
                    throw new HalconException(hv_Exception);
                }
                if ((int)(new HTuple(hv_Bold_COPY_INP_TMP.TupleEqual("true"))) != 0)
                {
                    hv_SubFamily = hv_SubFamily.TupleBor(2);
                }
                else if ((int)(new HTuple(hv_Bold_COPY_INP_TMP.TupleNotEqual("false"))) != 0)
                {
                    hv_Exception = "Wrong value of control parameter Bold";
                    throw new HalconException(hv_Exception);
                }
                if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("mono"))) != 0)
                {
                    hv_Fonts = new HTuple();
                    hv_Fonts[0] = "Menlo-Regular";
                    hv_Fonts[1] = "Menlo-Italic";
                    hv_Fonts[2] = "Menlo-Bold";
                    hv_Fonts[3] = "Menlo-BoldItalic";
                }
                else if ((int)((new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("Courier"))).TupleOr(
                    new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("courier")))) != 0)
                {
                    hv_Fonts = new HTuple();
                    hv_Fonts[0] = "CourierNewPSMT";
                    hv_Fonts[1] = "CourierNewPS-ItalicMT";
                    hv_Fonts[2] = "CourierNewPS-BoldMT";
                    hv_Fonts[3] = "CourierNewPS-BoldItalicMT";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("sans"))) != 0)
                {
                    hv_Fonts = new HTuple();
                    hv_Fonts[0] = "ArialMT";
                    hv_Fonts[1] = "Arial-ItalicMT";
                    hv_Fonts[2] = "Arial-BoldMT";
                    hv_Fonts[3] = "Arial-BoldItalicMT";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("serif"))) != 0)
                {
                    hv_Fonts = new HTuple();
                    hv_Fonts[0] = "TimesNewRomanPSMT";
                    hv_Fonts[1] = "TimesNewRomanPS-ItalicMT";
                    hv_Fonts[2] = "TimesNewRomanPS-BoldMT";
                    hv_Fonts[3] = "TimesNewRomanPS-BoldItalicMT";
                }
                else
                {
                    //Attempt to figure out which of the fonts installed on the system
                    //the user could have meant.
                    HOperatorSet.QueryFont(hv_WindowHandle, out hv_SystemFonts);
                    hv_Fonts = new HTuple();
                    hv_Fonts = hv_Fonts.TupleConcat(hv_Font_COPY_INP_TMP);
                    hv_Fonts = hv_Fonts.TupleConcat(hv_Font_COPY_INP_TMP);
                    hv_Fonts = hv_Fonts.TupleConcat(hv_Font_COPY_INP_TMP);
                    hv_Fonts = hv_Fonts.TupleConcat(hv_Font_COPY_INP_TMP);
                    hv_Guess = new HTuple();
                    hv_Guess = hv_Guess.TupleConcat(hv_Font_COPY_INP_TMP);
                    hv_Guess = hv_Guess.TupleConcat(hv_Font_COPY_INP_TMP + "-Regular");
                    hv_Guess = hv_Guess.TupleConcat(hv_Font_COPY_INP_TMP + "MT");
                    for (hv_I = 0; (int)hv_I <= (int)((new HTuple(hv_Guess.TupleLength())) - 1); hv_I = (int)hv_I + 1)
                    {
                        HOperatorSet.TupleFind(hv_SystemFonts, hv_Guess.TupleSelect(hv_I), out hv_Index);
                        if ((int)(new HTuple(hv_Index.TupleNotEqual(-1))) != 0)
                        {
                            if (hv_Fonts == null)
                                hv_Fonts = new HTuple();
                            hv_Fonts[0] = hv_Guess.TupleSelect(hv_I);
                            break;
                        }
                    }
                    //Guess name of slanted font
                    hv_Guess = new HTuple();
                    hv_Guess = hv_Guess.TupleConcat(hv_Font_COPY_INP_TMP + "-Italic");
                    hv_Guess = hv_Guess.TupleConcat(hv_Font_COPY_INP_TMP + "-ItalicMT");
                    hv_Guess = hv_Guess.TupleConcat(hv_Font_COPY_INP_TMP + "-Oblique");
                    for (hv_I = 0; (int)hv_I <= (int)((new HTuple(hv_Guess.TupleLength())) - 1); hv_I = (int)hv_I + 1)
                    {
                        HOperatorSet.TupleFind(hv_SystemFonts, hv_Guess.TupleSelect(hv_I), out hv_Index);
                        if ((int)(new HTuple(hv_Index.TupleNotEqual(-1))) != 0)
                        {
                            if (hv_Fonts == null)
                                hv_Fonts = new HTuple();
                            hv_Fonts[1] = hv_Guess.TupleSelect(hv_I);
                            break;
                        }
                    }
                    //Guess name of bold font
                    hv_Guess = new HTuple();
                    hv_Guess = hv_Guess.TupleConcat(hv_Font_COPY_INP_TMP + "-Bold");
                    hv_Guess = hv_Guess.TupleConcat(hv_Font_COPY_INP_TMP + "-BoldMT");
                    for (hv_I = 0; (int)hv_I <= (int)((new HTuple(hv_Guess.TupleLength())) - 1); hv_I = (int)hv_I + 1)
                    {
                        HOperatorSet.TupleFind(hv_SystemFonts, hv_Guess.TupleSelect(hv_I), out hv_Index);
                        if ((int)(new HTuple(hv_Index.TupleNotEqual(-1))) != 0)
                        {
                            if (hv_Fonts == null)
                                hv_Fonts = new HTuple();
                            hv_Fonts[2] = hv_Guess.TupleSelect(hv_I);
                            break;
                        }
                    }
                    //Guess name of bold slanted font
                    hv_Guess = new HTuple();
                    hv_Guess = hv_Guess.TupleConcat(hv_Font_COPY_INP_TMP + "-BoldItalic");
                    hv_Guess = hv_Guess.TupleConcat(hv_Font_COPY_INP_TMP + "-BoldItalicMT");
                    hv_Guess = hv_Guess.TupleConcat(hv_Font_COPY_INP_TMP + "-BoldOblique");
                    for (hv_I = 0; (int)hv_I <= (int)((new HTuple(hv_Guess.TupleLength())) - 1); hv_I = (int)hv_I + 1)
                    {
                        HOperatorSet.TupleFind(hv_SystemFonts, hv_Guess.TupleSelect(hv_I), out hv_Index);
                        if ((int)(new HTuple(hv_Index.TupleNotEqual(-1))) != 0)
                        {
                            if (hv_Fonts == null)
                                hv_Fonts = new HTuple();
                            hv_Fonts[3] = hv_Guess.TupleSelect(hv_I);
                            break;
                        }
                    }
                }
                hv_Font_COPY_INP_TMP = hv_Fonts.TupleSelect(hv_SubFamily);
                try
                {
                    HOperatorSet.SetFont(hv_WindowHandle, (hv_Font_COPY_INP_TMP + "-") + hv_Size_COPY_INP_TMP);
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    //throw (Exception)
                }
            }
            else
            {
                //Set font for UNIX systems
                hv_Size_COPY_INP_TMP = hv_Size_COPY_INP_TMP * 1.25;
                hv_AllowedFontSizes = new HTuple();
                hv_AllowedFontSizes[0] = 11;
                hv_AllowedFontSizes[1] = 14;
                hv_AllowedFontSizes[2] = 17;
                hv_AllowedFontSizes[3] = 20;
                hv_AllowedFontSizes[4] = 25;
                hv_AllowedFontSizes[5] = 34;
                if ((int)(new HTuple(((hv_AllowedFontSizes.TupleFind(hv_Size_COPY_INP_TMP))).TupleEqual(
                    -1))) != 0)
                {
                    hv_Distances = ((hv_AllowedFontSizes - hv_Size_COPY_INP_TMP)).TupleAbs();
                    HOperatorSet.TupleSortIndex(hv_Distances, out hv_Indices);
                    hv_Size_COPY_INP_TMP = hv_AllowedFontSizes.TupleSelect(hv_Indices.TupleSelect(
                        0));
                }
                if ((int)((new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("mono"))).TupleOr(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual(
                    "Courier")))) != 0)
                {
                    hv_Font_COPY_INP_TMP = "courier";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("sans"))) != 0)
                {
                    hv_Font_COPY_INP_TMP = "helvetica";
                }
                else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("serif"))) != 0)
                {
                    hv_Font_COPY_INP_TMP = "times";
                }
                if ((int)(new HTuple(hv_Bold_COPY_INP_TMP.TupleEqual("true"))) != 0)
                {
                    hv_Bold_COPY_INP_TMP = "bold";
                }
                else if ((int)(new HTuple(hv_Bold_COPY_INP_TMP.TupleEqual("false"))) != 0)
                {
                    hv_Bold_COPY_INP_TMP = "medium";
                }
                else
                {
                    hv_Exception = "Wrong value of control parameter Bold";
                    throw new HalconException(hv_Exception);
                }
                if ((int)(new HTuple(hv_Slant_COPY_INP_TMP.TupleEqual("true"))) != 0)
                {
                    if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("times"))) != 0)
                    {
                        hv_Slant_COPY_INP_TMP = "i";
                    }
                    else
                    {
                        hv_Slant_COPY_INP_TMP = "o";
                    }
                }
                else if ((int)(new HTuple(hv_Slant_COPY_INP_TMP.TupleEqual("false"))) != 0)
                {
                    hv_Slant_COPY_INP_TMP = "r";
                }
                else
                {
                    hv_Exception = "Wrong value of control parameter Slant";
                    throw new HalconException(hv_Exception);
                }
                try
                {
                    HOperatorSet.SetFont(hv_WindowHandle, ((((((("-adobe-" + hv_Font_COPY_INP_TMP) + "-") + hv_Bold_COPY_INP_TMP) + "-") + hv_Slant_COPY_INP_TMP) + "-normal-*-") + hv_Size_COPY_INP_TMP) + "-*-*-*-*-*-*-*");
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    if ((int)((new HTuple(((hv_OS.TupleSubstr(0, 4))).TupleEqual("Linux"))).TupleAnd(
                        new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("courier")))) != 0)
                    {
                        HOperatorSet.QueryFont(hv_WindowHandle, out hv_Fonts);
                        hv_FontSelRegexp = (("^-[^-]*-[^-]*[Cc]ourier[^-]*-" + hv_Bold_COPY_INP_TMP) + "-") + hv_Slant_COPY_INP_TMP;
                        hv_FontsCourier = ((hv_Fonts.TupleRegexpSelect(hv_FontSelRegexp))).TupleRegexpMatch(
                            hv_FontSelRegexp);
                        if ((int)(new HTuple((new HTuple(hv_FontsCourier.TupleLength())).TupleEqual(
                            0))) != 0)
                        {
                            hv_Exception = "Wrong font name";
                            //throw (Exception)
                        }
                        else
                        {
                            try
                            {
                                HOperatorSet.SetFont(hv_WindowHandle, (((hv_FontsCourier.TupleSelect(
                                    0)) + "-normal-*-") + hv_Size_COPY_INP_TMP) + "-*-*-*-*-*-*-*");
                            }
                            // catch (Exception) 
                            catch (HalconException HDevExpDefaultException2)
                            {
                                HDevExpDefaultException2.ToHTuple(out hv_Exception);
                                //throw (Exception)
                            }
                        }
                    }
                    //throw (Exception)
                }
            }
            // dev_set_preferences(...); only in hdevelop

            return;
        }

        //显示边缘线
        public void p_disp_edge_marker(HTuple hv_Rows, HTuple hv_Cols, HTuple hv_Phi,
            HTuple hv_Length, HTuple hv_Color, HTuple hv_LineWidth, HTuple hv_WindowHandle)
        {



            // Local iconic variables 

            HObject ho_Marker = null;

            // Local control variables 

            HTuple hv_NumRows = null, hv_NumCols = null;
            HTuple hv_Num = null, hv_i = null, hv_Row = new HTuple();
            HTuple hv_Col = new HTuple(), hv_RowStart = new HTuple();
            HTuple hv_RowEnd = new HTuple(), hv_ColStart = new HTuple();
            HTuple hv_ColEnd = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Marker);
            try
            {
                //Determine the number of edges
                hv_NumRows = new HTuple(hv_Rows.TupleLength());
                hv_NumCols = new HTuple(hv_Cols.TupleLength());
                hv_Num = ((hv_NumRows.TupleConcat(hv_NumCols))).TupleMin();
                //
                //Loop over the edges
                HTuple end_val6 = hv_Num - 1;
                HTuple step_val6 = 1;
                for (hv_i = 0; hv_i.Continue(end_val6, step_val6); hv_i = hv_i.TupleAdd(step_val6))
                {
                    hv_Row = hv_Rows.TupleSelect(hv_i);
                    hv_Col = hv_Cols.TupleSelect(hv_i);
                    //
                    //Determine start and end point of the edge marker.
                    hv_RowStart = hv_Row + (hv_Length * (hv_Phi.TupleCos()));
                    hv_RowEnd = hv_Row - (hv_Length * (hv_Phi.TupleCos()));
                    hv_ColStart = hv_Col + (hv_Length * (hv_Phi.TupleSin()));
                    hv_ColEnd = hv_Col - (hv_Length * (hv_Phi.TupleSin()));
                    //
                    //Generate a contour that connects the start and end point.
                    ho_Marker.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_Marker, hv_RowStart.TupleConcat(
                        hv_RowEnd), hv_ColStart.TupleConcat(hv_ColEnd));
                    //
                    //Display the contour with  the specified style.
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetColor(HDevWindowStack.GetActive(), hv_Color);
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetLineWidth(HDevWindowStack.GetActive(), hv_LineWidth);
                    }
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.DispObj(ho_Marker, HDevWindowStack.GetActive());
                    }
                }
                ho_Marker.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Marker.Dispose();

                throw HDevExpDefaultException;
            }
        }

        //My显示边缘线
        public void my_p_disp_edge_marker(HTuple hv_Rows, HTuple hv_Cols, HTuple hv_Phi,
            HTuple hv_Length, HTuple hv_Color, HTuple hv_LineWidth, HTuple hv_WindowHandle)
        {



            // Local iconic variables 

            HObject ho_Marker = null;
            HObject ho_AllMarker = null;

            // Local control variables 

            HTuple hv_NumRows = null, hv_NumCols = null;
            HTuple hv_Num = null, hv_i = null, hv_Row = new HTuple();
            HTuple hv_Col = new HTuple(), hv_RowStart = new HTuple();
            HTuple hv_RowEnd = new HTuple(), hv_ColStart = new HTuple();
            HTuple hv_ColEnd = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Marker);
            HOperatorSet.GenEmptyObj(out ho_AllMarker);
            //Determine the number of edges
            hv_NumRows = new HTuple(hv_Rows.TupleLength());
            hv_NumCols = new HTuple(hv_Cols.TupleLength());
            hv_Num = ((hv_NumRows.TupleConcat(hv_NumCols))).TupleMin();
            //
            //Loop over the edges
            HTuple end_val6 = hv_Num - 1;
            HTuple step_val6 = 1;
            for (hv_i = 0; hv_i.Continue(end_val6, step_val6); hv_i = hv_i.TupleAdd(step_val6))
            {
                hv_Row = hv_Rows.TupleSelect(hv_i);
                hv_Col = hv_Cols.TupleSelect(hv_i);
                //
                //Determine start and end point of the edge marker.
                hv_RowStart = hv_Row + (hv_Length * (hv_Phi.TupleCos()));
                hv_RowEnd = hv_Row - (hv_Length * (hv_Phi.TupleCos()));
                hv_ColStart = hv_Col + (hv_Length * (hv_Phi.TupleSin()));
                hv_ColEnd = hv_Col - (hv_Length * (hv_Phi.TupleSin()));
                //
                //Generate a contour that connects the start and end point.
                ho_Marker.Dispose();
                HOperatorSet.GenContourPolygonXld(out ho_Marker, hv_RowStart.TupleConcat(hv_RowEnd),
                    hv_ColStart.TupleConcat(hv_ColEnd));
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_AllMarker, ho_Marker, out ExpTmpOutVar_0);
                    ho_AllMarker.Dispose();
                    ho_AllMarker = ExpTmpOutVar_0;
                }
                //
                //Display the contour with  the specified style.
                //if (HDevWindowStack.IsOpen())
                //{
                //    HOperatorSet.SetColor(HDevWindowStack.GetActive(), hv_Color);
                //}
                //if (HDevWindowStack.IsOpen())
                //{
                //    HOperatorSet.SetLineWidth(HDevWindowStack.GetActive(), hv_LineWidth);
                //}
                //if (HDevWindowStack.IsOpen())
                //{
                //   HOperatorSet.DispObj(ho_Marker, HDevWindowStack.GetActive());
                //}
            }
            string strErrMsg = "";
            if (HTupleValided(hv_WindowHandle, ref strErrMsg))
            {
                HOperatorSet.SetLineWidth(hv_WindowHandle, hv_LineWidth);
                HOperatorSet.SetColor(hv_WindowHandle, hv_Color);
                HOperatorSet.DispObj(ho_AllMarker, hv_WindowHandle);
            }
            ho_Marker.Dispose();
            ho_AllMarker.Dispose();

            return;
        }

        //显示基于形状的匹配结果。
        public void dev_display_shape_matching_results(HTuple hv_ModelID, HTuple hv_Color,
            HTuple hv_Row, HTuple hv_Column, HTuple hv_Angle, HTuple hv_ScaleR, HTuple hv_ScaleC,
            HTuple hv_Model)
        {



            // Local iconic variables 

            HObject ho_ModelContours = null, ho_ContoursAffinTrans = null;

            // Local control variables 

            HTuple hv_NumMatches = null, hv_Index = new HTuple();
            HTuple hv_Match = new HTuple(), hv_HomMat2DIdentity = new HTuple();
            HTuple hv_HomMat2DScale = new HTuple(), hv_HomMat2DRotate = new HTuple();
            HTuple hv_HomMat2DTranslate = new HTuple();
            HTuple hv_Model_COPY_INP_TMP = hv_Model.Clone();
            HTuple hv_ScaleC_COPY_INP_TMP = hv_ScaleC.Clone();
            HTuple hv_ScaleR_COPY_INP_TMP = hv_ScaleR.Clone();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ModelContours);
            HOperatorSet.GenEmptyObj(out ho_ContoursAffinTrans);
            try
            {
                //This procedure displays the results of Shape-Based Matching.
                //
                hv_NumMatches = new HTuple(hv_Row.TupleLength());
                if ((int)(new HTuple(hv_NumMatches.TupleGreater(0))) != 0)
                {
                    if ((int)(new HTuple((new HTuple(hv_ScaleR_COPY_INP_TMP.TupleLength())).TupleEqual(
                        1))) != 0)
                    {
                        HOperatorSet.TupleGenConst(hv_NumMatches, hv_ScaleR_COPY_INP_TMP, out hv_ScaleR_COPY_INP_TMP);
                    }
                    if ((int)(new HTuple((new HTuple(hv_ScaleC_COPY_INP_TMP.TupleLength())).TupleEqual(
                        1))) != 0)
                    {
                        HOperatorSet.TupleGenConst(hv_NumMatches, hv_ScaleC_COPY_INP_TMP, out hv_ScaleC_COPY_INP_TMP);
                    }
                    if ((int)(new HTuple((new HTuple(hv_Model_COPY_INP_TMP.TupleLength())).TupleEqual(
                        0))) != 0)
                    {
                        HOperatorSet.TupleGenConst(hv_NumMatches, 0, out hv_Model_COPY_INP_TMP);
                    }
                    else if ((int)(new HTuple((new HTuple(hv_Model_COPY_INP_TMP.TupleLength()
                        )).TupleEqual(1))) != 0)
                    {
                        HOperatorSet.TupleGenConst(hv_NumMatches, hv_Model_COPY_INP_TMP, out hv_Model_COPY_INP_TMP);
                    }
                    for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_ModelID.TupleLength()
                        )) - 1); hv_Index = (int)hv_Index + 1)
                    {
                        ho_ModelContours.Dispose();
                        HOperatorSet.GetShapeModelContours(out ho_ModelContours, hv_ModelID.TupleSelect(
                            hv_Index), 1);
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.SetColor(HDevWindowStack.GetActive(), hv_Color.TupleSelect(
                                hv_Index % (new HTuple(hv_Color.TupleLength()))));
                        }
                        HTuple end_val18 = hv_NumMatches - 1;
                        HTuple step_val18 = 1;
                        for (hv_Match = 0; hv_Match.Continue(end_val18, step_val18); hv_Match = hv_Match.TupleAdd(step_val18))
                        {
                            if ((int)(new HTuple(hv_Index.TupleEqual(hv_Model_COPY_INP_TMP.TupleSelect(
                                hv_Match)))) != 0)
                            {
                                HOperatorSet.HomMat2dIdentity(out hv_HomMat2DIdentity);
                                HOperatorSet.HomMat2dScale(hv_HomMat2DIdentity, hv_ScaleR_COPY_INP_TMP.TupleSelect(
                                    hv_Match), hv_ScaleC_COPY_INP_TMP.TupleSelect(hv_Match), 0, 0,
                                    out hv_HomMat2DScale);
                                HOperatorSet.HomMat2dRotate(hv_HomMat2DScale, hv_Angle.TupleSelect(
                                    hv_Match), 0, 0, out hv_HomMat2DRotate);
                                HOperatorSet.HomMat2dTranslate(hv_HomMat2DRotate, hv_Row.TupleSelect(
                                    hv_Match), hv_Column.TupleSelect(hv_Match), out hv_HomMat2DTranslate);
                                ho_ContoursAffinTrans.Dispose();
                                HOperatorSet.AffineTransContourXld(ho_ModelContours, out ho_ContoursAffinTrans,
                                    hv_HomMat2DTranslate);
                                if (HDevWindowStack.IsOpen())
                                {
                                    HOperatorSet.DispObj(ho_ContoursAffinTrans, HDevWindowStack.GetActive()
                                        );
                                }
                            }
                        }
                    }
                }
                ho_ModelContours.Dispose();
                ho_ContoursAffinTrans.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_ModelContours.Dispose();
                ho_ContoursAffinTrans.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // 获取给定路径下的所有图像文件
        public void list_image_files(HTuple hv_ImageDirectory, HTuple hv_Extensions, HTuple hv_Options,
            out HTuple hv_ImageFiles)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_HalconImages = null, hv_OS = null;
            HTuple hv_Directories = null, hv_Index = null, hv_Length = null;
            HTuple hv_network_drive = null, hv_Substring = new HTuple();
            HTuple hv_FileExists = new HTuple(), hv_AllFiles = new HTuple();
            HTuple hv_i = new HTuple(), hv_Selection = new HTuple();
            HTuple hv_Extensions_COPY_INP_TMP = hv_Extensions.Clone();
            HTuple hv_ImageDirectory_COPY_INP_TMP = hv_ImageDirectory.Clone();

            // Initialize local and output iconic variables 
            //This procedure returns all files in a given directory
            //with one of the suffixes specified in Extensions.
            //
            //input parameters:
            //ImageDirectory: as the name says
            //   If a tuple of directories is given, only the images in the first
            //   existing directory are returned.
            //   If a local directory is not found, the directory is searched
            //   under %HALCONIMAGES%/ImageDirectory. If %HALCONIMAGES% is not set,
            //   %HALCONROOT%/images is used instead.
            //Extensions: A string tuple containing the extensions to be found
            //   e.g. ['png','tif',jpg'] or others
            //If Extensions is set to 'default' or the empty string '',
            //   all image suffixes supported by HALCON are used.
            //Options: as in the operator list_files, except that the 'files'
            //   option is always used. Note that the 'directories' option
            //   has no effect but increases runtime, because only files are
            //   returned.
            //
            //output parameter:
            //ImageFiles: A tuple of all found image file names
            //
            if ((int)((new HTuple((new HTuple(hv_Extensions_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleOr(
                new HTuple(hv_Extensions_COPY_INP_TMP.TupleEqual(""))))).TupleOr(new HTuple(hv_Extensions_COPY_INP_TMP.TupleEqual(
                "default")))) != 0)
            {
                hv_Extensions_COPY_INP_TMP = new HTuple();
                hv_Extensions_COPY_INP_TMP[0] = "ima";
                hv_Extensions_COPY_INP_TMP[1] = "tif";
                hv_Extensions_COPY_INP_TMP[2] = "tiff";
                hv_Extensions_COPY_INP_TMP[3] = "gif";
                hv_Extensions_COPY_INP_TMP[4] = "bmp";
                hv_Extensions_COPY_INP_TMP[5] = "jpg";
                hv_Extensions_COPY_INP_TMP[6] = "jpeg";
                hv_Extensions_COPY_INP_TMP[7] = "jp2";
                hv_Extensions_COPY_INP_TMP[8] = "jxr";
                hv_Extensions_COPY_INP_TMP[9] = "png";
                hv_Extensions_COPY_INP_TMP[10] = "pcx";
                hv_Extensions_COPY_INP_TMP[11] = "ras";
                hv_Extensions_COPY_INP_TMP[12] = "xwd";
                hv_Extensions_COPY_INP_TMP[13] = "pbm";
                hv_Extensions_COPY_INP_TMP[14] = "pnm";
                hv_Extensions_COPY_INP_TMP[15] = "pgm";
                hv_Extensions_COPY_INP_TMP[16] = "ppm";
                //
            }
            if ((int)(new HTuple(hv_ImageDirectory_COPY_INP_TMP.TupleEqual(""))) != 0)
            {
                hv_ImageDirectory_COPY_INP_TMP = ".";
            }
            HOperatorSet.GetSystem("image_dir", out hv_HalconImages);
            HOperatorSet.GetSystem("operating_system", out hv_OS);
            if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Win"))) != 0)
            {
                hv_HalconImages = hv_HalconImages.TupleSplit(";");
            }
            else
            {
                hv_HalconImages = hv_HalconImages.TupleSplit(":");
            }
            hv_Directories = hv_ImageDirectory_COPY_INP_TMP.Clone();
            for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_HalconImages.TupleLength()
                )) - 1); hv_Index = (int)hv_Index + 1)
            {
                hv_Directories = hv_Directories.TupleConcat(((hv_HalconImages.TupleSelect(hv_Index)) + "/") + hv_ImageDirectory_COPY_INP_TMP);
            }
            HOperatorSet.TupleStrlen(hv_Directories, out hv_Length);
            HOperatorSet.TupleGenConst(new HTuple(hv_Length.TupleLength()), 0, out hv_network_drive);
            if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Win"))) != 0)
            {
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_Length.TupleLength())) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)(new HTuple(((((hv_Directories.TupleSelect(hv_Index))).TupleStrlen()
                        )).TupleGreater(1))) != 0)
                    {
                        HOperatorSet.TupleStrFirstN(hv_Directories.TupleSelect(hv_Index), 1, out hv_Substring);
                        if ((int)(new HTuple(hv_Substring.TupleEqual("//"))) != 0)
                        {
                            if (hv_network_drive == null)
                                hv_network_drive = new HTuple();
                            hv_network_drive[hv_Index] = 1;
                        }
                    }
                }
            }
            hv_ImageFiles = new HTuple();
            for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_Directories.TupleLength()
                )) - 1); hv_Index = (int)hv_Index + 1)
            {
                HOperatorSet.FileExists(hv_Directories.TupleSelect(hv_Index), out hv_FileExists);
                if ((int)(hv_FileExists) != 0)
                {
                    HOperatorSet.ListFiles(hv_Directories.TupleSelect(hv_Index), (new HTuple("files")).TupleConcat(
                        hv_Options), out hv_AllFiles);
                    hv_ImageFiles = new HTuple();
                    for (hv_i = 0; (int)hv_i <= (int)((new HTuple(hv_Extensions_COPY_INP_TMP.TupleLength()
                        )) - 1); hv_i = (int)hv_i + 1)
                    {
                        HOperatorSet.TupleRegexpSelect(hv_AllFiles, (((".*" + (hv_Extensions_COPY_INP_TMP.TupleSelect(
                            hv_i))) + "$")).TupleConcat("ignore_case"), out hv_Selection);
                        hv_ImageFiles = hv_ImageFiles.TupleConcat(hv_Selection);
                    }
                    HOperatorSet.TupleRegexpReplace(hv_ImageFiles, (new HTuple("\\\\")).TupleConcat(
                        "replace_all"), "/", out hv_ImageFiles);
                    if ((int)(hv_network_drive.TupleSelect(hv_Index)) != 0)
                    {
                        HOperatorSet.TupleRegexpReplace(hv_ImageFiles, (new HTuple("//")).TupleConcat(
                            "replace_all"), "/", out hv_ImageFiles);
                        hv_ImageFiles = "/" + hv_ImageFiles;
                    }
                    else
                    {
                        HOperatorSet.TupleRegexpReplace(hv_ImageFiles, (new HTuple("//")).TupleConcat(
                            "replace_all"), "/", out hv_ImageFiles);
                    }

                    return;
                }
            }

            return;
        }

        // 创建一个箭头形状的XLD轮廓。
        public void gen_arrow_contour_xld(out HObject ho_Arrow, HTuple hv_Row1, HTuple hv_Column1,
            HTuple hv_Row2, HTuple hv_Column2, HTuple hv_HeadLength, HTuple hv_HeadWidth)
        {



            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_TempArrow = null;

            // Local control variables 

            HTuple hv_Length = null, hv_ZeroLengthIndices = null;
            HTuple hv_DR = null, hv_DC = null, hv_HalfHeadWidth = null;
            HTuple hv_RowP1 = null, hv_ColP1 = null, hv_RowP2 = null;
            HTuple hv_ColP2 = null, hv_Index = null;
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Arrow);
            HOperatorSet.GenEmptyObj(out ho_TempArrow);
            //This procedure generates arrow shaped XLD contours,
            //pointing from (Row1, Column1) to (Row2, Column2).
            //If starting and end point are identical, a contour consisting
            //of a single point is returned.
            //
            //input parameteres:
            //Row1, Column1: Coordinates of the arrows' starting points
            //Row2, Column2: Coordinates of the arrows' end points
            //HeadLength, HeadWidth: Size of the arrow heads in pixels
            //
            //output parameter:
            //Arrow: The resulting XLD contour
            //
            //The input tuples Row1, Column1, Row2, and Column2 have to be of
            //the same length.
            //HeadLength and HeadWidth either have to be of the same length as
            //Row1, Column1, Row2, and Column2 or have to be a single element.
            //If one of the above restrictions is violated, an error will occur.
            //
            //
            //Init
            ho_Arrow.Dispose();
            HOperatorSet.GenEmptyObj(out ho_Arrow);
            //
            //Calculate the arrow length
            HOperatorSet.DistancePp(hv_Row1, hv_Column1, hv_Row2, hv_Column2, out hv_Length);
            //
            //Mark arrows with identical start and end point
            //(set Length to -1 to avoid division-by-zero exception)
            hv_ZeroLengthIndices = hv_Length.TupleFind(0);
            if ((int)(new HTuple(hv_ZeroLengthIndices.TupleNotEqual(-1))) != 0)
            {
                if (hv_Length == null)
                    hv_Length = new HTuple();
                hv_Length[hv_ZeroLengthIndices] = -1;
            }
            //
            //Calculate auxiliary variables.
            hv_DR = (1.0 * (hv_Row2 - hv_Row1)) / hv_Length;
            hv_DC = (1.0 * (hv_Column2 - hv_Column1)) / hv_Length;
            hv_HalfHeadWidth = hv_HeadWidth / 2.0;
            //
            //Calculate end points of the arrow head.
            hv_RowP1 = (hv_Row1 + ((hv_Length - hv_HeadLength) * hv_DR)) + (hv_HalfHeadWidth * hv_DC);
            hv_ColP1 = (hv_Column1 + ((hv_Length - hv_HeadLength) * hv_DC)) - (hv_HalfHeadWidth * hv_DR);
            hv_RowP2 = (hv_Row1 + ((hv_Length - hv_HeadLength) * hv_DR)) - (hv_HalfHeadWidth * hv_DC);
            hv_ColP2 = (hv_Column1 + ((hv_Length - hv_HeadLength) * hv_DC)) + (hv_HalfHeadWidth * hv_DR);
            //
            //Finally create output XLD contour for each input point pair
            for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_Length.TupleLength())) - 1); hv_Index = (int)hv_Index + 1)
            {
                if ((int)(new HTuple(((hv_Length.TupleSelect(hv_Index))).TupleEqual(-1))) != 0)
                {
                    //Create_ single points for arrows with identical start and end point
                    ho_TempArrow.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_TempArrow, hv_Row1.TupleSelect(hv_Index),
                        hv_Column1.TupleSelect(hv_Index));
                }
                else
                {
                    //Create arrow contour
                    ho_TempArrow.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_TempArrow, ((((((((((hv_Row1.TupleSelect(
                        hv_Index))).TupleConcat(hv_Row2.TupleSelect(hv_Index)))).TupleConcat(
                        hv_RowP1.TupleSelect(hv_Index)))).TupleConcat(hv_Row2.TupleSelect(hv_Index)))).TupleConcat(
                        hv_RowP2.TupleSelect(hv_Index)))).TupleConcat(hv_Row2.TupleSelect(hv_Index)),
                        ((((((((((hv_Column1.TupleSelect(hv_Index))).TupleConcat(hv_Column2.TupleSelect(
                        hv_Index)))).TupleConcat(hv_ColP1.TupleSelect(hv_Index)))).TupleConcat(
                        hv_Column2.TupleSelect(hv_Index)))).TupleConcat(hv_ColP2.TupleSelect(
                        hv_Index)))).TupleConcat(hv_Column2.TupleSelect(hv_Index)));
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Arrow, ho_TempArrow, out ExpTmpOutVar_0);
                    ho_Arrow.Dispose();
                    ho_Arrow = ExpTmpOutVar_0;
                }
            }
            ho_TempArrow.Dispose();

            return;
        }

        //打开一个新的图形窗口，保留给定图像的宽宽比(显示图像大小自适应，参3、4为极大值)。
        public void dev_open_window_fit_image(HObject ho_Image, HTuple hv_Row, HTuple hv_Column,
            HTuple hv_WidthLimit, HTuple hv_HeightLimit, out HTuple hv_WindowHandle, HWindowControl hWindowCCD)
        {

            // Local iconic variables 

            // Local control variables 

            HTuple hv_MinWidth = new HTuple(), hv_MaxWidth = new HTuple();
            HTuple hv_MinHeight = new HTuple(), hv_MaxHeight = new HTuple();
            HTuple hv_ResizeFactor = null, hv_ImageWidth = null, hv_ImageHeight = null;
            HTuple hv_TempWidth = null, hv_TempHeight = null, hv_WindowWidth = null;
            HTuple hv_WindowHeight = null;
            // Initialize local and output iconic variables 
            //This procedure opens a new graphics window and adjusts the size
            //such that it fits into the limits specified by WidthLimit
            //and HeightLimit, but also maintains the correct image aspect ratio.
            //
            //If it is impossible to match the minimum and maximum extent requirements
            //at the same time (f.e. if the image is very long but narrow),
            //the maximum value gets a higher priority,
            //
            //Parse input tuple WidthLimit
            if ((int)((new HTuple((new HTuple(hv_WidthLimit.TupleLength())).TupleEqual(0))).TupleOr(
                new HTuple(hv_WidthLimit.TupleLess(0)))) != 0)
            {
                hv_MinWidth = 500;
                hv_MaxWidth = 800;
            }
            else if ((int)(new HTuple((new HTuple(hv_WidthLimit.TupleLength())).TupleEqual(
                1))) != 0)
            {
                hv_MinWidth = 0;
                hv_MaxWidth = hv_WidthLimit.Clone();
            }
            else
            {
                hv_MinWidth = hv_WidthLimit[0];
                hv_MaxWidth = hv_WidthLimit[1];
            }
            //Parse input tuple HeightLimit
            if ((int)((new HTuple((new HTuple(hv_HeightLimit.TupleLength())).TupleEqual(0))).TupleOr(
                new HTuple(hv_HeightLimit.TupleLess(0)))) != 0)
            {
                hv_MinHeight = 400;
                hv_MaxHeight = 600;
            }
            else if ((int)(new HTuple((new HTuple(hv_HeightLimit.TupleLength())).TupleEqual(
                1))) != 0)
            {
                hv_MinHeight = 0;
                hv_MaxHeight = hv_HeightLimit.Clone();
            }
            else
            {
                hv_MinHeight = hv_HeightLimit[0];
                hv_MaxHeight = hv_HeightLimit[1];
            }
            //
            //Test, if window size has to be changed.
            hv_ResizeFactor = 1;
            HOperatorSet.GetImageSize(ho_Image, out hv_ImageWidth, out hv_ImageHeight);
            //First, expand window to the minimum extents (if necessary).
            if ((int)((new HTuple(hv_MinWidth.TupleGreater(hv_ImageWidth))).TupleOr(new HTuple(hv_MinHeight.TupleGreater(
                hv_ImageHeight)))) != 0)
            {
                hv_ResizeFactor = (((((hv_MinWidth.TupleReal()) / hv_ImageWidth)).TupleConcat(
                    (hv_MinHeight.TupleReal()) / hv_ImageHeight))).TupleMax();
            }
            hv_TempWidth = hv_ImageWidth * hv_ResizeFactor;
            hv_TempHeight = hv_ImageHeight * hv_ResizeFactor;
            //Then, shrink window to maximum extents (if necessary).
            if ((int)((new HTuple(hv_MaxWidth.TupleLess(hv_TempWidth))).TupleOr(new HTuple(hv_MaxHeight.TupleLess(
                hv_TempHeight)))) != 0)
            {
                hv_ResizeFactor = hv_ResizeFactor * ((((((hv_MaxWidth.TupleReal()) / hv_TempWidth)).TupleConcat(
                    (hv_MaxHeight.TupleReal()) / hv_TempHeight))).TupleMin());
            }
            hv_WindowWidth = hv_ImageWidth * hv_ResizeFactor;
            hv_WindowHeight = hv_ImageHeight * hv_ResizeFactor;
            //Resize window
            HOperatorSet.SetWindowAttr("background_color", "black");
            HOperatorSet.OpenWindow(hv_Row, hv_Column, hv_WindowWidth, hv_WindowHeight, hWindowCCD.HalconWindow, "visible", "", out hv_WindowHandle);
            HDevWindowStack.Push(hv_WindowHandle);
            if (HDevWindowStack.IsOpen())
            {
                HOperatorSet.SetPart(HDevWindowStack.GetActive(), 0, 0, hv_ImageHeight - 1, hv_ImageWidth - 1);
            }

            return;
        }

        //打开一个新的图形窗口，保留给定图像大小的宽比(显示图像大小有参3、4决定，5、6为极大值)。
        public void dev_open_window_fit_size(HTuple hv_Row, HTuple hv_Column, HTuple hv_Width,
            HTuple hv_Height, HTuple hv_WidthLimit, HTuple hv_HeightLimit, out HTuple hv_WindowHandle, HWindowControl hWindowCCD)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_MinWidth = new HTuple(), hv_MaxWidth = new HTuple();
            HTuple hv_MinHeight = new HTuple(), hv_MaxHeight = new HTuple();
            HTuple hv_ResizeFactor = null, hv_TempWidth = null, hv_TempHeight = null;
            HTuple hv_WindowWidth = null, hv_WindowHeight = null;
            // Initialize local and output iconic variables 
            //This procedure open a new graphic window
            //such that it fits into the limits specified by WidthLimit
            //and HeightLimit, but also maintains the correct aspect ratio
            //given by Width and Height.
            //
            //If it is impossible to match the minimum and maximum extent requirements
            //at the same time (f.e. if the image is very long but narrow),
            //the maximum value gets a higher priority.
            //
            //Parse input tuple WidthLimit
            if ((int)((new HTuple((new HTuple(hv_WidthLimit.TupleLength())).TupleEqual(0))).TupleOr(
                new HTuple(hv_WidthLimit.TupleLess(0)))) != 0)
            {
                hv_MinWidth = 500;
                hv_MaxWidth = 800;
            }
            else if ((int)(new HTuple((new HTuple(hv_WidthLimit.TupleLength())).TupleEqual(
                1))) != 0)
            {
                hv_MinWidth = 0;
                hv_MaxWidth = hv_WidthLimit.Clone();
            }
            else
            {
                hv_MinWidth = hv_WidthLimit[0];
                hv_MaxWidth = hv_WidthLimit[1];
            }
            //Parse input tuple HeightLimit
            if ((int)((new HTuple((new HTuple(hv_HeightLimit.TupleLength())).TupleEqual(0))).TupleOr(
                new HTuple(hv_HeightLimit.TupleLess(0)))) != 0)
            {
                hv_MinHeight = 400;
                hv_MaxHeight = 600;
            }
            else if ((int)(new HTuple((new HTuple(hv_HeightLimit.TupleLength())).TupleEqual(
                1))) != 0)
            {
                hv_MinHeight = 0;
                hv_MaxHeight = hv_HeightLimit.Clone();
            }
            else
            {
                hv_MinHeight = hv_HeightLimit[0];
                hv_MaxHeight = hv_HeightLimit[1];
            }
            //
            //Test, if window size has to be changed.
            hv_ResizeFactor = 1;
            //First, expand window to the minimum extents (if necessary).
            if ((int)((new HTuple(hv_MinWidth.TupleGreater(hv_Width))).TupleOr(new HTuple(hv_MinHeight.TupleGreater(
                hv_Height)))) != 0)
            {
                hv_ResizeFactor = (((((hv_MinWidth.TupleReal()) / hv_Width)).TupleConcat((hv_MinHeight.TupleReal()
                    ) / hv_Height))).TupleMax();
            }
            hv_TempWidth = hv_Width * hv_ResizeFactor;
            hv_TempHeight = hv_Height * hv_ResizeFactor;
            //Then, shrink window to maximum extents (if necessary).
            if ((int)((new HTuple(hv_MaxWidth.TupleLess(hv_TempWidth))).TupleOr(new HTuple(hv_MaxHeight.TupleLess(
                hv_TempHeight)))) != 0)
            {
                hv_ResizeFactor = hv_ResizeFactor * ((((((hv_MaxWidth.TupleReal()) / hv_TempWidth)).TupleConcat(
                    (hv_MaxHeight.TupleReal()) / hv_TempHeight))).TupleMin());
            }
            hv_WindowWidth = hv_Width * hv_ResizeFactor;
            hv_WindowHeight = hv_Height * hv_ResizeFactor;
            //Resize window
            HOperatorSet.SetWindowAttr("background_color", "black");
            HOperatorSet.OpenWindow(hv_Row, hv_Column, hv_WindowWidth, hv_WindowHeight, hWindowCCD.HalconWindow, "visible", "", out hv_WindowHandle);
            HDevWindowStack.Push(hv_WindowHandle);
            if (HDevWindowStack.IsOpen())
            {
                HOperatorSet.SetPart(HDevWindowStack.GetActive(), 0, 0, hv_Height - 1, hv_Width - 1);
            }

            return;
        }

        //(内有短暂改写setPart不可高频使用)这个程序绘制在坐标系中表示函数或曲线的元组。
        //if (HDevWindowStack.IsOpen())是否打开窗口的判断结果关闭，造成一些语句无法执行
        public void plot_tuple(HTuple hv_WindowHandle, HTuple hv_XValues, HTuple hv_YValues,
            HTuple hv_XLabel, HTuple hv_YLabel, HTuple hv_Color, HTuple hv_GenParamNames,
            HTuple hv_GenParamValues)
        {


            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_ContourXGrid = null, ho_ContourYGrid = null;
            HObject ho_XArrow = null, ho_YArrow = null, ho_ContourXTick = null;
            HObject ho_ContourYTick = null, ho_Contour = null, ho_Cross = null;
            HObject ho_Filled = null;

            // Local control variables 

            HTuple hv_PreviousWindowHandle = new HTuple();
            HTuple hv_ClipRegion = null, hv_Row = null, hv_Column = null;
            HTuple hv_Width = null, hv_Height = null, hv_PartRow1 = null;
            HTuple hv_PartColumn1 = null, hv_PartRow2 = null, hv_PartColumn2 = null;
            HTuple hv_Red = null, hv_Green = null, hv_Blue = null;
            HTuple hv_DrawMode = null, hv_OriginStyle = null, hv_XAxisEndValue = new HTuple();
            HTuple hv_YAxisEndValue = new HTuple(), hv_XAxisStartValue = new HTuple();
            HTuple hv_YAxisStartValue = new HTuple(), hv_XValuesAreStrings = new HTuple();
            HTuple hv_XTickValues = new HTuple(), hv_XTicks = null;
            HTuple hv_OriginX = null, hv_OriginY = null, hv_LeftBorder = null;
            HTuple hv_RightBorder = null, hv_UpperBorder = null, hv_LowerBorder = null;
            HTuple hv_AxesColor = null, hv_Style = null, hv_Clip = null;
            HTuple hv_YTicks = null, hv_XGrid = null, hv_YGrid = null;
            HTuple hv_GridColor = null, hv_NumGenParamNames = null;
            HTuple hv_NumGenParamValues = null, hv_SetOriginXToDefault = null;
            HTuple hv_SetOriginYToDefault = null, hv_GenParamIndex = null;
            HTuple hv_XGridTicks = new HTuple(), hv_XAxisWidthPx = null;
            HTuple hv_XAxisWidth = null, hv_XScaleFactor = null, hv_YAxisHeightPx = null;
            HTuple hv_YAxisHeight = null, hv_YScaleFactor = null, hv_YAxisOffsetPx = null;
            HTuple hv_XAxisOffsetPx = null, hv_DotStyle = new HTuple();
            HTuple hv_XGridValues = new HTuple(), hv_XGridStart = new HTuple();
            HTuple hv_XPosition = new HTuple(), hv_IndexGrid = new HTuple();
            HTuple hv_YGridValues = new HTuple(), hv_YGridStart = new HTuple();
            HTuple hv_YPosition = new HTuple(), hv_Ascent = new HTuple();
            HTuple hv_Descent = new HTuple(), hv_TextWidthXLabel = new HTuple();
            HTuple hv_TextHeightXLabel = new HTuple(), hv_XTickStart = new HTuple();
            HTuple hv_TypeTicks = new HTuple(), hv_IndexTicks = new HTuple();
            HTuple hv_YTickValues = new HTuple(), hv_YTickStart = new HTuple();
            HTuple hv_Ascent1 = new HTuple(), hv_Descent1 = new HTuple();
            HTuple hv_TextWidthYTicks = new HTuple(), hv_TextHeightYTicks = new HTuple();
            HTuple hv_Num = new HTuple(), hv_I = new HTuple(), hv_YSelected = new HTuple();
            HTuple hv_Y1Selected = new HTuple(), hv_X1Selected = new HTuple();
            HTuple hv_XValues_COPY_INP_TMP = hv_XValues.Clone();
            HTuple hv_YValues_COPY_INP_TMP = hv_YValues.Clone();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ContourXGrid);
            HOperatorSet.GenEmptyObj(out ho_ContourYGrid);
            HOperatorSet.GenEmptyObj(out ho_XArrow);
            HOperatorSet.GenEmptyObj(out ho_YArrow);
            HOperatorSet.GenEmptyObj(out ho_ContourXTick);
            HOperatorSet.GenEmptyObj(out ho_ContourYTick);
            HOperatorSet.GenEmptyObj(out ho_Contour);
            HOperatorSet.GenEmptyObj(out ho_Cross);
            HOperatorSet.GenEmptyObj(out ho_Filled);
            try
            {
                //This procedure plots tuples representing functions
                //or curves in a coordinate system.
                //
                //Input parameters:
                //
                //XValues: X values of the function to be plotted
                //         If XValues is set to [], it is interally set to 0,1,2,...,|YValues|-1.
                //         If XValues is a tuple of strings, the values are taken as categories.
                //
                //YValues: Y values of the function(s) to be plotted
                //         If YValues is set to [], it is interally set to 0,1,2,...,|XValues|-1.
                //         The number of y values must be equal to the number of x values
                //         or an integral multiple. In the latter case,
                //         multiple functions are plotted, that share the same x values.
                //
                //XLabel: X axis label
                //
                //XLabel: Y axis label
                //
                //Color: Color of the plotted function
                //       If [] is given, the currently set display color is used.
                //       If 'none is given, the function is not plotted, but only
                //       the coordinate axes as specified.
                //       If more than one color is given, multiple functions
                //       can be displayed in different colors.
                //
                //GenParamNames: Generic parameters to control the presentation
                //               Possible Values:
                //   'axes_color': coordinate system color
                //                 Default: 'white'
                //                 If 'none' is given, no coordinate system is shown.
                //   'style': Graph style
                //            Possible values: 'line' (default), 'cross', 'filled'
                //   'clip': Clip graph to coordinate system area
                //           Possibile values: 'yes', 'no' (default)
                //   'ticks': Control display of ticks on the axes
                //            If 'min_max_origin' is given (default), ticks are shown
                //            at the minimum and maximum values of the axes and at the
                //            intercept point of x- and y-axis.
                //            If 'none' is given, no ticks are shown.
                //            If any number != 0 is given, it is interpreted as distance
                //            between the ticks.
                //   'ticks_x': Control display of ticks on x-axis only
                //   'ticks_y': Control display of ticks on y-axis only
                //   'grid': Control display of grid lines within the coordinate system
                //           If 'min_max_origin' is given (default), grid lines are shown
                //           at the minimum and maximum values of the axes.
                //           If 'none' is given, no grid lines are shown.
                //           If any number != 0 is given, it is interpreted as distance
                //           between the grid lines.
                //   'grid_x': Control display of grid lines for the x-axis only
                //   'grid_y': Control display of grid lines for the y-axis only
                //   'grid_color': Color of the grid (default: 'dim gray')
                //   'margin': The distance in pixels of the coordinate system area
                //             to all four window borders.
                //   'margin_left': The distance in pixels of the coordinate system area
                //                  to the left window border.
                //   'margin_right': The distance in pixels of the coordinate system area
                //                   to the right window border.
                //   'margin_top': The distance in pixels of the coordinate system area
                //                 to the upper window border.
                //   'margin_bottom': The distance in pixels of the coordinate system area
                //                    to the lower window border.
                //   'start_x': Lowest x value of the x axis
                //              Default: min(XValues)
                //   'end_x': Highest x value of the x axis
                //            Default: max(XValues)
                //   'start_y': Lowest y value of the x axis
                //              Default: min(YValues)
                //   'end_y': Highest y value of the x axis
                //            Default: max(YValues)
                //   'origin_x': X coordinate of the intercept point of x- and y-axis.
                //               Default: same as start_x
                //   'origin_y': Y coordinate of the intercept point of x- and y-axis.
                //               Default: same as start_y
                //
                //GenParamValues: Values of the generic parameters of GenericParamNames
                //
                //
                //Store current display settings
                if (HDevWindowStack.IsOpen())
                {
                    hv_PreviousWindowHandle = HDevWindowStack.GetActive();
                }
                HDevWindowStack.SetActive(hv_WindowHandle);
                HOperatorSet.GetSystem("clip_region", out hv_ClipRegion);
                HOperatorSet.GetWindowExtents(hv_WindowHandle, out hv_Row, out hv_Column, out hv_Width,
                    out hv_Height);
                HOperatorSet.GetPart(hv_WindowHandle, out hv_PartRow1, out hv_PartColumn1,
                    out hv_PartRow2, out hv_PartColumn2);
                HOperatorSet.GetRgb(hv_WindowHandle, out hv_Red, out hv_Green, out hv_Blue);
                HOperatorSet.GetDraw(hv_WindowHandle, out hv_DrawMode);
                HOperatorSet.GetLineStyle(hv_WindowHandle, out hv_OriginStyle);
                //
                //Set display parameters
                HOperatorSet.SetLineStyle(hv_WindowHandle, new HTuple());
                HOperatorSet.SetSystem("clip_region", "false");
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetPart(HDevWindowStack.GetActive(), 0, 0, hv_Height - 1, hv_Width - 1);
                }
                //
                //Check input coordinates
                //
                if ((int)((new HTuple(hv_XValues_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleAnd(
                    new HTuple(hv_YValues_COPY_INP_TMP.TupleEqual(new HTuple())))) != 0)
                {
                    //Neither XValues nor YValues are given:
                    //Set axes to interval [0,1]
                    hv_XAxisEndValue = 1;
                    hv_YAxisEndValue = 1;
                    hv_XAxisStartValue = 0;
                    hv_YAxisStartValue = 0;
                    hv_XValuesAreStrings = 0;
                }
                else
                {
                    if ((int)(new HTuple(hv_XValues_COPY_INP_TMP.TupleEqual(new HTuple()))) != 0)
                    {
                        //XValues are omitted:
                        //Set equidistant XValues
                        hv_XValues_COPY_INP_TMP = HTuple.TupleGenSequence(0, (new HTuple(hv_YValues_COPY_INP_TMP.TupleLength()
                            )) - 1, 1);
                        hv_XValuesAreStrings = 0;
                    }
                    else if ((int)(new HTuple(hv_YValues_COPY_INP_TMP.TupleEqual(new HTuple()))) != 0)
                    {
                        //YValues are omitted:
                        //Set equidistant YValues
                        hv_YValues_COPY_INP_TMP = HTuple.TupleGenSequence(0, (new HTuple(hv_XValues_COPY_INP_TMP.TupleLength()
                            )) - 1, 1);
                    }
                    if ((int)(new HTuple((new HTuple((new HTuple(hv_YValues_COPY_INP_TMP.TupleLength()
                        )) % (new HTuple(hv_XValues_COPY_INP_TMP.TupleLength())))).TupleNotEqual(
                        0))) != 0)
                    {
                        //Number of YValues does not match number of XValues
                        throw new HalconException("Number of YValues is no multiple of the number of XValues!");
                        //ho_ContourXGrid.Dispose();
                        //ho_ContourYGrid.Dispose();
                        //ho_XArrow.Dispose();
                        //ho_YArrow.Dispose();
                        //ho_ContourXTick.Dispose();
                        //ho_ContourYTick.Dispose();
                        //ho_Contour.Dispose();
                        //ho_Cross.Dispose();
                        //ho_Filled.Dispose();

                        //return;
                    }
                    hv_XValuesAreStrings = hv_XValues_COPY_INP_TMP.TupleIsStringElem();
                    hv_XValuesAreStrings = new HTuple(((hv_XValuesAreStrings.TupleSum())).TupleEqual(
                        new HTuple(hv_XValuesAreStrings.TupleLength())));
                    if ((int)(hv_XValuesAreStrings) != 0)
                    {
                        //XValues are given as strings:
                        //Show XValues as ticks
                        hv_XTickValues = hv_XValues_COPY_INP_TMP.Clone();
                        hv_XTicks = 1;
                        //Set x-axis dimensions
                        hv_XValues_COPY_INP_TMP = HTuple.TupleGenSequence(1, new HTuple(hv_XValues_COPY_INP_TMP.TupleLength()
                            ), 1);
                    }
                    //Set default x-axis dimensions
                    if ((int)(new HTuple((new HTuple(hv_XValues_COPY_INP_TMP.TupleLength())).TupleGreater(
                        1))) != 0)
                    {
                        hv_XAxisStartValue = hv_XValues_COPY_INP_TMP.TupleMin();
                        hv_XAxisEndValue = hv_XValues_COPY_INP_TMP.TupleMax();
                    }
                    else
                    {
                        hv_XAxisEndValue = (hv_XValues_COPY_INP_TMP.TupleSelect(0)) + 0.5;
                        hv_XAxisStartValue = (hv_XValues_COPY_INP_TMP.TupleSelect(0)) - 0.5;
                    }
                }
                //Set default y-axis dimensions
                if ((int)(new HTuple((new HTuple(hv_YValues_COPY_INP_TMP.TupleLength())).TupleGreater(
                    1))) != 0)
                {
                    hv_YAxisStartValue = hv_YValues_COPY_INP_TMP.TupleMin();
                    hv_YAxisEndValue = hv_YValues_COPY_INP_TMP.TupleMax();
                }
                else if ((int)(new HTuple((new HTuple(hv_YValues_COPY_INP_TMP.TupleLength()
                    )).TupleEqual(1))) != 0)
                {
                    hv_YAxisStartValue = (hv_YValues_COPY_INP_TMP.TupleSelect(0)) - 0.5;
                    hv_YAxisEndValue = (hv_YValues_COPY_INP_TMP.TupleSelect(0)) + 0.5;
                }
                else
                {
                    hv_YAxisStartValue = 0;
                    hv_YAxisEndValue = 1;
                }
                //Set default interception point of x- and y- axis
                hv_OriginX = hv_XAxisStartValue.Clone();
                hv_OriginY = hv_YAxisStartValue.Clone();
                //
                //Set more defaults
                hv_LeftBorder = hv_Width * 0.1;
                hv_RightBorder = hv_Width * 0.1;
                hv_UpperBorder = hv_Height * 0.1;
                hv_LowerBorder = hv_Height * 0.1;
                hv_AxesColor = "white";
                hv_Style = "line";
                hv_Clip = "no";
                hv_XTicks = "min_max_origin";
                hv_YTicks = "min_max_origin";
                hv_XGrid = "none";
                hv_YGrid = "none";
                hv_GridColor = "dim gray";
                //
                //Parse generic parameters
                //
                hv_NumGenParamNames = new HTuple(hv_GenParamNames.TupleLength());
                hv_NumGenParamValues = new HTuple(hv_GenParamValues.TupleLength());
                if ((int)(new HTuple(hv_NumGenParamNames.TupleNotEqual(hv_NumGenParamValues))) != 0)
                {
                    throw new HalconException("Number of generic parameter names does not match generic parameter values!");
                    //ho_ContourXGrid.Dispose();
                    //ho_ContourYGrid.Dispose();
                    //ho_XArrow.Dispose();
                    //ho_YArrow.Dispose();
                    //ho_ContourXTick.Dispose();
                    //ho_ContourYTick.Dispose();
                    //ho_Contour.Dispose();
                    //ho_Cross.Dispose();
                    //ho_Filled.Dispose();

                    //return;
                }
                //
                hv_SetOriginXToDefault = 1;
                hv_SetOriginYToDefault = 1;
                for (hv_GenParamIndex = 0; (int)hv_GenParamIndex <= (int)((new HTuple(hv_GenParamNames.TupleLength()
                    )) - 1); hv_GenParamIndex = (int)hv_GenParamIndex + 1)
                {
                    //
                    //Set 'axes_color'
                    if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "axes_color"))) != 0)
                    {
                        hv_AxesColor = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        //
                        //Set 'style'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "style"))) != 0)
                    {
                        hv_Style = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        //
                        //Set 'clip'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "clip"))) != 0)
                    {
                        hv_Clip = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        if ((int)((new HTuple(hv_Clip.TupleNotEqual("yes"))).TupleAnd(new HTuple(hv_Clip.TupleNotEqual(
                            "no")))) != 0)
                        {
                            throw new HalconException(("Unsupported clipping option: '" + hv_Clip) + "'");
                        }
                        //
                        //Set 'ticks'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "ticks"))) != 0)
                    {
                        hv_XTicks = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        hv_YTicks = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        //
                        //Set 'ticks_x'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "ticks_x"))) != 0)
                    {
                        hv_XTicks = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        //
                        //Set 'ticks_y'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "ticks_y"))) != 0)
                    {
                        hv_YTicks = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        //
                        //Set 'grid'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "grid"))) != 0)
                    {
                        hv_XGrid = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        hv_YGrid = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        hv_XGridTicks = hv_XTicks.Clone();
                        //
                        //Set 'grid_x'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "grid_x"))) != 0)
                    {
                        hv_XGrid = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        //
                        //Set 'grid_y'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "grid_y"))) != 0)
                    {
                        hv_YGrid = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        //
                        //Set 'grid_color'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "grid_color"))) != 0)
                    {
                        hv_GridColor = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        //
                        //Set 'start_x'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "start_x"))) != 0)
                    {
                        hv_XAxisStartValue = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        //
                        //Set 'end_x'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "end_x"))) != 0)
                    {
                        hv_XAxisEndValue = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        //
                        //Set 'start_y'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "start_y"))) != 0)
                    {
                        hv_YAxisStartValue = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        //
                        //Set 'end_y'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "end_y"))) != 0)
                    {
                        hv_YAxisEndValue = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        //
                        //Set 'origin_x'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "origin_x"))) != 0)
                    {
                        hv_OriginX = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        hv_SetOriginXToDefault = 0;
                        //
                        //Set 'origin_y'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "origin_y"))) != 0)
                    {
                        hv_OriginY = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        hv_SetOriginYToDefault = 0;
                        //
                        //Set 'margin'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "margin"))) != 0)
                    {
                        hv_LeftBorder = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        hv_RightBorder = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        hv_UpperBorder = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        hv_LowerBorder = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        //
                        //Set 'margin_left'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "margin_left"))) != 0)
                    {
                        hv_LeftBorder = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        //
                        //Set 'margin_right'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "margin_right"))) != 0)
                    {
                        hv_RightBorder = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        //
                        //Set 'margin_top'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "margin_top"))) != 0)
                    {
                        hv_UpperBorder = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                        //
                        //Set 'margin_bottom'
                    }
                    else if ((int)(new HTuple(((hv_GenParamNames.TupleSelect(hv_GenParamIndex))).TupleEqual(
                        "margin_bottom"))) != 0)
                    {
                        hv_LowerBorder = hv_GenParamValues.TupleSelect(hv_GenParamIndex);
                    }
                    else
                    {
                        throw new HalconException(("Unknown generic parameter: '" + (hv_GenParamNames.TupleSelect(
                            hv_GenParamIndex))) + "'");
                    }
                }
                //
                //
                //Check consistency of start and end values
                //of the axes.
                if ((int)(new HTuple(hv_XAxisStartValue.TupleGreater(hv_XAxisEndValue))) != 0)
                {
                    throw new HalconException("Value for 'start_x' is greater than value for 'end_x'");
                }
                if ((int)(new HTuple(hv_YAxisStartValue.TupleGreater(hv_YAxisEndValue))) != 0)
                {
                    throw new HalconException("Value for 'start_y' is greater than value for 'end_y'");
                }
                //
                //Set default origin to lower left corner
                if ((int)(hv_SetOriginXToDefault) != 0)
                {
                    hv_OriginX = hv_XAxisStartValue.Clone();
                }
                if ((int)(hv_SetOriginYToDefault) != 0)
                {
                    hv_OriginY = hv_YAxisStartValue.Clone();
                }
                //
                //
                //Calculate basic pixel coordinates and scale factors
                //
                hv_XAxisWidthPx = (hv_Width - hv_LeftBorder) - hv_RightBorder;
                hv_XAxisWidth = hv_XAxisEndValue - hv_XAxisStartValue;
                if ((int)(new HTuple(hv_XAxisWidth.TupleEqual(0))) != 0)
                {
                    hv_XAxisStartValue = hv_XAxisStartValue - 0.5;
                    hv_XAxisEndValue = hv_XAxisEndValue + 0.5;
                    hv_XAxisWidth = 1;
                }
                hv_XScaleFactor = hv_XAxisWidthPx / (hv_XAxisWidth.TupleReal());
                hv_YAxisHeightPx = (hv_Height - hv_LowerBorder) - hv_UpperBorder;
                hv_YAxisHeight = hv_YAxisEndValue - hv_YAxisStartValue;
                if ((int)(new HTuple(hv_YAxisHeight.TupleEqual(0))) != 0)
                {
                    hv_YAxisStartValue = hv_YAxisStartValue - 0.5;
                    hv_YAxisEndValue = hv_YAxisEndValue + 0.5;
                    hv_YAxisHeight = 1;
                }
                hv_YScaleFactor = hv_YAxisHeightPx / (hv_YAxisHeight.TupleReal());
                hv_YAxisOffsetPx = (hv_OriginX - hv_XAxisStartValue) * hv_XScaleFactor;
                hv_XAxisOffsetPx = (hv_OriginY - hv_YAxisStartValue) * hv_YScaleFactor;
                //
                //Display grid lines
                //
                if ((int)(new HTuple(hv_GridColor.TupleNotEqual("none"))) != 0)
                {
                    hv_DotStyle = new HTuple();
                    hv_DotStyle[0] = 5;
                    hv_DotStyle[1] = 7;
                    HOperatorSet.SetLineStyle(hv_WindowHandle, hv_DotStyle);
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetColor(HDevWindowStack.GetActive(), hv_GridColor);
                    }
                    //
                    //Display x grid lines
                    if ((int)(new HTuple(hv_XGrid.TupleNotEqual("none"))) != 0)
                    {
                        if ((int)(new HTuple(hv_XGrid.TupleEqual("min_max_origin"))) != 0)
                        {
                            //Calculate 'min_max_origin' grid line coordinates
                            if ((int)(new HTuple(hv_OriginX.TupleEqual(hv_XAxisStartValue))) != 0)
                            {
                                hv_XGridValues = new HTuple();
                                hv_XGridValues = hv_XGridValues.TupleConcat(hv_XAxisStartValue);
                                hv_XGridValues = hv_XGridValues.TupleConcat(hv_XAxisEndValue);
                            }
                            else
                            {
                                hv_XGridValues = new HTuple();
                                hv_XGridValues = hv_XGridValues.TupleConcat(hv_XAxisStartValue);
                                hv_XGridValues = hv_XGridValues.TupleConcat(hv_OriginX);
                                hv_XGridValues = hv_XGridValues.TupleConcat(hv_XAxisEndValue);
                            }
                        }
                        else
                        {
                            //Calculate equidistant grid line coordinates
                            hv_XGridStart = (((hv_XAxisStartValue / hv_XGrid)).TupleCeil()) * hv_XGrid;
                            hv_XGridValues = HTuple.TupleGenSequence(hv_XGridStart, hv_XAxisEndValue,
                                hv_XGrid);
                        }
                        hv_XPosition = (hv_XGridValues - hv_XAxisStartValue) * hv_XScaleFactor;
                        //Generate and display grid lines
                        for (hv_IndexGrid = 0; (int)hv_IndexGrid <= (int)((new HTuple(hv_XGridValues.TupleLength()
                            )) - 1); hv_IndexGrid = (int)hv_IndexGrid + 1)
                        {
                            ho_ContourXGrid.Dispose();
                            HOperatorSet.GenContourPolygonXld(out ho_ContourXGrid, ((hv_Height - hv_LowerBorder)).TupleConcat(
                                hv_UpperBorder), ((hv_LeftBorder + (hv_XPosition.TupleSelect(hv_IndexGrid)))).TupleConcat(
                                hv_LeftBorder + (hv_XPosition.TupleSelect(hv_IndexGrid))));
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispObj(ho_ContourXGrid, HDevWindowStack.GetActive());
                            }
                        }
                    }
                    //
                    //Display y grid lines
                    if ((int)(new HTuple(hv_YGrid.TupleNotEqual("none"))) != 0)
                    {
                        if ((int)(new HTuple(hv_YGrid.TupleEqual("min_max_origin"))) != 0)
                        {
                            //Calculate 'min_max_origin' grid line coordinates
                            if ((int)(new HTuple(hv_OriginY.TupleEqual(hv_YAxisStartValue))) != 0)
                            {
                                hv_YGridValues = new HTuple();
                                hv_YGridValues = hv_YGridValues.TupleConcat(hv_YAxisStartValue);
                                hv_YGridValues = hv_YGridValues.TupleConcat(hv_YAxisEndValue);
                            }
                            else
                            {
                                hv_YGridValues = new HTuple();
                                hv_YGridValues = hv_YGridValues.TupleConcat(hv_YAxisStartValue);
                                hv_YGridValues = hv_YGridValues.TupleConcat(hv_OriginY);
                                hv_YGridValues = hv_YGridValues.TupleConcat(hv_YAxisEndValue);
                            }
                        }
                        else
                        {
                            //Calculate equidistant grid line coordinates
                            hv_YGridStart = (((hv_YAxisStartValue / hv_YGrid)).TupleCeil()) * hv_YGrid;
                            hv_YGridValues = HTuple.TupleGenSequence(hv_YGridStart, hv_YAxisEndValue,
                                hv_YGrid);
                        }
                        hv_YPosition = (hv_YGridValues - hv_YAxisStartValue) * hv_YScaleFactor;
                        //Generate and display grid lines
                        for (hv_IndexGrid = 0; (int)hv_IndexGrid <= (int)((new HTuple(hv_YGridValues.TupleLength()
                            )) - 1); hv_IndexGrid = (int)hv_IndexGrid + 1)
                        {
                            ho_ContourYGrid.Dispose();
                            HOperatorSet.GenContourPolygonXld(out ho_ContourYGrid, (((hv_Height - hv_LowerBorder) - (hv_YPosition.TupleSelect(
                                hv_IndexGrid)))).TupleConcat((hv_Height - hv_LowerBorder) - (hv_YPosition.TupleSelect(
                                hv_IndexGrid))), hv_LeftBorder.TupleConcat(hv_Width - hv_RightBorder));
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispObj(ho_ContourYGrid, HDevWindowStack.GetActive());
                            }
                        }
                    }
                }
                HOperatorSet.SetLineStyle(hv_WindowHandle, new HTuple());
                //
                //
                //Display the coordinate sytem axes
                if ((int)(new HTuple(hv_AxesColor.TupleNotEqual("none"))) != 0)
                {
                    //Display axes
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetColor(HDevWindowStack.GetActive(), hv_AxesColor);
                    }
                    ho_XArrow.Dispose();
                    gen_arrow_contour_xld(out ho_XArrow, (hv_Height - hv_LowerBorder) - hv_XAxisOffsetPx,
                        hv_LeftBorder, (hv_Height - hv_LowerBorder) - hv_XAxisOffsetPx, hv_Width - hv_RightBorder,
                        0, 0);
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.DispObj(ho_XArrow, HDevWindowStack.GetActive());
                    }
                    ho_YArrow.Dispose();
                    gen_arrow_contour_xld(out ho_YArrow, hv_Height - hv_LowerBorder, hv_LeftBorder + hv_YAxisOffsetPx,
                        hv_UpperBorder, hv_LeftBorder + hv_YAxisOffsetPx, 0, 0);
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.DispObj(ho_YArrow, HDevWindowStack.GetActive());
                    }
                    //Display labels
                    HOperatorSet.GetStringExtents(hv_WindowHandle, hv_XLabel, out hv_Ascent,
                        out hv_Descent, out hv_TextWidthXLabel, out hv_TextHeightXLabel);
                    disp_message(hv_WindowHandle, hv_XLabel, "window", ((hv_Height - hv_LowerBorder) - hv_TextHeightXLabel) - hv_XAxisOffsetPx,
                        ((hv_Width - hv_RightBorder) - hv_TextWidthXLabel) - 3, hv_AxesColor, "false");
                    disp_message(hv_WindowHandle, " " + hv_YLabel, "window", hv_UpperBorder, (hv_LeftBorder + 3) + hv_YAxisOffsetPx,
                        hv_AxesColor, "false");
                }
                //
                //Display ticks
                //
                if ((int)(new HTuple(hv_AxesColor.TupleNotEqual("none"))) != 0)
                {
                    if (HDevWindowStack.IsOpen())
                    {
                        HOperatorSet.SetColor(HDevWindowStack.GetActive(), hv_AxesColor);
                    }
                    if ((int)(new HTuple(hv_XTicks.TupleNotEqual("none"))) != 0)
                    {
                        //
                        //Display x ticks
                        if ((int)(hv_XValuesAreStrings) != 0)
                        {
                            //Display string XValues as categories
                            hv_XTicks = (new HTuple(hv_XValues_COPY_INP_TMP.TupleLength())) / (new HTuple(hv_XTickValues.TupleLength()
                                ));
                            hv_XPosition = (hv_XValues_COPY_INP_TMP - hv_XAxisStartValue) * hv_XScaleFactor;
                        }
                        else
                        {
                            //Display tick values
                            if ((int)(new HTuple(hv_XTicks.TupleEqual("min_max_origin"))) != 0)
                            {
                                //Calculate 'min_max_origin' tick coordinates
                                if ((int)(new HTuple(hv_OriginX.TupleEqual(hv_XAxisStartValue))) != 0)
                                {
                                    hv_XTickValues = new HTuple();
                                    hv_XTickValues = hv_XTickValues.TupleConcat(hv_XAxisStartValue);
                                    hv_XTickValues = hv_XTickValues.TupleConcat(hv_XAxisEndValue);
                                }
                                else
                                {
                                    hv_XTickValues = new HTuple();
                                    hv_XTickValues = hv_XTickValues.TupleConcat(hv_XAxisStartValue);
                                    hv_XTickValues = hv_XTickValues.TupleConcat(hv_OriginX);
                                    hv_XTickValues = hv_XTickValues.TupleConcat(hv_XAxisEndValue);
                                }
                            }
                            else
                            {
                                //Calculate equidistant tick coordinates
                                hv_XTickStart = (((hv_XAxisStartValue / hv_XTicks)).TupleCeil()) * hv_XTicks;
                                hv_XTickValues = HTuple.TupleGenSequence(hv_XTickStart, hv_XAxisEndValue,
                                    hv_XTicks);
                            }
                            hv_XPosition = (hv_XTickValues - hv_XAxisStartValue) * hv_XScaleFactor;
                            hv_TypeTicks = hv_XTicks.TupleType();
                            if ((int)(new HTuple(hv_TypeTicks.TupleEqual(4))) != 0)
                            {
                                //String ('min_max_origin')
                                //Format depends on actual values
                                hv_TypeTicks = hv_XTickValues.TupleType();
                            }
                            if ((int)(new HTuple(hv_TypeTicks.TupleEqual(1))) != 0)
                            {
                                //Round to integer
                                hv_XTickValues = hv_XTickValues.TupleInt();
                            }
                            else
                            {
                                //Use floating point numbers
                                hv_XTickValues = hv_XTickValues.TupleString(".2f");
                            }
                        }
                        //Generate and display ticks
                        for (hv_IndexTicks = 0; (int)hv_IndexTicks <= (int)((new HTuple(hv_XTickValues.TupleLength()
                            )) - 1); hv_IndexTicks = (int)hv_IndexTicks + 1)
                        {
                            ho_ContourXTick.Dispose();
                            HOperatorSet.GenContourPolygonXld(out ho_ContourXTick, (((hv_Height - hv_LowerBorder) - hv_XAxisOffsetPx)).TupleConcat(
                                ((hv_Height - hv_LowerBorder) - hv_XAxisOffsetPx) - 5), ((hv_LeftBorder + (hv_XPosition.TupleSelect(
                                hv_IndexTicks)))).TupleConcat(hv_LeftBorder + (hv_XPosition.TupleSelect(
                                hv_IndexTicks))));
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispObj(ho_ContourXTick, HDevWindowStack.GetActive());
                            }
                            disp_message(hv_WindowHandle, hv_XTickValues.TupleSelect(hv_IndexTicks),
                                "window", ((hv_Height - hv_LowerBorder) + 2) - hv_XAxisOffsetPx, hv_LeftBorder + (hv_XPosition.TupleSelect(
                                hv_IndexTicks)), hv_AxesColor, "false");
                        }
                    }
                    //
                    if ((int)(new HTuple(hv_YTicks.TupleNotEqual("none"))) != 0)
                    {
                        //
                        //Display y ticks
                        if ((int)(new HTuple(hv_YTicks.TupleEqual("min_max_origin"))) != 0)
                        {
                            //Calculate 'min_max_origin' tick coordinates
                            if ((int)(new HTuple(hv_OriginY.TupleEqual(hv_YAxisStartValue))) != 0)
                            {
                                hv_YTickValues = new HTuple();
                                hv_YTickValues = hv_YTickValues.TupleConcat(hv_YAxisStartValue);
                                hv_YTickValues = hv_YTickValues.TupleConcat(hv_YAxisEndValue);
                            }
                            else
                            {
                                hv_YTickValues = new HTuple();
                                hv_YTickValues = hv_YTickValues.TupleConcat(hv_YAxisStartValue);
                                hv_YTickValues = hv_YTickValues.TupleConcat(hv_OriginY);
                                hv_YTickValues = hv_YTickValues.TupleConcat(hv_YAxisEndValue);
                            }
                        }
                        else
                        {
                            //Calculate equidistant tick coordinates
                            hv_YTickStart = (((hv_YAxisStartValue / hv_YTicks)).TupleCeil()) * hv_YTicks;
                            hv_YTickValues = HTuple.TupleGenSequence(hv_YTickStart, hv_YAxisEndValue,
                                hv_YTicks);
                        }
                        hv_YPosition = (hv_YTickValues - hv_YAxisStartValue) * hv_YScaleFactor;
                        hv_TypeTicks = hv_YTicks.TupleType();
                        if ((int)(new HTuple(hv_TypeTicks.TupleEqual(4))) != 0)
                        {
                            //String ('min_max_origin')
                            //Format depends on actual values
                            hv_TypeTicks = hv_YTickValues.TupleType();
                        }
                        if ((int)(new HTuple(hv_TypeTicks.TupleEqual(1))) != 0)
                        {
                            //Round to integer
                            hv_YTickValues = hv_YTickValues.TupleInt();
                        }
                        else
                        {
                            //Use floating point numbers
                            hv_YTickValues = hv_YTickValues.TupleString(".2f");
                        }
                        //Generate and display ticks
                        for (hv_IndexTicks = 0; (int)hv_IndexTicks <= (int)((new HTuple(hv_YTickValues.TupleLength()
                            )) - 1); hv_IndexTicks = (int)hv_IndexTicks + 1)
                        {
                            ho_ContourYTick.Dispose();
                            HOperatorSet.GenContourPolygonXld(out ho_ContourYTick, (((hv_Height - hv_LowerBorder) - (hv_YPosition.TupleSelect(
                                hv_IndexTicks)))).TupleConcat((hv_Height - hv_LowerBorder) - (hv_YPosition.TupleSelect(
                                hv_IndexTicks))), ((hv_LeftBorder + hv_YAxisOffsetPx)).TupleConcat(
                                (hv_LeftBorder + hv_YAxisOffsetPx) + 5));
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispObj(ho_ContourYTick, HDevWindowStack.GetActive());
                            }
                            HOperatorSet.GetStringExtents(hv_WindowHandle, hv_YTickValues.TupleSelect(
                                hv_IndexTicks), out hv_Ascent1, out hv_Descent1, out hv_TextWidthYTicks,
                                out hv_TextHeightYTicks);
                            disp_message(hv_WindowHandle, hv_YTickValues.TupleSelect(hv_IndexTicks),
                                "window", (((hv_Height - hv_LowerBorder) - hv_TextHeightYTicks) + 3) - (hv_YPosition.TupleSelect(
                                hv_IndexTicks)), ((hv_LeftBorder - hv_TextWidthYTicks) - 2) + hv_YAxisOffsetPx,
                                hv_AxesColor, "false");
                        }
                    }
                }
                //
                //Display funtion plot
                //
                if ((int)(new HTuple(hv_Color.TupleNotEqual("none"))) != 0)
                {
                    if ((int)((new HTuple(hv_XValues_COPY_INP_TMP.TupleNotEqual(new HTuple()))).TupleAnd(
                        new HTuple(hv_YValues_COPY_INP_TMP.TupleNotEqual(new HTuple())))) != 0)
                    {
                        hv_Num = (new HTuple(hv_YValues_COPY_INP_TMP.TupleLength())) / (new HTuple(hv_XValues_COPY_INP_TMP.TupleLength()
                            ));
                        //
                        //Iterate over all functions to be displayed
                        HTuple end_val482 = hv_Num - 1;
                        HTuple step_val482 = 1;
                        for (hv_I = 0; hv_I.Continue(end_val482, step_val482); hv_I = hv_I.TupleAdd(step_val482))
                        {
                            //Select y values for current function
                            hv_YSelected = hv_YValues_COPY_INP_TMP.TupleSelectRange(hv_I * (new HTuple(hv_XValues_COPY_INP_TMP.TupleLength()
                                )), ((hv_I + 1) * (new HTuple(hv_XValues_COPY_INP_TMP.TupleLength()))) - 1);
                            //Set color
                            if ((int)(new HTuple(hv_Color.TupleEqual(new HTuple()))) != 0)
                            {
                                HOperatorSet.SetRgb(hv_WindowHandle, hv_Red, hv_Green, hv_Blue);
                            }
                            else
                            {
                                if (HDevWindowStack.IsOpen())
                                {
                                    HOperatorSet.SetColor(HDevWindowStack.GetActive(), hv_Color.TupleSelect(
                                        hv_I % (new HTuple(hv_Color.TupleLength()))));
                                }
                            }
                            //
                            //Display in different styles
                            //
                            if ((int)((new HTuple(hv_Style.TupleEqual("line"))).TupleOr(new HTuple(hv_Style.TupleEqual(
                                new HTuple())))) != 0)
                            {
                                //Line
                                ho_Contour.Dispose();
                                HOperatorSet.GenContourPolygonXld(out ho_Contour, ((hv_Height - hv_LowerBorder) - (hv_YSelected * hv_YScaleFactor)) + (hv_YAxisStartValue * hv_YScaleFactor),
                                    ((hv_XValues_COPY_INP_TMP * hv_XScaleFactor) + hv_LeftBorder) - (hv_XAxisStartValue * hv_XScaleFactor));
                                //Clip, if necessary
                                if ((int)(new HTuple(hv_Clip.TupleEqual("yes"))) != 0)
                                {
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ClipContoursXld(ho_Contour, out ExpTmpOutVar_0, hv_UpperBorder,
                                            hv_LeftBorder, hv_Height - hv_LowerBorder, hv_Width - hv_RightBorder);
                                        ho_Contour.Dispose();
                                        ho_Contour = ExpTmpOutVar_0;
                                    }
                                }
                                if (HDevWindowStack.IsOpen())
                                {
                                    HOperatorSet.DispObj(ho_Contour, HDevWindowStack.GetActive());
                                }
                            }
                            else if ((int)(new HTuple(hv_Style.TupleEqual("cross"))) != 0)
                            {
                                //Cross
                                ho_Cross.Dispose();
                                HOperatorSet.GenCrossContourXld(out ho_Cross, ((hv_Height - hv_LowerBorder) - (hv_YSelected * hv_YScaleFactor)) + (hv_YAxisStartValue * hv_YScaleFactor),
                                    ((hv_XValues_COPY_INP_TMP * hv_XScaleFactor) + hv_LeftBorder) - (hv_XAxisStartValue * hv_XScaleFactor),
                                    6, 0.785398);
                                //Clip, if necessary
                                if ((int)(new HTuple(hv_Clip.TupleEqual("yes"))) != 0)
                                {
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ClipContoursXld(ho_Cross, out ExpTmpOutVar_0, hv_UpperBorder,
                                            hv_LeftBorder, hv_Height - hv_LowerBorder, hv_Width - hv_RightBorder);
                                        ho_Cross.Dispose();
                                        ho_Cross = ExpTmpOutVar_0;
                                    }
                                }
                                if (HDevWindowStack.IsOpen())
                                {
                                    HOperatorSet.DispObj(ho_Cross, HDevWindowStack.GetActive());
                                }
                            }
                            else if ((int)(new HTuple(hv_Style.TupleEqual("filled"))) != 0)
                            {
                                //Filled
                                hv_Y1Selected = new HTuple();
                                hv_Y1Selected = hv_Y1Selected.TupleConcat(0 + hv_OriginY);
                                hv_Y1Selected = hv_Y1Selected.TupleConcat(hv_YSelected);
                                hv_Y1Selected = hv_Y1Selected.TupleConcat(0 + hv_OriginY);
                                hv_X1Selected = new HTuple();
                                hv_X1Selected = hv_X1Selected.TupleConcat(hv_XValues_COPY_INP_TMP.TupleMin()
                                    );
                                hv_X1Selected = hv_X1Selected.TupleConcat(hv_XValues_COPY_INP_TMP);
                                hv_X1Selected = hv_X1Selected.TupleConcat(hv_XValues_COPY_INP_TMP.TupleMax()
                                    );
                                if (HDevWindowStack.IsOpen())
                                {
                                    HOperatorSet.SetDraw(HDevWindowStack.GetActive(), "fill");
                                }
                                ho_Filled.Dispose();
                                HOperatorSet.GenRegionPolygonFilled(out ho_Filled, ((hv_Height - hv_LowerBorder) - (hv_Y1Selected * hv_YScaleFactor)) + (hv_YAxisStartValue * hv_YScaleFactor),
                                    ((hv_X1Selected * hv_XScaleFactor) + hv_LeftBorder) - (hv_XAxisStartValue * hv_XScaleFactor));
                                //Clip, if necessary
                                if ((int)(new HTuple(hv_Clip.TupleEqual("yes"))) != 0)
                                {
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ClipRegion(ho_Filled, out ExpTmpOutVar_0, hv_UpperBorder,
                                            hv_LeftBorder, hv_Height - hv_LowerBorder, hv_Width - hv_RightBorder);
                                        ho_Filled.Dispose();
                                        ho_Filled = ExpTmpOutVar_0;
                                    }
                                }
                                if (HDevWindowStack.IsOpen())
                                {
                                    HOperatorSet.DispObj(ho_Filled, HDevWindowStack.GetActive());
                                }
                            }
                            else
                            {
                                throw new HalconException("Unsupported style: " + hv_Style);
                            }
                        }
                    }
                }
                //
                //
                //Reset original display settings
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetPart(HDevWindowStack.GetActive(), hv_PartRow1, hv_PartColumn1,
                        hv_PartRow2, hv_PartColumn2);
                }
                HDevWindowStack.SetActive(hv_PreviousWindowHandle);
                HOperatorSet.SetRgb(hv_WindowHandle, hv_Red, hv_Green, hv_Blue);
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetDraw(HDevWindowStack.GetActive(), hv_DrawMode);
                }
                HOperatorSet.SetLineStyle(hv_WindowHandle, hv_OriginStyle);
                HOperatorSet.SetSystem("clip_region", hv_ClipRegion);
                ho_ContourXGrid.Dispose();
                ho_ContourYGrid.Dispose();
                ho_XArrow.Dispose();
                ho_YArrow.Dispose();
                ho_ContourXTick.Dispose();
                ho_ContourYTick.Dispose();
                ho_Contour.Dispose();
                ho_Cross.Dispose();
                ho_Filled.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_ContourXGrid.Dispose();
                ho_ContourYGrid.Dispose();
                ho_XArrow.Dispose();
                ho_YArrow.Dispose();
                ho_ContourXTick.Dispose();
                ho_ContourYTick.Dispose();
                ho_Contour.Dispose();
                ho_Cross.Dispose();
                ho_Filled.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // 这个程序绘制在坐标系中表示函数或曲线的元组。
        public void plot_funct_1d(HTuple hv_WindowHandle, HTuple hv_Function, HTuple hv_XLabel,
            HTuple hv_YLabel, HTuple hv_Color, HTuple hv_GenParamNames, HTuple hv_GenParamValues)
        {



            // Local control variables 

            HTuple hv_XValues = null, hv_YValues = null;
            // Initialize local and output iconic variables 
            //This procedure plots a function in a coordinate system.
            //
            //Input parameters:
            //
            //Function: 1d function
            //
            //XLabel: X axis label
            //
            //XLabel: Y axis label
            //
            //Color: Color of the plotted function
            //       If [] is given, the currently set display color is used.
            //       If 'none is given, the function is not plotted, but only
            //       the coordinate axes as specified.
            //
            //GenParamNames: Generic parameters to control the presentation
            //               The parameters are evaluated from left to right.
            //
            //               Possible Values:
            //   'axes_color': coordinate system color
            //                 Default: 'white'
            //                 If 'none' is given, no coordinate system is shown.
            //   'style': Graph style
            //            Possible values: 'line' (default), 'cross', 'filled'
            //   'clip': Clip graph to coordinate system area
            //           Possibile values: 'yes' (default), 'no'
            //   'ticks': Control display of ticks on the axes
            //            If 'min_max_origin' is given (default), ticks are shown
            //            at the minimum and maximum values of the axes and at the
            //            intercept point of x- and y-axis.
            //            If 'none' is given, no ticks are shown.
            //            If any number != 0 is given, it is interpreted as distance
            //            between the ticks.
            //   'ticks_x': Control display of ticks on x-axis only
            //   'ticks_y': Control display of ticks on y-axis only
            //   'grid': Control display of grid lines within the coordinate system
            //           If 'min_max_origin' is given (default), grid lines are shown
            //           at the minimum and maximum values of the axes.
            //           If 'none' is given, no grid lines are shown.
            //           If any number != 0 is given, it is interpreted as distance
            //           between the grid lines.
            //   'grid_x': Control display of grid lines for the x-axis only
            //   'grid_y': Control display of grid lines for the y-axis only
            //   'grid_color': Color of the grid (default: 'dim gray')
            //   'margin': The distance in pixels of the coordinate system area
            //             to all four window borders.
            //   'margin_left': The distance in pixels of the coordinate system area
            //                  to the left window border.
            //   'margin_right': The distance in pixels of the coordinate system area
            //                   to the right window border.
            //   'margin_top': The distance in pixels of the coordinate system area
            //                 to the upper window border.
            //   'margin_bottom': The distance in pixels of the coordinate system area
            //                    to the lower window border.
            //   'start_x': Lowest x value of the x axis
            //              Default: min(XValues)
            //   'end_x': Highest x value of the x axis
            //            Default: max(XValues)
            //   'start_y': Lowest y value of the x axis
            //              Default: min(YValues)
            //   'end_y': Highest y value of the x axis
            //            Default: max(YValues)
            //   'origin_x': X coordinate of the intercept point of x- and y-axis.
            //               Default: same as start_x
            //   'origin_y': Y coordinate of the intercept point of x- and y-axis.
            //               Default: same as start_y
            //
            //GenParamValues: Values of the generic parameters of GenericParamNames
            //
            //
            HOperatorSet.Funct1dToPairs(hv_Function, out hv_XValues, out hv_YValues);
            plot_tuple(hv_WindowHandle, hv_XValues, hv_YValues, hv_XLabel, hv_YLabel, hv_Color,
                hv_GenParamNames, hv_GenParamValues);

            return;
        }



        #endregion


        #region/********Halcon12封装算法*****/


        /************************************************
        功能： 判断一个对象(图像、区域，xld）是否有效
         * (有效指：表对象首先不为null,且对象已经初始化，且对象个数大于0(即不为空对象))
         * 参1 输入对象(图像、区域，xld）
         * 参2：错误描述信息
         * 返回值： true表对象有效，false表对象无效
         最近更改日期:2019-4-10
       ************************************************/
        public bool HObjectValided(HObject Obj, ref string strErrMsg)
        {
            strErrMsg = "";
            try//捕获C#语法异常，比如“未将对象的引用对象的实例”此异常HalconException捕获不住
            {
                try//捕获halcon算子异常
                {
                    if (Obj == null)//如果对象为null)
                    {
                        strErrMsg = "输入对象为null！";
                        return false;
                    }

                    if (Obj.CountObj() < 1)//是空对象
                    {
                        strErrMsg = "输入为空对象！";
                        return false;
                    }
                    if (!Obj.IsInitialized())//对象不为空对象但为初始化
                    {

                        strErrMsg = "对象不为空对象但未初始化！";
                        return false;
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return false;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return false;
            }

            return true;
        }

        /************************************************
           功能： 判断一个halcon元组变量是否有效
            * (有效指：表变量首先不为null,且不为"[]"空数组,即元素个数不为0)
            * 参1 输入HTuple变量
            * 参2：错误描述信息
            * 返回值： true表对象有效，false表对象无效
            最近更改日期:2019-4-10
        ************************************************/
        public bool HTupleValided(HTuple tuple, ref string strErrMsg)
        {
            strErrMsg = "";
            try//捕获C#语法异常，比如“未将对象的引用对象的实例”此异常HalconException捕获不住
            {
                try
                {
                    if (tuple == null)
                    {
                        strErrMsg = "输入元组为null！";
                        return false;
                    }
                    if ((int)(new HTuple((new HTuple(tuple.TupleLength())).TupleLess(1))) != 0)//元素个数<1,
                    {
                        strErrMsg = "输入元组为[](空)！";
                        return false;
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return false;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return false;
            }


            return true;
        }


        #region 基于halcon接口图像采集工具相关

        /************************************************
       功能：查询指定接口下,指定信息名称的信息
        输入参数：
        * 参1 输入相机接口类型
        * 参2: 输入要查询的某项信息的项名
        * 参3：输出描述，对查询结果的描述
        * 参4：输出查询结果列表
        * 参5：输出异常信息 
        * 返回值： 成功返回0、失败返回-1
        最近更改日期:2019-4-23
      ************************************************/
        public int HdevQueryCameraInfo(string strCameraInterfaceType, string strQueryName, ref HTuple hv_DescInfo, ref HTuple hv_ValueList, ref string strErrMsg)
        {
            strErrMsg = "";//复位
            // Stack for temporary objects 

            // Local iconic variables 

            // Local control variables 
            // Initialize local and output iconic variables 

            try
            {
                try
                {
                    HOperatorSet.InfoFramegrabber(strCameraInterfaceType, strQueryName, out hv_DescInfo, out hv_ValueList);
                }
                catch (HalconException hEx)
                {
                    //如果查询出错
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                //如果查询出错
                strErrMsg = "" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /************************************************
          功能：查询指定接口下发现的本计算机当前连接的相机数
           输入参数：
           * 参1 输入相机接口类型
           * 参2: 输出查询到的相机名列表,如果相机数为0则列表名默认 "default"
           * 参3：输出查询到的相机数
           * 参4：输出异常信息 
           * 返回值： 成功返回0,相机数>=0、失败返回-1相机数=0,
           最近更改日期:2019-8-19
         ************************************************/
        public int HdevQueryCameraList(string strCameraInterfaceType, ref int DeviceNum, ref HTuple hv_DeviceNameList, ref string strErrMsg)
        {
            strErrMsg = "";
            DeviceNum = 0;
            // Stack for temporary objects 

            // Local iconic variables 

            // Local control variables 
            HTuple hv_Information = new HTuple();
            // Initialize local and output iconic variables 

            try
            {
                try
                {
                    HOperatorSet.InfoFramegrabber(strCameraInterfaceType, "device", out hv_Information, out hv_DeviceNameList);
                    DeviceNum = hv_DeviceNameList.TupleLength();
                    if (DeviceNum == 0)//如果没有查询到采集设备，设置成默认值"default"，
                    {
                        hv_DeviceNameList = hv_DeviceNameList.TupleConcat("default");
                    }
                    else
                    {
                        DeviceNum = 0;
                        for (int CamId = 0; CamId < hv_DeviceNameList.TupleLength(); CamId++)
                        {
                            if (hv_DeviceNameList[CamId].S != "default")//判断查询到的相机名的有效性
                            {
                                DeviceNum++;
                            }
                        }
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    //如果查询出错，默认查到连接相机数为-1，并返回
                    DeviceNum = 0;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                //如果查询出错，默认查到连接相机数为-1，并返回
                DeviceNum = 0;
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /************************************************
        功能：按指定接口，打开指定相机
         输入参数：
         * 参1 输入相机接口类型
         * 参2：输入相机类型名，其他接口默认"default"，"File"接口则为图像文件路径名，或单张图像完整路径名
         * 不断执行抓图算子时，将循环从该路径读取图像
         * 参3: 输入指定要打开的相机名字
         * 参4：输出相机句柄
         * 参5：输出异常信息 
         * 返回值：成功返回0、失败返回-1,
         * 相机未打开前为hv_AcqHandle=null,出错时hv_AcqHandle = [],成功时hv_AcqHandle.Length>0
         最近更改日期:2019-8-19
       ************************************************/
        public int HdevOpenCamera(string strCameraInterfaceType, string strCameraType, string strDeviceName, ref HTuple hv_AcqHandle, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects 
            // Local iconic variables 
            // Local control variables 
            // Initialize local and output iconic variables 

            try
            {
                try
                {
                    if (strCameraInterfaceType.Trim() == "File")//File接口则有效
                    {
                        if (strCameraType.Trim() == "")
                        {
                            strCameraType = "default";
                        }
                    }
                    else//其他接口
                    {
                        strCameraType = "default";
                    }

                    if (strDeviceName.Trim() == "")
                    {
                        strErrMsg = "相机名不存在！";
                        return -1;
                    }
                    if (strCameraInterfaceType.Trim() != "DirectShow")
                    {
                        HOperatorSet.OpenFramegrabber(strCameraInterfaceType, 0, 0, 0, 0, 0, 0, "default",
                         -1, "default", -1, "false", strCameraType, strDeviceName, 0, -1, out hv_AcqHandle);
                    }
                    else
                    {
                        HOperatorSet.OpenFramegrabber(strCameraInterfaceType, 1, 1, 0, 0, 0, 0, "default", 8, "rgb", -1, "false", strCameraType, strDeviceName, 0, -1, out hv_AcqHandle);
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /************************************************
          功能：查询指定相机的参数设置，
           输入参数：
           * 参1:输入指定相机句柄
           * 参2：输入要查询相机参数名称
           * 参3：输出查询到的值，整形，实型、字符串
           * 参4：输出异常信息 
           * 返回值： 成功返回0、失败返回-1
           最近更改日期:2019-4-23
        ************************************************/
        public int HdevGetCameraParam(HTuple hv_AcqHandle, string strParamName, ref HTuple hv_ParamValue, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects  

            // Local iconic variables 

            // Local control variables 

            // Initialize local and output iconic variables 

            try
            {
                try
                {
                    //查询参数
                    HOperatorSet.GetFramegrabberParam(hv_AcqHandle, strParamName, out hv_ParamValue);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /************************************************
         功能：设置指定相机的参数，
          输入参数：
           * 参1:输入指定相机句柄
           * 参2：输入要查询相机参数名称
           * 参3：输入设置的值，整形，实型、字符串
           * 参4：输出异常信息 
          * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-3-23
        ************************************************/
        public int HdevSetCameraParam(HTuple hv_AcqHandle, string strParamName, HTuple hv_ParamValue, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects  

            // Local iconic variables 

            // Local control variables 

            // Initialize local and output iconic variables 

            try
            {
                try
                {
                    //设置参数
                    HOperatorSet.SetFramegrabberParam(hv_AcqHandle, strParamName, hv_ParamValue);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;
        }



        /************************************************
         功能：查询相机在取像时在指定时间点注册了哪些回调函数
          输入参数：
           * 参1:输入指定相机句柄
           * 参2：输入在哪个地方(什么时候)执行注册回调函数，字符串'exception', 'exposure_end', 'exposure_start', 'transfer_end'
           * 参3：输出回调函数地址，整型
           * 参4：输出用户对函数功能描述内容的地址，整型
           * 参5：输出异常信息 
          * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-4-23
         ************************************************/
        public int HdevGetCameraCallback(HTuple hv_AcqHandle, string strCallbackSite, ref HTuple hv_CallbackFuncPointer, ref HTuple hv_UserDescTextPointer, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects  

            // Local iconic variables 

            // Local control variables 

            // Initialize local and output iconic variables 

            try
            {
                try
                {
                    HOperatorSet.GetFramegrabberCallback(hv_AcqHandle, strCallbackSite, out hv_CallbackFuncPointer, out hv_UserDescTextPointer);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /************************************************
         功能：相机在取像时，在指定时间点注册并执行相机回调函数
          输入参数：
           * 参1:输入指定相机句柄
           * 参2：输入在哪个地方(什么时候)执行注册回调函数，字符串,'exception', 'exposure_end', 'exposure_start', 'transfer_end'
           * 参3：输入回调函数地址，整型
           * 参4：输入用户对函数功能描述内容的地址，整型
           * 参5：输出异常信息 
           * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-4-23
        ************************************************/
        public int HdevSetCameraCallback(HTuple hv_AcqHandle, string strCallbackSite, HTuple hv_CallbackFuncPointer, HTuple hv_UserDescTextPointer, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects   

            // Local iconic variables 

            // Local control variables 

            // Initialize local and output iconic variables 

            try
            {
                try
                {
                    //注册一些相机回调函数
                    HOperatorSet.SetFramegrabberCallback(hv_AcqHandle, strCallbackSite, hv_CallbackFuncPointer, hv_UserDescTextPointer);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;
        }



        /************************************************
       功能：相机同步取像
        输入参数：
        * 参1:输入指定相机句柄
        * 参2：输出采集的图像变量
        * 参3：输出异常信息 
        * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-4-23
        ************************************************/
        //public int HdevCameraSynGrabImg(out HObject ho_Image, HTuple hv_AcqHandle, ref string strErrMsg)
        //{
        //    strErrMsg = "";
        //    HOperatorSet.GenEmptyObj(out ho_Image);
        //    /*类中所有方法外定义变量与方法内定义的局部变量的区别:
        //     * (1)方法内部定义的局部变量定义时没有默认自动赋任何值,因此不能直接使用，
        //     * 只有方法内的其他方法可通过out参数来使用，非out修饰的参数不能使用。
        //     * (2)类中所有方法外定义的变量定义时即使未赋值也会默认自动赋值，自动赋null(引用类型)或其他值(值类型)
        //     */

        //    /*ref与out与params与数组(int[] a)方法参数的区别:
        //    * (1)ref修饰参数必须方法外先赋值(可以赋null或其他值)方法内可以重新赋新值使用并传出，或不赋值只使用。
        //    * (2)out修饰的参数的方法外只定义不必赋值(也可以赋值，但如果方法外赋非null的其他值则会生成垃圾内存,
        //    * 此时方法外需先释放变量内存,避免内存泄漏)，方法内必须先赋新值再使用最后传出，或先赋值不使用直接传出。
        //    * (3)params修饰方法的参数只能是数组(比如:params int[] a)：表示让方法接收任意个数同类型的参数，
        //     * 修饰的参数只能传入不能传出，且一个方法只允许有一个params参数且只能放到方法参数的最后一个参数
        //     * (比如:调用 func("nihao",1,2,3),定义 public void func(string str, params int[] a))
        //    */

        //    try
        //    {
        //        //try
        //        //{

        //        ho_Image.Dispose();
        //        HOperatorSet.GrabImage(out ho_Image, hv_AcqHandle);

        //        //}
        //        //catch (HalconException hEx)
        //        //{
        //        //    strErrMsg = "" + hEx;
        //        //    return -1;
        //        //}
        //    }
        //    catch (Exception Ex)
        //    {
        //        strErrMsg = "" + Ex;
        //        return -1;
        //    }
        //    finally
        //    {
        //        ho_Image.Dispose();
        //    }
        //    return 0;
        //}

        /***功能：相机同步取像************
* 参1: 输出采集的图像变量，如果采集失败返回“空图像”
* 参2：输入指定相机句柄
* 参3：输出异常信息(halcon异常信息，无异常返回空)
* 返回值： 返回错误代码(halcon12定义的错误代码:2~10000)
最近更改日期:2020-7-15
************************************************/
        public int HdevCameraSynGrabImg(out HObject ho_Image, HTuple hv_AcqHandle, out HTuple hv_ErrText)
        {
            hv_ErrText = new HTuple();
            HTuple hv_ErrCode = 2;//=H_MSG_OK=H_MSG_TRUE=表正常返回
            //H_MSG_OK=2,正常返回
            //H_MSG_TRUE=2,TRUE 
            //H_MSG_FALSE=3,FALSE
            //H_MSG_VOID=4,返回空，停止处理
            //H_MSG_FAIL=5,调用失败
            //其他代码，错误代码

            HOperatorSet.GenEmptyObj(out ho_Image);
            /*类中所有方法外定义变量与方法内定义的局部变量的区别:
             * (1)方法内部定义的局部变量定义时没有默认自动赋任何值,因此不能直接使用，
             * 只有方法内的其他方法可通过out参数来使用，非out修饰的参数不能使用。
             * (2)类中所有方法外定义的变量定义时即使未赋值也会默认自动赋值，自动赋null(引用类型)或其他值(值类型)
             */

            /*ref与out与params与数组(int[] a)方法参数的区别:
            * (1)ref修饰参数必须方法外先赋值(可以赋null或其他值)方法内可以重新赋新值使用并传出，或不赋值只使用。
            * (2)out修饰的参数的方法外只定义不必赋值(也可以赋值，但如果方法外赋非null的其他值则会生成垃圾内存,
            * 此时方法外需先释放变量内存,避免内存泄漏)，方法内必须先赋新值再使用最后传出，或先赋值不使用直接传出。
            * (3)params修饰方法的参数只能是数组(比如:params int[] a)：表示让方法接收任意个数同类型的参数，
             * 修饰的参数只能传入不能传出，且一个方法只允许有一个params参数且只能放到方法参数的最后一个参数
             * (比如:调用 func("nihao",1,2,3),定义 public void func(string str, params int[] a))
            */

            try
            {
                ho_Image.Dispose();
                HOperatorSet.GrabImage(out ho_Image, hv_AcqHandle);
            }
            catch (HalconException hEx)
            {
                HOperatorSet.GenEmptyObj(out ho_Image);//如果采集异常，返回空图像
                hv_ErrCode = hEx.GetErrorCode();
                HOperatorSet.GetErrorText(hv_ErrCode, out hv_ErrText);

                //halcon定义的错误码不会为负
                //如果出现负值不忽略异常，外抛未知异常，将中断该方法继续执行
                if ((int)hv_ErrCode < 0)
                    throw hEx;
            }
            //finally语句：continue、catch、reture 执行后都会执行
            //finally内和throw紧后不能写reture语句,throw紧前可写reture但是会返回不再执行throw
            //throw:表不处理异常外抛异常，会中断当前正在执行的方法，当前方法无返回值

            return (int)hv_ErrCode;
        }


        /************************************************
          功能：相机启动开始异步抓图，但还没抓，通常和HdevCameraStarAsyncGrabImg一起使用
           **其实用于同步采集也可以，同步和异步采集时不执行此算子也会正常采集 
           输入参数：
           * 参1:输入指定相机句柄
           * 参2：和HdevCameraStarAsyncGrabImg最后参数意义相同，但此参数已过时，没有任何效果。
           * MaxDelay参数已经过时，不会影响新的异步抓取。
           * 注意，可以通过分别使用运算符grab_image_async或grab_data_async的MaxDelay参数来检查太旧的图像。
           * 参3：输出异常信息 
           * 返回值： 成功返回0、失败返回-1
           最近更改日期:2019-4-23
           ************************************************/
        public int HdevCameraStarAsyncGrabImg(HTuple hv_AcqHandle, HTuple hv_MaxDelay, ref string strErrMsg)
        {
            strErrMsg = "";

            try
            {
                if (hv_AcqHandle == null)
                {
                    strErrMsg = "采集句柄为null,采集设备已关闭！";
                    return -1;
                }

                try
                {
                    //开始异步取像
                    HOperatorSet.GrabImageStart(hv_AcqHandle, hv_MaxDelay);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /************************************************
       功能：相机异步取像
        输入参数：
        * 参1:输出采集的图像变量
        * 参2：输入指定相机句柄
        * 参3：输入最大取像延迟时间，单位ms(即抓拍超时值),异步抓取开始到图像交付之间的最大允许延迟[ms],
        * 如果自异步抓取启动以来已经过了超过MaxDelay ms的时间，则认为异步抓取的图像太旧，如果有必要，将抓取新的图像。
        * 默认-1.0，如果给MaxDelay赋值为负值，则此控制机制将停用。
        * 参4：输出异常信息
        * 返回值： 成功返回0、失败返回-1
        最近更改日期:2019-4-23
        ************************************************/
        //public int HdevCameraAsyncGrabImg(out HObject ho_Image, HTuple hv_AcqHandle, HTuple hv_MaxDelay, ref string strErrMsg)
        //{
        //    strErrMsg = "";
        //    HOperatorSet.GenEmptyObj(out ho_Image);
        //    try
        //    {
        //        try
        //        {
        //            ho_Image.Dispose();
        //            HOperatorSet.GrabImageAsync(out ho_Image, hv_AcqHandle, hv_MaxDelay);
        //        }
        //        catch (HalconException hEx)
        //        {
        //            strErrMsg = "" + hEx;
        //            return -1;
        //        }
        //    }
        //    catch (Exception Ex)
        //    {
        //        strErrMsg = "" + Ex;
        //        return -1;
        //    }
        //    finally
        //    {
        //    }
        //    return 0;
        //}


        /***功能：相机异步取像
    * 参1:输出采集的图像变量，如果采集失败返回“空图像”
    * 参2：输入指定相机句柄
    * 参3：输入最大取像延迟时间，单位ms(即抓拍超时值),异步抓取开始到图像交付之间的最大允许延迟[ms],
    * 如果自异步抓取启动以来已经过了超过MaxDelay ms的时间，则认为异步抓取的图像太旧，如果有必要，将抓取新的图像。
    * 默认-1.0，如果给MaxDelay赋值为负值，则此控制机制将停用。
    * 参4：输出异常信息(halcon异常信息，无异常返回空)
    * 返回值： 返回错误代码(halcon12定义的错误代码:2~10000)
    最近更改日期:2020-7-15
    ************************************************/
        public int HdevCameraAsyncGrabImg(out HObject ho_Image, HTuple hv_AcqHandle, HTuple hv_MaxDelay, out HTuple hv_ErrText)
        {
            hv_ErrText = new HTuple();
            HTuple hv_ErrCode = 2;//=H_MSG_OK=H_MSG_TRUE=表正常返回
            //H_MSG_OK=2,正常返回
            //H_MSG_TRUE=2,TRUE 
            //H_MSG_FALSE=3,FALSE
            //H_MSG_VOID=4,返回空，停止处理
            //H_MSG_FAIL=5,调用失败
            //其他代码，错误代码

            HOperatorSet.GenEmptyObj(out ho_Image);

            try
            {
                ho_Image.Dispose();
                HOperatorSet.GrabImageAsync(out ho_Image, hv_AcqHandle, hv_MaxDelay);
            }
            catch (HalconException hEx)
            {
                HOperatorSet.GenEmptyObj(out ho_Image);//如果采集异常，返回空图像
                hv_ErrCode = hEx.GetErrorCode();
                HOperatorSet.GetErrorText(hv_ErrCode, out hv_ErrText);

                //halcon定义的错误码不会为负
                //如果出现负值不忽略异常，外抛未知异常，将中断该方法继续执行
                if ((int)hv_ErrCode < 0)
                    throw hEx;
            }
            //finally语句：continue、catch、reture 执行后都会执行
            //finally内和throw紧后不能写reture语句,throw紧前可写reture但是会返回不再执行throw
            //throw:表不处理异常外抛异常，会中断当前正在执行的方法，当前方法无返回值

            return (int)hv_ErrCode;
        }



        //关闭所有相机：close_all_framegrabber(C#:HOperatorSet.CloseAllFramegrabbers();)已经过时了，只提供它是为了向后兼容。
        //新应用程序不应该使用close_all_framegrabber。
        //无论相机有没有打开，执行close_all_framegrabbers()都不会报错，多次重复执行也不会报错
        /************************************************
          功能：关闭指定相机，
           输入参数：
           * 参1:输入相机句柄
           * 返回值： 成功返回0、失败返回-1,注意：这里，成功关闭返回时，将 hv_AcqHandle=null，关闭失败时不改写原样返回
           * 1. 如果相机掉线，AcqHandle值存在，其他用到AcqHandle算子执行出错，但是可以正常执行该关闭算子不会报错，
           * 2. 如果相机没打开，该值不存在,即使手动赋任意值，执行时该算子会报错，
           * 3. 无论相机有没有打开，执行close_all_framegrabbers()都不会报错，多次重复执行也不会报错
           * 4. 如果相机成功打开，执行close_all_framegrabbers() 关闭后再执行该算子会报错
           * 5. 如果相机成功打开，执行该算子关闭相机后，句柄值不为空值仍然存在，但是再次重复执行时会报错
           最近更改日期:2019-4-23
         ************************************************/
        public int HdevCloseCamera(ref HTuple hv_AcqHandle, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects 

            // Local iconic variables 

            // Local control variables 

            // Initialize local and output iconic variables 

            try
            {
                if (hv_AcqHandle == null)
                {
                    strErrMsg = "采集句柄为null,采集设备已关闭！";
                    return -1;
                }

                try
                {
                    //1. 如果相机掉线，AcqHandle值存在，其他用到AcqHandle算子执行出错，但是可以正常执行该关闭算子不会报错，
                    //2. 如果相机没打开，该值不存在,即使手动赋任意值，执行时该算子会报错，
                    //3. 无论相机有没有打开，执行close_all_framegrabbers()都不会报错，多次重复执行也不会报错
                    //4. 如果相机成功打开，执行close_all_framegrabbers() 关闭后再执行该算子会报错
                    //5. 如果相机成功打开，执行该算子关闭相机后，句柄值不为空值仍然存在，但是再次重复执行时会报错
                    HOperatorSet.CloseFramegrabber(hv_AcqHandle);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            finally
            {
            }
            hv_AcqHandle = null;
            return 0;
        }

      
        /******功能：相机取像，同异步抓拍************
  输入参数：
  * 参1:输出采集的图像变量，如果采集失败返回“空图像”
  * 参2：输入指定采集句柄
  * 参3：输入最大取像延迟时间(对异步有效)，单位ms(即抓拍超时值),异步抓取开始到图像交付之间的最大允许延迟[ms],
  * 如果自异步抓取启动以来已经过了超过MaxDelay ms的时间，则认为异步抓取的图像太旧，如果有必要，将抓取新的图像。
  * 默认-1.0，如果给MaxDelay赋值为负值，则此控制机制将停用。
  * 同步采集特点：
  * (1)有一个缓存,视野更新后抓拍后立即传来新视野图像；
  * (2)当多个相机同时执行同步抓拍算子时(即使是在不同的线程中)某个时刻只能有一个相机进行抓拍其他相机排队等候抓拍，
  * 这样当同时抓拍相机较多时会造成每个相机的图像显示界面“卡顿”
  * (3)当该某个相机在一个线程中抓拍还未结束或抓拍后图像处理未结束，在另个线程中也使用该相机同时抓拍这样会造成抓拍异常出错，
  * 异步采集特点：
  * (1)有5个或多个缓存,视野更新后抓拍后传来仍是旧视野图像，直到连续抓拍5个或多个次后才传来新视野图像
  * (2)当多个相机同时执行异步抓拍算子时(是在不同的线程中)某个时刻允许多个相机同时执行异步抓拍算子,每个相机不用排队抓拍，
  * 当同时抓拍相机较多时不会造成每个相机的图像显示界面“卡顿”
  * (3)当该某个相机在一个线程中抓拍还未结束或抓拍后图像处理未结束，允许在另个线程中也使用该相机同时抓拍不会造成抓拍异常.
  * 参4：输入采集模式，0：表同步采集；1：表异步采集，否则默认同步采集
  * 参5：输出异常信息(halcon异常信息，无异常返回空)
  * 返回值： 返回错误代码(halcon12定义的错误代码:2~10000)
  最近更改日期:2020-7-15
  ************************************************/
        public int CameraGrabImg(out HObject ho_Image, HTuple hv_AcqHandle, HTuple hv_MaxDelay, int intGrabModeIndex, out HTuple hv_ErrText)
        {
            hv_ErrText = new HTuple();
            HTuple hv_ErrCode = 2;//=H_MSG_OK=H_MSG_TRUE=表正常返回
            //H_MSG_OK=2,正常返回
            //H_MSG_TRUE=2,TRUE 
            //H_MSG_FALSE=3,FALSE
            //H_MSG_VOID=4,返回空，停止处理
            //H_MSG_FAIL=5,调用失败
            //其他代码，错误代码

            HOperatorSet.GenEmptyObj(out ho_Image);

            try
            {
                switch (intGrabModeIndex)
                {
                    case 0: //同步采集
                        ho_Image.Dispose();
                        hv_ErrCode = HdevCameraSynGrabImg(out ho_Image, hv_AcqHandle, out hv_ErrText);

                        break;

                    case 1: //异步采集
                        ho_Image.Dispose();
                        hv_ErrCode = HdevCameraAsyncGrabImg(out ho_Image, hv_AcqHandle, hv_MaxDelay, out hv_ErrText);

                        break;
                    default: //同步采集
                        ho_Image.Dispose();
                        hv_ErrCode = HdevCameraSynGrabImg(out ho_Image, hv_AcqHandle, out hv_ErrText);
                        break;
                }
            }
            catch (HalconException hEx)
            {
                HOperatorSet.GenEmptyObj(out ho_Image);//如果采集异常，返回空图像
                hv_ErrCode = hEx.GetErrorCode();
                HOperatorSet.GetErrorText(hv_ErrCode, out hv_ErrText);

                //halcon定义的错误码不会为负
                //如果出现负值不忽略异常，外抛未知异常，将中断该方法继续执行
                if ((int)hv_ErrCode < 0)
                    throw hEx;
            }

            //finally语句：continue、catch、reture 执行后都会执行
            //finally内和throw紧后不能写reture语句,throw紧前可写reture但是会返回不再执行throw
            //throw:表不处理异常外抛异常，会中断当前正在执行的方法，当前方法无返回值
            return (int)hv_ErrCode;
        }



        /******功能：基于各种接口，设置采集超时时间****
         * 比如设置5000ms当触发模式开启时，如果5s到了触发信号还没来临采集算子将报错，
         * 比如设置10ms即使当触发模式没开启，如果采集耗时大于10ms采集算子也会报错
         输入参数：
          * 参1：输入相机句柄
          * 参2：输入采集接口名(后续继续增加)：
          * "GigEVision"(20190426):支持海康、Basler
          * "MVision"(20190621):海康专用，支持海康、Basler
          * "DirectShow"(20190820):支持Basler、不支持海康
          * "GenICamTL"(20190820):支持Basler、不支持海康
          * 参3：输入超时时间(整数或浮点数)
          * 参4：输出异常信息
          * 返回值：成功返回0，失败返回-1
         最近更改日期:2019-8-20 
        ************************************************/
        public int SetGrabTimeout(HTuple hv_AcqHandle, string strCameraInterfaceType, HTuple hv_Timeout, ref string strErrMsg)
        {
            int returnResult = -1;
            strErrMsg = "";
            try
            {
                try
                {
                    //整数或浮点数,这里取整数
                    HOperatorSet.TupleInt(hv_Timeout, out hv_Timeout);
                    switch (strCameraInterfaceType)
                    {
                        case "GigEVision2":
                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "grab_timeout", hv_Timeout.I, ref strErrMsg))//整数或浮点数,这里取整数
                            {
                                returnResult = -1;//设置超时失败
                            }
                            else
                            {
                                returnResult = 0;//成功设置超时
                            }
                            break;
                        case "MVision"://海康halcon专用接口
                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "grab_timeout", hv_Timeout.I, ref strErrMsg))
                            {
                                returnResult = -1;//设置超时失败
                            }
                            else
                            {
                                returnResult = 0;//成功设置超时
                            }
                            break;
                        case "DirectShow":
                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "grab_timeout", hv_Timeout.I, ref strErrMsg))
                            {
                                returnResult = -1;//设置超时失败
                            }
                            else
                            {
                                returnResult = 0;//成功设置超时
                            }
                            break;
                        case "GenICamTL":
                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "grab_timeout", hv_Timeout.I, ref strErrMsg))
                            {
                                returnResult = -1;//设置超时失败
                            }
                            else
                            {
                                returnResult = 0;//成功设置超时
                            }
                            break;

                        default:
                            returnResult = -1;
                            strErrMsg = "输入的接口名暂不支持！";
                            break;
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }

            return returnResult;
        }
        /******功能：基于各种接口，查询采集超时时间****
        输入参数：
        * 参1：输入相机句柄
        * 参2：输入采集接口名(后续继续增加)：
        * "GigEVision"(20190426):支持海康、Basler
        * "MVision"(20190621):海康专用，支持海康、Basler
        * "DirectShow"(20190820):支持Basler、不支持海康
        * "GenICamTL"(20190820):支持Basler、不支持海康
        * 参3：输出设置的超时时间
        * 参4：输出异常信息
        * 返回值：成功返回0，失败返回-1
        最近更改日期:2019-8-20  
       ************************************************/
        public int GetGrabTimeout(HTuple hv_AcqHandle, string strCameraInterfaceType, ref HTuple hv_Timeout, ref string strErrMsg)
        {
            int returnResult = -1;
            strErrMsg = "";

            try
            {
                try
                {
                    switch (strCameraInterfaceType)
                    {
                        case "GigEVision2":
                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "grab_timeout", ref hv_Timeout, ref strErrMsg))//整数或浮点数,这里取整数
                            {
                                returnResult = -1;
                            }
                            else
                            {
                                if (hv_Timeout.TupleLength() > 0)
                                {
                                    returnResult = 0;//成功
                                }
                                else
                                {
                                    returnResult = -1;
                                    strErrMsg = "查询值为空！";
                                }
                            }
                            break;
                        case "MVision"://海康halcon专用接口
                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "grab_timeout", ref hv_Timeout, ref strErrMsg))
                            {
                                returnResult = -1;//失败
                            }
                            else
                            {
                                if (hv_Timeout.TupleLength() > 0)
                                {
                                    returnResult = 0;//成功
                                }
                                else
                                {
                                    returnResult = -1;
                                    strErrMsg = "查询值为空！";
                                }
                            }
                            break;
                        case "DirectShow":
                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "grab_timeout", ref hv_Timeout, ref strErrMsg))
                            {
                                returnResult = -1;//失败
                            }
                            else
                            {
                                if (hv_Timeout.TupleLength() > 0)
                                {
                                    returnResult = 0;//成功
                                }
                                else
                                {
                                    returnResult = -1;
                                    strErrMsg = "查询值为空！";
                                }
                            }
                            break;
                        case "GenICamTL":
                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "grab_timeout", ref hv_Timeout, ref strErrMsg))
                            {
                                returnResult = -1;//失败
                            }
                            else
                            {
                                if (hv_Timeout.TupleLength() > 0)
                                {
                                    returnResult = 0;//成功
                                }
                                else
                                {
                                    returnResult = -1;
                                    strErrMsg = "查询值为空！";
                                }
                            }
                            break;

                        default:
                            returnResult = -1;
                            strErrMsg = "输入的接口名暂不支持！";
                            break;
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }

            return returnResult;
        }

        /******功能：基于各种接口，设置曝光时间********
          输入参数：
          * 参1：输入相机句柄
          * 参2：输入采集接口名(后续继续增加)：
          * "GigEVision"(20190426):支持海康、Basler
          * "MVision"(20190621):海康专用，支持海康、不支持Basler
          * "DirectShow"(20190820):支持Basler、不支持海康，-14~-1，只能为整数
          * "GenICamTL"(20190820):支持Basler、不支持海康
          * 参3：输入曝光时间(整数或浮点数)
          * 参4：输出异常信息
          * 返回值：成功返回0，失败返回-1
          最近更改日期:2019-4-23  
        ************************************************/
        public int SetExposureTime(HTuple hv_AcqHandle, string strCameraInterfaceType, HTuple hv_ExposureTime, ref string strErrMsg)
        {

            int returnResult = -1;
            strErrMsg = "";
            try
            {
                try
                {
                    //整数或浮点数,这里取整数
                    HOperatorSet.TupleInt(hv_ExposureTime, out hv_ExposureTime);

                    switch (strCameraInterfaceType)
                    {
                        case "GigEVision2":
                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "ExposureTime", hv_ExposureTime.I, ref strErrMsg))//整数或浮点数,这里取整数,支持basler、海康
                            {
                                if (-1 == HdevSetCameraParam(hv_AcqHandle, "ExposureTimeRaw", hv_ExposureTime.I, ref strErrMsg))//只能是整数,仅适用：basler
                                {
                                    if (-1 == HdevSetCameraParam(hv_AcqHandle, "ExposureTimeAbs", hv_ExposureTime.I, ref strErrMsg))//整数或浮点数,仅适用：basler
                                    {
                                        returnResult = -1;//失败
                                        strErrMsg = strCameraInterfaceType + "接口不支持该相机曝光设置！";
                                    }
                                    else
                                    {
                                        returnResult = 0;
                                    }
                                }
                                else
                                {
                                    returnResult = 0;
                                }
                            }
                            else
                            {
                                returnResult = 0;//成功设置曝光
                            }
                            break;
                        case "MVision"://海康halcon专用接口
                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "ExposureTime", hv_ExposureTime.D, ref strErrMsg))//海康只能浮点数
                            {
                                if (-1 == HdevSetCameraParam(hv_AcqHandle, "ExposureTime", hv_ExposureTime.I, ref strErrMsg))//尝试用整数
                                {
                                    returnResult = -1;//失败
                                    strErrMsg = strCameraInterfaceType + "接口不支持该相机曝光设置！";
                                }
                                else
                                {
                                    returnResult = 0;//成功
                                }
                            }
                            else
                            {
                                returnResult = 0;
                            }
                            break;
                        case "DirectShow":
                            //支持Basler，-14~-1，只能为整数
                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "exposure", hv_ExposureTime.I, ref strErrMsg))
                            {
                                returnResult = -1;//失败
                                if ((hv_ExposureTime.I > -1) || (hv_ExposureTime.I < -14))
                                {
                                    strErrMsg = "值超范围,请输入-14~-1之间整数值！";
                                }
                                else
                                {
                                    strErrMsg = strCameraInterfaceType + "接口不支持该相机曝光设置！";
                                }
                            }
                            else
                            {
                                returnResult = 0;
                            }
                            break;
                        case "GenICamTL":
                            //整数或浮点数,这里取整数,不支持basler、不支持海康
                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "ExposureTime", hv_ExposureTime.I, ref strErrMsg))
                            {
                                if (-1 == HdevSetCameraParam(hv_AcqHandle, "ExposureTimeRaw", hv_ExposureTime.I, ref strErrMsg))//只能是整数,仅适用：basler
                                {
                                    if (-1 == HdevSetCameraParam(hv_AcqHandle, "ExposureTimeAbs", hv_ExposureTime.I, ref strErrMsg))//整数或浮点数,仅适用：basler
                                    {
                                        returnResult = -1;//失败
                                        strErrMsg = strCameraInterfaceType + "接口不支持该相机曝光设置！";
                                    }
                                    else
                                    {
                                        returnResult = 0;
                                    }
                                }
                                else
                                {
                                    returnResult = 0;
                                }
                            }
                            else
                            {
                                returnResult = 0;//成功设置曝光
                            }
                            break;

                        default:
                            returnResult = -1;
                            strErrMsg = "输入的接口名暂不支持！";
                            break;
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            return returnResult;
        }
        /******功能：基于各种接口，查询曝光时间********
          输入参数：
          * 参1：输入相机句柄
          * 参2：输入采集接口名(后续继续增加)：
          * "GigEVision"(20190426):支持海康、Basler
          * "MVision"(20190621):海康专用，支持海康、不支持Basler
          * "DirectShow"(20190820):支持Basler、不支持海康，-14~-1，只能为整数
          * "GenICamTL"(20190820):支持Basler、不支持海康
          * 参3：输出曝光时间(整数或浮点数)
          * 参4：输出异常信息
          * 返回值：成功返回0，失败返回-1
          最近更改日期:2019-4-23  
        ************************************************/
        public int GetExposureTime(HTuple hv_AcqHandle, string strCameraInterfaceType, ref HTuple hv_ExposureTime, ref string strErrMsg)
        {
            int returnResult = -1;
            strErrMsg = "";

            try
            {
                try
                {
                    switch (strCameraInterfaceType)
                    {
                        case "GigEVision2":
                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "ExposureTime", ref hv_ExposureTime, ref strErrMsg))//整数或浮点数,这里取整数,支持basler、海康
                            {
                                if (-1 == HdevGetCameraParam(hv_AcqHandle, "ExposureTimeRaw", ref hv_ExposureTime, ref strErrMsg))//只能是整数,仅适用：basler
                                {
                                    if (-1 == HdevGetCameraParam(hv_AcqHandle, "ExposureTimeAbs", ref hv_ExposureTime, ref strErrMsg))//整数或浮点数,仅适用：basler
                                    {
                                        returnResult = -1;//失败
                                    }
                                    else
                                    {
                                        if (hv_ExposureTime.TupleLength() > 0)
                                        {
                                            returnResult = 0;
                                        }
                                        else
                                        {
                                            returnResult = -1;
                                            strErrMsg = "查询值为空！";
                                        }
                                    }
                                }
                                else
                                {
                                    if (hv_ExposureTime.TupleLength() > 0)
                                    {
                                        returnResult = 0;
                                    }
                                    else
                                    {
                                        returnResult = -1;
                                        strErrMsg = "查询值为空！";
                                    }
                                }
                            }
                            else
                            {
                                if (hv_ExposureTime.TupleLength() > 0)
                                {
                                    returnResult = 0;
                                }
                                else
                                {
                                    returnResult = -1;
                                    strErrMsg = "查询值为空！";
                                }
                            }
                            break;
                        case "MVision"://海康halcon专用接口
                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "ExposureTime", ref hv_ExposureTime, ref strErrMsg))//海康只能浮点数
                            {
                                returnResult = -1;//失败
                            }
                            else
                            {
                                if (hv_ExposureTime.TupleLength() > 0)
                                {
                                    returnResult = 0;
                                }
                                else
                                {
                                    returnResult = -1;
                                    strErrMsg = "查询值为空！";
                                }
                            }
                            break;
                        case "DirectShow":
                            //支持Basler，-14~-1，只能为整数
                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "exposure", ref hv_ExposureTime, ref strErrMsg))
                            {
                                returnResult = -1;//失败
                            }
                            else
                            {
                                if (hv_ExposureTime.TupleLength() > 0)
                                {
                                    returnResult = 0;
                                }
                                else
                                {
                                    returnResult = -1;
                                    strErrMsg = "查询值为空！";
                                }
                            }
                            break;
                        case "GenICamTL":
                            //整数或浮点数,这里取整数,不支持basler、不支持海康
                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "ExposureTime", ref hv_ExposureTime, ref strErrMsg))
                            {
                                if (-1 == HdevGetCameraParam(hv_AcqHandle, "ExposureTimeRaw", ref hv_ExposureTime, ref strErrMsg))//只能是整数,仅适用：basler
                                {
                                    if (-1 == HdevGetCameraParam(hv_AcqHandle, "ExposureTimeAbs", ref hv_ExposureTime, ref strErrMsg))//整数或浮点数,仅适用：basler
                                    {
                                        returnResult = -1;//失败
                                    }
                                    else
                                    {
                                        if (hv_ExposureTime.TupleLength() > 0)
                                        {
                                            returnResult = 0;
                                        }
                                        else
                                        {
                                            returnResult = -1;
                                            strErrMsg = "查询值为空！";
                                        }
                                    }
                                }
                                else
                                {
                                    if (hv_ExposureTime.TupleLength() > 0)
                                    {
                                        returnResult = 0;
                                    }
                                    else
                                    {
                                        returnResult = -1;
                                        strErrMsg = "查询值为空！";
                                    }
                                }
                            }
                            else
                            {
                                if (hv_ExposureTime.TupleLength() > 0)
                                {
                                    returnResult = 0;
                                }
                                else
                                {
                                    returnResult = -1;
                                    strErrMsg = "查询值为空！";
                                }
                            }
                            break;

                        default:
                            returnResult = -1;
                            strErrMsg = "输入的接口名暂不支持！";
                            break;
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            return returnResult;
        }

        /********功能：基于各种接口，设置增益*********
        输入参数：
        * 参1：输入相机句柄
        * 参2：输入采集接口名(后续继续增加)：
        * "GigEVision"(20190426):支持海康、Basler
        * "MVision"(20190621):海康专用，支持海康、不支持Basler
        * "DirectShow"(20190820):支持Basler、不支持海康，0~63，只能为整数
        * "GenICamTL"(20190820):支持Basler、不支持海康
        * 参3：输入增益(整数或浮点数)
        * 参4：输出异常信息
        * 返回值：成功返回0，失败返回-1
        最近更改日期:2019-8-20  
       ************************************************/
        public int SetGainRow(HTuple hv_AcqHandle, string strCameraInterfaceType, HTuple hv_GainRow, ref string strErrMsg)
        {
            int returnResult = -1;
            strErrMsg = "";

            try
            {
                try
                {
                    //整数或浮点数,这里取整数
                    HOperatorSet.TupleInt(hv_GainRow, out hv_GainRow);
                    switch (strCameraInterfaceType)
                    {
                        case "GigEVision2":
                            //'GainRaw'整数或浮点数(适用：HIK起点0.0);只能是整数(仅适用：basler起点0)
                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "GainRow", hv_GainRow.I, ref strErrMsg))//这里取整数
                            {
                                //'Gain'增益整数或浮点数(仅适用：HIK)
                                if (-1 == HdevSetCameraParam(hv_AcqHandle, "Gain", hv_GainRow.I, ref strErrMsg))//这里取整数
                                {
                                    returnResult = -1;//失败
                                    strErrMsg = strCameraInterfaceType + "接口不支持该相机增益设置！";
                                }
                                else
                                {
                                    returnResult = 0;
                                }
                            }
                            else
                            {
                                returnResult = 0;
                            }
                            break;
                        case "MVision"://海康halcon专用接口
                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "Gain", hv_GainRow.D, ref strErrMsg))//HIK起点0.0取浮点数
                            {
                                returnResult = -1;//失败
                                strErrMsg = strCameraInterfaceType + "接口不支持该相机增益设置！";
                            }
                            else
                            {
                                returnResult = 0;
                            }
                            break;
                        case "DirectShow":
                            //支持Basler，0~63，只能为整数
                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "video_gain", hv_GainRow.I, ref strErrMsg))
                            {
                                returnResult = -1;//失败
                                if ((hv_GainRow.I > 63) || (hv_GainRow.I < 0))
                                {
                                    strErrMsg = "增益值超范围,请输入0~63之间整数值！";
                                }
                                else
                                {
                                    strErrMsg = strCameraInterfaceType + "接口不支持该相机增益设置！";
                                }
                            }
                            else
                            {
                                returnResult = 0;
                            }
                            break;
                        case "GenICamTL":
                            //'GainRaw'整数或浮点数,(仅适用：basler起点0),这里取整数
                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "GainRaw", hv_GainRow.I, ref strErrMsg))
                            {
                                strErrMsg = strCameraInterfaceType + "接口不支持该相机增益设置！";
                                returnResult = -1;//失败
                            }
                            else
                            {
                                returnResult = 0;
                            }
                            break;

                        default:
                            returnResult = -1;
                            strErrMsg = "输入的接口名暂不支持！";
                            break;
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            return returnResult;
        }
        /********功能：基于各种接口，查询增益*********
        输入参数：
        * 参1：输入相机句柄
        * 参2：输入采集接口名(后续继续增加)：
        * "GigEVision"(20190426):支持海康、Basler
        * "MVision"(20190621):海康专用，支持海康、不支持Basler
        * "DirectShow"(20190820):支持Basler、不支持海康，0~63，只能为整数
        * "GenICamTL"(20190820):支持Basler、不支持海康
        * 参3：输出增益(整数或浮点数)
        * 参4：输出异常信息
        * 返回值：成功返回0，失败返回-1
        最近更改日期:2019-8-20  
       ************************************************/
        public int GetGainRow(HTuple hv_AcqHandle, string strCameraInterfaceType, ref HTuple hv_GainRow, ref string strErrMsg)
        {
            int returnResult = -1;
            strErrMsg = "";

            try
            {
                try
                {
                    switch (strCameraInterfaceType)
                    {
                        case "GigEVision2":
                            //'GainRaw'整数或浮点数(适用：HIK起点0.0);只能是整数(仅适用：basler起点0)
                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "GainRaw", ref hv_GainRow, ref strErrMsg))
                            {
                                //'Gain'增益整数或浮点数(仅适用：HIK)
                                if (-1 == HdevGetCameraParam(hv_AcqHandle, "Gain", ref hv_GainRow, ref strErrMsg))
                                {
                                    returnResult = -1;//失败
                                }
                                else
                                {
                                    if (hv_GainRow.TupleLength() > 0)
                                    {
                                        returnResult = 0;
                                    }
                                    else
                                    {
                                        returnResult = -1;
                                        strErrMsg = "查询值为空！";
                                    }
                                }
                            }
                            else
                            {
                                returnResult = 0;
                            }
                            break;
                        case "MVision"://海康halcon专用接口
                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "Gain", ref hv_GainRow, ref strErrMsg))//HIK起点0.0取浮点数
                            {
                                returnResult = -1;//失败
                            }
                            else
                            {
                                if (hv_GainRow.TupleLength() > 0)
                                {
                                    returnResult = 0;
                                }
                                else
                                {
                                    returnResult = -1;
                                    strErrMsg = "查询值为空！";
                                }
                            }
                            break;
                        case "DirectShow":
                            //支持Basler，0~63，只能为整数
                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "video_gain", ref hv_GainRow, ref strErrMsg))
                            {
                                returnResult = -1;//失败
                            }
                            else
                            {
                                if (hv_GainRow.TupleLength() > 0)
                                {
                                    returnResult = 0;
                                }
                                else
                                {
                                    returnResult = -1;
                                    strErrMsg = "查询值为空！";
                                }
                            }
                            break;
                        case "GenICamTL":
                            //'GainRaw'整数或浮点数,(仅适用：basler起点0),这里取整数
                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "GainRaw", ref hv_GainRow, ref strErrMsg))
                            {
                                returnResult = -1;//失败
                            }
                            else
                            {
                                if (hv_GainRow.TupleLength() > 0)
                                {
                                    returnResult = 0;
                                }
                                else
                                {
                                    returnResult = -1;
                                    strErrMsg = "查询值为空！";
                                }
                            }
                            break;

                        default:
                            returnResult = -1;
                            strErrMsg = "输入的接口名暂不支持！";
                            break;
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            return returnResult;
        }

        /******功能：基于各种接口，设置触发模式****
          输入参数：
          * 参1：输入相机句柄
          * 参2：输入采集接口名(后续继续增加)：
          * "GigEVision"(20190426):支持海康、Basler
          * "MVision"(20190621):海康专用，不支持海康设置触发模式、也不支持Basler
          * "DirectShow"(20190820):支持Basler、不支持海康
          * "GenICamTL"(20190820):支持Basler、不支持海康
          * 参3：输入触发源选择："Off"：表关闭触发，"Software"：表打开触发，设置软触发，"Line1"：表打开触发设置成外触发
          * 参5：输出异常信息
          * 返回值：成功返回0，失败返回-1
          最近更改日期:2019-8-20  
        ************************************************/
        public int SetTriggerMode(HTuple hv_AcqHandle, string strCameraInterfaceType, HTuple hv_TriggerMode, ref string strErrMsg)
        {
            int returnResult = -1;
            strErrMsg = "";

            try
            {
                try
                {
                    switch (strCameraInterfaceType)
                    {
                        case "GigEVision2":
                            {
                                switch (hv_TriggerMode.S)
                                {
                                    case "Off": //关闭触发
                                        if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerMode", "Off", ref strErrMsg))//一般都支持
                                        {
                                            returnResult = -1;
                                            strErrMsg = strCameraInterfaceType + "接口关闭触发出错！";
                                        }
                                        else
                                        {
                                            returnResult = 0;
                                        }
                                        break;
                                    case "Software": //打开触发，并设置软触发
                                        if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerMode", "On", ref strErrMsg))//一般都支持
                                        {
                                            returnResult = -1;
                                            strErrMsg = strCameraInterfaceType + "接口打开触发出错！";
                                        }
                                        else
                                        {
                                            //成功打开触发，设置软触发
                                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerSource", "Software", ref strErrMsg))//一般都支持
                                            {
                                                returnResult = -1;
                                                strErrMsg = strCameraInterfaceType + "接口打开触发并设置软触发出错！";
                                            }
                                            else
                                            {
                                                returnResult = 0;
                                            }
                                        }

                                        break;
                                    case "Line1": //打开触发，并设置外触发
                                        if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerMode", "On", ref strErrMsg))
                                        {
                                            returnResult = -1;
                                            strErrMsg = strCameraInterfaceType + "接口打开触发出错！";
                                        }
                                        else
                                        {
                                            //成功打开触发，设置外触发
                                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerSource", "Line1", ref strErrMsg))//basler、大华(海康不支持)
                                            {
                                                if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerSource", "Line0", ref strErrMsg))//比如：海康相机专用
                                                {
                                                    if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerSource", "Line2", ref strErrMsg))//比如：海康相机专用
                                                    {
                                                        returnResult = -1;
                                                        strErrMsg = strCameraInterfaceType + "接口打开触发并设置外触发出错！";
                                                    }
                                                    else
                                                    {
                                                        returnResult = 0;
                                                    }
                                                }
                                                else
                                                {
                                                    returnResult = 0;
                                                }
                                            }
                                            else
                                            {
                                                returnResult = 0;
                                            }
                                        }
                                        break;
                                    default:
                                        strErrMsg = "输入触发模式不支持！";
                                        returnResult = -1;//写设置出错标识
                                        break;
                                }
                            }
                            break;

                        case "MVision"://海康halcon专用接口,不支持海康、也不支持巴斯勒相机设置
                            {
                                switch (hv_TriggerMode.S)
                                {
                                    case "Off": //关闭触发
                                        if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerMode", "Off", ref strErrMsg))
                                        {
                                            returnResult = -1;
                                            strErrMsg = strCameraInterfaceType + "接口关闭触发出错！";
                                        }
                                        else
                                        {
                                            returnResult = 0;
                                        }
                                        break;
                                    case "Software": //打开触发，并设置软触发

                                        if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerMode", "On", ref strErrMsg))
                                        {
                                            returnResult = -1;
                                            strErrMsg = strCameraInterfaceType + "接口打开触发出错！";
                                        }
                                        else
                                        {
                                            //成功打开触发，设置软触发
                                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerSource", "Software", ref strErrMsg))
                                            {
                                                returnResult = -1;
                                                strErrMsg = strCameraInterfaceType + "接口打开触发并设置软触发出错！";
                                            }
                                            else
                                            {
                                                returnResult = 0;
                                            }
                                        }

                                        break;
                                    case "Line1": //打开触发，并设置外触发

                                        if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerMode", "On", ref strErrMsg))
                                        {
                                            returnResult = -1;
                                            strErrMsg = strCameraInterfaceType + "接口打开触发出错！";
                                        }
                                        else
                                        {
                                            //成功打开触发，设置外触发
                                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerSource", "Line1", ref strErrMsg))
                                            {
                                                if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerSource", "Line0", ref strErrMsg))
                                                {
                                                    if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerSource", "Line2", ref strErrMsg))
                                                    {
                                                        returnResult = -1;//出错
                                                        strErrMsg = strCameraInterfaceType + "接口打开触发并设置外触发出错";
                                                    }
                                                    else
                                                    {
                                                        returnResult = 0;
                                                    }
                                                }
                                                else
                                                {
                                                    returnResult = 0;
                                                }
                                            }
                                            else
                                            {
                                                returnResult = 0;
                                            }
                                        }
                                        break;
                                    default:
                                        strErrMsg = "输入触发模式不支持！";
                                        returnResult = -1;//写设置出错标识
                                        break;
                                }
                            }

                            break;
                        case "DirectShow"://支持basler、不支持海康
                            {
                                switch (hv_TriggerMode.S)
                                {
                                    case "Off": //关闭触发
                                        if (-1 == HdevSetCameraParam(hv_AcqHandle, "external_trigger", "false", ref strErrMsg))
                                        {
                                            returnResult = -1;//出错
                                            strErrMsg = strCameraInterfaceType + "接口关闭触发出错！";
                                        }
                                        else
                                        {
                                            returnResult = 0;//写成功设置关闭触发的标识
                                        }
                                        break;
                                    case "Software": //打开触发，并设置软触发
                                        if (-1 == HdevSetCameraParam(hv_AcqHandle, "external_trigger", "true", ref strErrMsg))
                                        {
                                            returnResult = -1;
                                            strErrMsg = strCameraInterfaceType + "接口打开触发出错！";
                                        }
                                        else
                                        {
                                            //支持basler触发模式开关，无软硬触发设置
                                            returnResult = 0;//成功
                                        }

                                        break;
                                    case "Line1": //打开触发，并设置外触发
                                        if (-1 == HdevSetCameraParam(hv_AcqHandle, "external_trigger", "true", ref strErrMsg))
                                        {
                                            returnResult = -1;
                                            strErrMsg = strCameraInterfaceType + "接口打开触发出错！";
                                        }
                                        else
                                        {
                                            //支持basler触发模式开关，无软硬触发设置
                                            returnResult = 0;//成功
                                        }

                                        break;
                                    default:
                                        strErrMsg = "输入触发模式不支持！";
                                        returnResult = -1;//写设置出错标识
                                        break;
                                }
                            }
                            break;
                        case "GenICamTL"://不支持海康、支持巴斯勒相机
                            {
                                switch (hv_TriggerMode.S)
                                {
                                    case "Off": //关闭触发
                                        if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerMode", "Off", ref strErrMsg))
                                        {
                                            returnResult = -1;//写设置出错标识
                                            strErrMsg = strCameraInterfaceType + "接口关闭触发出错！";
                                        }
                                        else
                                        {
                                            returnResult = 0;//写成功设置关闭触发的标识
                                        }
                                        break;
                                    case "Software": //打开触发，并设置软触发

                                        if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerMode", "On", ref strErrMsg))//一般都支持
                                        {
                                            returnResult = -1;//写设置出错标识
                                            strErrMsg = strCameraInterfaceType + "接口打开触发出错！";
                                        }
                                        else
                                        {
                                            //成功打开触发，设置软触发
                                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerSource", "Software", ref strErrMsg))//一般都支持
                                            {
                                                returnResult = -1;//写设置出错标识
                                                strErrMsg = strCameraInterfaceType + "接口打开触发并设置软触发出错！";
                                            }
                                            else
                                            {
                                                returnResult = 0;//写成功设置关闭触发的标识
                                            }
                                        }

                                        break;
                                    case "Line1": //打开触发，并设置外触发

                                        if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerMode", "On", ref strErrMsg))
                                        {
                                            returnResult = -1;//写设置出错标识
                                            strErrMsg = strCameraInterfaceType + "接口打开触发出错！";
                                        }
                                        else
                                        {
                                            //成功打开触发，设置外触发
                                            if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerSource", "Line1", ref strErrMsg))//比如：basler、大华
                                            {
                                                if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerSource", "Line0", ref strErrMsg))//比如：海康
                                                {
                                                    if (-1 == HdevSetCameraParam(hv_AcqHandle, "TriggerSource", "Line2", ref strErrMsg))//比如：海康
                                                    {
                                                        returnResult = -1;//写设置出错标识
                                                        strErrMsg = strCameraInterfaceType + "接口打开触发并设置外触发出错！";
                                                    }
                                                    else
                                                    {
                                                        returnResult = 0;//写成功设置标识
                                                    }
                                                }
                                                else
                                                {
                                                    returnResult = 0;//写成功设置标识
                                                }
                                            }
                                            else
                                            {
                                                returnResult = 0;//写成功设置标识
                                            }
                                        }
                                        break;
                                    default:
                                        strErrMsg = "输入触发模式不支持！";
                                        returnResult = -1;//写设置出错标识
                                        break;
                                }
                            }
                            break;

                        default:
                            returnResult = -1;
                            strErrMsg = "输入的接口名暂不支持！";
                            break;
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            return returnResult;
        }
        /******功能：基于各种接口，查询触发模式****
         输入参数：
         * 参1：输入相机句柄
         * 参2：输入采集接口名(后续继续增加)：
         * "GigEVision"(20190426):支持海康、Basler
         * "MVision"(20190621):海康专用，不支持海康设置触发模式、也不支持Basler
         * "DirectShow"(20190820):支持Basler、不支持海康
         * "GenICamTL"(20190820):支持Basler、不支持海康
         * 参3：输出触发源选择："Off"表：触发关闭、"Software"表软触发开(一定为On)、"Line1"表外触发开(一定为On)、
         * "On"表触发打开(正确查询触发是否打开但触发源查询出错)
         * 参5：输出异常信息
         * 返回值：成功返回0，失败返回-1
         最近更改日期:2019-8-20  
       ************************************************/
        public int GetTriggerMode(HTuple hv_AcqHandle, string strCameraInterfaceType, ref HTuple hv_TriggerMode, ref string strErrMsg)
        {
            int returnResult = -1;
            strErrMsg = "";

            try
            {
                try
                {
                    switch (strCameraInterfaceType)
                    {
                        case "GigEVision2":
                            //一般都支持，比如：basler、dahua、HIK
                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "TriggerMode", ref hv_TriggerMode, ref strErrMsg))
                            {
                                returnResult = -1;
                                strErrMsg = strCameraInterfaceType + "接口触发模式查询出错！";
                            }
                            else
                            {
                                HTuple hv_IsString = new HTuple();
                                HOperatorSet.TupleIsString(hv_TriggerMode, out hv_IsString);
                                if ((int)(hv_IsString) == 0)//如果不是字符串
                                {
                                    returnResult = -1;
                                    strErrMsg = strCameraInterfaceType + "接口触发模式查询出错！";
                                }
                                else
                                {
                                    switch (hv_TriggerMode.S)
                                    {
                                        case "On":
                                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "TriggerSource", ref hv_TriggerMode, ref strErrMsg))
                                            {
                                                returnResult = -1;
                                                strErrMsg = strCameraInterfaceType + "接口触发已打开但触发源查询出错！";
                                                hv_TriggerMode = "On";
                                            }
                                            else
                                            {
                                                returnResult = 0;
                                                if (hv_TriggerMode.S == "Line0" || hv_TriggerMode.S == "Line1" || hv_TriggerMode.S == "Line2")
                                                {
                                                    hv_TriggerMode = "Line1";
                                                }
                                                else if (hv_TriggerMode.S == "Software")
                                                {
                                                    hv_TriggerMode = "Software";
                                                }
                                            }
                                            break;

                                        case "Off":
                                            returnResult = 0;
                                            hv_TriggerMode = "Off";
                                            break;
                                        default:
                                            returnResult = -1;
                                            strErrMsg = "无法识别查询到的触发模式！";
                                            break;
                                    }
                                }
                            }
                            break;

                        case "MVision"://海康halcon专用接口,不支持海康、也不支持巴斯勒相机设置
                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "TriggerMode", ref hv_TriggerMode, ref strErrMsg))
                            {
                                returnResult = -1;
                                strErrMsg = strCameraInterfaceType + "接口触发模式查询出错！";
                            }
                            else
                            {
                                HTuple hv_IsString = new HTuple();
                                HOperatorSet.TupleIsString(hv_TriggerMode, out hv_IsString);
                                if ((int)(hv_IsString) == 0)//如果不是字符串
                                {
                                    returnResult = -1;
                                    strErrMsg = strCameraInterfaceType + "接口触发模式查询出错！";
                                }
                                else
                                {
                                    switch (hv_TriggerMode.S)
                                    {
                                        case "On":
                                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "TriggerSource", ref hv_TriggerMode, ref strErrMsg))
                                            {
                                                returnResult = -1;
                                                strErrMsg = strCameraInterfaceType + "接口触发已打开但触发源查询出错！";
                                                hv_TriggerMode = "On";
                                            }
                                            else
                                            {
                                                returnResult = 0;
                                                if (hv_TriggerMode.S == "Line0" || hv_TriggerMode.S == "Line1" || hv_TriggerMode.S == "Line2")
                                                {
                                                    hv_TriggerMode = "Line1";
                                                }
                                                else if (hv_TriggerMode.S == "Software")
                                                {
                                                    hv_TriggerMode = "Software";
                                                }
                                            }
                                            break;

                                        case "Off":
                                            returnResult = 0;
                                            hv_TriggerMode = "Off";
                                            break;
                                        default:
                                            returnResult = -1;
                                            strErrMsg = "无法识别查询到的触发模式！";
                                            break;
                                    }
                                }
                            }
                            break;

                        case "DirectShow"://支持basler、不支持海康
                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "external_trigger", ref hv_TriggerMode, ref strErrMsg))
                            {
                                returnResult = -1;
                                strErrMsg = strCameraInterfaceType + "接口触发模式查询出错！";
                            }
                            else
                            {
                                HTuple hv_IsString = new HTuple();
                                HOperatorSet.TupleIsString(hv_TriggerMode, out hv_IsString);
                                if ((int)(hv_IsString) == 0)//如果不是字符串
                                {
                                    returnResult = -1;
                                    strErrMsg = strCameraInterfaceType + "接口触发模式查询出错！";
                                }
                                else
                                {

                                    switch (hv_TriggerMode.S)
                                    {
                                        case "true":
                                            returnResult = 0;
                                            hv_TriggerMode = "On";

                                            //if (-1 == HdevGetCameraParam(hv_AcqHandle, "TriggerSource", ref hv_TriggerMode, ref strErrMsg))
                                            //{
                                            //    returnResult = -1;
                                            //    strErrMsg = strCameraInterfaceType + "接口触发已打开但触发源查询出错！";
                                            //    hv_TriggerMode = "On";
                                            //}
                                            //else
                                            //{
                                            //    returnResult = 0;
                                            //    if (hv_TriggerMode.S == "Line0" || hv_TriggerMode.S == "Line1" || hv_TriggerMode.S == "Line2")
                                            //    {
                                            //        hv_TriggerMode = "Line1";
                                            //    }
                                            //    else if (hv_TriggerMode.S == "Software")
                                            //    {
                                            //        hv_TriggerMode = "Software";
                                            //    }
                                            //}

                                            break;
                                        case "false":
                                            returnResult = 0;
                                            hv_TriggerMode = "Off";
                                            break;
                                        default:
                                            returnResult = -1;
                                            strErrMsg = "无法识别查询到的触发模式！";
                                            break;
                                    }
                                }
                            }
                            break;

                        case "GenICamTL"://不支持海康、支持巴斯勒相机
                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "TriggerMode", ref hv_TriggerMode, ref strErrMsg))
                            {
                                returnResult = -1;
                                strErrMsg = strCameraInterfaceType + "接口触发模式查询出错！";
                            }
                            else
                            {
                                HTuple hv_IsString = new HTuple();
                                HOperatorSet.TupleIsString(hv_TriggerMode, out hv_IsString);
                                if ((int)(hv_IsString) == 0)//如果不是字符串
                                {
                                    returnResult = -1;
                                    strErrMsg = strCameraInterfaceType + "接口触发模式查询出错！";
                                }
                                else
                                {
                                    switch (hv_TriggerMode.S)
                                    {
                                        case "On":
                                            if (-1 == HdevGetCameraParam(hv_AcqHandle, "TriggerSource", ref hv_TriggerMode, ref strErrMsg))
                                            {
                                                returnResult = -1;
                                                strErrMsg = strCameraInterfaceType + "接口触发已打开但触发源查询出错！";
                                                hv_TriggerMode = "On";
                                            }
                                            else
                                            {
                                                returnResult = 0;
                                                if (hv_TriggerMode.S == "Line0" || hv_TriggerMode.S == "Line1" || hv_TriggerMode.S == "Line2")
                                                {
                                                    hv_TriggerMode = "Line1";
                                                }
                                                else if (hv_TriggerMode.S == "Software")
                                                {
                                                    hv_TriggerMode = "Software";
                                                }
                                            }
                                            break;

                                        case "Off":
                                            returnResult = 0;
                                            hv_TriggerMode = "Off";
                                            break;
                                        default:
                                            returnResult = -1;
                                            strErrMsg = "无法识别查询到的触发模式！";
                                            break;
                                    }
                                }
                            }
                            break;

                        default:
                            returnResult = -1;
                            strErrMsg = "输入的接口名暂不支持！";
                            break;
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            return returnResult;
        }


        //其他参数待增加。。。。

        #endregion


        #region "形状模板匹配"相关
        //之前别人写的：图像放大缩小，平移，适合窗口、获得鼠标像素位置、鼠标移动图像

        /************************************************
        功能：	图像上更新显示对象(区域，xld或只显示图像)
        输入参数：
         * Image  图像变量
         * objDisp   显示对象
         * hWindowHandle   窗口句柄
         * bInitial  是否对对象(图像、region、xld)进行初始化(空对象)
        日期:2018-12-06
         ************************************************/
        public void UpdateImage(HObject Image, ref HObject objDisp, HTuple hWindowHandle, bool isInitialObj = false)
        {
            //复位显示图形
            if (isInitialObj == true)
            {
                objDisp.Dispose();
                HOperatorSet.GenEmptyObj(out objDisp);
            }
            //HOperatorSet.SetSystem("flush_graphic", "false");
            HOperatorSet.SetSystem("flush_graphic", "true");
            //清除显示窗口
            HOperatorSet.ClearWindow(hWindowHandle);

            //显示图像和图形
            if (Image.IsInitialized())//即：对象不为null
            {
                HOperatorSet.DispObj(Image, hWindowHandle);
            }
            HOperatorSet.SetSystem("flush_graphic", "true");
            if (objDisp.IsInitialized())//即：对象不为null
            {
                HOperatorSet.DispObj(objDisp, hWindowHandle);
            }
        }
        /************************************************
        功能：	图像界面放大缩小
        输入参数：
         * hv_WH  窗体，
         * ho_Image   图像
         * hv_Zoom   “+”  -  放大缩小变量 
         * 
        日期:2017-10-13
        ************************************************/
        public int ZoonImage(HObject ho_Image, HTuple hv_WH, HTuple hv_Zoom)
        {
            // 放大，缩小图片
            HTuple hv_RS = null; HTuple hv_RE = null;
            HTuple hv_CS = null; HTuple hv_CE = null;
            HTuple hv_Row1 = null; HTuple hv_Column1 = null;
            HTuple hv_Row2 = null; HTuple hv_Column2 = null;
            HTuple hv_Width = null; HTuple hv_Height = null;
            HTuple hv_Sclae = null; HTuple hv_RL = null;
            HTuple hv_CL = null;// HTuple hv_Exception = null;
            try
            {
                hv_RS = new HTuple(0);
                hv_RE = new HTuple(0);
                hv_CS = new HTuple(0);
                hv_CE = new HTuple(0);

                HOperatorSet.GetPart(hv_WH, out hv_Row1, out hv_Column1, out hv_Row2, out hv_Column2);
                hv_Width = hv_Row2.TupleSub(hv_Row1);
                hv_Height = hv_Column2.TupleSub(hv_Column1);

                if (new HTuple(hv_Zoom.TupleEqual(new HTuple("+"))).I != 0)
                {

                    if (hv_Row1.D < 1000)
                    {
                        hv_Sclae = new HTuple(0.7);
                    }
                    else
                    {
                        hv_Sclae = new HTuple(1);
                    }
                }
                else if (new HTuple(hv_Zoom.TupleEqual(new HTuple("-"))).I != 0)
                {
                    if (hv_Row1.D > -400)
                    {
                        hv_Sclae = new HTuple(1.3);
                    }
                    else
                    {
                        hv_Sclae = new HTuple(1);
                    }

                }

                hv_RL = ((hv_Width.TupleMult(hv_Sclae.TupleSub(new HTuple(1))))).TupleDiv(new HTuple(2));

                hv_CL = ((hv_Height.TupleMult(hv_Sclae.TupleSub(new HTuple(1))))).TupleDiv(new HTuple(2));
                hv_RS = hv_Row1.TupleSub(hv_RL);
                hv_RE = hv_Row2.TupleAdd(hv_RL);
                hv_CS = hv_Column1.TupleSub(hv_CL);
                hv_CE = hv_Column2.TupleAdd(hv_CL);
                HOperatorSet.SetPart(hv_WH, hv_RS, hv_CS, hv_RE, hv_CE);
                HOperatorSet.ClearWindow(hv_WH);
                HOperatorSet.DispObj(ho_Image, hv_WH);

                ////   int i = new HTuple(hv_Zoom.TupleEqual(new HTuple("+"))).I;

                return 0;
            }
            //(HalconException HDevExpDefaultException1)
            catch
            {
                //   HDevExpDefaultException1.ToHTuple(out hv_Exception);

                return -1;

            }
        }
        /************************************************
       功能：	图像适应界面
       输入参数：
        * hv_WH  窗体，
        * ho_Image   图像
        * sImageRotateState   图像旋转状态 
        * ImageRotateAngle    图像旋转角度
        * circleRadius         显示圆半径
       日期:2017-10-13
       ************************************************/
        public void FitWindow1(HTuple hv_Window, HObject ho_Image, string sImageRotateState, HTuple ImageRotateAngle, HTuple circleRadius, ref HTuple hv_Err)
        {
            HTuple hv_ImageWidth = new HTuple();
            HTuple hv_ImageHeight = new HTuple();
            HTuple hv_Exception = null;
            HTuple hv_OutInt = new HTuple();
            try
            {
                HOperatorSet.GetImageSize(ho_Image, out hv_ImageWidth, out hv_ImageHeight);
                if ((sImageRotateState == "1" && (ImageRotateAngle.D == 90) || ImageRotateAngle.D == 270))
                {
                    HOperatorSet.SetPart(hv_Window, 0, 0, hv_ImageWidth, hv_ImageHeight);
                    HOperatorSet.DispObj(ho_Image, hv_Window);
                }
                else
                {
                    HOperatorSet.SetPart(hv_Window, 0, 0, hv_ImageHeight, hv_ImageWidth);
                    HOperatorSet.DispObj(ho_Image, hv_Window);
                }
                hv_OutInt = 0;
            }
            catch (HalconException HDevExpDefaultException1)
            {
                HDevExpDefaultException1.ToHTuple(out hv_Exception);
                //     disp_message(hv_Window, hv_Exception, "image", 0, 0, "red", "false");
                hv_OutInt = 1;
                return;
            }

        }
        /************************************************
       功能：	图像适应窗口界面
       输入参数：
       * hv_WH  窗体，
       * ho_Image   图像
       * hv_WinW   窗体宽
       * hv_WinH    窗体高
       *   hv_FitType = "FWin"、"FHeight"、"FWidth" 、"FAll"("FWin"保持图像宽高比适应窗体，"FAll"完全适应窗体，"FHeight"表按图像高适应窗体，"FWidth"按图像宽适应窗体)
       日期:2017-10-13
       ************************************************/
        public int FitWindow(HObject ho_Image, HTuple hv_WH, HTuple hv_WinW, HTuple hv_WinH, HTuple hv_FitType)
        {
            ///适应 图片
            //HTuple hv_Row1 = null; HTuple hv_Column1 = null;
            //HTuple hv_Row2 = null; HTuple hv_Column2 = null;
            HTuple hv_Width = null; HTuple hv_Height = null;
            HTuple hv_WRate = null; HTuple hv_IRate = null;
            HTuple hv_CS = null; HTuple hv_CE = null;
            HTuple hv_RS = null; HTuple hv_RE = null;
            HTuple hv_CLenght = null; HTuple hv_RLenght = null;

            //HTuple hv_Exception = null;

            try
            {
                //HOperatorSet.GetPart(hv_WH, out hv_Row1, out hv_Column1, out hv_Row2, out hv_Column2);

                HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
                hv_WRate = ((hv_WinW.TupleReal())).TupleDiv(hv_WinH.TupleReal());
                hv_IRate = ((hv_Width.TupleReal())).TupleDiv(hv_Height.TupleReal());

                hv_RS = new HTuple(0);
                hv_RE = new HTuple(0);
                hv_CS = new HTuple(0);
                hv_CE = new HTuple(0);
                //new HTuple(0);
                if (new HTuple(hv_FitType.TupleEqual(new HTuple("FWin"))).I != 0)
                {
                    //适应窗体
                    //int i = new HTuple(hv_FitType.TupleEqual(new HTuple("FWin"))).I; //相等，则返回1
                    if (new HTuple(hv_WRate.TupleGreater(hv_IRate)).I != 0)///比较两个数大小，大于
                    {

                        hv_RS = new HTuple(0);
                        hv_RE = hv_Height.Clone();
                        hv_CLenght = ((((hv_Height.TupleMult(hv_WRate))).TupleSub(hv_Width))).TupleDiv(new HTuple(2));
                        hv_CS = hv_CLenght.TupleNeg();
                        hv_CE = hv_CLenght.TupleAdd(hv_Width);

                    }
                    else
                    {
                        hv_CS = new HTuple(0);
                        hv_CE = hv_Width.Clone();
                        hv_RLenght = ((((hv_Width.TupleDiv(hv_WRate))).TupleSub(hv_Height))).TupleDiv(new HTuple(2));
                        hv_RS = hv_RLenght.TupleNeg();
                        hv_RE = hv_RLenght.TupleAdd(hv_Height);

                    }
                }
                else if (new HTuple(hv_FitType.TupleEqual(new HTuple("FHeight"))).I != 0)
                {
                    //高度适应
                    hv_RS = new HTuple(0);
                    hv_RE = hv_Height.Clone();

                    hv_CLenght = ((((hv_Height.TupleMult(hv_WRate))).TupleSub(hv_Width))).TupleDiv(new HTuple(2));
                    hv_CS = hv_CLenght.TupleNeg();
                    hv_CE = hv_CLenght.TupleAdd(hv_Width);

                }
                else if (new HTuple(hv_FitType.TupleEqual(new HTuple("FWidth"))).I != 0)
                {
                    //宽度适应
                    hv_CS = new HTuple(0);
                    hv_CE = hv_Width.Clone();

                    hv_RLenght = ((((hv_Width.TupleDiv(hv_WRate))).TupleSub(hv_Height))).TupleDiv(new HTuple(2));
                    hv_RS = hv_RLenght.TupleNeg();
                    hv_RE = hv_RLenght.TupleAdd(hv_Height);
                }
                else if (new HTuple(hv_FitType.TupleEqual(new HTuple("FAll"))).I != 0)
                {
                    hv_RS = new HTuple(0);
                    hv_RE = hv_Height.Clone();
                    hv_CS = new HTuple(0);
                    hv_CE = hv_Width.Clone();
                }


                //hv_RL = hv_HRate;
                //hv_CL = hv_WRate;
                //hv_RS = hv_Row1.TupleSub(hv_RL);
                //hv_RE = hv_Row2.TupleSub(hv_RL);
                //hv_CS = hv_Column1.TupleSub(hv_CL);
                //hv_CE = hv_Column2.TupleSub(hv_CL);

                HOperatorSet.SetPart(hv_WH, hv_RS, hv_CS, hv_RE, hv_CE);
                HOperatorSet.ClearWindow(hv_WH);
                HOperatorSet.DispObj(ho_Image, hv_WH);

                return 0;//成功返回0

            }
            catch (HalconException /*HDevExpDefaultException1*/)
            {
                //HDevExpDefaultException1.ToHTuple(out hv_Exception);
                return -1;//失败返回-1
            }

        }
        /************************************************
                功能：	图像界面平移
        输入参数：
         * hv_WH  窗体，
         * ho_Image   图像
         * hv_Direction   平移方向  L R D U
         * iStep    平移距离
        日期:2017-10-13
        ************************************************/
        public void MoveImage(HObject ho_Image, HTuple hv_WH, HTuple hv_Direction, int iStep, ref HTuple hv_Err)
        {
            // active = true;

            ///平移 图片
            HTuple hv_RS = null; HTuple hv_RE = null;
            HTuple hv_CS = null; HTuple hv_CE = null;
            HTuple hv_Row1 = null; HTuple hv_Column1 = null;
            HTuple hv_Row2 = null; HTuple hv_Column2 = null;
            HTuple hv_Width = null; HTuple hv_Height = null;
            HTuple hv_HRate = null; HTuple hv_WRate = null;
            HTuple hv_RL = null; HTuple hv_CL = null;
            HTuple hv_Exception = null;
            //HTuple hv_Err = null;
            try
            {
                hv_RS = new HTuple(0);
                hv_RE = new HTuple(0);
                hv_CS = new HTuple(0);
                hv_CE = new HTuple(0);

                HOperatorSet.GetPart(hv_WH, out hv_Row1, out hv_Column1, out hv_Row2, out hv_Column2);
                hv_Width = hv_Row2.TupleSub(hv_Row1);
                hv_Height = hv_Column2.TupleSub(hv_Column1);
                hv_HRate = new HTuple(0);
                hv_WRate = new HTuple(0);

                if (new HTuple(hv_Direction.TupleEqual(new HTuple("L"))).I != 0)
                {
                    hv_WRate = new HTuple(-iStep);
                }
                else if (new HTuple(hv_Direction.TupleEqual(new HTuple("R"))).I != 0)
                {
                    hv_WRate = new HTuple(iStep);
                }
                else if (new HTuple(hv_Direction.TupleEqual(new HTuple("D"))).I != 0)
                {
                    hv_HRate = new HTuple(-iStep);
                }
                else if (new HTuple(hv_Direction.TupleEqual(new HTuple("U"))).I != 0)
                {
                    hv_HRate = new HTuple(iStep);
                }
                hv_RL = hv_HRate;
                hv_CL = hv_WRate;
                hv_RS = hv_Row1.TupleSub(hv_RL);
                hv_RE = hv_Row2.TupleSub(hv_RL);
                hv_CS = hv_Column1.TupleSub(hv_CL);
                hv_CE = hv_Column2.TupleSub(hv_CL);

                HOperatorSet.SetPart(hv_WH, hv_RS, hv_CS, hv_RE, hv_CE);
                HOperatorSet.ClearWindow(hv_WH);
                HOperatorSet.DispObj(ho_Image, hv_WH);
                hv_Err = new HTuple(0);

                ////   int i = new HTuple(hv_Zoom.TupleEqual(new HTuple("+"))).I;


            }
            catch (HalconException HDevExpDefaultException1)
            {

                HDevExpDefaultException1.ToHTuple(out hv_Exception);
                hv_Err = new HTuple(1);
                return;
            }
            //  active = false;

        }
        /************************************************
       功能：	图像界面平移
       输入参数：
        * hv_WH  窗体，
        * ho_Image   图像
        * rowMove   row平移距离 
        * columMove    colum平移距离
       日期:2017-10-13
       ************************************************/
        public int MoveImage1(HObject ho_Image, HTuple hv_WH, HTuple rowMove, HTuple columMove)
        {

            //平移 图片
            HTuple hv_RS = null; HTuple hv_RE = null;
            HTuple hv_CS = null; HTuple hv_CE = null;
            HTuple hv_Row1 = null; HTuple hv_Column1 = null;
            HTuple hv_Row2 = null; HTuple hv_Column2 = null;
            HTuple hv_Width = null; HTuple hv_Height = null;
            HTuple hv_HRate = null; HTuple hv_WRate = null;
            HTuple hv_RL = null; HTuple hv_CL = null;
            //HTuple hv_Exception = null;

            try
            {
                hv_RS = new HTuple(0);
                hv_RE = new HTuple(0);
                hv_CS = new HTuple(0);
                hv_CE = new HTuple(0);

                HOperatorSet.GetPart(hv_WH, out hv_Row1, out hv_Column1, out hv_Row2, out hv_Column2);
                hv_Width = hv_Row2.TupleSub(hv_Row1);
                hv_Height = hv_Column2.TupleSub(hv_Column1);
                //hv_HRate = new HTuple(0);
                //hv_WRate = new HTuple(0);

                hv_WRate = columMove;
                hv_HRate = rowMove;
                hv_RL = hv_HRate;
                hv_CL = hv_WRate;
                hv_RS = hv_Row1.TupleSub(hv_RL);
                hv_RE = hv_Row2.TupleSub(hv_RL);
                hv_CS = hv_Column1.TupleSub(hv_CL);
                hv_CE = hv_Column2.TupleSub(hv_CL);

                HOperatorSet.SetPart(hv_WH, hv_RS, hv_CS, hv_RE, hv_CE);
                HOperatorSet.ClearWindow(hv_WH);
                HOperatorSet.DispObj(ho_Image, hv_WH);
                return 0;
            }
            //  (HalconException HDevExpDefaultException1)
            catch
            {

                // HDevExpDefaultException1.ToHTuple(out hv_Exception);
                return -1;
            }
        }
        /************************************************
        功能：	图像鼠标获取像素位置
        输入参数：
        * hv_WH  窗体，
        输出返回参数
        * hv_Row   返回像素row
        * hv_Colum  返回像素  colum
        * hv_Button   返回功能键值
        日期:2017-10-13
        ************************************************/
        public void GetMpositionSubPix(HTuple hv_WH, out HTuple hv_Row, out HTuple hv_Colum, out HTuple hv_Button)
        {
            bool loop = true;
            hv_Row = null;
            hv_Colum = null;
            hv_Button = null;
            //int num = 0;
            while (loop)
            {
                try
                {
                    HOperatorSet.GetMpositionSubPix(hv_WH, out hv_Row, out hv_Colum, out hv_Button);
                    loop = false;
                }
                catch
                {
                    //num++;
                    //if (num > 100)//如果出错，多次尝试重新获取该值
                    //{
                    //    break;
                    //}

                }
                System.Threading.Thread.Sleep(3);
            }
            return;
        }
        /************************************************
        功能：	鼠标按下拖动图像并显示拖到新位置的图像
        输入参数：
        * hv_WH  窗体，
        * ho_Image   图像
        * hv_DownRow   鼠标左击按下时记录的row
        * hv_DownColum   鼠标左击记录按下时记录的colum
        * hv_MinDis     移动最小像素  ，用来防抖，比如设置为5
        日期:2017-10-13
        ************************************************/
        public void MoveImageShow(HObject ho_Image, HTuple hv_WH, HTuple hv_DownRow, HTuple hv_DownColum, HTuple hv_MinDis)
        {
            bool loop = true;
            int num = 0;
            while (loop)
            {
                try
                {
                    HTuple currentRow = null;
                    HTuple currentColum = null;
                    HTuple hv_Button = null;
                    GetMpositionSubPix(hv_WH, out currentRow, out currentColum, out hv_Button);
                    HTuple moveRow = (double)(currentRow - hv_DownRow);
                    HTuple moveCol = (double)(currentColum - hv_DownColum);
                    double dis = System.Math.Sqrt(moveRow * moveRow + moveCol * moveCol);
                    if (dis > hv_MinDis)
                    {
                        MoveImage1(ho_Image, hv_WH, moveRow, moveCol);
                    }
                    loop = false;
                }
                catch
                {
                    num++;
                    if (num > 50)//如果出错，多次尝试重新获取该值
                    {
                        break;
                    }
                }
            }
            return;
        }


        /*********图像显示区域设置相关******************/

        /************************************************
      功能：基于指定位置来缩放显示窗口的区域（相当于缩放显示对象：图像、Region、xld）
      参数：
       * hv_WindowHandle  窗体句柄，
       * refPointSelect   1表示使用当前显示区域中心位置为参考点，2表示以鼠标当前位置为参考点
       * zoomMult：是个大于0的数，缩放系数(小于1缩小，大于1放大)
       * 返回错误信息描述
       * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-11-09
      ************************************************/
        public int ZoomObjectSetPart(HTuple hv_WindowHandle, int refPointSelect, double zoomMult, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            // Local control variables 
            HTuple hv_Width = new HTuple();
            HTuple hv_Height = new HTuple();

            HTuple hv_currentUpleftR = new HTuple(), hv_currentUpleftC = new HTuple();
            HTuple hv_currentLowrightR = new HTuple(), hv_currentLowrightC = new HTuple();
            HTuple hv_refRow = new HTuple(), hv_refColumn = new HTuple();
            HTuple hv_upleftTan = new HTuple(), hv_radAngle1 = new HTuple();
            HTuple hv_lowrightTan = new HTuple(), hv_radAngle2 = new HTuple();
            HTuple hv_newUpleftR = new HTuple(), hv_newUpleftC = new HTuple();
            HTuple hv_newlowrightR = new HTuple(), hv_newlowrightC = new HTuple();
            HTuple hv_refBtn = new HTuple();
            // Initialize local and output iconic variables  

            try//捕获C#语法异常，比如“未将对象的引用对象的实例”此异常HalconException捕获不住
            {
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }

                try
                {
                    HOperatorSet.GetPart(hv_WindowHandle, out hv_currentUpleftR, out hv_currentUpleftC,
                               out hv_currentLowrightR, out hv_currentLowrightC);//获得当前显示区域
                    hv_Width = hv_currentLowrightC - hv_currentUpleftC;
                    hv_Height = hv_currentLowrightR - hv_currentUpleftR;

                    switch (refPointSelect)
                    {
                        case 1:
                            //获得参考点1(显示区域中心)
                            hv_refRow = (hv_currentUpleftR + hv_currentLowrightR) / 2;
                            hv_refColumn = (hv_currentUpleftC + hv_currentLowrightC) / 2;
                            break;
                        case 2:
                            //获得参考点2：(鼠标)
                            //HOperatorSet.GetMposition(hv_WindowHandle, out hv_refRow, out hv_refColumn,out hv_refBtn);
                            if (-1 == HdevGetMposition(hv_WindowHandle, out hv_refRow, out hv_refColumn, out hv_refBtn))
                            {
                                strErrMsg = "获取鼠标位置出错！";
                                return -1;
                            }
                            break;
                        default:
                            strErrMsg = "输入参考点模式不支持！";
                            return -1;
                            //break;
                    }

                    /***以考点为基准来放大或缩小显示区域***/

                    //新的显示区域左上角坐标
                    hv_newUpleftR = hv_refRow - (hv_refRow - hv_currentUpleftR) * zoomMult;
                    hv_newUpleftC = hv_refColumn - (hv_refColumn - hv_currentUpleftC) * zoomMult;

                    ////将原始区域平移到新左上角位置，则此时原始区域右下角位置为：
                    //hv_currentLowrightR = hv_currentLowrightR + (hv_newUpleftR - hv_currentUpleftR);
                    //hv_currentLowrightC = hv_currentLowrightC + (hv_newUpleftC - hv_currentUpleftC);

                    //缩放平移后新的显示区域右下角坐标为(以左上角为固定点缩放区域)：
                    hv_newlowrightR = hv_newUpleftR + hv_Height * zoomMult;
                    hv_newlowrightC = hv_newUpleftC + hv_Width * zoomMult;

                    //新的显示区域右下角坐标
                    HOperatorSet.SetPart(hv_WindowHandle, hv_newUpleftR, hv_newUpleftC, hv_newlowrightR, hv_newlowrightC);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /************************************************
          功能：根据不同图像大小设置窗体显示区域的大小，使图像全屏适应窗口界面.
          参数：
           * ho_Image  输入图像，
           * hv_WindowHandle  要设置的指定窗体句柄
           * 返回错误信息描述 
          返回值：成功返回0，失败返回-1
          最近更改日期:2019-8-13
      ************************************************/
        public int ImageFitWindowSetPart(HObject ho_Image, HTuple hv_WindowHandle, ref string strErrMsg)
        {
            strErrMsg = "";
            HTuple hv_Width = new HTuple();
            HTuple hv_Height = new HTuple();
            HTuple hv_Row = new HTuple();
            HTuple hv_Column = new HTuple();
            HTuple hv_WinW = new HTuple();
            HTuple hv_WinH = new HTuple();
            try
            {
                if (!HObjectValided(ho_Image, ref strErrMsg))
                {
                    strErrMsg = "输入图像无效："+ strErrMsg;
                    return -1;
                }
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }

                try
                {
                    HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
                    //设置窗体显示图片时，窗体左上角和右下角相对于图片像素坐标的位置
                    //0, 0, Height,Width：表示图像适应窗体完全显示, 注意右下角坐标为(高,宽)
                    HOperatorSet.SetPart(hv_WindowHandle, 0, 0, hv_Height, hv_Width);//------>宽、高全屏显示

                    //功能开关：
                    bool isByImgBestLong = false;//true:按图像最长边(宽或高)自适应窗口大小显示,否则最短边
                    bool isOptimized = true;//true:适应智能处理图像自适应窗口大小，
                    //否则不智能化(无论图像宽高是否和控件宽高匹配均以图像大小完全显示)处理,智能处理：窗口大小不同图像显示看起来会拉伸
                    if (isOptimized)//已经智能处理：根据窗口大小自动适应窗体，且图像看起来不会拉伸变形
                    {
                        //求窗体控件宽高
                        HOperatorSet.GetWindowExtents(hv_WindowHandle, out hv_Row, out hv_Column, out hv_WinW, out hv_WinH);
                        if (hv_WinW >= hv_WinH)//如果窗体控件宽大于高
                        {
                            if (hv_Width >= hv_Height)//如果图像宽大于高：图像旋转0<=A<=360且A!=90、A!=270度时
                            {
                                //设置窗体显示图片时，窗体左上角和右下角相对于图片像素坐标的位置
                                //0, 0, Height,Width：表示图像适应窗体完全显示, 注意右下角坐标为(高,宽)
                                HOperatorSet.SetPart(hv_WindowHandle, 0, 0, hv_Height, hv_Width);//--->宽、高全屏显示
                            }
                            else//如果图像高大于宽：在图像旋转90、270度时
                            {
                                if (isByImgBestLong)
                                {
                                    //(1)如果按图像最长边：(图像高恰好充满控件高)，自适应显示
                                    double ScaleH = hv_Height * 1.0 / hv_WinH;//以高为准，缩放系数
                                    //第一步：区域起点坐标(0,startCol)
                                    //第二步:区域的终点坐标(hv_Height, endCol)
                                    HTuple startCol = -(ScaleH * hv_WinW - hv_Width) / 2.0;
                                    HTuple endCol = hv_Width - startCol;
                                    HOperatorSet.TupleInt(startCol, out startCol);
                                    HOperatorSet.TupleInt(endCol, out endCol);

                                    HOperatorSet.SetPart(hv_WindowHandle, 0, startCol, hv_Height, endCol);//--->图像最长边(高)全屏显示
                                }
                                else
                                {
                                    //(2)如果按图像最短边:(图像宽恰好充满控件宽)，自适应显示
                                    double ScaleW = hv_Width * 1.0 / hv_WinW;//以宽为准，缩放系数
                                    //第一步：区域起点坐标(startRow,0)
                                    //第二步:区域的终点坐标( endRow, hv_Width)
                                    HTuple startRow = (hv_Height - ScaleW * hv_WinH) / 2.0;
                                    HTuple endRow = hv_Height - startRow;
                                    HOperatorSet.TupleInt(startRow, out startRow);
                                    HOperatorSet.TupleInt(endRow, out endRow);
                                    HOperatorSet.SetPart(hv_WindowHandle, startRow, 0, endRow, hv_Width);//--->图像最短边(宽)全屏显示
                                }
                            }
                        }
                        else
                        {
                            if (hv_Width >= hv_Height)//如果图像宽大于高：图像旋转0<=A<=360且A!=90、A!=270度时
                            {
                                if (isByImgBestLong)
                                {
                                    //(1)如果按图像最长边：(图像宽恰好充满控件高)，自适应显示
                                    double ScaleW = hv_Width * 1.0 / hv_WinW;//以宽为准，缩放系数
                                    //第一步：区域起点坐标(startRow,0)
                                    //第二步:区域的终点坐标( endRow, hv_Width)
                                    HTuple startRow = -(ScaleW * hv_WinH - hv_Height) / 2.0;
                                    HTuple endRow = hv_Height - startRow;
                                    HOperatorSet.TupleInt(startRow, out startRow);
                                    HOperatorSet.TupleInt(endRow, out endRow);
                                    HOperatorSet.SetPart(hv_WindowHandle, startRow, 0, endRow, hv_Width);//--->图像最长边(宽)全屏显示           
                                }
                                else
                                {
                                    //(2)如果按图像最短边:(图像高恰好充满控件宽)，自适应显示
                                    double ScaleH = hv_Height * 1.0 / hv_WinH;//以高为准，缩放系数
                                    //第一步：区域起点坐标(0,startCol)
                                    //第二步:区域的终点坐标(hv_Height, endCol)
                                    HTuple startCol = (hv_Width - ScaleH * hv_WinW) / 2.0;
                                    HTuple endCol = hv_Width - startCol;
                                    HOperatorSet.TupleInt(startCol, out startCol);
                                    HOperatorSet.TupleInt(endCol, out endCol);
                                    HOperatorSet.SetPart(hv_WindowHandle, 0, startCol, hv_Height, endCol);//--->图像最短边(高)全屏显示
                                }
                            }
                            else//如果图像高大于宽：在图像旋转90、270度时
                            {
                                HOperatorSet.SetPart(hv_WindowHandle, 0, 0, hv_Height, hv_Width);//------>宽、高全屏显示
                            }
                        }
                    }
                }
                catch (HalconException HDevExpDefaultException)
                {
                    strErrMsg = "错因：" + HDevExpDefaultException;
                    return -1;//失败返回-1
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;//失败返回-1
            }
            return 0;
        }

        /************************************************
         功能：设置鼠标平移窗体显示区域(看起来好像是在平移image/xld/region).
         参数：
          * hv_WindowHandle  要设置的指定窗体句柄
          * hv_intRow = null; 鼠标平移对象(图像、xld、region)时，初始鼠标行位置，暂时记录
          * hv_intCol = null; 鼠标平移视野对象(图像、xld、region)时，初始鼠标列位置，暂时记录
          * hv_minDis  最小忽略值，如果移动距离大于设定最小距离就移动对象(消抖)
          * 返回错误信息描述
          * 返回值： 成功返回0、失败返回-1
           最近更改日期:2019-4-10
        ************************************************/
        public int MoveObjectSetPart(HTuple hv_WindowHandle, HTuple hv_intRow, HTuple hv_intCol, HTuple hv_minDis, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            // Local control variables 
            HTuple hv_currentRow = new HTuple(), hv_currentCol = new HTuple(), hv_currentBtn = new HTuple();
            HTuple hv_moveRow = new HTuple(), hv_moveCol = new HTuple(), hv_moveDis = new HTuple();

            HTuple hv_currentPartR1 = new HTuple(), hv_currentPartC1 = new HTuple();
            HTuple hv_currentPartR2 = new HTuple(), hv_currentPartC2 = new HTuple();
            HTuple hv_newPartR1 = new HTuple(), hv_newPartC1 = new HTuple();
            HTuple hv_newPartR2 = new HTuple(), hv_newPartC2 = new HTuple();

            // Initialize local and output iconic variables  
            try//捕获C#语法异常，比如“未将对象的引用对象的实例”此异常HalconException捕获不住
            {
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }

                try
                {
                    //获得鼠标移动一段距离后坐标
                    //HOperatorSet.GetMposition(hv_WindowHandle, out hv_currentRow, out hv_currentCol,out hv_currentBtn);//相对于图片左上角的坐标
                    if (-1 == HdevGetMposition(hv_WindowHandle, out hv_currentRow, out hv_currentCol, out hv_currentBtn))
                    {
                        strErrMsg = "获取鼠标位置出错！";
                        return -1;
                    }
                    //移动距离
                    hv_moveRow = hv_currentRow - hv_intRow;
                    hv_moveCol = hv_currentCol - hv_intCol;
                    hv_moveDis = (((hv_moveRow * hv_moveRow) + (hv_moveCol * hv_moveCol))).TupleSqrt();

                    if ((int)(new HTuple(hv_moveDis.TupleGreater(hv_minDis))) != 0) //如果移动距离大于设定最小距离就移动图像(消抖)
                    {
                        //获得当前显示区域
                        HOperatorSet.GetPart(hv_WindowHandle, out hv_currentPartR1, out hv_currentPartC1, out hv_currentPartR2, out hv_currentPartC2);
                        hv_newPartR1 = hv_currentPartR1 - hv_moveRow;
                        hv_newPartC1 = hv_currentPartC1 - hv_moveCol;
                        hv_newPartR2 = hv_currentPartR2 - hv_moveRow;
                        hv_newPartC2 = hv_currentPartC2 - hv_moveCol;
                        HOperatorSet.SetPart(hv_WindowHandle, hv_newPartR1, hv_newPartC1, hv_newPartR2, hv_newPartC2);
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /************************************************
           功能：根据不同Xld大小设置显示区域，按Xld最小外接平行矩形来适应窗口显示
            输入参数：
            * 参1 输入原始xld
            * 参2，要设置的窗体句柄
            * 返回错误信息描述 
          返回值：成功返回0，失败返回-1
          最近更改日期:2019-4-10
          ************************************************/
        public int XldRec1FitWindowSetPart(HObject ho_Xld, HTuple hv_WindowHandle, ref string strErrMsg)
        {
            strErrMsg = "";

            HTuple hv_Row1 = new HTuple();
            HTuple hv_Column1 = new HTuple();
            HTuple hv_Row2 = new HTuple();
            HTuple hv_Column2 = new HTuple();

            try
            {
                if (!HObjectValided(ho_Xld, ref strErrMsg))
                {
                    strErrMsg = "输入Xld无效:" + strErrMsg;
                    return -1;
                }
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }
                try
                {
                    HOperatorSet.SmallestRectangle1Xld(ho_Xld, out hv_Row1, out hv_Column1, out hv_Row2, out hv_Column2);
                    HOperatorSet.SetPart(hv_WindowHandle, hv_Row1, hv_Column1, hv_Row2, hv_Column2);
                }
                catch (HalconException HDevExpDefaultException)
                {
                    strErrMsg = "错因：" + HDevExpDefaultException;
                    return -1;//失败返回-1
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;//失败返回-1
            }
            return 0;
        }

        /************************************************
          功能：按指定宽和高(左上角为0，0)来设置显示区域
          参数：
          * hv_WindowHandle  要设置的指定窗体句柄
          * hv_HeightR：输入高度
          * hv_WidthC：手动输入宽度
          返回值：成功返回0，失败返回-1
          最近更改日期:2018-12-06
          ************************************************/
        public int inputImgHWSetPart(HTuple hv_WindowHandle, HTuple hv_HeightR, HTuple hv_WidthC, ref string strErrMsg)
        {
            strErrMsg = "";
            try
            {
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }
                try
                {
                    //设置窗体显示图片时，窗体左上角和右下角相对于图片像素坐标的位置
                    //0, 0, Height,Width：表示图像适应窗体完全显示, 注意右下角坐标为(高,宽)
                    HOperatorSet.SetPart(hv_WindowHandle, 0, 0, hv_HeightR, hv_WidthC);
                }
                catch (HalconException HDevExpDefaultException)
                {
                    strErrMsg = "错因：" + HDevExpDefaultException;
                    return -1;//失败返回-1
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;//失败返回-1
            }
            return 0;
        }


        /********鼠标指针相关****************/

        /************************************************
         功能：(多次稳定获取)输出鼠标在窗体上的像素(整型)或亚像素(浮点型)的位置及鼠标状态
         * 参1：  窗体，
         * 参2：  输出Row位置
         * 参3：  输出Col位置
         * 参4：  输出鼠标状态
         * 返回值，成功返回0，失败返回-1
         日期:2019-9-5
       ************************************************/
        public int HdevGetMposition(HTuple hv_WindowHandle, out HTuple hv_CurrentRow, out HTuple hv_CurrentCol, out HTuple hv_CurrentBtn)
        {
            hv_CurrentRow = new HTuple();
            hv_CurrentCol = new HTuple();
            hv_CurrentBtn = new HTuple();
            int num = 0;
            while (num < 5)//如果获取坐标出错，允许尝试xxx次，如果还是失败则失败
            {
                try
                {
                    HOperatorSet.GetMposition(hv_WindowHandle, out hv_CurrentRow, out hv_CurrentCol, out hv_CurrentBtn);//像素精度
                    //HOperatorSet.GetMpositionSubPix(hv_WindowHandle, out hv_CurrentRow, out hv_CurrentCol, out hv_CurrentBtn);//亚像素精度
                    break;
                }
                catch
                {
                    num++;
                }
            }
            if (num >= 5)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
        /************************************************
       功能： 获取鼠标在窗体上的的(相对于显示图片)行列像素坐标值，同时获取图像当前位置的灰度值
        输入参数：
        * 参1 输入原始图像
        * 参2：窗体句柄
        * 参3、4：鼠标位置行列像素坐标
        * 参5：获取灰度值，单通道为一个值，RGB图像为三值数组
        * 参6：返回错误信息描述
        * 返回值： 成功返回0、失败返回-1
        最近更改日期:2019-4-03
      ************************************************/
        public int queryMpositionGray(HObject ho_Images, HTuple hv_WindowHandle,
            ref HTuple hv_mouseRow, ref HTuple hv_mouseCol, ref HTuple hv_Grayval, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            // Local control variables 
            HTuple hv_mouseBtn = new HTuple();
            // Initialize local and output iconic variables 


            try
            {//这里不能判断图像和窗体输入是否有效
                try
                {
                    //获取鼠标当前坐标
                    //HOperatorSet.GetMposition(hv_WindowHandle, out hv_mouseRow, out hv_mouseCol, out hv_mouseBtn);
                    if (-1 == HdevGetMposition(hv_WindowHandle, out hv_mouseRow, out hv_mouseCol, out hv_mouseBtn))
                    {
                        strErrMsg = "获取鼠标位置出错！";
                        return -1;
                    }
                    //图片可以是多通道，获取指定位置灰度值
                    HOperatorSet.GetGrayval(ho_Images, hv_mouseRow, hv_mouseCol, out hv_Grayval);
                    return 0;
                }
                catch (HalconException hex)
                {
                    strErrMsg = "错因：" + hex;
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strErrMsg = "错因：" + ex;
                return -1;
            }
            finally
            {
            }
        }

        /************************************************
          功能： 当鼠标进入指定区域后鼠标指针变成指定形状，离开后变成另外指定的形状
           输入参数：
           * 参1 输入指定区域，不能为空对象
           * 参2：窗体句柄
           * 参3：进入区域后的形状(比如"Size All")
           * 参5：离开区域后的形状(比如"arrow")
           * 参6：返回错误信息描述
           * 返回值： 成功返回0、失败返回-1
           最近更改日期:2019-4-03
         ************************************************/
        public int setROIMouseShape(HObject ho_allRegionTuple, HTuple hv_WindowHandle,
            string strInShape, string strOutShape, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            // Local control variables 
            HTuple hv_interR = new HTuple();
            HTuple hv_interC = new HTuple(), hv_interBtn = new HTuple();
            HTuple hv_isInter = new HTuple();

            try
            {
                if (!HObjectValided(ho_allRegionTuple, ref strErrMsg))
                {
                    strErrMsg = "输入ROI无效：" + strErrMsg;
                    return -1;
                }
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }
                try
                {
                    //HOperatorSet.GetMposition(hv_WindowHandle, out hv_interR, out hv_interC,out hv_interBtn);
                    if (-1 == HdevGetMposition(hv_WindowHandle, out hv_interR, out hv_interC, out hv_interBtn))
                    {
                        strErrMsg = "获取鼠标位置出错！";
                        return -1;
                    }

                    HOperatorSet.TestRegionPoint(ho_allRegionTuple, hv_interR, hv_interC, out hv_isInter);
                    if ((int)(hv_isInter) != 0)
                    {
                        ////鼠标指针设置成“+”
                        //HOperatorSet.SetMshape(hv_WindowHandle, "crosshair");
                        HOperatorSet.SetMshape(hv_WindowHandle, strInShape);//"Size All"
                    }
                    else
                    {
                        //如果鼠标没有进入区域中
                        HOperatorSet.SetMshape(hv_WindowHandle, strOutShape);//"arrow"
                    }

                    return 0;
                }
                catch (HalconException hex)
                {
                    strErrMsg = "错因：" + hex;
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strErrMsg = "错因：" + ex;
                return -1;
            }
        }

        /************************************************
         功能： 查询当前窗口句柄所支持设置的鼠标指针形状种类
          输入参数：
          * 参1：窗体句柄
          * 参2：输出鼠标指针形状名称，比如['arrow', 'default', 'crosshair', 'text I-beam', 'Slashed circle', 
          * 'Size All', 'Size NESW', 'Size S', 'Size NWSE', 'Size WE', 'Vertical Arrow', 'Hourglass']
          * 参3：返回错误信息描述
          * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-4-03
        ************************************************/
        public int queryMshape(HTuple hv_WindowHandle, ref HTuple hv_ShapeNames, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            // Local control variables 

            // Initialize local and output iconic variables 
            try
            {
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }

                try
                {
                    //获取当前窗口鼠标指针形状名称
                    HOperatorSet.QueryMshape(hv_WindowHandle, out hv_ShapeNames);

                    return 0;
                }
                catch (HalconException hex)
                {
                    strErrMsg = "错因：" + hex;
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strErrMsg = "错因：" + ex;
                return -1;
            }
            finally
            {
            }
        }

        /************************************************
          功能： 设置当前窗口上鼠标指针的形状
           输入参数：
           * 参1：窗体句柄
           * 参2：输入要设置的鼠标指针形状某一个名称，比如：['arrow', 'default', 'crosshair', 'text I-beam', 'Slashed circle', 
           * 'Size All', 'Size NESW', 'Size S', 'Size NWSE', 'Size WE', 'Vertical Arrow', 'Hourglass']中的一个
           * 参3：返回错误信息描述
           * 返回值： 成功返回0、失败返回-1
           最近更改日期:2019-4-03
         ************************************************/
        public int setMshape(HTuple hv_WindowHandle, string strShapeName, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            // Local control variables 
            HTuple hv_interR = new HTuple();
            HTuple hv_interC = new HTuple(), hv_interBtn = new HTuple();
            HTuple hv_isInter = new HTuple();
            try
            {
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }
                try
                {
                    HOperatorSet.SetMshape(hv_WindowHandle, strShapeName);
                    return 0;
                }
                catch (HalconException hex)
                {
                    strErrMsg = "错因：" + hex;
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strErrMsg = "错因：" + ex;
                return -1;
            }

        }


      
        /************图像或xld变换相关*****************/

        /************************************************
       功能：(方式1)对图像进行变换:像素上放缩、规则镜像、裁剪
       参数：
        * 参1输入原始图像,输出变换后图像，
        * 参2、3、4：是否缩放、行缩放系数、列缩放系数
        * 参5、6：是否对称(反射、镜像)变换、镜像类型(关于'row'、'column'、'diagonal'对角线，对称)
        * 参7、8、9、10、11：是否裁剪图像、裁剪起点行、裁剪起点列，终点行、终点列 
        * 返回值：成功返回0、失败返回-1
       最近更改日期:2018-12-06
       ************************************************/
        public int changeImage(ref HObject ho_IntImage, bool isScale, HTuple hv_ScaleRow, HTuple hv_ScaleCol,
                 bool isReflect, HTuple hv_MapType,
                bool isCrop, HTuple hv_Row1, HTuple hv_Col1, HTuple hv_Row2, HTuple hv_Col2)
        {
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            // Local control variables 

            // Initialize local and output iconic variables 

            string strErrMsg = "";

            try
            {
                if (!HObjectValided(ho_IntImage, ref strErrMsg))
                {
                    strErrMsg = "输入图像无效："+ strErrMsg;
                    return -1;
                }
                try
                {
                    if (isScale)
                    {
                        ////以原点(0,0)为基准点来对整副图像进行像素上的缩放
                        //{
                        //    HObject ExpTmpOutVar_0;
                        //    HOperatorSet.ZoomImageSize(ho_IntImage, out ExpTmpOutVar_0, hv_imageWidth,
                        //        hv_imageHeight, "constant");
                        //    ho_IntImage.Dispose();
                        //    ho_IntImage = ExpTmpOutVar_0;
                        //}

                        //按指定宽高缩放系数放缩
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ZoomImageFactor(ho_IntImage, out ExpTmpOutVar_0, hv_ScaleCol,
                                hv_ScaleRow, "constant");
                            ho_IntImage.Dispose();
                            ho_IntImage = ExpTmpOutVar_0;
                        }

                    }
                    else if (isReflect)
                    {
                        //原图像大小不变，图像镜像(关于'row'、'column'、'diagonal'对角线，对称)
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.MirrorImage(ho_IntImage, out ExpTmpOutVar_0, "diagonal");
                            ho_IntImage.Dispose();
                            ho_IntImage = ExpTmpOutVar_0;
                        }
                    }
                    else if (isCrop)
                    {
                        //*以指定矩形来将原图裁剪，新图重新以0,0为起点适应窗体
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.CropRectangle1(ho_IntImage, out ExpTmpOutVar_0, hv_Row1, hv_Col1, hv_Row2, hv_Col2);
                            ho_IntImage.Dispose();
                            ho_IntImage = ExpTmpOutVar_0;
                        }
                    }
                }
                catch (HalconException)
                {
                    return -1;
                }
            }
            catch (Exception)
            {
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /************************************************
         功能：(方式2)对图像进行变换:“仿射变换”,
          输入参数：
          * 参1 输入原始图像,输出变换后图像，
          * 参2、3、4：是否执行平移图像、行平移量、列平移量
          * 参5、6、7、8：是否旋转图像、旋转角增量(角度)、旋转参考点行列值
          * 参9、10、11、12、13：是否缩放、行列缩放系数、缩放参考点
          * 参14、15、16、17、18：是否对称(反射)变换、关于点P和点Q决定的直线对称
          * 参19、20：设置变换后的图像的宽高 
          * 返回值：成功返回0、失败返回-1
          最近更改日期:2018-12-06
        ************************************************/
        public int affineImage(ref HObject ho_IntImage,
                bool isMove, HTuple hv_MoveRow, HTuple hv_MoveCol,
                bool isRotate, HTuple hv_RotateAngle, HTuple hv_AngleBaseRow, HTuple hv_AngleBaseCol,
                bool isScale, HTuple hv_ScaleRow, HTuple hv_ScaleCol, HTuple hv_ScaleBaseR, HTuple hv_ScaleBaseC,
                bool isReflect, HTuple hv_ReflectRowP, HTuple hv_ReflectColP, HTuple hv_ReflectRowQ, HTuple hv_ReflectColQ,
                HTuple hv_setImageWidth, HTuple hv_setImageHeight)
        {
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            // Local control variables 
            HTuple hv_HomMat2D = new HTuple();
            // Initialize local and output iconic variables 


            string strErrMsg = "";

            try
            {

                if (!HObjectValided(ho_IntImage, ref strErrMsg))
                {
                    strErrMsg = "输入图像无效："+ strErrMsg;
                    return -1;
                }

                try
                {

                    //定义二维齐次单位变换矩阵
                    HOperatorSet.HomMat2dIdentity(out hv_HomMat2D);
                    if (isMove)
                    {
                        //添加平移(参2/3：行列平移增量,图像大小宽高不会改变但图像有效区域变小(裁剪))
                        HOperatorSet.HomMat2dTranslate(hv_HomMat2D, hv_MoveRow, hv_MoveCol, out hv_HomMat2D);
                    }
                    if (isRotate)
                    {
                        hv_RotateAngle = (new HTuple(hv_RotateAngle)).TupleRad();
                        //添加旋转(参2要旋转多少角度，参3/4旋转参考基准点)
                        HOperatorSet.HomMat2dRotate(hv_HomMat2D, hv_RotateAngle, hv_AngleBaseRow, hv_AngleBaseCol, out hv_HomMat2D);
                    }
                    if (isScale)
                    {
                        //添加放缩(参2/3行列放缩系数，参4/5放缩参考基准点)
                        HOperatorSet.HomMat2dScale(hv_HomMat2D, hv_ScaleRow, hv_ScaleCol, hv_ScaleBaseR, hv_ScaleBaseC, out hv_HomMat2D);
                    }
                    if (isReflect)
                    {
                        //添加反射变换(镜像,即关于(ReflectRow, ReflectCol）,(ReflectRowQ, ReflectColQ)决定的对称轴对称)(参2/3：行列平移增量)
                        HOperatorSet.HomMat2dReflect(hv_HomMat2D, hv_ReflectRowP, hv_ReflectColP, hv_ReflectRowQ, hv_ReflectColQ, out hv_HomMat2D);
                    }

                    ////'false'右下角或左下角发生裁剪保存图像大小不变，'true'表图像大小会自动改变左上角会发生裁剪右下角不会裁剪，
                    //{
                    //    HObject ExpTmpOutVar_0;
                    //    HOperatorSet.AffineTransImage(ho_IntImage, out ExpTmpOutVar_0, hv_HomMat2D,
                    //        "constant", "false");
                    //    ho_IntImage.Dispose();
                    //    ho_IntImage = ExpTmpOutVar_0;
                    //}

                    //手动设置图像大小，左上角或右下角均可能发生裁剪
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.AffineTransImageSize(ho_IntImage, out ExpTmpOutVar_0, hv_HomMat2D,
                            "constant", hv_setImageWidth, hv_setImageHeight);
                        ho_IntImage.Dispose();
                        ho_IntImage = ExpTmpOutVar_0;
                    }

                }
                catch (HalconException)
                {
                    return -1;
                }

            }
            catch (Exception)
            {
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /************************************************
         功能：对xld进行变换方式:“仿射变换”,
          输入参数：
          * 参1 输入原始xld,输出变换后xld，
          * 参2、3、4：是否执行平移xld、行平移量、列平移量
          * 参5、6、7、8：是否旋转xld、旋转角增量(角度)、旋转参考点行列值
          * 参9、10、11、12、13：是否缩放、行列缩放系数、缩放参考点
          * 参14、15、16、17、18：是否对称(反射)变换、关于点P和点Q决定的直线对称
          * 返回值：成功返回0、失败返回-1
          最近更改日期:2018-12-06
        ************************************************/
        public int affineXld(ref HObject ho_IntXld, bool isMove, HTuple hv_MoveRow, HTuple hv_MoveCol,
                bool isRotate, HTuple hv_RotateAngle, HTuple hv_AngleBaseRow, HTuple hv_AngleBaseCol,
                bool isScale, HTuple hv_ScaleRow, HTuple hv_ScaleCol, HTuple hv_ScaleBaseR, HTuple hv_ScaleBaseC,
                bool isReflect, HTuple hv_ReflectRowP, HTuple hv_ReflectColP, HTuple hv_ReflectRowQ, HTuple hv_ReflectColQ)
        {
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            // Local control variables 
            HTuple hv_HomMat2D = new HTuple();
            // Initialize local and output iconic variables 


            string strErrMsg = "";

            try
            {

                if (!HObjectValided(ho_IntXld, ref strErrMsg))
                {
                    strErrMsg = "输入Xld无效：" + strErrMsg;
                    return -1;
                }

                try
                {

                    //定义二维齐次单位变换矩阵
                    HOperatorSet.HomMat2dIdentity(out hv_HomMat2D);
                    if (isMove)
                    {
                        //添加平移(参2/3：行列平移增量,Xld大小宽高不会改变但xld有效区域变小(裁剪))
                        HOperatorSet.HomMat2dTranslate(hv_HomMat2D, hv_MoveRow, hv_MoveCol, out hv_HomMat2D);
                    }
                    if (isRotate)
                    {
                        hv_RotateAngle = (new HTuple(hv_RotateAngle)).TupleRad();
                        //添加旋转(参2要旋转多少角度，参3/4旋转参考基准点)
                        HOperatorSet.HomMat2dRotate(hv_HomMat2D, hv_RotateAngle, hv_AngleBaseRow, hv_AngleBaseCol, out hv_HomMat2D);
                    }
                    if (isScale)
                    {
                        //添加放缩(参2/3行列放缩系数，参4/5放缩参考基准点)
                        HOperatorSet.HomMat2dScale(hv_HomMat2D, hv_ScaleRow, hv_ScaleCol, hv_ScaleBaseR, hv_ScaleBaseC, out hv_HomMat2D);
                    }
                    if (isReflect)
                    {
                        //添加反射变换(镜像,即关于(ReflectRow, ReflectCol）,(ReflectRowQ, ReflectColQ)决定的对称轴对称)(参2/3：行列平移增量)
                        HOperatorSet.HomMat2dReflect(hv_HomMat2D, hv_ReflectRowP, hv_ReflectColP, hv_ReflectRowQ, hv_ReflectColQ, out hv_HomMat2D);
                    }

                    //原始轮廓xld关于变换矩阵变换后的xld
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.AffineTransContourXld(ho_IntXld, out ExpTmpOutVar_0, hv_HomMat2D);
                        ho_IntXld.Dispose();
                        ho_IntXld = ExpTmpOutVar_0;
                    }
                    ////原始多边形轮廓xld关于变换矩阵变换后的xld
                    //{
                    //    HObject ExpTmpOutVar_0;
                    //    HOperatorSet.AffineTransPolygonXld(ho_IntXld, out ExpTmpOutVar_0, hv_HomMat2D);
                    //    ho_IntXld.Dispose();
                    //    ho_IntXld = ExpTmpOutVar_0;
                    //}

                }
                catch (HalconException)
                {
                    //"对xld进行几何变换时出错！";
                    return -1;
                }

            }
            catch (Exception)
            {
                //"对xld进行几何变换时出错！";
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /*********画ROI或涂擦ROI相关*************/

        /************************************************
         功能： 在窗体上(有图像)画组合ROI，并窗体图片上显示画后的效果---有ROI运算方式和各ROI集合记录输出
          输入参数：
          * 注意：空区域的面积=0， 空对象Region的面积=[]也即面积<0，Region区域存在则面积>0
          * 参1 输入原始图像，参2：输入上次画的已存在ROI(第一次画为空区域，注意不是空对象)
          * 参3：输入上次已画ROI集合记录，(第一次画输入为空对象，注意不是空区域)，参4：窗体句柄
          * 参5：是否自动生成初始化的ROI然后在调整，参6：输入ROI运算方式的记录(第一次画输入为空数组)
          * 参7：选择所画ROI的行形状(目前支持："水平矩形"、"旋转矩形"、"椭圆形"、"椭圆形"、"任意形状")
          * 参8：选择ROI运算方式("并集"、"差集"、"交集"、"对称差"、"补集")
          * 返回错误信息描述 
          * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-4-8
        ************************************************/
        public int DrawROI(HObject ho_Image, ref HObject ho_allRegion, ref HObject ho_allRegionTuple,
            HTuple hv_WindowHandle, bool isInitRegion, ref HTuple hv_recordROISetTuple, string strROIShape, string strSetROI, ref string strErrMsg)
        {
            /*类中所有方法外定义变量与方法内定义的局部变量的区别:
            * (1)方法内部定义的局部变量定义时没有默认自动赋任何值,因此不能直接使用，
            * 只有方法内的其他方法可通过out参数来使用，非out修饰的参数不能使用。
            * (2)类中所有方法外定义的变量定义时即使未赋值也会默认自动赋值，自动赋null(引用类型)或其他值(值类型)
            */

            /*ref与out与params与数组(int[] a)方法参数的区别:
            * (1)ref修饰参数必须方法外先赋值(可以赋null或其他值)方法内可以重新赋新值使用并传出，或不赋值只使用。
            * (2)out修饰的参数的方法外只定义不必赋值(也可以赋值，但如果方法外赋非null的其他值则会生成垃圾内存,
            * 此时方法外需先释放变量内存,避免内存泄漏)，方法内必须先赋新值再使用最后传出，或先赋值不使用直接传出。
            * (3)params修饰方法的参数只能是数组(比如:params int[] a)：表示让方法接收任意个数同类型的参数，
             * 修饰的参数只能传入不能传出，且一个方法只允许有一个params参数且只能放到方法参数的最后一个参数
             * (比如:调用 func("nihao",1,2,3),定义 public void func(string str, params int[] a))
            */

            strErrMsg = "";
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 
            HObject ho_Rec1 = null, ho_Rec2 = null;
            HObject ho_Circle = null, ho_Ellipse = null, ho_anyRegion = null;

            // Local control variables 
            HTuple hv_ROINum = new HTuple(), hv_Area = new HTuple();
            HTuple hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_rec1RowL = new HTuple(), hv_rec1ColL = new HTuple();
            HTuple hv_rec1RowR = new HTuple(), hv_rec1ColR = new HTuple();
            HTuple hv_rec2RowC = new HTuple(), hv_rec2ColC = new HTuple();
            HTuple hv_Phirec2 = new HTuple(), hv_rec2BigHalf = new HTuple();
            HTuple hv_rec2SmaHalf = new HTuple(), hv_CirRow = new HTuple();
            HTuple hv_CirCol = new HTuple(), hv_CirR = new HTuple();
            HTuple hv_elliRow = new HTuple(), hv_elliColumn = new HTuple();
            HTuple hv_elliPhi = new HTuple(), hv_firstR = new HTuple();
            HTuple hv_SecondR = new HTuple();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Rec1);
            HOperatorSet.GenEmptyObj(out ho_Rec2);
            HOperatorSet.GenEmptyObj(out ho_Circle);
            HOperatorSet.GenEmptyObj(out ho_Ellipse);
            HOperatorSet.GenEmptyObj(out ho_anyRegion);

            HTuple hv_ImgW = 1600;
            HTuple hv_ImgH = 1200;
            HTuple hv_WinW = 400;
            HTuple hv_WinH = 300;
            HTuple hv_FontSize = 10;//基准字体大小

            try
            {
                if (!HObjectValided(ho_Image, ref strErrMsg))
                {
                    strErrMsg = "输入图像无效："+ strErrMsg;
                    return -1;
                }

                if (!HObjectValided(ho_allRegion, ref strErrMsg))
                {
                    strErrMsg = "画区域时输入的初始区域无效：" + strErrMsg;
                    return -1;
                }
                if (ho_allRegionTuple == null)
                {
                    strErrMsg = "画区域时输入的初始区域数组不能为null！";
                    return -1;
                }
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }
                try
                {
                    HOperatorSet.GetImageSize(ho_Image, out hv_ImgW, out hv_ImgH);//求图像宽高
                    HTuple hv_Row1 = null, hv_Column1 = null;
                    HOperatorSet.GetWindowExtents(hv_WindowHandle, out hv_Row1, out hv_Column1, out hv_WinW, out hv_WinH);//求窗体控件宽高
                    hv_FontSize = hv_FontSize * (hv_WinH * 1.0 / 270.0);

                    //显示未创建前或修改前的ROI
                    HOperatorSet.SetDraw(hv_WindowHandle, "margin");
                    HOperatorSet.SetLineWidth(hv_WindowHandle, 1);
                    HOperatorSet.SetColor(hv_WindowHandle, "dim gray");
                    HOperatorSet.DispObj(ho_allRegionTuple, hv_WindowHandle);
                    HOperatorSet.SetColor(hv_WindowHandle, "blue");
                    HOperatorSet.DispObj(ho_allRegion, hv_WindowHandle);

                    set_display_font(hv_WindowHandle, hv_FontSize * 0.9, "mono", "false", "false");
                    disp_message(hv_WindowHandle, (new HTuple("鼠标左键画或修改区域，右键确认！")).TupleConcat(
                    ""), "window", (hv_WinH * 0.03), (hv_WinW * 0.01), "red", "false");

                    HOperatorSet.SetDraw(hv_WindowHandle, "margin");
                    HOperatorSet.SetLineWidth(hv_WindowHandle, 1);
                    HOperatorSet.SetColor(hv_WindowHandle, "blue");

                    switch (strROIShape)
                    {
                        case "0"://水平矩形

                            if (isInitRegion)
                            {
                                HOperatorSet.DrawRectangle1Mod(hv_WindowHandle, (hv_ImgH / 2) - 50,
                                 (hv_ImgW / 2) - 50, (hv_ImgH / 2) + 50, (hv_ImgW / 2) + 50,
                                 out hv_rec1RowL, out hv_rec1ColL, out hv_rec1RowR, out hv_rec1ColR);
                            }
                            else
                            {
                                HOperatorSet.DrawRectangle1(hv_WindowHandle, out hv_rec1RowL, out hv_rec1ColL,
                                  out hv_rec1RowR, out hv_rec1ColR);
                            }
                            ho_Rec1.Dispose();
                            HOperatorSet.GenRectangle1(out ho_Rec1, hv_rec1RowL, hv_rec1ColL, hv_rec1RowR,
                                hv_rec1ColR);
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ConcatObj(ho_allRegionTuple, ho_Rec1, out ExpTmpOutVar_0
                                    );
                                ho_allRegionTuple.Dispose();
                                ho_allRegionTuple = ExpTmpOutVar_0;
                            }
                            HOperatorSet.CountObj(ho_allRegionTuple, out hv_ROINum);
                            HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                            switch (strSetROI)
                            {
                                case "0"://并集

                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("0");
                                    }

                                    break;


                                case "1"://差集
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Difference(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("1");
                                    }
                                    break;

                                case "2"://交集
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Intersection(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("2");
                                    }
                                    break;
                                case "3"://对称差
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.SymmDifference(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("3");
                                    }
                                    break;
                                case "4"://补集
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        ho_allRegion.Dispose();
                                        HOperatorSet.Complement(ho_Rec1, out ho_allRegion);
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("4");
                                    }
                                    break;

                                default:
                                    strErrMsg = "选择ROI运算方式列表中不存在！";
                                    return -1;
                                    //break;
                            }
                            break;

                        case "1"://旋转矩形
                            if (isInitRegion)
                            {
                                HOperatorSet.DrawRectangle2Mod(hv_WindowHandle, hv_ImgH / 2, hv_ImgW / 2,
                                 0.785, 70, 70, out hv_rec2RowC, out hv_rec2ColC, out hv_Phirec2,
                                 out hv_rec2BigHalf, out hv_rec2SmaHalf);
                            }
                            else
                            {
                                HOperatorSet.DrawRectangle2(hv_WindowHandle, out hv_rec2RowC, out hv_rec2ColC,
                                  out hv_Phirec2, out hv_rec2BigHalf, out hv_rec2SmaHalf);
                            }
                            ho_Rec2.Dispose();
                            HOperatorSet.GenRectangle2(out ho_Rec2, hv_rec2RowC, hv_rec2ColC, hv_Phirec2,
                                hv_rec2BigHalf, hv_rec2SmaHalf);
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ConcatObj(ho_allRegionTuple, ho_Rec2, out ExpTmpOutVar_0
                                    );
                                ho_allRegionTuple.Dispose();
                                ho_allRegionTuple = ExpTmpOutVar_0;
                            }
                            HOperatorSet.CountObj(ho_allRegionTuple, out hv_ROINum);
                            HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                            switch (strSetROI)
                            {
                                case "0":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("0");
                                    }
                                    break;

                                case "1":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Difference(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("1");
                                    }
                                    break;

                                case "2":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Intersection(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("2");
                                    }
                                    break;

                                case "3":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.SymmDifference(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("3");
                                    }
                                    break;

                                case "4":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        ho_allRegion.Dispose();
                                        HOperatorSet.Complement(ho_Rec2, out ho_allRegion);
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("4");
                                    }
                                    break;
                                default:
                                    strErrMsg = "选择ROI运算方式列表中不存在！";
                                    return -1;
                                    //break;
                            }
                            break;
                        case "2"://圆形
                            if (isInitRegion)
                            {
                                HOperatorSet.DrawCircleMod(hv_WindowHandle, hv_ImgH / 2, hv_ImgW / 2,
                                  100, out hv_CirRow, out hv_CirCol, out hv_CirR);
                            }
                            else
                            {
                                HOperatorSet.DrawCircle(hv_WindowHandle, out hv_CirRow, out hv_CirCol,
                                 out hv_CirR);
                            }
                            ho_Circle.Dispose();
                            HOperatorSet.GenCircle(out ho_Circle, hv_CirRow, hv_CirCol, hv_CirR);
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ConcatObj(ho_allRegionTuple, ho_Circle, out ExpTmpOutVar_0
                                    );
                                ho_allRegionTuple.Dispose();
                                ho_allRegionTuple = ExpTmpOutVar_0;
                            }
                            HOperatorSet.CountObj(ho_allRegionTuple, out hv_ROINum);
                            HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                            switch (strSetROI)
                            {
                                case "0":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Circle, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Circle, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("0");
                                    }
                                    break;

                                case "1":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Circle, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Difference(ho_allRegion, ho_Circle, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("1");
                                    }
                                    break;

                                case "2":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Circle, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Intersection(ho_allRegion, ho_Circle, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("2");
                                    }
                                    break;

                                case "3":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Circle, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.SymmDifference(ho_allRegion, ho_Circle, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("3");
                                    }
                                    break;

                                case "4":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Circle, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        ho_allRegion.Dispose();
                                        HOperatorSet.Complement(ho_Circle, out ho_allRegion);
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("4");
                                    }
                                    break;
                                default:
                                    strErrMsg = "选择ROI运算方式列表中不存在！";
                                    return -1;
                                    //break;
                            }
                            break;
                        case "3"://椭圆形
                            if (isInitRegion)
                            {
                                HOperatorSet.DrawEllipseMod(hv_WindowHandle, hv_ImgH / 2, hv_ImgW / 2,
                                  -0.785, 150, 100, out hv_elliRow, out hv_elliColumn, out hv_elliPhi,
                                  out hv_firstR, out hv_SecondR);
                            }
                            else
                            {
                                HOperatorSet.DrawEllipse(hv_WindowHandle, out hv_elliRow, out hv_elliColumn,
                                 out hv_elliPhi, out hv_firstR, out hv_SecondR);
                            }
                            ho_Ellipse.Dispose();
                            HOperatorSet.GenEllipse(out ho_Ellipse, hv_elliRow, hv_elliColumn, hv_elliPhi,
                                hv_firstR, hv_SecondR);
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ConcatObj(ho_allRegionTuple, ho_Ellipse, out ExpTmpOutVar_0
                                    );
                                ho_allRegionTuple.Dispose();
                                ho_allRegionTuple = ExpTmpOutVar_0;
                            }
                            HOperatorSet.CountObj(ho_allRegionTuple, out hv_ROINum);
                            HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                            switch (strSetROI)
                            {
                                case "0":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("0");
                                    }
                                    break;
                                case "1":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Difference(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("1");
                                    }
                                    break;
                                case "2":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Intersection(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("2");
                                    }
                                    break;

                                case "3":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.SymmDifference(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("3");
                                    }
                                    break;
                                case "4":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        ho_allRegion.Dispose();
                                        HOperatorSet.Complement(ho_Ellipse, out ho_allRegion);
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("4");
                                    }
                                    break;
                                default:
                                    strErrMsg = "选择ROI运算方式列表中不存在！";
                                    return -1;
                                    //break;
                            }
                            break;
                        case "4"://任意形状
                            ho_anyRegion.Dispose();
                            HOperatorSet.DrawRegion(out ho_anyRegion, hv_WindowHandle);
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ConcatObj(ho_allRegionTuple, ho_anyRegion, out ExpTmpOutVar_0
                                    );
                                ho_allRegionTuple.Dispose();
                                ho_allRegionTuple = ExpTmpOutVar_0;
                            }
                            HOperatorSet.CountObj(ho_allRegionTuple, out hv_ROINum);
                            HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                            switch (strSetROI)
                            {
                                case "0":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("0");
                                    }
                                    break;
                                case "1":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Difference(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("1");
                                    }
                                    break;
                                case "2":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Intersection(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("2");
                                    }
                                    break;
                                case "3":
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.SymmDifference(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("3");
                                    }
                                    break;
                                case "4"://补集
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        ho_allRegion.Dispose();
                                        HOperatorSet.Complement(ho_anyRegion, out ho_allRegion);
                                    }

                                    if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                                    {
                                        hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("4");
                                    }
                                    break;
                                default:
                                    strErrMsg = "选择ROI运算方式列表中不存在！";
                                    return -1;
                                    //break;
                            }
                            break;

                        default:
                            strErrMsg = "选择所画的ROI形状列表中不存在！";
                            return -1;
                            //break;
                    }
                    //显示创建的ROI
                    HOperatorSet.ClearWindow(hv_WindowHandle);
                    HOperatorSet.DispObj(ho_Image, hv_WindowHandle);
                    HOperatorSet.SetDraw(hv_WindowHandle, "margin");
                    HOperatorSet.SetLineWidth(hv_WindowHandle, 1);
                    HOperatorSet.SetColor(hv_WindowHandle, "dim gray");
                    HOperatorSet.DispObj(ho_allRegionTuple, hv_WindowHandle);
                    HOperatorSet.SetColor(hv_WindowHandle, "blue");
                    HOperatorSet.DispObj(ho_allRegion, hv_WindowHandle);
                    set_display_font(hv_WindowHandle, hv_FontSize * 0.8, "mono", "false", "false");
                    disp_message(hv_WindowHandle, "画ROI区域完成！", "window", (hv_WinH * 0.1), (hv_WinW * 0.01), "green", "false");

                    return 0;
                }
                catch (HalconException hex)
                {
                    strErrMsg = "错因：" + hex;
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strErrMsg = "错因：" + ex;
                return -1;
            }
            finally
            {
                ho_Rec1.Dispose();
                ho_Rec2.Dispose();
                ho_Circle.Dispose();
                ho_Ellipse.Dispose();
                ho_anyRegion.Dispose();
            }
        }

        /************************************************
        功能： 在窗体上(有图像)涂刷ROI(以小矩形笔画涂抹)，并窗体图片上显示涂刷后的效果---有ROI运算方式和各ROI集合记录输出
         输入参数：
         * * 注意：空区域的面积=0， 空对象Region的面积=[]也即面积<0，Region区域存在则面积>0
         * 参1 输入原始图像，参2：输入上次画的已存在ROI(第一次画为空区域，注意不是空对象)
         * 参3：输入上次已画ROI集合记录，(第一次画输入为空对象,注意不是空区域)，参4：窗体句柄
         * 参5：输入ROI运算方式的记录(第一次画输入为空数组)
         * 参6：选择ROI运算方式("并集"、"差集"、"交集"、"对称差"、"补集")
         * 参7：设置刷子笔画的大小，比如"20"只刷子大小为20个像素的小矩形
         * 返回错误信息描述 
         * 返回值： 成功返回0、失败返回-1
         最近更改日期:2019-4-8
       ************************************************/
        public int BrushROI(HObject ho_Image, ref HObject ho_allRegion, ref HObject ho_allRegionTuple,
            HTuple hv_WindowHandle, ref HTuple hv_recordROISetTuple, string strSetROI, string strBrushSize, ref string strErrMsg)
        {
            /*类中所有方法外定义变量与方法内定义的局部变量的区别:
           * (1)方法内部定义的局部变量定义时没有默认自动赋任何值,因此不能直接使用，
           * 只有方法内的其他方法可通过out参数来使用，非out修饰的参数不能使用。
           * (2)类中所有方法外定义的变量定义时即使未赋值也会默认自动赋值，自动赋null(引用类型)或其他值(值类型)
           */

            /*ref与out与params与数组(int[] a)方法参数的区别:
            * (1)ref修饰参数必须方法外先赋值(可以赋null或其他值)方法内可以重新赋新值使用并传出，或不赋值只使用。
            * (2)out修饰的参数的方法外只定义不必赋值(也可以赋值，但如果方法外赋非null的其他值则会生成垃圾内存,
            * 此时方法外需先释放变量内存,避免内存泄漏)，方法内必须先赋新值再使用最后传出，或先赋值不使用直接传出。
            * (3)params修饰方法的参数只能是数组(比如:params int[] a)：表示让方法接收任意个数同类型的参数，
             * 修饰的参数只能传入不能传出，且一个方法只允许有一个params参数且只能放到方法参数的最后一个参数
             * (比如:调用 func("nihao",1,2,3),定义 public void func(string str, params int[] a))
            */

            strErrMsg = "";
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 
            HObject ho_BrushROI = null, ho_RecBrush = null;

            // Local control variables 
            HTuple hv_ROINum = new HTuple(), hv_Area = new HTuple();
            HTuple hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_btnRow = new HTuple();
            HTuple hv_btnColumn = new HTuple(), hv_Button = new HTuple();
            HTuple hv_brushSize = new HTuple();

            // Initialize local and output iconic variables  
            HOperatorSet.GenEmptyObj(out ho_BrushROI);
            HOperatorSet.GenEmptyObj(out ho_RecBrush);


            //HTuple hv_ImgW = 1600;
            //HTuple hv_ImgH = 1200;
            HTuple hv_WinW = 400;
            HTuple hv_WinH = 300;
            HTuple hv_FontSize = 10;//基准字体大小

            try
            {
                if (!HObjectValided(ho_Image, ref strErrMsg))
                {
                    strErrMsg = "输入图像无效："+ strErrMsg;
                    return -1;
                }

                if (!HObjectValided(ho_allRegion, ref strErrMsg))
                {
                    strErrMsg = "画区域时输入的初始区域无效：" + strErrMsg;
                    return -1;
                }
                if (ho_allRegionTuple == null)
                {
                    strErrMsg = "画区域时输入的初始区域数组不能为null！";
                    return -1;
                }
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }


                try
                {
                    //HOperatorSet.GetImageSize(ho_Image, out hv_ImgW, out hv_ImgH);//求图像宽高
                    HTuple hv_Row1 = null, hv_Column1 = null;
                    HOperatorSet.GetWindowExtents(hv_WindowHandle, out hv_Row1, out hv_Column1, out hv_WinW, out hv_WinH);//求窗体控件宽高
                    hv_FontSize = hv_FontSize * (hv_WinH * 1.0 / 270.0);

                    //显示未创建前或修改前的ROI
                    HOperatorSet.SetDraw(hv_WindowHandle, "margin");
                    HOperatorSet.SetLineWidth(hv_WindowHandle, 1);
                    HOperatorSet.SetColor(hv_WindowHandle, "dim gray");
                    HOperatorSet.DispObj(ho_allRegionTuple, hv_WindowHandle);
                    HOperatorSet.SetColor(hv_WindowHandle, "blue");
                    HOperatorSet.DispObj(ho_allRegion, hv_WindowHandle);

                    set_display_font(hv_WindowHandle, hv_FontSize * 0.9, "mono", "false", "false");
                    disp_message(hv_WindowHandle, (new HTuple("鼠标左键画或修改区域，右键确认！")).TupleConcat(
                    ""), "window", (hv_WinH * 0.03), (hv_WinW * 0.01), "red", "false");

                    HOperatorSet.SetDraw(hv_WindowHandle, "fill");
                    HOperatorSet.SetColor(hv_WindowHandle, "red");

                    try
                    {
                        hv_brushSize = Convert.ToInt32(strBrushSize);//设置擦涂刷子的大小
                    }
                    catch
                    {
                        strErrMsg = "涂擦模板区域时刷子大小设置不正确！";
                        return -1;
                    }

                    //进入等待鼠标左或右键按下，输出坐标和当前鼠标状态，1表鼠标左键，4表右键(0表鼠标没按下,这里不会为0)
                    HOperatorSet.GetMbutton(hv_WindowHandle, out hv_btnRow, out hv_btnColumn, out hv_Button);
                    ho_BrushROI.Dispose();
                    HOperatorSet.GenEmptyRegion(out ho_BrushROI);
                    while ((int)((new HTuple(hv_Button.TupleEqual(1))).TupleOr(new HTuple(hv_Button.TupleEqual(
                        0)))) != 0)
                    {
                        ////系统休眠一会
                        //Thread.Sleep(1);
                        //一直在循环,需要让halcon控件也响应事件,不然到时候跳出循环,之前的事件会一起爆发触发,
                        //Application.DoEvents();
                         Task.Delay(1);
                        //获取当前鼠标位置及状态，1表鼠标左键，4表右键，0表鼠标没按0下
                        //HOperatorSet.GetMposition(hv_WindowHandle, out hv_btnRow, out hv_btnColumn, out hv_Button);
                        if (-1 == HdevGetMposition(hv_WindowHandle, out hv_btnRow, out hv_btnColumn, out hv_Button))
                        {
                            //"获取鼠标位置出错！"
                            hv_Button = 0;
                        }

                        //如果左键按下
                        if ((int)(new HTuple(hv_Button.TupleEqual(1))) != 0)
                        {
                            ho_RecBrush.Dispose();
                            HOperatorSet.GenRectangle2(out ho_RecBrush, hv_btnRow, hv_btnColumn,
                                0, hv_brushSize, hv_brushSize);
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.Union2(ho_BrushROI, ho_RecBrush, out ExpTmpOutVar_0);
                                ho_BrushROI.Dispose();
                                ho_BrushROI = ExpTmpOutVar_0;
                            }
                        }
                        HOperatorSet.DispObj(ho_BrushROI, hv_WindowHandle);
                        //如果鼠标松开，不做任何动作
                        if ((int)(new HTuple(hv_Button.TupleEqual(0))) != 0)
                        {
                            continue;
                        }
                    }
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_allRegionTuple, ho_BrushROI, out ExpTmpOutVar_0
                            );
                        ho_allRegionTuple.Dispose();
                        ho_allRegionTuple = ExpTmpOutVar_0;
                    }
                    HOperatorSet.CountObj(ho_allRegionTuple, out hv_ROINum);
                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                    switch (strSetROI)
                    {
                        case "0"://并集
                            if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                            {
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.Union2(ho_allRegion, ho_BrushROI, out ExpTmpOutVar_0);
                                    ho_allRegion.Dispose();
                                    ho_allRegion = ExpTmpOutVar_0;
                                }
                            }
                            else
                            {
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.Union2(ho_allRegion, ho_BrushROI, out ExpTmpOutVar_0);
                                    ho_allRegion.Dispose();
                                    ho_allRegion = ExpTmpOutVar_0;
                                }
                            }

                            if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                            {
                                hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("0");
                            }
                            break;
                        case "1"://差集
                            if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                            {
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.Union2(ho_allRegion, ho_BrushROI, out ExpTmpOutVar_0);
                                    ho_allRegion.Dispose();
                                    ho_allRegion = ExpTmpOutVar_0;
                                }
                            }
                            else
                            {
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.Difference(ho_allRegion, ho_BrushROI, out ExpTmpOutVar_0
                                        );
                                    ho_allRegion.Dispose();
                                    ho_allRegion = ExpTmpOutVar_0;
                                }
                            }

                            if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                            {
                                hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("1");
                            }
                            break;
                        case "2"://交集
                            if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                            {
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.Union2(ho_allRegion, ho_BrushROI, out ExpTmpOutVar_0);
                                    ho_allRegion.Dispose();
                                    ho_allRegion = ExpTmpOutVar_0;
                                }
                            }
                            else
                            {
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.Intersection(ho_allRegion, ho_BrushROI, out ExpTmpOutVar_0
                                        );
                                    ho_allRegion.Dispose();
                                    ho_allRegion = ExpTmpOutVar_0;
                                }
                            }

                            if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                            {
                                hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("2");
                            }
                            break;
                        case "3"://对称差
                            if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                            {
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.Union2(ho_allRegion, ho_BrushROI, out ExpTmpOutVar_0);
                                    ho_allRegion.Dispose();
                                    ho_allRegion = ExpTmpOutVar_0;
                                }
                            }
                            else
                            {
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.SymmDifference(ho_allRegion, ho_BrushROI, out ExpTmpOutVar_0
                                        );
                                    ho_allRegion.Dispose();
                                    ho_allRegion = ExpTmpOutVar_0;
                                }
                            }

                            if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                            {
                                hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("3");
                            }
                            break;
                        case "4"://补集
                            if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                            {
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.Union2(ho_allRegion, ho_BrushROI, out ExpTmpOutVar_0);
                                    ho_allRegion.Dispose();
                                    ho_allRegion = ExpTmpOutVar_0;
                                }
                            }
                            else
                            {
                                ho_allRegion.Dispose();
                                HOperatorSet.Complement(ho_BrushROI, out ho_allRegion);
                            }

                            if ((int)(new HTuple(hv_ROINum.TupleGreater(1))) != 0)
                            {
                                hv_recordROISetTuple = hv_recordROISetTuple.TupleConcat("4");
                            }
                            break;
                        default:
                            strErrMsg = "选择ROI运算方式列表中不存在！";
                            return -1;
                            //break;
                    }
                    //显示创建的ROI
                    HOperatorSet.ClearWindow(hv_WindowHandle);
                    HOperatorSet.DispObj(ho_Image, hv_WindowHandle);
                    HOperatorSet.SetDraw(hv_WindowHandle, "margin");
                    HOperatorSet.SetLineWidth(hv_WindowHandle, 1);
                    HOperatorSet.SetColor(hv_WindowHandle, "dim gray");
                    HOperatorSet.DispObj(ho_allRegionTuple, hv_WindowHandle);
                    HOperatorSet.SetColor(hv_WindowHandle, "blue");
                    HOperatorSet.DispObj(ho_allRegion, hv_WindowHandle);
                    set_display_font(hv_WindowHandle, hv_FontSize * 0.8, "mono", "false", "false");
                    disp_message(hv_WindowHandle, "画ROI区域完成！", "window", (hv_WinH * 0.1), (hv_WinW * 0.01), "green", "false");

                    return 0;
                }
                catch (HalconException hex)
                {
                    strErrMsg = "错因：" + hex;
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strErrMsg = "错因：" + ex;
                return -1;
            }
            finally
            {
                ho_BrushROI.Dispose();
                ho_RecBrush.Dispose();
            }
        }

        /************************************************
          功能：可以多次画画多个ROI组合一个整体ROI,并显示在窗体上(可以无图像)画的效果(用在画搜索ROI和创建模板ROI)---无ROI运算方式和各ROI集合记录输出
           输入参数：
           * 注意：空区域的面积=0， 空对象Region的面积=[]也即面积<0，Region区域存在则面积>0
           * 参1ho_allRegion：输入输出：输出为所画多ROI组合一个整体ROI，
           * 输入：初始输入为“空区域”(不能为“空对象”)，后续输入为上次所画结果区域
           * 参2：窗体句柄,
           * 参3：选择所画ROI的行形状(目前支持："水平矩形"、"旋转矩形"、"椭圆形"、"椭圆形"、"任意形状")
           * 参4：选择ROI运算方式("并集"、"差集"、"交集"、"对称差"、"补集")
           * 返回错误信息描述 
           * 返回值： 成功返回0、失败返回-1
           最近更改日期:2019-4-08
         ************************************************/
        public int drawGroupROI(ref HObject ho_allRegion, HTuple hv_WindowHandle, string strROIShape, string strSetROI, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 
            HObject ho_Rec1 = null, ho_Rec2 = null;
            HObject ho_Circle = null, ho_Ellipse = null, ho_anyRegion = null;

            // Local control variables 
            HTuple hv_Area = new HTuple(), hv_Row = new HTuple(), hv_Column = new HTuple();

            HTuple hv_rec1RowL = new HTuple(), hv_rec1ColL = new HTuple();
            HTuple hv_rec1RowR = new HTuple(), hv_rec1ColR = new HTuple();
            HTuple hv_rec2RowC = new HTuple(), hv_rec2ColC = new HTuple();
            HTuple hv_Phirec2 = new HTuple(), hv_rec2BigHalf = new HTuple();
            HTuple hv_rec2SmaHalf = new HTuple(), hv_CirRow = new HTuple();
            HTuple hv_CirCol = new HTuple(), hv_CirR = new HTuple();
            HTuple hv_elliRow = new HTuple(), hv_elliColumn = new HTuple();
            HTuple hv_elliPhi = new HTuple(), hv_firstR = new HTuple();
            HTuple hv_SecondR = new HTuple();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Rec1);
            HOperatorSet.GenEmptyObj(out ho_Rec2);
            HOperatorSet.GenEmptyObj(out ho_Circle);
            HOperatorSet.GenEmptyObj(out ho_Ellipse);
            HOperatorSet.GenEmptyObj(out ho_anyRegion);

            //HTuple hv_ImgW = 1600;
            //HTuple hv_ImgH = 1200;
            HTuple hv_WinW = 400;
            HTuple hv_WinH = 300;
            HTuple hv_FontSize = 10;//基准字体大小

            try
            {
                if (!HObjectValided(ho_allRegion, ref strErrMsg))
                {
                    strErrMsg = "画区域时输入的初始区域无效：" + strErrMsg;
                    return -1;
                }
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }

                try
                {
                    //HOperatorSet.GetImageSize(ho_Image, out hv_ImgW, out hv_ImgH);//求图像宽高
                    HTuple hv_Row1 = null, hv_Column1 = null;
                    HOperatorSet.GetWindowExtents(hv_WindowHandle, out hv_Row1, out hv_Column1, out hv_WinW, out hv_WinH);//求窗体控件宽高
                    hv_FontSize = hv_FontSize * (hv_WinH * 1.0 / 270.0);

                    //*******************画搜索区域ROI**************************

                    //显示未创建前或修改前的ROI
                    HOperatorSet.SetDraw(hv_WindowHandle, "margin");
                    HOperatorSet.SetLineWidth(hv_WindowHandle, 1);
                    HOperatorSet.SetColor(hv_WindowHandle, "yellow");
                    HOperatorSet.DispObj(ho_allRegion, hv_WindowHandle);

                    set_display_font(hv_WindowHandle, hv_FontSize * 0.9, "mono", "false", "false");
                    disp_message(hv_WindowHandle, (new HTuple("鼠标左键画或修改区域，右键确认！")).TupleConcat(
                    ""), "window", (hv_WinH * 0.03), (hv_WinW * 0.01), "red", "false");

                    switch (strROIShape)
                    {
                        case "0":
                            //水平矩形
                            HOperatorSet.DrawRectangle1(hv_WindowHandle, out hv_rec1RowL, out hv_rec1ColL,
                                out hv_rec1RowR, out hv_rec1ColR);
                            ho_Rec1.Dispose();
                            HOperatorSet.GenRectangle1(out ho_Rec1, hv_rec1RowL, hv_rec1ColL, hv_rec1RowR,
                                hv_rec1ColR);

                            switch (strSetROI)
                            {
                                case "0":

                                    //并集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }

                                    break;


                                case "1":
                                    //差集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Difference(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;
                                case "2":
                                    //交集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Intersection(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;
                                case "3":
                                    //对称差
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.SymmDifference(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;
                                case "4":
                                    //补集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec1, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        ho_allRegion.Dispose();
                                        HOperatorSet.Complement(ho_Rec1, out ho_allRegion);
                                    }
                                    break;
                                default:
                                    strErrMsg = "选择ROI运算方式列表中不存在！";
                                    return -1;
                                    //break;
                            }
                            break;
                        case "1":

                            //旋转矩形
                            HOperatorSet.DrawRectangle2(hv_WindowHandle, out hv_rec2RowC, out hv_rec2ColC,
                                out hv_Phirec2, out hv_rec2BigHalf, out hv_rec2SmaHalf);
                            ho_Rec2.Dispose();
                            HOperatorSet.GenRectangle2(out ho_Rec2, hv_rec2RowC, hv_rec2ColC, hv_Phirec2,
                            hv_rec2BigHalf, hv_rec2SmaHalf);

                            switch (strSetROI)
                            {

                                case "0":
                                    //并集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;

                                case "1":
                                    //差集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Difference(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;

                                case "2":
                                    //交集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Intersection(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;

                                case "3":
                                    //对称差
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.SymmDifference(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;

                                case "4":
                                    //补集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Rec2, out ExpTmpOutVar_0);
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        ho_allRegion.Dispose();
                                        HOperatorSet.Complement(ho_Rec2, out ho_allRegion);
                                    }
                                    break;
                                default:
                                    strErrMsg = "选择ROI运算方式列表中不存在！";
                                    return -1;
                                    //break;
                            }
                            break;
                        case "2":
                            //圆形
                            HOperatorSet.DrawCircle(hv_WindowHandle, out hv_CirRow, out hv_CirCol,
                                out hv_CirR);
                            ho_Circle.Dispose();
                            HOperatorSet.GenCircle(out ho_Circle, hv_CirRow, hv_CirCol, hv_CirR);

                            switch (strSetROI)
                            {

                                case "0":
                                    //并集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Circle, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Circle, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;

                                case "1":
                                    //差集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Circle, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Difference(ho_allRegion, ho_Circle, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;

                                case "2":
                                    //交集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Circle, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Intersection(ho_allRegion, ho_Circle, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;

                                case "3":
                                    //对称差
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Circle, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.SymmDifference(ho_allRegion, ho_Circle, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;

                                case "4":
                                    //补集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Circle, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        ho_allRegion.Dispose();
                                        HOperatorSet.Complement(ho_Circle, out ho_allRegion);
                                    }
                                    break;
                                default:
                                    strErrMsg = "选择ROI运算方式列表中不存在！";
                                    return -1;
                                    //break;

                            }
                            break;


                        case "3":
                            HOperatorSet.DrawEllipse(hv_WindowHandle, out hv_elliRow, out hv_elliColumn,
                            out hv_elliPhi, out hv_firstR, out hv_SecondR);
                            ho_Ellipse.Dispose();
                            HOperatorSet.GenEllipse(out ho_Ellipse, hv_elliRow, hv_elliColumn, hv_elliPhi,
                                hv_firstR, hv_SecondR);

                            switch (strSetROI)
                            {
                                case "0":
                                    //并集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;

                                case "1":
                                    //差集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Difference(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;

                                case "2":
                                    //交集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Intersection(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;

                                case "3":
                                    //对称差
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.SymmDifference(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;

                                case "4":
                                    //补集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_Ellipse, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        ho_allRegion.Dispose();
                                        HOperatorSet.Complement(ho_Ellipse, out ho_allRegion);
                                    }
                                    break;
                                default:
                                    strErrMsg = "选择ROI运算方式列表中不存在！";
                                    return -1;
                                    //break;
                            }
                            break;

                        case "4":
                            //任意形状
                            ho_anyRegion.Dispose();
                            HOperatorSet.DrawRegion(out ho_anyRegion, hv_WindowHandle);

                            switch (strSetROI)
                            {
                                case "0":
                                    //并集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;

                                case "1":
                                    //差集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Difference(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;

                                case "2":
                                    //交集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Intersection(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;

                                case "3":
                                    //对称差
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.SymmDifference(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    break;

                                case "4":
                                    //补集
                                    HOperatorSet.AreaCenter(ho_allRegion, out hv_Area, out hv_Row, out hv_Column);
                                    if ((int)(new HTuple(hv_Area.TupleLessEqual(0))) != 0)
                                    {
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.Union2(ho_allRegion, ho_anyRegion, out ExpTmpOutVar_0
                                                );
                                            ho_allRegion.Dispose();
                                            ho_allRegion = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        ho_allRegion.Dispose();
                                        HOperatorSet.Complement(ho_anyRegion, out ho_allRegion);
                                    }
                                    break;
                                default:
                                    strErrMsg = "选择ROI运算方式列表中不存在！";
                                    return -1;
                                    //break;
                            }
                            break;
                        default:
                            strErrMsg = "选择所画的ROI形状列表中不存在！";
                            return -1;
                            //break;
                    }

                    //显示创建的ROI
                    //HOperatorSet.ClearWindow(hv_WindowHandle);
                    HOperatorSet.SetDraw(hv_WindowHandle, "margin");
                    HOperatorSet.SetLineWidth(hv_WindowHandle, 1);
                    HOperatorSet.SetColor(hv_WindowHandle, "yellow");
                    HOperatorSet.DispObj(ho_allRegion, hv_WindowHandle);
                    set_display_font(hv_WindowHandle, hv_FontSize * 0.8, "mono", "false", "false");
                    disp_message(hv_WindowHandle, "画ROI区域完成！", "window", (hv_WinH * 0.1), (hv_WinW * 0.01), "green", "false");

                    return 0;
                }
                catch (HalconException hex)
                {
                    strErrMsg = "错因：" + hex;
                    return -1;
                    //throw ex;
                }
            }
            catch (Exception ex)
            {
                strErrMsg = "错因：" + ex;
                return -1;
                //throw ex;
            }
            finally
            {
                ho_Rec1.Dispose();
                ho_Rec2.Dispose();
                ho_Circle.Dispose();
                ho_Ellipse.Dispose();
                ho_anyRegion.Dispose();
            }
        }


        /***************画直线******************/
        public int DrawLine(HTuple hv_WindowHandle, ref HTuple hv_Row1, ref HTuple hv_Column1, ref HTuple hv_Row2, ref HTuple hv_Column2, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            HObject ho_RegionLines;
            HOperatorSet.GenEmptyObj(out ho_RegionLines);

            HTuple hv_WinW = 400;
            HTuple hv_WinH = 300;
            HTuple hv_FontSize = 10;//基准字体大小

            try
            {
                //if (!HObjectValided(ho_allRegion, ref strErrMsg))
                //{
                //    strErrMsg = "画区域时输入的初始区域无效：" + strErrMsg;
                //    return -1;
                //}
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }

                try
                {
                    //HOperatorSet.GetImageSize(ho_Image, out hv_ImgW, out hv_ImgH);//求图像宽高
                    HTuple hv_Row = null, hv_Column = null;
                    HOperatorSet.GetWindowExtents(hv_WindowHandle, out hv_Row, out hv_Column, out hv_WinW, out hv_WinH);//求窗体控件宽高
                    hv_FontSize = hv_FontSize * (hv_WinH * 1.0 / 270.0);

                    //*******************画区域**************************

                    //显示未创建前或修改前的ROI
                    //HOperatorSet.SetDraw(hv_WindowHandle, "margin");
                    //HOperatorSet.SetLineWidth(hv_WindowHandle, 1);
                    //HOperatorSet.SetColor(hv_WindowHandle, "red");
                    //HOperatorSet.DispObj(ho_RegionLines, hv_WindowHandle);

                    set_display_font(hv_WindowHandle, hv_FontSize * 0.9, "mono", "false", "false");
                    disp_message(hv_WindowHandle, (new HTuple("鼠标左键画直线，右键确认！")).TupleConcat(
                    ""), "window", (hv_WinH * 0.03), (hv_WinW * 0.01), "red", "false");

                    HOperatorSet.DrawLine(hv_WindowHandle, out hv_Row1, out hv_Column1, out hv_Row2, out hv_Column2);
                    ho_RegionLines.Dispose();
                    HOperatorSet.GenRegionLine(out ho_RegionLines, hv_Row1, hv_Column1, hv_Row2,
                     hv_Column2);

                    //显示创建的ROI
                    //HOperatorSet.ClearWindow(hv_WindowHandle);
                    //HOperatorSet.SetDraw(hv_WindowHandle, "margin");
                    HOperatorSet.SetLineWidth(hv_WindowHandle, 1);
                    HOperatorSet.SetColor(hv_WindowHandle, "blue");
                    HOperatorSet.DispObj(ho_RegionLines, hv_WindowHandle);
                    set_display_font(hv_WindowHandle, hv_FontSize * 0.8, "mono", "false", "false");
                    disp_message(hv_WindowHandle, "画ROI直线完成！", "window", (hv_WinH * 0.1), (hv_WinW * 0.01), "green", "false");

                    return 0;
                }
                catch (HalconException hex)
                {
                    strErrMsg = "错因：" + hex;
                    return -1;
                    //throw ex;
                }
            }
            catch (Exception ex)
            {
                strErrMsg = "错因：" + ex;
                return -1;
                //throw ex;
            }
            finally
            {
                ho_RegionLines.Dispose();

            }


        }


        /*********创建模板、搜寻模板、删除模板、显示模板相关**************/

        /************************************************
           功能：从图像创建模板并不显示创建效果
            输入参数：
            * 参1 输入ho_Image
            * 参2：ho_allRegion图像上感兴趣区域，当区域为空对象或空区域时，感兴趣区域为整幅图像
            * 注意：空区域的面积=0， 空对象Region的面积=[]也即面积<0，Region区域存在则面积>0
            * 参3：窗体句柄,参4：匹配算法("不缩放"、"同步缩放"、"异步缩放")
            * 参5~18：创建模板输入参数
            * 参19、20、21：输出生成的模板ID，和模板中心坐标pix
            * 增加返回错误消息字符串
            * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-4-4
          ************************************************/

        HObject ho_Edges = null;//定义亚相素轮廓参数

        public int imageCreateModel(HObject ho_Image, HObject ho_allRegion, HTuple hv_WindowHandle, string matchType,
             HTuple hv_numLevels, HTuple hv_angleStart, HTuple hv_angleExtent, HTuple hv_angleStep,
             HTuple hv_scaleRMin, HTuple hv_scaleRMax, HTuple hv_scaleRStep, HTuple hv_scaleCMin, HTuple hv_scaleCMax, HTuple hv_scaleCStep,
             HTuple hv_optimization, HTuple hv_metric, HTuple hv_contrast, HTuple hv_minContrast,
             ref HTuple hv_modelRow, ref HTuple hv_modelCol, ref HTuple hv_modelID, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 
            HObject ho_ROIImage = null;

            // Local control variables 
            HTuple hv_allRegionArea = new HTuple();
            HTuple hv_allRegionRow = new HTuple();
            HTuple hv_allRegionCol = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ROIImage);

            try
            {
                if (!HObjectValided(ho_Image, ref strErrMsg))
                {
                    strErrMsg = "输入图像无效："+ strErrMsg;
                    return -1;
                }
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }

                try
                {
                    //**从图像创建模板

                    if (!HObjectValided(ho_allRegion, ref strErrMsg))
                    {
                        strErrMsg = "输入模板区域无效：" + strErrMsg;
                        return -1;
                    }
                    else //如果不为空对象，
                    {
                        HOperatorSet.AreaCenter(ho_allRegion, out hv_allRegionArea, out hv_allRegionRow, out hv_allRegionCol);
                        if ((int)(new HTuple(hv_allRegionArea.TupleGreater(0))) != 0)//面积>0，则区域不为空区域
                        {
                            ho_ROIImage.Dispose();
                            HOperatorSet.ReduceDomain(ho_Image, ho_allRegion, out ho_ROIImage);
                            HOperatorSet.EdgesSubPix(ho_ROIImage, out ho_Edges, "canny", 2, 20, 40);
                        }
                        else//如果为空区域
                        {
                            strErrMsg = "输入模板区域为空区域！";
                            return -1;
                        }
                    }

                    switch (matchType)
                    {
                        case "0":
                            //以该图像做（可以旋转、平移）模板。
                            //参1：模板图像；参2：最大金字塔等级，0表默认，默认'auto';参3：模板旋转起始角(弧度)
                            //参4：模板旋转角度范围；参5：角度步长，默认auto；
                            //参6：模板优化类型和创建方法，默认auto;参7：匹配标准(极性设置)；参8：模板图像的模板边缘对比度阈值或滞后阈值，
                            //以及可选的最小组件尺寸(边缘轮廓的最小长度)。默认值：auto; 参数9：搜索图像的边缘最小对比度(小于前者)，默认auto。参10：模板句柄
                            //HOperatorSet.CreateShapeModel(ho_ROIImage, "auto", (new HTuple(0)).TupleRad()
                            //    , (new HTuple(360)).TupleRad(), "auto", "auto", "use_polarity", "auto",
                            //    "auto", out hv_modelID);
                            if (bModeType)//判断是用基于形状创建模板还是XLD创建模板
                            {
                                HOperatorSet.CreateShapeModelXld(ho_Edges,
                                 hv_numLevels, hv_angleStart, hv_angleExtent, hv_angleStep,
                                 hv_optimization, "ignore_local_polarity", 10, out hv_modelID);
                            }
                            else
                            {
                                HOperatorSet.CreateShapeModel(ho_ROIImage,
                               hv_numLevels, hv_angleStart, hv_angleExtent, hv_angleStep,
                               hv_optimization, hv_metric, hv_contrast, hv_minContrast, out hv_modelID);

                            }


                            break;
                        case "1":
                            //以该图像做（各向同步的：可以旋转、平移、xy方向同步放缩）模板。
                            //参1：模板图像；参2：最大金字塔等级，0表默认，默认'auto';参3：模板旋转起始角(弧度)
                            //参4：模板旋转角度范围；参5：角度步长，默认auto；参6:最小缩放系数，默认0.9；参7：最大缩放系数，默认1.1；参8：缩放步长，默认auto
                            //参9：模板优化类型和创建方法，默认auto;参10：匹配标准(极性设置)；参11：模板图像的模板边缘对比度阈值或滞后阈值，
                            //以及可选的最小组件尺寸(边缘轮廓的最小长度)。默认值：auto; 参数12：搜索图像的边缘最小对比度(小于前者)，默认auto。参13：模板句柄
                            //HOperatorSet.CreateScaledShapeModel(ho_ROIImage, "auto", (new HTuple(0)).TupleRad()
                            //    , (new HTuple(360)).TupleRad(), "auto", 0.9, 1.1, "auto", "auto",
                            //    "use_polarity", "auto", "auto", out hv_modelID);
                            HOperatorSet.CreateScaledShapeModel(ho_ROIImage,
                                hv_numLevels, hv_angleStart, hv_angleExtent, hv_angleStep,
                                hv_scaleRMin, hv_scaleRMax, hv_scaleRStep,
                                hv_optimization, hv_metric, hv_contrast, hv_minContrast, out hv_modelID);

                            break;
                        case "2":
                            //以该图像做（各向异性的：可以旋转、平移、xy方向异步放缩）模板。
                            //参1：模板图像；参2：最大金字塔等级，0表默认，默认'auto';参3：模板旋转起始角(弧度)
                            //参4：模板旋转角度范围；参5：角度步长，默认auto；参6:行方向最小缩放系数，默认0.9；参7：行方向最大缩放系数，默认1.1；参8：行方向缩放步长，默认auto
                            //参9:列方向最小缩放系数，默认0.9；参10：列方向最大缩放系数，默认1.1；参11：列方向缩放步长，默认auto
                            //参12：模板优化类型和创建方法，默认auto;参13：匹配标准(极性设置)；参14：模板图像的模板边缘对比度阈值或滞后阈值，
                            //以及可选的最小组件尺寸(边缘轮廓的最小长度)。默认值：auto; 参数15：搜索图像的边缘最小对比度(小于前者)，默认auto。参16：模板句柄
                            //HOperatorSet.CreateAnisoShapeModel(ho_ROIImage, "auto", (new HTuple(0)).TupleRad()
                            //    , (new HTuple(360)).TupleRad(), "auto", 0.9, 1.1, "auto", 0.9, 1.1,
                            //    "auto", "auto", "use_polarity", "auto", "auto", out hv_modelID);
                            HOperatorSet.CreateAnisoShapeModel(ho_ROIImage,
                                hv_numLevels, hv_angleStart, hv_angleExtent, hv_angleStep,
                                hv_scaleRMin, hv_scaleRMax, hv_scaleRStep, hv_scaleCMin, hv_scaleCMax, hv_scaleCStep,
                                hv_optimization, hv_metric, hv_contrast, hv_minContrast, out hv_modelID);

                            break;

                        default:
                            strErrMsg = "选择的匹配算法列表中不存在！";
                            return -1;
                            //break;
                    }
                    //走到这里则，模板区域不为空

                    //HOperatorSet.GetShapeModelOrigin(hv_modelID, out hv_modelRow, out hv_modelCol);//获得模板原点坐标

                    hv_modelRow = hv_allRegionRow;
                    hv_modelCol = hv_allRegionCol;
                    if ((int)(new HTuple((new HTuple(hv_modelID.TupleLength())).TupleLess(1))) != 0)//模板数据为空
                    {
                        strErrMsg = "模板句柄数据为空！";
                        return -1;
                    }

                }
                catch (HalconException herr)
                {
                    strErrMsg = "错因：" + herr;
                    return -1;
                }
            }
            catch (Exception err)
            {
                strErrMsg = "错因：" + err;
                return -1;
            }
            finally
            {
                ho_ROIImage.Dispose();
            }
            return 0;
        }

        /************************************************
         功能：从Xlds(单个多多个xld对象)创建模板,并不显示创建效果
          输入参数：
          * 参1 输入ho_Xlds
          * 参2：窗体句柄
          * 参3：匹配算法("不缩放"、"同步缩放"、"异步缩放")
          * 参4~16：创建模板输入参数
          * 参17：输出生成的模板ID
          * 返回值： 成功返回0、失败返回-1
          最近更改日期:2018-12-06
        ************************************************/
        public int xldsCreateModel(HObject ho_Xlds, HTuple hv_WindowHandle, string matchType,
             HTuple hv_numLevels, HTuple hv_angleStart, HTuple hv_angleExtent, HTuple hv_angleStep,
             HTuple hv_scaleRMin, HTuple hv_scaleRMax, HTuple hv_scaleRStep, HTuple hv_scaleCMin, HTuple hv_scaleCMax, HTuple hv_scaleCStep,
             HTuple hv_optimization, HTuple hv_metric, HTuple hv_minContrast, ref HTuple hv_modelID, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            // Local control variables 

            // Initialize local and output iconic variables 

            try
            {
                if (!HObjectValided(ho_Xlds, ref strErrMsg))
                {
                    strErrMsg = "输入ho_Xld无效：" + strErrMsg;
                    return -1;
                }
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }

                try
                {
                    //**从xlds创建模板
                    switch (matchType)
                    {
                        case "0":
                            //*****(1)不缩放
                            //以该Xlds做（不缩放的：可以旋转、平移）模板。
                            //参1：模板Xlds数组；参2：最大金字塔等级，0表默认，默认'auto';参3：模板旋转起始角(弧度)
                            //参4：模板旋转角度范围；参5：角度步长，默认auto；
                            //参6：模板优化类型和创建方法，默认auto;参7：匹配标准(极性设置)；
                            //参数8：搜索图像的边缘最小对比度，默认5。参9：模板句柄
                            //HOperatorSet.CreateShapeModelXld(ho_Xlds, "auto", (new HTuple(-22.5)).TupleRad()
                            //    , (new HTuple(45)).TupleRad(), "auto", "auto", "ignore_local_polarity",
                            //    5, out hv_modelID);

                            HOperatorSet.CreateShapeModelXld(ho_Xlds,
                                hv_numLevels, hv_angleStart, hv_angleExtent, hv_angleStep,
                                hv_optimization, hv_metric, hv_minContrast, out hv_modelID);
                            break;
                        case "1":
                            //*****(2)同步缩放
                            //同步缩放：可以旋转、平移、放缩
                            //参1：模板Xlds；参2：最大金字塔等级，0表默认，默认'auto';参3：模板旋转起始角(弧度)
                            //参4：模板旋转角度范围；参5：角度步长，默认auto；参6:最小缩放系数，默认0.9；参7：最大缩放系数，默认1.1；参8：缩放步长，默认auto
                            //参9：模板优化类型和创建方法，默认auto;参10：匹配标准(极性设置)；
                            //参数11：搜索图像的边缘最小对比度，默认5。参13：模板句柄
                            //HOperatorSet.CreateScaledShapeModelXld(ho_Xlds, "auto", (new HTuple(-22.5)).TupleRad()
                            //    , (new HTuple(45)).TupleRad(), "auto", 0.9, 1.1, "auto", "auto", "ignore_local_polarity",
                            //    5, out hv_modelID);

                            HOperatorSet.CreateScaledShapeModelXld(ho_Xlds,
                                hv_numLevels, hv_angleStart, hv_angleExtent, hv_angleStep,
                                hv_scaleRMin, hv_scaleRMax, hv_scaleRStep,
                                hv_optimization, hv_metric, hv_minContrast, out hv_modelID);
                            break;
                        case "2":
                            //*****(3)异步缩放
                            //各向异性的：可以旋转、平移、放缩、xy方向异步放缩。
                            //参1：模板Xlds；参2：最大金字塔等级，0表默认，默认'auto';参3：模板旋转起始角(弧度)
                            //参4：模板旋转角度范围；参5：角度步长，默认auto；参6:行方向最小缩放系数，默认0.9；参7：行方向最大缩放系数，默认1.1；参8：行方向缩放步长，默认auto
                            //参9:列方向最小缩放系数，默认0.9；参10：列方向最大缩放系数，默认1.1；参11：列方向缩放步长，默认auto
                            //参12：模板优化类型和创建方法，默认auto;参13：匹配标准(极性设置)；
                            //参数14：搜索图像的边缘最小对比度，默认5。参15：模板句柄
                            //HOperatorSet.CreateAnisoShapeModelXld(ho_Xlds, "auto", (new HTuple(-22.5)).TupleRad()
                            //    , (new HTuple(45)).TupleRad(), "auto", 0.9, 1.1, "auto", 0.9, 1.1, "auto",
                            //    "auto", "ignore_local_polarity", 5, out hv_modelID);

                            HOperatorSet.CreateAnisoShapeModelXld(ho_Xlds,
                                hv_numLevels, hv_angleStart, hv_angleExtent, hv_angleStep,
                                hv_scaleRMin, hv_scaleRMax, hv_scaleRStep, hv_scaleCMin, hv_scaleCMax, hv_scaleCStep,
                                hv_optimization, hv_metric, hv_minContrast, out hv_modelID);
                            break;

                        default:
                            strErrMsg = "选择的匹配算法列表中不存在！";
                            return -1;
                            //break;

                    }

                    if ((int)(new HTuple((new HTuple(hv_modelID.TupleLength())).TupleLess(1))) != 0)//模板数据为空
                    {
                        strErrMsg = "模板句柄数据为空！";
                        return -1;
                    }
                }
                catch (HalconException herr)
                {
                    strErrMsg = "错因：" + herr;
                    return -1;
                }
            }
            catch (Exception err)
            {
                strErrMsg = "错因：" + err;
                return -1;
            }
            finally
            {
            }
            return 0;
        }



        /*********************坐标排序算法************************/
        public void CoordinateOrdering(HTuple Row, HTuple Col, HTuple Angle, HTuple Score, double rowDistanceImage, double colDistanceImage, out HTuple hv_findRow, out HTuple hv_findCol, out HTuple hv_findAngle, out HTuple hv_findScore)
        {
            HTuple hv_SortedRow = null, hv_SortedCol = null, ColSelect = null, RowSelect = null, AngleSelect = null, ScoreSelect = null;

            HOperatorSet.TupleSort(Row, out hv_SortedRow);
            HOperatorSet.TupleSort(Col, out hv_SortedCol);

            double Rnumber = (hv_SortedRow[hv_SortedRow.Length - 1] - hv_SortedRow[0]) / rowDistanceImage;
            double Cnumber = (hv_SortedCol[hv_SortedCol.Length - 1] - hv_SortedCol[0]) / colDistanceImage;

            List<double> R = new List<double>();
            List<double> C = new List<double>();
            List<double> A = new List<double>();
            List<double> S = new List<double>();

            for (int i = 1; i < Rnumber + 1; i++)
            {

                for (int j = 1; j < Cnumber + 1; j++)
                {
                    for (int k = 0; k < Row.Length; k++)
                    {
                        if (Row[k] <= hv_SortedRow[0] + i * rowDistanceImage && Col[k] <= hv_SortedCol[0] + j * colDistanceImage && Col[k] != 0.0 && Row[k] != 0.0)
                        {
                            HOperatorSet.TupleSelect(Col, k, out ColSelect);
                            C.Add(ColSelect);
                            HOperatorSet.TupleSelect(Row, k, out RowSelect);
                            R.Add(RowSelect);
                            HOperatorSet.TupleSelect(Angle, k, out AngleSelect);
                            A.Add(AngleSelect);
                            HOperatorSet.TupleSelect(Score, k, out ScoreSelect);
                            S.Add(ScoreSelect);

                            HOperatorSet.TupleReplace(Col, k, 0.0, out Col);
                            HOperatorSet.TupleReplace(Row, k, 0.0, out Row);
                            //Col[k] = 0;
                            //Row[k] = 0;
                        }
                    }

                }

            }
            hv_findRow = R.ToArray();
            hv_findCol = C.ToArray();
            hv_findAngle = A.ToArray();
            hv_findScore = S.ToArray();

        }


        /************************************************
         功能：搜寻单种类模板，不显示找到模板的匹配效果
          输入参数：
          * 参1：ho_findImage输入搜寻图像
          * 参2：ho_allRegionFind图像上感兴趣区域，当搜寻区域为null或空对象或空区域时，感兴趣区域为整幅图像
          * 参3：窗体句柄,参4：模板ID,参5：匹配算法("不缩放"、"同步缩放"、"异步缩放")
          * 参6~17：输入搜寻模板参数
          * 参18~23：输出生成的匹配结果
          * 参24：返回错误信息 
          * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-3-25
        ************************************************/
        public int findShapeModel(HObject ho_findImage, HObject ho_allRegionFind, HTuple hv_WindowHandle, HTuple hv_ModelID, string matchType,
            HTuple hv_angleStart, HTuple hv_angleExtent, HTuple hv_scaleRMin, HTuple hv_scaleRMax, HTuple hv_scaleCMin, HTuple hv_scaleCMax,
            HTuple hv_minScore, HTuple hv_numMatches, HTuple hv_maxOverlap, HTuple hv_subPixel, HTuple hv_numLevels, HTuple hv_greediness,
             double rowDistanceImage, double colDistanceImage, ref HTuple hv_findRow, ref HTuple hv_findCol, ref HTuple hv_findAngle,
            ref HTuple hv_findScaleR, ref HTuple hv_findScaleC, ref HTuple hv_findScore, ref string strErrMsg)
        {

            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 
            HObject ho_Image = null;
            // Local control variables 
            HTuple hv_findRegionArea = new HTuple();
            HTuple hv_findRegionRow = new HTuple();
            HTuple hv_findRegionCol = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);
            strErrMsg = "";//复位
            HTuple hv_findRowOld = new HTuple(), hv_findColOld = new HTuple(), hv_findAngleOld = new HTuple(), hv_findScoreOld = new HTuple();

            try
            {
                if (!HObjectValided(ho_findImage, ref strErrMsg))
                {
                    strErrMsg = "输入找模板图像无效：" + strErrMsg;
                    return -1;
                }
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }

                if (!HTupleValided(hv_ModelID, ref strErrMsg))
                {
                    strErrMsg = "输入模板ID数据无效：" + strErrMsg;
                    return -1;
                }

                try
                {
                    string strErr = "";
                    if (!HObjectValided(ho_allRegionFind, ref strErr))//如果搜寻区域为"null"或“空对象”
                    {
                        ho_Image.Dispose();
                        ho_Image = ho_findImage.CopyObj(1, -1);
                    }
                    else
                    {
                        HOperatorSet.AreaCenter(ho_allRegionFind, out hv_findRegionArea, out hv_findRegionRow, out hv_findRegionCol);
                        if ((int)(new HTuple(hv_findRegionArea.TupleGreater(0))) != 0)//面积>0，则有限制搜索区域
                        {
                            ho_Image.Dispose();
                            HOperatorSet.ReduceDomain(ho_findImage, ho_allRegionFind, out ho_Image);
                        }
                        else//如果区域为“空区域”
                        {
                            ho_Image.Dispose();
                            ho_Image = ho_findImage.CopyObj(1, -1);
                        }
                    }



                    switch (matchType)
                    {
                        case "0":
                            //初步尝试找到模板:
                            //参3：模板旋转起始角；参4：旋转范围；
                            //参5：最小匹配分数；参6：最多匹配数量；参7：最大重叠度；
                            //参8：是否使用亚像素精度；参9：匹配使用的金字塔级别；参10：搜索贪婪度(0~1:慢到快精度高到低)
                            //参11、12、13：匹配到的目标坐标和旋转角度；参14:匹配分值
                            //hv_SubPixel = "least_squares";
                            //HOperatorSet.FindShapeModel(ho_Image, hv_ModelXldID, (new HTuple(-22.5)).TupleRad()
                            //    , (new HTuple(45)).TupleRad(), 0.5, 1, 0.5, hv_SubPixel, 0, 0.9, out hv_tryFindRow,
                            //    out hv_tryFindCol, out hv_tryFindAngle, out hv_tryFindScore);

                            HOperatorSet.FindShapeModel(ho_Image, hv_ModelID,
                           hv_angleStart, hv_angleExtent,
                           hv_minScore, hv_numMatches, hv_maxOverlap, hv_subPixel, hv_numLevels, hv_greediness,
                           out hv_findRowOld, out hv_findColOld, out hv_findAngleOld, out hv_findScoreOld);

                            if ((int)(new HTuple((new HTuple(hv_findScoreOld.TupleLength())).TupleLess(1))) != 0)
                            {
                                //如果失败
                                hv_findScaleC = new HTuple();
                                hv_findScaleR = new HTuple();
                            }
                            else
                            {
                                for (int i = 0; i < hv_findScoreOld.TupleLength(); i++)
                                {
                                    hv_findScaleC[i] = 1.0;
                                    hv_findScaleR[i] = 1.0;
                                }
                            }
                            if ((int)hv_findScoreOld.TupleLength() <= 1)
                            {
                                hv_findRow = hv_findRowOld;
                                hv_findCol = hv_findColOld;
                                hv_findAngle = hv_findAngleOld;
                                hv_findScore = hv_findScoreOld;
                            }
                            else
                            {
                                CoordinateOrdering(hv_findRowOld, hv_findColOld, hv_findAngleOld, hv_findScoreOld, rowDistanceImage, colDistanceImage,
                                   out hv_findRow, out hv_findCol, out hv_findAngle, out hv_findScore);

                            }


                            break;

                        case "1":
                            //初步尝试找到模板:
                            //参3：模板旋转起始角；参4：旋转范围；参5：最小放缩系数；参6:最大放缩系数；
                            //参7：最小匹配分数；参8：最多匹配数量；参9：最大重叠度；
                            //参10：是否使用亚像素精度；参11：匹配使用的金字塔级别；参12：搜索贪婪度(0~1:慢到快精度高到低)
                            //参13、14、15：匹配到的目标坐标和旋转角度；参16：放缩系数参;17:匹配分值

                            //hv_SubPixel = "least_squares";
                            //HOperatorSet.FindScaledShapeModel(ho_Image, hv_ModelXldID, (new HTuple(-22.5)).TupleRad()
                            //    , (new HTuple(45)).TupleRad(), 0.9, 1.1, 0.5, 1, 0.5, hv_SubPixel, 0,
                            //    0.9, out hv_tryFindRow, out hv_tryFindCol, out hv_tryFindAngle, out hv_tryFindScale,
                            //    out hv_tryFindScore);
                            HOperatorSet.FindScaledShapeModel(ho_Image, hv_ModelID,
                               hv_angleStart, hv_angleExtent, hv_scaleRMin, hv_scaleRMax,
                               hv_minScore, hv_numMatches, hv_maxOverlap, hv_subPixel, hv_numLevels, hv_greediness,
                               out hv_findRow, out hv_findCol, out hv_findAngle, out hv_findScaleR, out hv_findScore);

                            hv_findScaleC = hv_findScaleR;

                            break;

                        case "2":

                            //初步尝试找到模板:
                            //参3：模板旋转起始角；参4：旋转范围；
                            //参5：最小匹配分数；参6：最多匹配数量；参7：最大重叠度；
                            //参8：是否使用亚像素精度；参9：匹配使用的金字塔级别；参10：搜索贪婪度(0~1:慢到快精度高到低)
                            //参11、12、13：匹配到的目标坐标和旋转角度；参14:匹配分值

                            //hv_SubPixel = "least_squares";
                            //HOperatorSet.FindAnisoShapeModel(ho_Image, hv_ModelID, (new HTuple(-22.5)).TupleRad()
                            //    , (new HTuple(45)).TupleRad(), 0.9, 1.1, 0.9, 1.1, 0.5, 1, 0.5, hv_SubPixel,
                            //    0, 0.9, out hv_tryFindRow, out hv_tryFindCol, out hv_tryFindAngle, out hv_tryFindScaleR,
                            //    out hv_tryFindScaleC, out hv_tryFindScore);

                            HOperatorSet.FindAnisoShapeModel(ho_Image, hv_ModelID,
                              hv_angleStart, hv_angleExtent, hv_scaleRMin, hv_scaleRMax, hv_scaleCMin, hv_scaleCMax,
                              hv_minScore, hv_numMatches, hv_maxOverlap, hv_subPixel, hv_numLevels, hv_greediness,
                              out hv_findRow, out hv_findCol, out hv_findAngle, out hv_findScaleR, out hv_findScaleC, out hv_findScore);
                            break;
                        default:
                            strErrMsg = "选择的匹配算法列表中不存在！";
                            return -1;
                            //break;
                    }

                    if ((int)(new HTuple((new HTuple(hv_findScoreOld.TupleLength())).TupleLess(1))) != 0)
                    {
                        strErrMsg = "模板匹配结果为空！";
                        return -1;
                    }
                }
                catch (HalconException herr)
                {
                    //"搜寻模板时出错！";
                    strErrMsg = "错因：" + herr;
                    return -1;
                }
            }
            catch (Exception err)
            {
                //"搜寻模板时出错！";
                strErrMsg = "错因：" + err;
                return -1;
            }
            finally
            {
                ho_Image.Dispose();

            }
            return 0;
        }

        /************************************************
          功能：搜寻多种类模板，不显示找到模板的匹配效果
           输入参数：
           * 参1：ho_findImage输入搜寻图像
           * 参2：ho_allRegionFind图像上感兴趣区域，当搜寻区域为null或空对象或空区域时，感兴趣区域为整幅图像
           * 参3：窗体句柄,参4：模板ID数组,参5：匹配算法("不缩放"、"同步缩放"、"异步缩放")
           * 参6~17：输入搜寻模板参数数组
           * 参18~23：输出生成的匹配结果数组
           * 参24：输出模板索引数组
           * 参25：返回错误信息 
           * 返回值： 成功返回0、失败返回-1
           最近更改日期:2019-3-25
         ************************************************/
        public int findShapeModels(HObject ho_findImage, HObject ho_allRegionFind, HTuple hv_WindowHandle, HTuple hv_ModelIDs, string matchType,
          HTuple hv_angleStarts, HTuple hv_angleExtents, HTuple hv_scaleRMins, HTuple hv_scaleRMaxs, HTuple hv_scaleCMins, HTuple hv_scaleCMaxs,
          HTuple hv_minScores, HTuple hv_numMatches, HTuple hv_maxOverlaps, HTuple hv_subPixels, HTuple hv_numLevels, HTuple hv_greediness,
          ref HTuple hv_findRow, ref HTuple hv_findCol, ref HTuple hv_findAngle,
            ref HTuple hv_findScaleR, ref HTuple hv_findScaleC, ref HTuple hv_findScore, ref HTuple hv_modelIndex, ref string strErrMsg)
        {

            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 
            HObject ho_Image = null;
            // Local control variables 
            HTuple hv_findRegionArea = new HTuple();
            HTuple hv_findRegionRow = new HTuple();
            HTuple hv_findRegionCol = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image);

            strErrMsg = "";//复位

            try
            {
                if (!HObjectValided(ho_findImage, ref strErrMsg))
                {
                    strErrMsg = "输入找模板图像无效：" + strErrMsg;
                    return -1;
                }
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }

                if (!HTupleValided(hv_ModelIDs, ref strErrMsg))
                {
                    strErrMsg = "输入模板ID数据无效：" + strErrMsg;
                    return -1;
                }


                try
                {
                    string strErr = "";
                    if (!HObjectValided(ho_allRegionFind, ref strErr))//如果搜寻区域为"null"或“空对象”
                    {
                        ho_Image.Dispose();
                        ho_Image = ho_findImage.CopyObj(1, -1);
                    }
                    else
                    {
                        HOperatorSet.AreaCenter(ho_allRegionFind, out hv_findRegionArea, out hv_findRegionRow, out hv_findRegionCol);
                        if ((int)(new HTuple(hv_findRegionArea.TupleGreater(0))) != 0)//面积>0，则有限制搜索区域
                        {
                            ho_Image.Dispose();
                            HOperatorSet.ReduceDomain(ho_findImage, ho_allRegionFind, out ho_Image);
                        }
                        else  //面积=0:表空区域 面积=[]也即面积<0表空对象
                        {
                            ho_Image.Dispose();
                            ho_Image = ho_findImage.CopyObj(1, -1);
                        }
                    }


                    switch (matchType)
                    {
                        case "0":
                            //初步尝试找到模板:
                            //参3：模板旋转起始角；参4：旋转范围；
                            //参5：最小匹配分数；参6：最多匹配数量；参7：最大重叠度；
                            //参8：是否使用亚像素精度；参9：匹配使用的金字塔级别；参10：搜索贪婪度(0~1:慢到快精度高到低)
                            //参11、12、13：匹配到的目标坐标和旋转角度；参14:匹配分值,参数15：搜寻到的模板索引
                            //hv_SubPixel = "least_squares";
                            //HOperatorSet.FindShapeModels(ho_Image, hv_ModelXldID, (new HTuple(-22.5)).TupleRad()
                            //    , (new HTuple(45)).TupleRad(), 0.5, 1, 0.5, hv_SubPixel, 0, 0.9, out hv_tryFindRow,
                            //    out hv_tryFindCol, out hv_tryFindAngle, out hv_tryFindScore,out hv_modelIndex);

                            HOperatorSet.FindShapeModels(ho_Image, hv_ModelIDs,
                             hv_angleStarts, hv_angleExtents,
                             hv_minScores, hv_numMatches, hv_maxOverlaps, hv_subPixels, hv_numLevels, hv_greediness,
                             out hv_findRow, out hv_findCol, out hv_findAngle, out hv_findScore, out hv_modelIndex);
                            if ((int)(new HTuple((new HTuple(hv_findScore.TupleLength())).TupleLess(1))) != 0)
                            {
                                //如果失败
                                hv_findScaleC = new HTuple();
                                hv_findScaleR = new HTuple();
                            }
                            else
                            {
                                for (int i = 0; i < hv_findScore.TupleLength(); i++)
                                {
                                    hv_findScaleC[i] = 1.0;
                                    hv_findScaleR[i] = 1.0;
                                }
                            }

                            break;

                        case "1":
                            //初步尝试找到模板:
                            //参3：模板旋转起始角；参4：旋转范围；参5：最小放缩系数；参6:最大放缩系数；
                            //参7：最小匹配分数；参8：最多匹配数量；参9：最大重叠度；
                            //参10：是否使用亚像素精度；参11：匹配使用的金字塔级别；参12：搜索贪婪度(0~1:慢到快精度高到低)
                            //参13、14、15：匹配到的目标坐标和旋转角度；参16：放缩系数参;17:匹配分值,18:搜寻到的模板索引

                            //hv_SubPixel = "least_squares";
                            //HOperatorSet.FindScaledShapeModels(ho_Image, hv_ModelXldID, (new HTuple(-22.5)).TupleRad()
                            //    , (new HTuple(45)).TupleRad(), 0.9, 1.1, 0.5, 1, 0.5, hv_SubPixel, 0,
                            //    0.9, out hv_tryFindRow, out hv_tryFindCol, out hv_tryFindAngle, out hv_tryFindScale,
                            //    out hv_tryFindScore,out hv_modelIndex);

                            HOperatorSet.FindScaledShapeModels(ho_Image, hv_ModelIDs,
                               hv_angleStarts, hv_angleExtents, hv_scaleRMins, hv_scaleRMaxs,
                               hv_minScores, hv_numMatches, hv_maxOverlaps, hv_subPixels, hv_numLevels, hv_greediness,
                               out hv_findRow, out hv_findCol, out hv_findAngle, out hv_findScaleR, out hv_findScore, out hv_modelIndex);

                            hv_findScaleC = hv_findScaleR;

                            break;

                        case "2":

                            //hv_SubPixel = "least_squares";
                            //HOperatorSet.FindAnisoShapeModels(ho_Image, hv_ModelID, (new HTuple(-22.5)).TupleRad()
                            //    , (new HTuple(45)).TupleRad(), 0.9, 1.1, 0.9, 1.1, 0.5, 1, 0.5, hv_SubPixel,
                            //    0, 0.9, out hv_tryFindRow, out hv_tryFindCol, out hv_tryFindAngle, out hv_tryFindScaleR,
                            //    out hv_tryFindScaleC, out hv_tryFindScore,out hv_modelIndex);

                            HOperatorSet.FindAnisoShapeModels(ho_Image, hv_ModelIDs,
                                hv_angleStarts, hv_angleExtents, hv_scaleRMins, hv_scaleRMaxs, hv_scaleCMins, hv_scaleCMaxs,
                                hv_minScores, hv_numMatches, hv_maxOverlaps, hv_subPixels, hv_numLevels, hv_greediness,
                                out hv_findRow, out hv_findCol, out hv_findAngle, out hv_findScaleR, out hv_findScaleC, out hv_findScore, out hv_modelIndex);

                            break;

                        default:
                            strErrMsg = "选择的匹配算法列表中不存在！";
                            return -1;
                            //break;
                    }

                    if ((int)(new HTuple((new HTuple(hv_findScore.TupleLength())).TupleLess(1))) != 0)
                    {
                        strErrMsg = "模板匹配结果为空！";
                        return -1;
                    }
                }
                catch (HalconException herr)
                {
                    //"搜寻模板时出错！";
                    strErrMsg = "错因：" + herr;
                    return -1;
                }
            }
            catch (Exception err)
            {
                //"搜寻模板时出错！";
                strErrMsg = "错因：" + err;
                return -1;
            }
            finally
            {
                ho_Image.Dispose();
            }
            return 0;
        }


        /************************************************
        * 功能：删除模板数据(除搜寻ROI外的所有模板数据：包括删除模板ID、模板ROI、模板ROI数组、模板ROI运算记录、模板图像)，并显示删除后效果
        * 即：模板图像初始化空对象、模板ROI初始化空区域(不是空对象),模板ROI数组初始化空对象、模板ID数据和模板ROI运算记录初始化空【】
        * 参数： 输入输出,
        * 返回值：成功返回0，失败返回-1；
         最近更改日期:20190404
        ************************************************/
        public int DelModelData(HTuple hv_WindowHandle, ref HObject ho_modelImage, ref HObject ho_allRegion, ref HObject ho_allRegionTuple,
            ref HTuple hv_recordROISetTuple, ref HTuple hv_modelId, ref string strErrMsg)
        {
            strErrMsg = "";
            /*类中所有方法外定义变量与方法内定义的局部变量的区别:
          * (1)方法内部定义的局部变量定义时没有默认自动赋任何值,因此不能直接使用，
          * 只有方法内的其他方法可通过out参数来使用，非out修饰的参数不能使用。
          * (2)类中所有方法外定义的变量定义时即使未赋值也会默认自动赋值，自动赋null(引用类型)或其他值(值类型)
          */

            /*ref与out与params与数组(int[] a)方法参数的区别:
            * (1)ref修饰参数必须方法外先赋值(可以赋null或其他值)方法内可以重新赋新值使用并传出，或不赋值只使用。
            * (2)out修饰的参数的方法外只定义不必赋值(也可以赋值，但如果方法外赋非null的其他值则会生成垃圾内存,
            * 此时方法外需先释放变量内存,避免内存泄漏)，方法内必须先赋新值再使用最后传出，或先赋值不使用直接传出。
            * (3)params修饰方法的参数只能是数组(比如:params int[] a)：表示让方法接收任意个数同类型的参数，
             * 修饰的参数只能传入不能传出，且一个方法只允许有一个params参数且只能放到方法参数的最后一个参数
             * (比如:调用 func("nihao",1,2,3),定义 public void func(string str, params int[] a))
            */

            try
            {
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }

                try
                {
                    //必须先释放内存避免垃圾内存泄漏
                    if (ho_modelImage != null)
                    {
                        ho_modelImage.Dispose();
                    }
                    if (ho_allRegion != null)
                    {
                        ho_modelImage.Dispose();
                    }
                    if (ho_allRegionTuple != null)
                    {
                        ho_modelImage.Dispose();
                    }
                    HOperatorSet.GenEmptyObj(out ho_modelImage);//空对象
                    HOperatorSet.GenEmptyRegion(out ho_allRegion);//空区域
                    HOperatorSet.GenEmptyObj(out ho_allRegionTuple);//空对象

                    hv_recordROISetTuple = new HTuple();//空【】
                    hv_modelId = new HTuple();//空【】
                    //刷新显示删除后的情况
                    //HOperatorSet.ClearWindow(hv_WindowHandle);
                    //HOperatorSet.DispObj(ho_modelImage, hv_WindowHandle);
                    //HOperatorSet.SetDraw(hv_WindowHandle, "margin");
                    //HOperatorSet.SetLineWidth(hv_WindowHandle, 1);
                    //HOperatorSet.SetColor(hv_WindowHandle, "blue");
                    //HOperatorSet.DispObj(ho_allRegion, hv_WindowHandle);
                    //HOperatorSet.SetColor(hv_WindowHandle, "dim gray");
                    //HOperatorSet.DispObj(ho_allRegionTuple, hv_WindowHandle);

                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;
        }


        /************************************************
        功能：显示创建的模板(单个轮廓),无偏移量
              1.显示创建的模板轮廓，颜色：绿色green
              2.显示创建模板的ROI和ROI中心，颜色：两者蓝色blue
              3.显示创建模板后修改参考原点后的模板点，颜色：蓝绿色cyan
             注意：空区域的面积=0， 空对象Region的面积=[]也即面积<0，Region区域存在则面积>0
         输入参数：
         * 参1 输入图像
         * 参2：ho_allRegion图像上感兴趣区域，区域可以为空区域，但不能为空对象，
         * 参3：窗体句柄,
         * 参4：要显示的模板ID(可以获得模板轮廓)
         * 增加返回错误消息字符串
         * 返回值： 成功返回0、失败返回-1
         最近更改日期:2019-4-12
       ************************************************/
        public int showCreatModel(HObject ho_modelImage, HObject ho_allRegion, HTuple hv_WindowHandle, HTuple hv_modelID, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];
            // Local iconic variables 
            HObject ho_modelXLD = null;
            HObject ho_mapModelXLD = null;

            HObject ho_allRegionXLD = null;

            HObject ho_creatCrossRC = null;
            HObject ho_ROICrossRC = null;
            //HObject ho_offsetCrossRC = null;

            // Local control variables 
            HTuple hv_ROIArea = new HTuple();
            HTuple hv_ROIRow = new HTuple();
            HTuple hv_ROICol = new HTuple();

            HTuple hv_origRow = new HTuple();
            HTuple hv_origCol = new HTuple();
            HTuple hv_NegOrigRow = new HTuple();
            HTuple hv_NegOrigCol = new HTuple();

            HTuple hv_ModelHomMat2D = new HTuple();


            // Initialize local and output iconic variables 

            HOperatorSet.GenEmptyObj(out ho_modelXLD);
            HOperatorSet.GenEmptyObj(out ho_mapModelXLD);

            HOperatorSet.GenEmptyObj(out ho_allRegionXLD);

            HOperatorSet.GenEmptyObj(out ho_creatCrossRC);
            HOperatorSet.GenEmptyObj(out ho_ROICrossRC);
            //HOperatorSet.GenEmptyObj(out ho_offsetCrossRC);

            try
            {
                try
                {
                    if (!HObjectValided(ho_modelImage, ref strErrMsg))
                    {
                        strErrMsg = "输入模板图像无效：" + strErrMsg;
                        return -1;
                    }

                    if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                    {
                        strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                        return -1;
                    }

                    if (!HTupleValided(hv_modelID, ref strErrMsg))
                    {
                        strErrMsg = "输入模板ID数据无效：" + strErrMsg;
                        return -1;
                    }

                    if (!HObjectValided(ho_allRegion, ref strErrMsg))
                    {
                        strErrMsg = "输入模板区域无效：" + strErrMsg;
                        return -1;
                    }
                    else //如果不为空对象，
                    {
                        HOperatorSet.AreaCenter(ho_allRegion, out hv_ROIArea, out hv_ROIRow, out hv_ROICol);
                        if ((int)(new HTuple(hv_ROIArea.TupleGreater(0))) != 0)
                        {//面积>0，则区域不为空区域
                        }
                        else
                        {//如果为空区域
                            strErrMsg = "输入模板区域为空区域！";
                            return -1;
                        }
                    }

                    //求ROI中心并转化为xld
                    ho_allRegionXLD.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_allRegion, out ho_allRegionXLD, "border");
                    //获取模板第一等级轮廓
                    ho_modelXLD.Dispose();
                    HOperatorSet.GetShapeModelContours(out ho_modelXLD, hv_modelID, 1);
                    //获取模板参考原点坐标(默认为0,0;方向默认0);
                    //元组取反（origRow, origCol为正，则模板轮廓向图像左上角移动,匹配到模板坐标结果向右下增加)
                    HOperatorSet.GetShapeModelOrigin(hv_modelID, out hv_origRow, out hv_origCol);
                    HOperatorSet.TupleNeg(hv_origRow, out hv_NegOrigRow);
                    HOperatorSet.TupleNeg(hv_origCol, out hv_NegOrigCol);

                    //模板ROI不能为空区域
                    if ((int)(new HTuple(hv_ROIArea.TupleGreater(0))) != 0)
                    {
                        //*(1).将当前模板轮廓从左上角平移旋转放缩到所画的地方上***
                        //定义二维齐次单位变换矩阵
                        HOperatorSet.HomMat2dIdentity(out hv_ModelHomMat2D);
                        //添加放缩(参2/3行列放缩系数，参4/5放缩参考基准点)-在参考原点上缩放
                        HOperatorSet.HomMat2dScale(hv_ModelHomMat2D, 1.0, 1.0, hv_NegOrigRow,
                            hv_NegOrigCol, out hv_ModelHomMat2D);
                        //添加旋转(参2要旋转多少角度，参3/4旋转参考基准点)-在参考原点上旋转
                        HOperatorSet.HomMat2dRotate(hv_ModelHomMat2D, 0, hv_NegOrigRow, hv_NegOrigCol,
                            out hv_ModelHomMat2D);
                        //添加平移，参2/3：行列平移增量(相当于从(NegOrigRow, NegOrigRow)平移到所画的地方上(ROIRow, ROICol))
                        HOperatorSet.HomMat2dTranslate(hv_ModelHomMat2D, hv_ROIRow + hv_origRow,
                            hv_ROICol + hv_origCol, out hv_ModelHomMat2D);
                        //**或者
                        //平移，旋转
                        //vector_angle_to_rigid (0, 0, 0, ROIRow+origRow, ROICol+ origCol, 0, ModelHomMat2D)
                        //缩放,相对于ROI中心放缩
                        //hom_mat2d_scale (HomMat2D, 1.0, 1.0, ROIRow, ROICol, ModelHomMat2D)
                        //将可能修改过参考点的模板轮廓(参考点为正轮廓向左上移动)平移旋转放缩到平移到所画的地方上
                        ho_mapModelXLD.Dispose();
                        HOperatorSet.AffineTransContourXld(ho_modelXLD, out ho_mapModelXLD, hv_ModelHomMat2D);

                        //**(2).将当前的模板ROI轮廓从左上角平移旋转放缩到画的地方***
                        //**这里不用变换直接求取模板ROI轮廓
                        //**(3).将当前的模板ROI轮廓中心从左上角平移旋转放缩到到画的地方,生成"+"号轮廓***
                        //*这里不用变换直接求取
                        ho_ROICrossRC.Dispose();
                        HOperatorSet.GenCrossContourXld(out ho_ROICrossRC, hv_ROIRow, hv_ROICol,
                            30, (new HTuple(45)).TupleRad());
                        //**(4).将当前可能修改过参考原点的模板点生成"+"号轮廓,
                        ho_creatCrossRC.Dispose();
                        HOperatorSet.GenCrossContourXld(out ho_creatCrossRC, hv_ROIRow + hv_origRow,
                            hv_ROICol + hv_origCol, 30, (new HTuple(45)).TupleRad());
                        //或者
                        //affine_trans_point_2d (ModelHomMat2D, 0, 0, ROIOrigRow, ROIOrigCol)
                        //gen_cross_contour_xld (creatCrossRC, ROIOrigRow, ROIOrigCol, 30, rad(45))

                        ////**(5).将当前模板点追加偏移量后的目标点生成"+"号轮廓,***
                        //ho_offsetCrossRC.Dispose();
                        //HOperatorSet.GenCrossContourXld(out ho_offsetCrossRC, (hv_ROIRow + hv_origRow) + hv_offsetRow,
                        //    (hv_ROICol + hv_origCol) + hv_offsetCol, 30, (new HTuple(45)).TupleRad()
                        //    );

                    }

                    //**可视化创建后的模板的效果
                    HOperatorSet.ClearWindow(hv_WindowHandle);
                    HOperatorSet.DispObj(ho_modelImage, hv_WindowHandle);
                    HOperatorSet.SetLineWidth(hv_WindowHandle, 1);

                    HOperatorSet.SetColor(hv_WindowHandle, "green");
                    if (bModeType)//判断是否为xld创建模板
                    {
                        HOperatorSet.DispObj(ho_Edges, hv_WindowHandle);
                    }
                    else
                    {

                        HOperatorSet.DispObj(ho_mapModelXLD, hv_WindowHandle);
                    }

                    HOperatorSet.SetColor(hv_WindowHandle, "blue");
                    HOperatorSet.DispObj(ho_allRegionXLD, hv_WindowHandle);
                    HOperatorSet.DispObj(ho_ROICrossRC, hv_WindowHandle);

                    HOperatorSet.SetColor(hv_WindowHandle, "cyan");
                    HOperatorSet.DispObj(ho_creatCrossRC, hv_WindowHandle);

                    //HOperatorSet.SetColor(hv_WindowHandle, "red");
                    //HOperatorSet.DispObj(ho_offsetCrossRC, hv_WindowHandle);
                }
                catch (HalconException HDevExpDefaultException)
                {
                    strErrMsg = "错因：" + HDevExpDefaultException;
                    return -1;//失败
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;//失败
            }
            finally
            {
                ho_modelXLD.Dispose();
                ho_mapModelXLD.Dispose();
                ho_allRegionXLD.Dispose();
                ho_creatCrossRC.Dispose();
                ho_ROICrossRC.Dispose();
                //ho_offsetCrossRC.Dispose();
            }
            return 0;//成功
        }

        /************************************************
           功能：|modelIDs|>1显示由find_xxx_shape_models找到的多种类模板
                 |modelIDs|=1显示由find_xxx_shape_model或find_xxx_shape_models找到的单种类模板，无偏移量
                  1.显示匹配到的(所有种类的所有)模板轮廓，颜色：绿色green
                  2.显示匹配到的模板ROI和ROI中心，，颜色：两者蓝色,blue
                  3.显示匹配到的坐标点(若无追加偏移量则是发给外界的目标点)，颜色：蓝绿色cyan
                  4.显示匹配到的模板"索引号_模板顺序号"，比如:0_1表第一种模板的第一个模板，颜色：蓝绿色cyan
            * 注意：空区域的面积=0， 空对象Region的面积=[]也即面积<0，Region区域存在则面积>0
            输入参数：
            * 参1 输入ho_Image 
            * 参2：ho_allRegions添加的模板ROI集合,元素个数代表模板种类数,不允许输入空对象
            * 参3：窗体句柄,
            * 参4：hv_modelIDs添加的模板ID数据集合,元素个数代表模板种类数,不允许输入[]
            * 参5、6、7:(hv_findRowss,hv_findColss,hv_findAngless)元素个数代表了所有种类所有模板的总和,不允许输入[]
            * 参8、9：(hv_findScaleRss,hv_findScaleCss)元素数和findRowss相同。只有一个元素[x]代表所有元素都为[x],不允许输入[]
            * 参10：hv_modelIndexs:是模板索引号集合(索引号从0开始按添加modelIDs顺序排序),
            * (1)记录找模板的顺序,找模板顺序无规律，比如:[0,0,1,1]、[1,0,0,1]
            * (2)每个元素代表findRowss、findColss、findAngless每个元素结果是哪个模板的
            * (3)元素数和findRowss相同。只有一个元素[x]代表所有元素都为[x](多种模板将出现奇怪现象),不允许输入[]
            * 参数strErrMsg：返回错误消息字符串
            * 返回值： 成功返回0、失败返回-1
            最近更改日期:2019-4-12
          ************************************************/
        public int showFindModel(HObject ho_Image, HObject ho_allRegions, HTuple hv_WindowHandle, HTuple hv_modelIDs,
               HTuple hv_findRowss, HTuple hv_findColss, HTuple hv_findAngless,
               HTuple hv_findScaleRss, HTuple hv_findScaleCss, HTuple hv_modelIndexs, ref string strErrMsg)
        {
            strErrMsg = "";

            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 
            HObject ho_allRegion = null;
            HObject ho_allRegionXLD = null;
            HObject ho_mapAllRegionXLD = null;
            HObject ho_mapAllRegionXLDss = null;//暂时记录所有种类模板的所有模板的ROI轮廓集合，添加顺序：先第一种模板然后第二种

            HObject ho_modelXLD = null;
            HObject ho_mapModelXLD = null;
            HObject ho_mapModelXLDss = null;//暂时记录所有种类模板的所有模板轮廓的集合，添加顺序：先第一种模板然后第二种

            //HObject ho_offsetCrossRC = null;//临时变量
            HObject ho_findCrossRC = null;//临时变量
            HObject ho_mapROICrossRC = null;//临时变量

            HObject ho_mapROICrossRCss = null;//暂时记录所有种类模板的所有模板的匹配ROI中心点轮廓集合，添加顺序：先第一种模板然后第二种
            HObject ho_findCrossRCss = null;//暂时记录所有种类模板的所有模板的匹配点轮廓集合，添加顺序：先第一种模板然后第二种
            //HObject ho_offsetCrossRCss = null;//暂时记录所有种类模板的所有模板的匹配点的偏移点轮廓集合，添加顺序：先第一种模板然后第二种

            // Local control variables 
            HTuple hv_NumMatches = new HTuple();//记录总模板数
            HTuple hv_Match = new HTuple();//记录总模板数的索引
            HTuple hv_Index = new HTuple();//模板索引
            HTuple hv_oneModelNum = new HTuple();//单种模板模板数

            HTuple hv_origRow = new HTuple();
            HTuple hv_origCol = new HTuple();
            HTuple hv_NegOrigRow = new HTuple();
            HTuple hv_NegOrigCol = new HTuple();

            HTuple hv_ModelHomMat2D = new HTuple();
            HTuple hv_ROIHomMat2D = new HTuple();

            HTuple hv_ROIArea = new HTuple();
            HTuple hv_ROIRow = new HTuple();
            HTuple hv_ROICol = new HTuple();

            HTuple hv_mapROIRow = new HTuple();
            HTuple hv_mapROICol = new HTuple();

            HTuple hv_modelNamess = new HTuple();//暂时记录所有种类模板的所有模板的名字(比"0_1"表0号模板的第一个匹配对象)，添加顺序：先第一种模板然后第二种
            HTuple hv_newFindRowss = new HTuple();//暂时记录所有种类模板的所有模板的新排序后的Row，添加顺序：先第一种模板然后第二种
            HTuple hv_newFindColss = new HTuple();//暂时记录所有种类模板的所有模板的新排序后的Col，添加顺序：先第一种模板然后第二种

            // Initialize local and output iconic variables 

            HOperatorSet.GenEmptyObj(out ho_allRegion);
            HOperatorSet.GenEmptyObj(out ho_allRegionXLD);
            HOperatorSet.GenEmptyObj(out ho_mapAllRegionXLD);
            HOperatorSet.GenEmptyObj(out ho_mapAllRegionXLDss);

            HOperatorSet.GenEmptyObj(out ho_modelXLD);
            HOperatorSet.GenEmptyObj(out ho_mapModelXLD);
            HOperatorSet.GenEmptyObj(out ho_mapModelXLDss);

            //HOperatorSet.GenEmptyObj(out ho_offsetCrossRC);
            HOperatorSet.GenEmptyObj(out ho_findCrossRC);
            HOperatorSet.GenEmptyObj(out ho_mapROICrossRC);

            HOperatorSet.GenEmptyObj(out ho_mapROICrossRCss);
            HOperatorSet.GenEmptyObj(out ho_findCrossRCss);
            //HOperatorSet.GenEmptyObj(out ho_offsetCrossRCss);

            try
            {
                try
                {
                    if (!HObjectValided(ho_Image, ref strErrMsg))
                    {
                        strErrMsg = "输入图像无效："+ strErrMsg;
                        return -1;
                    }

                    if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                    {
                        strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                        return -1;
                    }

                    if (!HTupleValided(hv_modelIDs, ref strErrMsg))
                    {
                        strErrMsg = "输入模板ID数据无效：" + strErrMsg;
                        return -1;
                    }
                    //模板种类数
                    int iModelKindNum = hv_modelIDs.TupleLength();
                    if (ho_allRegions.CountObj() != iModelKindNum)
                    {
                        strErrMsg = "输入模板ID、模板ROI元素个数不一致！";
                    }

                    if (!HTupleValided(hv_findAngless, ref strErrMsg))
                    {
                        strErrMsg = "输入角度无效：" + strErrMsg;
                        return -1;
                    }
                    if ((hv_findRowss.TupleLength() != hv_findAngless.TupleLength()) || (hv_findColss.TupleLength() != hv_findAngless.TupleLength()))
                    {
                        strErrMsg = "输入行、列、角度元素个数不一致！";
                        return -1;
                    }


                    if (!HTupleValided(hv_findScaleRss, ref strErrMsg))
                    {
                        strErrMsg = "输入行缩放系数无效：" + strErrMsg;
                        return -1;
                    }

                    if (!HTupleValided(hv_findScaleCss, ref strErrMsg))
                    {
                        strErrMsg = "输入列缩放系数无效：" + strErrMsg;
                        return -1;
                    }
                    if (!HTupleValided(hv_modelIndexs, ref strErrMsg))
                    {
                        strErrMsg = "输入模板索引无效：" + strErrMsg;
                        return -1;
                    }


                    //暂时记录，findRows元素个数代表了所有种类所有模板数总和
                    hv_NumMatches = new HTuple(hv_findRowss.TupleLength());
                    if ((int)(new HTuple(hv_NumMatches.TupleGreater(0))) != 0)
                    {
                        if ((int)(new HTuple((new HTuple(hv_findScaleRss.TupleLength())).TupleEqual(
                            1))) != 0)
                        {
                            HOperatorSet.TupleGenConst(hv_NumMatches, hv_findScaleRss, out hv_findScaleRss);
                        }
                        if ((int)(new HTuple((new HTuple(hv_findScaleCss.TupleLength())).TupleEqual(
                            1))) != 0)
                        {
                            HOperatorSet.TupleGenConst(hv_NumMatches, hv_findScaleCss, out hv_findScaleCss);
                        }
                        if ((int)(new HTuple((new HTuple(hv_modelIndexs.TupleLength())).TupleEqual(
                            1))) != 0)
                        {
                            HOperatorSet.TupleGenConst(hv_NumMatches, hv_modelIndexs, out hv_modelIndexs);
                        }

                        //模板集合的索引
                        for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_modelIDs.TupleLength()
                            )) - 1); hv_Index = (int)hv_Index + 1)
                        {
                            //记录当前种类模板个数
                            hv_oneModelNum = 0;
                            //获得对应模板id的ROI轮廓
                            ho_allRegion.Dispose();
                            HOperatorSet.SelectObj(ho_allRegions, out ho_allRegion, hv_Index + 1);
                            HOperatorSet.AreaCenter(ho_allRegion, out hv_ROIArea, out hv_ROIRow,
                                out hv_ROICol);
                            ho_allRegionXLD.Dispose();
                            HOperatorSet.GenContourRegionXld(ho_allRegion, out ho_allRegionXLD, "border");

                            //获取模板第一等级轮廓
                            ho_modelXLD.Dispose();
                            HOperatorSet.GetShapeModelContours(out ho_modelXLD, hv_modelIDs.TupleSelect(
                                hv_Index), 1);
                            //获取模板参考原点坐标(默认为0,0;方向默认0);
                            //元组取反（origRow, origCol为正，则模板轮廓向图像左上角移动,匹配到模板坐标结果向右下增加)
                            HOperatorSet.GetShapeModelOrigin(hv_modelIDs.TupleSelect(hv_Index), out hv_origRow,
                                out hv_origCol);
                            HOperatorSet.TupleNeg(hv_origRow, out hv_NegOrigRow);
                            HOperatorSet.TupleNeg(hv_origCol, out hv_NegOrigCol);

                            HTuple end_val2293 = hv_NumMatches - 1;
                            HTuple step_val2293 = 1;
                            for (hv_Match = 0; hv_Match.Continue(end_val2293, step_val2293); hv_Match = hv_Match.TupleAdd(step_val2293))
                            {
                                //当前模板属于该种类模板
                                if ((int)(new HTuple(hv_Index.TupleEqual(hv_modelIndexs.TupleSelect(
                                    hv_Match)))) != 0)
                                {
                                    hv_oneModelNum = hv_oneModelNum + 1;

                                    //**(1).将当前模板轮廓从左上角平移旋转放缩到匹配对象上,并收集***
                                    //定义二维齐次单位变换矩阵
                                    HOperatorSet.HomMat2dIdentity(out hv_ModelHomMat2D);
                                    //添加平移，参2/3：行列平移增量(相当于从0,0平移到匹配点上)
                                    HOperatorSet.HomMat2dTranslate(hv_ModelHomMat2D, hv_findRowss.TupleSelect(
                                        hv_Match), hv_findColss.TupleSelect(hv_Match), out hv_ModelHomMat2D);
                                    //添加放缩(参2/3行列放缩系数，参4/5放缩参考基准点)-在匹配点上缩放
                                    HOperatorSet.HomMat2dScale(hv_ModelHomMat2D, hv_findScaleRss.TupleSelect(
                                        hv_Match), hv_findScaleCss.TupleSelect(hv_Match), hv_findRowss.TupleSelect(
                                        hv_Match), hv_findColss.TupleSelect(hv_Match), out hv_ModelHomMat2D);
                                    //添加旋转(参2要旋转多少角度，参3/4旋转参考基准点)-在匹配点上旋转
                                    HOperatorSet.HomMat2dRotate(hv_ModelHomMat2D, hv_findAngless.TupleSelect(
                                        hv_Match), hv_findRowss.TupleSelect(hv_Match), hv_findColss.TupleSelect(
                                        hv_Match), out hv_ModelHomMat2D);
                                    //将修改过参考点的模板轮廓(参考点为正轮廓向左上移动)平移旋转放缩到匹配对象上
                                    ho_mapModelXLD.Dispose();
                                    HOperatorSet.AffineTransContourXld(ho_modelXLD, out ho_mapModelXLD,
                                        hv_ModelHomMat2D);
                                    //收集所有种类模板所有模板轮廓(按modelIDs添加顺序0-N从modelIndexs收集)
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ConcatObj(ho_mapModelXLDss, ho_mapModelXLD, out ExpTmpOutVar_0
                                            );
                                        ho_mapModelXLDss.Dispose();
                                        ho_mapModelXLDss = ExpTmpOutVar_0;
                                    }

                                    //**(2).将当前的模板ROI轮廓从左上角平移旋转放缩到匹配对象上,并收集***
                                    //将模板ROI轮廓(首先模板区域存在且不为空区域)从画的地方变换到左上角(模板参考原点)
                                    if ((int)(new HTuple(hv_ROIArea.TupleGreater(0))) != 0)
                                    {
                                        HOperatorSet.VectorAngleToRigid(hv_ROIRow, hv_ROICol, 0, hv_NegOrigRow,
                                            hv_NegOrigCol, 0, out hv_ROIHomMat2D);
                                        ho_mapAllRegionXLD.Dispose();
                                        HOperatorSet.AffineTransContourXld(ho_allRegionXLD, out ho_mapAllRegionXLD,
                                            hv_ROIHomMat2D);
                                        //再从左上角变换到匹配对象上
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.AffineTransContourXld(ho_mapAllRegionXLD, out ExpTmpOutVar_0,
                                                hv_ModelHomMat2D);
                                            ho_mapAllRegionXLD.Dispose();
                                            ho_mapAllRegionXLD = ExpTmpOutVar_0;
                                        }
                                        //收集
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.ConcatObj(ho_mapAllRegionXLDss, ho_mapAllRegionXLD,
                                                out ExpTmpOutVar_0);
                                            ho_mapAllRegionXLDss.Dispose();
                                            ho_mapAllRegionXLDss = ExpTmpOutVar_0;
                                        }
                                    }
                                    else
                                    {
                                        strErrMsg = "输入模板ROI不能为空区域！";
                                        return -1;
                                    }

                                    //**(3).将当前的模板ROI轮廓中心从左上角平移旋转放缩到匹配对象上,生成"+"号轮廓,并收集***
                                    HOperatorSet.AffineTransPoint2d(hv_ModelHomMat2D, hv_NegOrigRow,
                                        hv_NegOrigCol, out hv_mapROIRow, out hv_mapROICol);
                                    ho_mapROICrossRC.Dispose();
                                    HOperatorSet.GenCrossContourXld(out ho_mapROICrossRC, hv_mapROIRow,
                                        hv_mapROICol, 30, (new HTuple(45)).TupleRad());
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ConcatObj(ho_mapROICrossRCss, ho_mapROICrossRC, out ExpTmpOutVar_0
                                            );
                                        ho_mapROICrossRCss.Dispose();
                                        ho_mapROICrossRCss = ExpTmpOutVar_0;
                                    }

                                    //**(4).将当前模板匹配坐标点生成"+"号轮廓,并收集***
                                    ho_findCrossRC.Dispose();
                                    HOperatorSet.GenCrossContourXld(out ho_findCrossRC, hv_findRowss.TupleSelect(
                                        hv_Match), hv_findColss.TupleSelect(hv_Match), 30, (new HTuple(45)).TupleRad()
                                        );
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ConcatObj(ho_findCrossRCss, ho_findCrossRC, out ExpTmpOutVar_0
                                            );
                                        ho_findCrossRCss.Dispose();
                                        ho_findCrossRCss = ExpTmpOutVar_0;
                                    }

                                    ////**(5).将当前模板匹配坐标追加偏移量后的目标点生成"+"号轮廓,并收集***
                                    //ho_offsetCrossRC.Dispose();
                                    //HOperatorSet.GenCrossContourXld(out ho_offsetCrossRC, (hv_findRowss.TupleSelect(
                                    //    hv_Match)) + (hv_offsetRows.TupleSelect(hv_Index)), (hv_findColss.TupleSelect(
                                    //    hv_Match)) + (hv_offsetCols.TupleSelect(hv_Index)), 30, (new HTuple(45)).TupleRad()
                                    //    );
                                    //{
                                    //    HObject ExpTmpOutVar_0;
                                    //    HOperatorSet.ConcatObj(ho_offsetCrossRCss, ho_offsetCrossRC, out ExpTmpOutVar_0
                                    //        );
                                    //    ho_offsetCrossRCss.Dispose();
                                    //    ho_offsetCrossRCss = ExpTmpOutVar_0;
                                    //}

                                    //**(6).记录当前模板(从新排序后)名字和当前模板的坐标值,并收集***
                                    HTuple hv_modelName = (hv_Index + "_") + hv_oneModelNum;
                                    hv_modelNamess = hv_modelNamess.TupleConcat(hv_modelName);
                                    //记录所有种类模板的所有模板的新排序后的Row，添加顺序：先第一种模板然后第二种
                                    hv_newFindRowss = hv_newFindRowss.TupleConcat(hv_findRowss.TupleSelect(
                                        hv_Match));
                                    //记录所有种类模板的所有模板的新排序后的Col，添加顺序：先第一种模板然后第二种
                                    hv_newFindColss = hv_newFindColss.TupleConcat(hv_findColss.TupleSelect(
                                        hv_Match));

                                }
                            }
                        }
                        //**********显示************
                        HOperatorSet.SetSystem("flush_graphic", "false");//防止闪屏
                        HOperatorSet.ClearWindow(hv_WindowHandle);
                        HOperatorSet.SetSystem("flush_graphic", "true");//防止闪屏

                        HOperatorSet.DispObj(ho_Image, hv_WindowHandle);
                        HOperatorSet.SetLineWidth(hv_WindowHandle, 1);

                        HOperatorSet.SetColor(hv_WindowHandle, "green");
                        HOperatorSet.DispObj(ho_mapModelXLDss, hv_WindowHandle);

                        HOperatorSet.SetColor(hv_WindowHandle, "blue");
                        HOperatorSet.DispObj(ho_mapAllRegionXLDss, hv_WindowHandle);
                        HOperatorSet.DispObj(ho_mapROICrossRCss, hv_WindowHandle);

                        HOperatorSet.SetColor(hv_WindowHandle, "cyan");
                        HOperatorSet.DispObj(ho_findCrossRCss, hv_WindowHandle);
                        set_display_font(hv_WindowHandle, 6, "mono", "false", "false");
                        //打印字符串消息:拓展了disp_message方法，Row、Col、hv_String元素数一致可以为多元素，
                        //hv_Color可为单或|Row|个元素,单元素时对每个字符有效，多元素时对应字符有效
                        my_disp_message(hv_WindowHandle, hv_modelNamess, "image", hv_newFindRowss + 15,
                            hv_newFindColss + 15, "cyan", "false");

                        //HOperatorSet.SetColor(hv_WindowHandle, "red");
                        //HOperatorSet.DispObj(ho_offsetCrossRCss, hv_WindowHandle);

                    }

                    ////或者，仅显示模板轮廓，其他信息不显示
                    //dev_display_shape_matching_results(hv_ModelXldID, "green", hv_tryFindRow, 
                    //    hv_tryFindCol, hv_tryFindAngle, 1.0, 1.0, 0);

                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return -1;
                }

            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;
            }
            finally
            {
                ho_allRegion.Dispose();
                ho_allRegionXLD.Dispose();
                ho_mapAllRegionXLD.Dispose();
                ho_mapAllRegionXLDss.Dispose();
                ho_modelXLD.Dispose();
                ho_mapModelXLD.Dispose();
                ho_mapModelXLDss.Dispose();
                //ho_offsetCrossRC.Dispose();
                ho_findCrossRC.Dispose();
                ho_mapROICrossRC.Dispose();
                ho_mapROICrossRCss.Dispose();
                ho_findCrossRCss.Dispose();
                //ho_offsetCrossRCss.Dispose();
            }
            return 0;
        }


        /************************************************
           功能：显示创建的模板(单个轮廓)，有偏移量
                 1.显示创建的模板轮廓，颜色：绿色green
                 2.显示创建模板的ROI和ROI中心，颜色：两者蓝色blue
                 3.显示创建模板后修改参考原点后的模板点，颜色：蓝绿色cyan
                 4.显示创建模板的模板点追加指定偏移量后的点(发给外界的目标点)，颜色：红色red
                注意：空区域的面积=0， 空对象Region的面积=[]也即面积<0，Region区域存在则面积>0
            输入参数：
            * 参1 输入图像
            * 参2：ho_allRegion图像上感兴趣区域，区域可以为空区域，但不能为空对象，
            * 参3：窗体句柄,
            * 参4：要显示的模板ID(可以获得模板轮廓)
            * 参5、6：相对于匹配结果点的行列偏移量(右正左负上负下正)
            * 增加返回错误消息字符串
            * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-4-4
          ************************************************/
        public int showCreatModelOffset(HObject ho_modelImage, HObject ho_allRegion, HTuple hv_WindowHandle,
            HTuple hv_modelID, HTuple hv_offsetRow, HTuple hv_offsetCol, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];
            // Local iconic variables 
            HObject ho_modelXLD = null;
            HObject ho_mapModelXLD = null;

            HObject ho_allRegionXLD = null;

            HObject ho_creatCrossRC = null;
            HObject ho_ROICrossRC = null;
            HObject ho_offsetCrossRC = null;

            // Local control variables 
            HTuple hv_ROIArea = new HTuple();
            HTuple hv_ROIRow = new HTuple();
            HTuple hv_ROICol = new HTuple();

            HTuple hv_origRow = new HTuple();
            HTuple hv_origCol = new HTuple();
            HTuple hv_NegOrigRow = new HTuple();
            HTuple hv_NegOrigCol = new HTuple();

            HTuple hv_ModelHomMat2D = new HTuple();


            // Initialize local and output iconic variables 

            HOperatorSet.GenEmptyObj(out ho_modelXLD);
            HOperatorSet.GenEmptyObj(out ho_mapModelXLD);

            HOperatorSet.GenEmptyObj(out ho_allRegionXLD);

            HOperatorSet.GenEmptyObj(out ho_creatCrossRC);
            HOperatorSet.GenEmptyObj(out ho_ROICrossRC);
            HOperatorSet.GenEmptyObj(out ho_offsetCrossRC);

            try
            {
                try
                {
                    if (!HObjectValided(ho_modelImage, ref strErrMsg))
                    {
                        strErrMsg = "输入模板图像无效：" + strErrMsg;
                        return -1;
                    }

                    if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                    {
                        strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                        return -1;
                    }

                    if (!HTupleValided(hv_modelID, ref strErrMsg))
                    {
                        strErrMsg = "输入模板ID数据无效：" + strErrMsg;
                        return -1;
                    }

                    if (!HObjectValided(ho_allRegion, ref strErrMsg))
                    {
                        strErrMsg = "输入模板区域无效：" + strErrMsg;
                        return -1;
                    }
                    else //如果不为空对象，
                    {
                        HOperatorSet.AreaCenter(ho_allRegion, out hv_ROIArea, out hv_ROIRow, out hv_ROICol);
                        if ((int)(new HTuple(hv_ROIArea.TupleGreater(0))) != 0)
                        {//面积>0，则区域不为空区域
                        }
                        else
                        {//如果为空区域
                            strErrMsg = "输入模板区域为空区域！";
                            return -1;
                        }
                    }

                    //求ROI中心并转化为xld
                    ho_allRegionXLD.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_allRegion, out ho_allRegionXLD, "border");
                    //获取模板第一等级轮廓
                    ho_modelXLD.Dispose();
                    HOperatorSet.GetShapeModelContours(out ho_modelXLD, hv_modelID, 1);
                    //获取模板参考原点坐标(默认为0,0;方向默认0);
                    //元组取反（origRow, origCol为正，则模板轮廓向图像左上角移动,匹配到模板坐标结果向右下增加)
                    HOperatorSet.GetShapeModelOrigin(hv_modelID, out hv_origRow, out hv_origCol);
                    HOperatorSet.TupleNeg(hv_origRow, out hv_NegOrigRow);
                    HOperatorSet.TupleNeg(hv_origCol, out hv_NegOrigCol);

                    //模板ROI不能为空区域
                    if ((int)(new HTuple(hv_ROIArea.TupleGreater(0))) != 0)
                    {
                        //*(1).将当前模板轮廓从左上角平移旋转放缩到所画的地方上***
                        //定义二维齐次单位变换矩阵
                        HOperatorSet.HomMat2dIdentity(out hv_ModelHomMat2D);
                        //添加放缩(参2/3行列放缩系数，参4/5放缩参考基准点)-在参考原点上缩放
                        HOperatorSet.HomMat2dScale(hv_ModelHomMat2D, 1.0, 1.0, hv_NegOrigRow,
                            hv_NegOrigCol, out hv_ModelHomMat2D);
                        //添加旋转(参2要旋转多少角度，参3/4旋转参考基准点)-在参考原点上旋转
                        HOperatorSet.HomMat2dRotate(hv_ModelHomMat2D, 0, hv_NegOrigRow, hv_NegOrigCol,
                            out hv_ModelHomMat2D);
                        //添加平移，参2/3：行列平移增量(相当于从(NegOrigRow, NegOrigRow)平移到所画的地方上(ROIRow, ROICol))
                        HOperatorSet.HomMat2dTranslate(hv_ModelHomMat2D, hv_ROIRow + hv_origRow,
                            hv_ROICol + hv_origCol, out hv_ModelHomMat2D);
                        //**或者
                        //平移，旋转
                        //vector_angle_to_rigid (0, 0, 0, ROIRow+origRow, ROICol+ origCol, 0, ModelHomMat2D)
                        //缩放,相对于ROI中心放缩
                        //hom_mat2d_scale (HomMat2D, 1.0, 1.0, ROIRow, ROICol, ModelHomMat2D)
                        //将可能修改过参考点的模板轮廓(参考点为正轮廓向左上移动)平移旋转放缩到平移到所画的地方上
                        ho_mapModelXLD.Dispose();
                        HOperatorSet.AffineTransContourXld(ho_modelXLD, out ho_mapModelXLD, hv_ModelHomMat2D);

                        //**(2).将当前的模板ROI轮廓从左上角平移旋转放缩到画的地方***
                        //**这里不用变换直接求取模板ROI轮廓
                        //**(3).将当前的模板ROI轮廓中心从左上角平移旋转放缩到到画的地方,生成"+"号轮廓***
                        //*这里不用变换直接求取
                        ho_ROICrossRC.Dispose();
                        HOperatorSet.GenCrossContourXld(out ho_ROICrossRC, hv_ROIRow, hv_ROICol,
                            30, (new HTuple(45)).TupleRad());
                        //**(4).将当前可能修改过参考原点的模板点生成"+"号轮廓,
                        ho_creatCrossRC.Dispose();
                        HOperatorSet.GenCrossContourXld(out ho_creatCrossRC, hv_ROIRow + hv_origRow,
                            hv_ROICol + hv_origCol, 30, (new HTuple(45)).TupleRad());
                        //或者
                        //affine_trans_point_2d (ModelHomMat2D, 0, 0, ROIOrigRow, ROIOrigCol)
                        //gen_cross_contour_xld (creatCrossRC, ROIOrigRow, ROIOrigCol, 30, rad(45))
                        //**(5).将当前模板点追加偏移量后的目标点生成"+"号轮廓,***
                        ho_offsetCrossRC.Dispose();
                        HOperatorSet.GenCrossContourXld(out ho_offsetCrossRC, (hv_ROIRow + hv_origRow) + hv_offsetRow,
                            (hv_ROICol + hv_origCol) + hv_offsetCol, 30, (new HTuple(45)).TupleRad()
                            );

                    }

                    //**可视化创建后的模板的效果
                    HOperatorSet.ClearWindow(hv_WindowHandle);
                    HOperatorSet.DispObj(ho_modelImage, hv_WindowHandle);
                    HOperatorSet.SetLineWidth(hv_WindowHandle, 1);

                    HOperatorSet.SetColor(hv_WindowHandle, "green");
                    if (bModeType)//判断是否为xld创建模板
                    {
                        HOperatorSet.DispObj(ho_Edges, hv_WindowHandle);
                    }
                    else
                    {
                        HOperatorSet.DispObj(ho_mapModelXLD, hv_WindowHandle);
                    }


                    HOperatorSet.SetColor(hv_WindowHandle, "blue");
                    HOperatorSet.DispObj(ho_allRegionXLD, hv_WindowHandle);
                    HOperatorSet.DispObj(ho_ROICrossRC, hv_WindowHandle);

                    HOperatorSet.SetColor(hv_WindowHandle, "cyan");
                    HOperatorSet.DispObj(ho_creatCrossRC, hv_WindowHandle);

                    HOperatorSet.SetColor(hv_WindowHandle, "red");
                    HOperatorSet.DispObj(ho_offsetCrossRC, hv_WindowHandle);
                }
                catch (HalconException HDevExpDefaultException)
                {
                    strErrMsg = "错因：" + HDevExpDefaultException;
                    return -1;//失败
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;//失败
            }
            finally
            {
                ho_modelXLD.Dispose();
                ho_mapModelXLD.Dispose();
                ho_allRegionXLD.Dispose();
                ho_creatCrossRC.Dispose();
                ho_ROICrossRC.Dispose();
                ho_offsetCrossRC.Dispose();
            }
            return 0;//成功
        }

        /************************************************
         功能：|modelIDs|>1显示由find_xxx_shape_models找到的多种类模板
               |modelIDs|=1显示由find_xxx_shape_model或find_xxx_shape_models找到的单种类模板，有偏移量
                1.显示匹配到的(所有种类的所有)模板轮廓，颜色：绿色green
                2.显示匹配到的模板ROI和ROI中心，，颜色：两者蓝色,blue
                3.显示匹配到的坐标点(若无追加偏移量则是发给外界的目标点)，颜色：蓝绿色cyan
                4.显示匹配到的模板"索引号_模板顺序号"，比如:0_1表第一种模板的第一个模板，颜色：蓝绿色cyan
                5.显示匹配到的坐标点追加指定偏移量后的点(发给外界的目标点)，颜色：红色red
                * 注意：空区域的面积=0， 空对象Region的面积=[]也即面积<0，Region区域存在则面积>0
          输入参数：
          * 参1 输入ho_Image 
          * 参2：ho_allRegions添加的模板ROI集合,元素个数代表模板种类数,不允许输入空对象
          * 参3：窗体句柄,
          * 参4：hv_modelIDs添加的模板ID数据集合,元素个数代表模板种类数,不允许输入[]
          * 参5、6：(hv_offsetRows,hv_offsetCols)添加的每种模板偏移量集合,元素个数代表模板种类数,不允许输入[]
          * 参7、8、9:(hv_findRowss,hv_findColss,hv_findAngless)元素个数代表了所有种类所有模板的总和,不允许输入[]
          * 参10、11：(hv_findScaleRss,hv_findScaleCss)元素数和findRowss相同。只有一个元素[x]代表所有元素都为[x],不允许输入[]
          * 参12：hv_modelIndexs:是模板索引号集合(索引号从0开始按添加modelIDs顺序排序),
          * (1)记录找模板的顺序,找模板顺序无规律，比如:[0,0,1,1]、[1,0,0,1]
          * (2)每个元素代表findRowss、findColss、findAngless每个元素结果是哪个模板的
          * (3)元素数和findRowss相同。只有一个元素[x]代表所有元素都为[x](多种模板将出现奇怪现象),不允许输入[]
          * 参数strErrMsg：返回错误消息字符串
          * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-4-12
        ************************************************/
        public int showFindModelOffset(HObject ho_Image, HObject ho_allRegions, HTuple hv_WindowHandle, HTuple hv_modelIDs, HTuple hv_offsetRows, HTuple hv_offsetCols,
               HTuple hv_findRowss, HTuple hv_findColss, HTuple hv_findAngless, HTuple hv_findScaleRss, HTuple hv_findScaleCss, HTuple hv_modelIndexs, ref string strErrMsg)
        {
            strErrMsg = "";

            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 
            HObject ho_allRegion = null;
            HObject ho_allRegionXLD = null;
            HObject ho_mapAllRegionXLD = null;
            HObject ho_mapAllRegionXLDss = null;//暂时记录所有种类模板的所有模板的ROI轮廓集合，添加顺序：先第一种模板然后第二种

            HObject ho_modelXLD = null;
            HObject ho_mapModelXLD = null;
            HObject ho_mapModelXLDss = null;//暂时记录所有种类模板的所有模板轮廓的集合，添加顺序：先第一种模板然后第二种

            HObject ho_offsetCrossRC = null;//临时变量
            HObject ho_findCrossRC = null;//临时变量
            HObject ho_mapROICrossRC = null;//临时变量
            HObject ho_mapROICrossRCss = null;//暂时记录所有种类模板的所有模板的匹配ROI中心点轮廓集合，添加顺序：先第一种模板然后第二种
            HObject ho_findCrossRCss = null;//暂时记录所有种类模板的所有模板的匹配点轮廓集合，添加顺序：先第一种模板然后第二种
            HObject ho_offsetCrossRCss = null;//暂时记录所有种类模板的所有模板的匹配点的偏移点轮廓集合，添加顺序：先第一种模板然后第二种

            // Local control variables 
            HTuple hv_NumMatches = new HTuple();//记录总模板数
            HTuple hv_Match = new HTuple();//记录总模板数的索引
            HTuple hv_Index = new HTuple();//模板索引
            HTuple hv_oneModelNum = new HTuple();//单种模板模板数

            HTuple hv_origRow = new HTuple();
            HTuple hv_origCol = new HTuple();
            HTuple hv_NegOrigRow = new HTuple();
            HTuple hv_NegOrigCol = new HTuple();

            HTuple hv_ModelHomMat2D = new HTuple();
            HTuple hv_ROIHomMat2D = new HTuple();

            HTuple hv_ROIArea = new HTuple();
            HTuple hv_ROIRow = new HTuple();
            HTuple hv_ROICol = new HTuple();

            HTuple hv_mapROIRow = new HTuple();
            HTuple hv_mapROICol = new HTuple();

            HTuple hv_modelNamess = new HTuple();//暂时记录所有种类模板的所有模板的名字(比"0_1"表0号模板的第一个匹配对象)，添加顺序：先第一种模板然后第二种
            HTuple hv_newFindRowss = new HTuple();//暂时记录所有种类模板的所有模板的新排序后的Row，添加顺序：先第一种模板然后第二种
            HTuple hv_newFindColss = new HTuple();//暂时记录所有种类模板的所有模板的新排序后的Col，添加顺序：先第一种模板然后第二种

            // Initialize local and output iconic variables 

            HOperatorSet.GenEmptyObj(out ho_allRegion);
            HOperatorSet.GenEmptyObj(out ho_allRegionXLD);
            HOperatorSet.GenEmptyObj(out ho_mapAllRegionXLD);
            HOperatorSet.GenEmptyObj(out ho_mapAllRegionXLDss);

            HOperatorSet.GenEmptyObj(out ho_modelXLD);
            HOperatorSet.GenEmptyObj(out ho_mapModelXLD);
            HOperatorSet.GenEmptyObj(out ho_mapModelXLDss);

            HOperatorSet.GenEmptyObj(out ho_offsetCrossRC);
            HOperatorSet.GenEmptyObj(out ho_findCrossRC);
            HOperatorSet.GenEmptyObj(out ho_mapROICrossRC);
            HOperatorSet.GenEmptyObj(out ho_mapROICrossRCss);
            HOperatorSet.GenEmptyObj(out ho_findCrossRCss);
            HOperatorSet.GenEmptyObj(out ho_offsetCrossRCss);

            try
            {
                try
                {
                    //if (!HObjectValided(ho_Image, ref strErrMsg))
                    //{
                    //    strErrMsg = "输入图像无效：" + strErrMsg;
                    //    return -1;
                    //}

                    if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                    {
                        strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                        return -1;
                    }

                    if (!HTupleValided(hv_modelIDs, ref strErrMsg))
                    {
                        strErrMsg = "输入模板ID数据无效：" + strErrMsg;
                        return -1;
                    }
                    //模板种类数
                    int iModelKindNum = hv_modelIDs.TupleLength();
                    if ((ho_allRegions.CountObj() != iModelKindNum) || (iModelKindNum != hv_offsetRows.TupleLength()) || (iModelKindNum != hv_offsetCols.TupleLength()))
                    {
                        strErrMsg = "输入模板ID、模板ROI、模板行列偏移量元素个数不一致！";
                    }

                    if (!HTupleValided(hv_findAngless, ref strErrMsg))
                    {
                        strErrMsg = "输入角度无效：" + strErrMsg;
                        return -1;
                    }
                    if ((hv_findRowss.TupleLength() != hv_findAngless.TupleLength()) || (hv_findColss.TupleLength() != hv_findAngless.TupleLength()))
                    {
                        strErrMsg = "输入行、列、角度元素个数不一致！";
                        return -1;
                    }


                    if (!HTupleValided(hv_findScaleRss, ref strErrMsg))
                    {
                        strErrMsg = "输入行缩放系数无效：" + strErrMsg;
                        return -1;
                    }

                    if (!HTupleValided(hv_findScaleCss, ref strErrMsg))
                    {
                        strErrMsg = "输入列缩放系数无效：" + strErrMsg;
                        return -1;
                    }
                    if (!HTupleValided(hv_modelIndexs, ref strErrMsg))
                    {
                        strErrMsg = "输入模板索引无效：" + strErrMsg;
                        return -1;
                    }


                    //暂时记录，findRows元素个数代表了所有种类所有模板数总和
                    hv_NumMatches = new HTuple(hv_findRowss.TupleLength());
                    if ((int)(new HTuple(hv_NumMatches.TupleGreater(0))) != 0)
                    {

                        if ((int)(new HTuple((new HTuple(hv_findScaleRss.TupleLength())).TupleEqual(
                            1))) != 0)
                        {
                            HOperatorSet.TupleGenConst(hv_NumMatches, hv_findScaleRss, out hv_findScaleRss);
                        }
                        if ((int)(new HTuple((new HTuple(hv_findScaleCss.TupleLength())).TupleEqual(
                            1))) != 0)
                        {
                            HOperatorSet.TupleGenConst(hv_NumMatches, hv_findScaleCss, out hv_findScaleCss);
                        }
                        if ((int)(new HTuple((new HTuple(hv_modelIndexs.TupleLength())).TupleEqual(
                            1))) != 0)
                        {
                            HOperatorSet.TupleGenConst(hv_NumMatches, hv_modelIndexs, out hv_modelIndexs);
                        }

                        //模板集合的索引
                        for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_modelIDs.TupleLength()
                            )) - 1); hv_Index = (int)hv_Index + 1)
                        {
                            //记录当前种类模板个数
                            hv_oneModelNum = 0;
                            //获得对应模板id的ROI轮廓
                            ho_allRegion.Dispose();
                            HOperatorSet.SelectObj(ho_allRegions, out ho_allRegion, hv_Index + 1);
                            HOperatorSet.AreaCenter(ho_allRegion, out hv_ROIArea, out hv_ROIRow,
                                out hv_ROICol);
                            ho_allRegionXLD.Dispose();
                            HOperatorSet.GenContourRegionXld(ho_allRegion, out ho_allRegionXLD, "border");

                            //获取模板第一等级轮廓
                            ho_modelXLD.Dispose();
                            HOperatorSet.GetShapeModelContours(out ho_modelXLD, hv_modelIDs.TupleSelect(
                                hv_Index), 1);
                            //获取模板参考原点坐标(默认为0,0;方向默认0);
                            //元组取反（origRow, origCol为正，则模板轮廓向图像左上角移动,匹配到模板坐标结果向右下增加)
                            HOperatorSet.GetShapeModelOrigin(hv_modelIDs.TupleSelect(hv_Index), out hv_origRow,
                                out hv_origCol);
                            HOperatorSet.TupleNeg(hv_origRow, out hv_NegOrigRow);
                            HOperatorSet.TupleNeg(hv_origCol, out hv_NegOrigCol);

                            HTuple end_val2293 = hv_NumMatches - 1;
                            HTuple step_val2293 = 1;
                            for (hv_Match = 0; hv_Match.Continue(end_val2293, step_val2293); hv_Match = hv_Match.TupleAdd(step_val2293))
                            {
                                //当前模板属于该种类模板
                                if ((int)(new HTuple(hv_Index.TupleEqual(hv_modelIndexs.TupleSelect(
                                    hv_Match)))) != 0)
                                {
                                    hv_oneModelNum = hv_oneModelNum + 1;

                                    //**(1).将当前模板轮廓从左上角平移旋转放缩到匹配对象上,并收集***
                                    //定义二维齐次单位变换矩阵
                                    HOperatorSet.HomMat2dIdentity(out hv_ModelHomMat2D);
                                    //添加平移，参2/3：行列平移增量(相当于从0,0平移到匹配点上)
                                    HOperatorSet.HomMat2dTranslate(hv_ModelHomMat2D, hv_findRowss.TupleSelect(
                                        hv_Match), hv_findColss.TupleSelect(hv_Match), out hv_ModelHomMat2D);
                                    //添加放缩(参2/3行列放缩系数，参4/5放缩参考基准点)-在匹配点上缩放
                                    HOperatorSet.HomMat2dScale(hv_ModelHomMat2D, hv_findScaleRss.TupleSelect(
                                        hv_Match), hv_findScaleCss.TupleSelect(hv_Match), hv_findRowss.TupleSelect(
                                        hv_Match), hv_findColss.TupleSelect(hv_Match), out hv_ModelHomMat2D);
                                    //添加旋转(参2要旋转多少角度，参3/4旋转参考基准点)-在匹配点上旋转
                                    HOperatorSet.HomMat2dRotate(hv_ModelHomMat2D, hv_findAngless.TupleSelect(
                                        hv_Match), hv_findRowss.TupleSelect(hv_Match), hv_findColss.TupleSelect(
                                        hv_Match), out hv_ModelHomMat2D);
                                    //将修改过参考点的模板轮廓(参考点为正轮廓向左上移动)平移旋转放缩到匹配对象上
                                    ho_mapModelXLD.Dispose();
                                    HOperatorSet.AffineTransContourXld(ho_modelXLD, out ho_mapModelXLD,
                                        hv_ModelHomMat2D);
                                    //收集所有种类模板所有模板轮廓(按modelIDs添加顺序0-N从modelIndexs收集)
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ConcatObj(ho_mapModelXLDss, ho_mapModelXLD, out ExpTmpOutVar_0
                                            );
                                        ho_mapModelXLDss.Dispose();
                                        ho_mapModelXLDss = ExpTmpOutVar_0;
                                    }

                                    //**(2).将当前的模板ROI轮廓从左上角平移旋转放缩到匹配对象上,并收集***
                                    //将模板ROI轮廓(首先模板区域存在且不为空区域)从画的地方变换到左上角(模板参考原点)
                                    if ((int)(new HTuple(hv_ROIArea.TupleGreater(0))) != 0)
                                    {
                                        HOperatorSet.VectorAngleToRigid(hv_ROIRow, hv_ROICol, 0, hv_NegOrigRow,
                                            hv_NegOrigCol, 0, out hv_ROIHomMat2D);
                                        ho_mapAllRegionXLD.Dispose();
                                        HOperatorSet.AffineTransContourXld(ho_allRegionXLD, out ho_mapAllRegionXLD,
                                            hv_ROIHomMat2D);
                                        //再从左上角变换到匹配对象上
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.AffineTransContourXld(ho_mapAllRegionXLD, out ExpTmpOutVar_0,
                                                hv_ModelHomMat2D);
                                            ho_mapAllRegionXLD.Dispose();
                                            ho_mapAllRegionXLD = ExpTmpOutVar_0;
                                        }
                                        //收集
                                        {
                                            HObject ExpTmpOutVar_0;
                                            HOperatorSet.ConcatObj(ho_mapAllRegionXLDss, ho_mapAllRegionXLD,
                                                out ExpTmpOutVar_0);
                                            ho_mapAllRegionXLDss.Dispose();
                                            ho_mapAllRegionXLDss = ExpTmpOutVar_0;
                                        }
                                    }

                                    //**(3).将当前的模板ROI轮廓中心从左上角平移旋转放缩到匹配对象上,生成"+"号轮廓,并收集***
                                    HOperatorSet.AffineTransPoint2d(hv_ModelHomMat2D, hv_NegOrigRow,
                                        hv_NegOrigCol, out hv_mapROIRow, out hv_mapROICol);
                                    ho_mapROICrossRC.Dispose();
                                    HOperatorSet.GenCrossContourXld(out ho_mapROICrossRC, hv_mapROIRow,
                                        hv_mapROICol, 30, (new HTuple(45)).TupleRad());
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ConcatObj(ho_mapROICrossRCss, ho_mapROICrossRC, out ExpTmpOutVar_0
                                            );
                                        ho_mapROICrossRCss.Dispose();
                                        ho_mapROICrossRCss = ExpTmpOutVar_0;
                                    }

                                    //**(4).将当前模板匹配坐标点生成"+"号轮廓,并收集***
                                    ho_findCrossRC.Dispose();
                                    HOperatorSet.GenCrossContourXld(out ho_findCrossRC, hv_findRowss.TupleSelect(
                                        hv_Match), hv_findColss.TupleSelect(hv_Match), 30, (new HTuple(45)).TupleRad()
                                        );
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ConcatObj(ho_findCrossRCss, ho_findCrossRC, out ExpTmpOutVar_0
                                            );
                                        ho_findCrossRCss.Dispose();
                                        ho_findCrossRCss = ExpTmpOutVar_0;
                                    }

                                    //**(5).将当前模板匹配坐标追加偏移量后的目标点生成"+"号轮廓,并收集***
                                    ho_offsetCrossRC.Dispose();
                                    HOperatorSet.GenCrossContourXld(out ho_offsetCrossRC, (hv_findRowss.TupleSelect(
                                        hv_Match)) + (hv_offsetRows.TupleSelect(hv_Index)), (hv_findColss.TupleSelect(
                                        hv_Match)) + (hv_offsetCols.TupleSelect(hv_Index)), 30, (new HTuple(45)).TupleRad()
                                        );
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ConcatObj(ho_offsetCrossRCss, ho_offsetCrossRC, out ExpTmpOutVar_0
                                            );
                                        ho_offsetCrossRCss.Dispose();
                                        ho_offsetCrossRCss = ExpTmpOutVar_0;
                                    }

                                    //**(6).记录当前模板(从新排序后)名字和当前模板的坐标值,并收集***
                                    HTuple hv_modelName = (hv_Index + "_") + hv_oneModelNum;
                                    hv_modelNamess = hv_modelNamess.TupleConcat(hv_modelName);
                                    //记录所有种类模板的所有模板的新排序后的Row，添加顺序：先第一种模板然后第二种
                                    hv_newFindRowss = hv_newFindRowss.TupleConcat(hv_findRowss.TupleSelect(
                                        hv_Match));
                                    //记录所有种类模板的所有模板的新排序后的Col，添加顺序：先第一种模板然后第二种
                                    hv_newFindColss = hv_newFindColss.TupleConcat(hv_findColss.TupleSelect(
                                        hv_Match));

                                }
                            }
                        }
                        //**********显示************
                        //HOperatorSet.ClearWindow(hv_WindowHandle);
                        //HOperatorSet.DispObj(ho_Image, hv_WindowHandle);
                        HOperatorSet.SetLineWidth(hv_WindowHandle, 1);

                        HOperatorSet.SetColor(hv_WindowHandle, "green");
                        HOperatorSet.DispObj(ho_mapModelXLDss, hv_WindowHandle);

                        HOperatorSet.SetColor(hv_WindowHandle, "blue");
                        HOperatorSet.DispObj(ho_mapAllRegionXLDss, hv_WindowHandle);
                        HOperatorSet.DispObj(ho_mapROICrossRCss, hv_WindowHandle);

                        HOperatorSet.SetColor(hv_WindowHandle, "cyan");
                        HOperatorSet.DispObj(ho_findCrossRCss, hv_WindowHandle);
                        set_display_font(hv_WindowHandle, 6, "mono", "false", "false");
                        //打印字符串消息:拓展了disp_message方法，Row、Col、hv_String元素数一致可以为多元素，
                        //hv_Color可为单或|Row|个元素,单元素时对每个字符有效，多元素时对应字符有效
                        my_disp_message(hv_WindowHandle, hv_modelNamess, "image", hv_newFindRowss + 15,
                            hv_newFindColss + 15, "cyan", "false");

                        HOperatorSet.SetColor(hv_WindowHandle, "red");
                        HOperatorSet.DispObj(ho_offsetCrossRCss, hv_WindowHandle);

                    }

                    ////或者，仅显示模板轮廓，其他信息不显示
                    //dev_display_shape_matching_results(hv_ModelXldID, "green", hv_tryFindRow, 
                    //    hv_tryFindCol, hv_tryFindAngle, 1.0, 1.0, 0);

                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;
            }
            finally
            {
                ho_allRegion.Dispose();
                ho_allRegionXLD.Dispose();
                ho_mapAllRegionXLD.Dispose();
                ho_mapAllRegionXLDss.Dispose();
                ho_modelXLD.Dispose();
                ho_mapModelXLD.Dispose();
                ho_mapModelXLDss.Dispose();
                ho_offsetCrossRC.Dispose();
                ho_findCrossRC.Dispose();
                ho_mapROICrossRC.Dispose();
                ho_mapROICrossRCss.Dispose();
                ho_findCrossRCss.Dispose();
                ho_offsetCrossRCss.Dispose();
            }
            return 0;
        }

        /**模板创建前参数查询，模板创建后参数修改与查询相关**/

        /************************************************
        功能： 画模板ROI后，模板创建前，查询所有自动参数的具体值(自动值根据当前ROI区域图像环境自动确定)
        *查询所有自动参数('num_levels', 'angle_step', 'scale_step', 'optimization', 'contrast_low', 'contrast_high', 'min_size', 'min_contrast')
        * 参1 输入ho_Image
        * 参2：ho_allRegion图像上感兴趣区域，当区域为空对象或空区域时，感兴趣区域为整幅图像
        * 参3、4：查到的参数名，及对应值
        * 增加返回错误消息字符串
          * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-4-8
        ************************************************/
        public int inquireModelAutoPara(HObject ho_Image, HObject ho_allRegion,
            ref HTuple hv_ParameterName, ref HTuple hv_ParameterValue, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 
            HObject ho_ROIImage = null;
            // Local control variables 
            HTuple hv_allRegionArea = new HTuple();
            HTuple hv_allRegionRow = new HTuple();
            HTuple hv_allRegionCol = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ROIImage);

            try
            {
                if (!HObjectValided(ho_Image, ref strErrMsg))
                {
                    strErrMsg = "输入模板图像无效：" + strErrMsg;
                    return -1;
                }

                try
                {
                    if (!HObjectValided(ho_allRegion, ref strErrMsg))
                    {
                        strErrMsg = "输入模板区域无效：" + strErrMsg;
                        return -1;
                    }
                    else //如果不为空对象，
                    {
                        HOperatorSet.AreaCenter(ho_allRegion, out hv_allRegionArea, out hv_allRegionRow, out hv_allRegionCol);
                        if ((int)(new HTuple(hv_allRegionArea.TupleGreater(0))) != 0)
                        {//面积>0，则区域不为空区域
                            ho_ROIImage.Dispose();
                            HOperatorSet.ReduceDomain(ho_Image, ho_allRegion, out ho_ROIImage);
                        }
                        else
                        {//如果为空区域
                            strErrMsg = "输入模板区域为空区域！";
                            return -1;
                        }
                    }
                    //注意当查询指定一个或几个自动值时，将对应值置"auto"置，其他自动值写具体值不能置"auto"
                    //查询范围为"all"表查询所有自动参数，所有自动参数均置"auto"
                    HOperatorSet.DetermineShapeModelParams(ho_ROIImage, "auto", -0.39, 0.79,
                        0.9, 1.1, "auto", "use_polarity", "auto", "auto", "all", out hv_ParameterName,
                        out hv_ParameterValue);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;
            }
            finally
            {
                ho_ROIImage.Dispose();
            }
            return 0;
        }

        /************************************************
        功能：模板创建前，查询当前所选金字塔等级和对比度生成的模板轮廓质量,选择指定层显示效果
         输入参数：
         * 参1 输入ho_Image
         * 参2：ho_allRegion图像上感兴趣区域，当区域为空对象或空区域时，感兴趣区域为整幅图像
         * 参3：窗体句柄
         * 参4、5：输入金字塔总层数和对比度
         * 参6：输入要显示观察哪一层的预生成模板轮廓
          * 增加返回错误消息字符串
          * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-4-8
        ************************************************/
        public int inquireModelContourQuality(HObject ho_Image, HObject ho_allRegion, HTuple hv_WindowHandle,
            HTuple hv_pyramidNum, HTuple hv_contrast, HTuple hv_pyramidObj, ref string strErrMsg)
        {
            strErrMsg = "";
            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 
            HObject ho_ROIImage = null;
            HObject ho_pyramidROIImages = null, ho_pyramidROIs = null;
            HObject ho_pyramidROI = null, ho_pyramidImages = null, ho_pyramidImage = null;
            HObject ho_pyramidAllRegions = null, ho_pyramidAllRegion = null;
            HObject ho_pyramidAllRegionXLD = null;
            // Local control variables 
            HTuple hv_allRegionArea = new HTuple();
            HTuple hv_allRegionRow = new HTuple();
            HTuple hv_allRegionCol = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ROIImage);
            HOperatorSet.GenEmptyObj(out ho_pyramidROIImages);
            HOperatorSet.GenEmptyObj(out ho_pyramidROIs);
            HOperatorSet.GenEmptyObj(out ho_pyramidROI);
            HOperatorSet.GenEmptyObj(out ho_pyramidImages);
            HOperatorSet.GenEmptyObj(out ho_pyramidImage);
            HOperatorSet.GenEmptyObj(out ho_pyramidAllRegions);
            HOperatorSet.GenEmptyObj(out ho_pyramidAllRegion);
            HOperatorSet.GenEmptyObj(out ho_pyramidAllRegionXLD);

            try
            {
                if (!HObjectValided(ho_Image, ref strErrMsg))
                {
                    strErrMsg = "输入模板图像无效：" + strErrMsg;
                    return -1;
                }

                try
                {
                    if (!HObjectValided(ho_allRegion, ref strErrMsg))
                    {
                        strErrMsg = "输入模板区域无效：" + strErrMsg;
                        return -1;
                    }
                    else //如果不为空对象，
                    {
                        HOperatorSet.AreaCenter(ho_allRegion, out hv_allRegionArea, out hv_allRegionRow, out hv_allRegionCol);
                        if ((int)(new HTuple(hv_allRegionArea.TupleGreater(0))) != 0)
                        {//面积>0，则区域不为空区域
                            ho_ROIImage.Dispose();
                            HOperatorSet.ReduceDomain(ho_Image, ho_allRegion, out ho_ROIImage);
                        }
                        else
                        {//如果为空区域
                            strErrMsg = "输入模板区域为空区域！";
                            return -1;
                        }
                    }
                    //获取指定等级的模板轮廓区域
                    ho_pyramidROIImages.Dispose();
                    ho_pyramidROIs.Dispose();
                    HOperatorSet.InspectShapeModel(ho_ROIImage, out ho_pyramidROIImages, out ho_pyramidROIs, hv_pyramidNum, hv_contrast);

                    ho_pyramidROI.Dispose();
                    HOperatorSet.SelectObj(ho_pyramidROIs, out ho_pyramidROI, hv_pyramidObj);
                    //获取指定等级的整幅模板图像,及ROI
                    //gen_gauss_pyramid (ModelImage, pyramidImages, 'constant', 0.5),select_obj (pyramidImages, pyramidImage,3)等效于
                    //inspect_shape_model (ModelImage, pyramidImages, IpyramidROIs ,10 , 30),select_obj (pyramidImages, pyramidImage,3)
                    ho_pyramidImages.Dispose();
                    HOperatorSet.GenGaussPyramid(ho_Image, out ho_pyramidImages, "constant", 0.5);
                    ho_pyramidImage.Dispose();
                    HOperatorSet.SelectObj(ho_pyramidImages, out ho_pyramidImage, hv_pyramidObj);
                    //获取指定等级的ROI
                    ho_pyramidAllRegions.Dispose();
                    HOperatorSet.GenGaussPyramid(ho_allRegion, out ho_pyramidAllRegions, "constant", 0.5);
                    ho_pyramidAllRegion.Dispose();
                    HOperatorSet.SelectObj(ho_pyramidAllRegions, out ho_pyramidAllRegion, hv_pyramidObj);
                    ho_pyramidAllRegionXLD.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_pyramidAllRegion, out ho_pyramidAllRegionXLD, "border");

                    //设置显示窗口大小：使窗口适应金字塔图像大小
                    string strErr = "";
                    ImageFitWindowSetPart(ho_pyramidImage, hv_WindowHandle, ref strErr);

                    HOperatorSet.ClearWindow(hv_WindowHandle);
                    HOperatorSet.DispObj(ho_pyramidImage, hv_WindowHandle);
                    HOperatorSet.SetDraw(hv_WindowHandle, "fill");
                    HOperatorSet.SetLineWidth(hv_WindowHandle, 1);
                    HOperatorSet.SetColor(hv_WindowHandle, "green");
                    HOperatorSet.DispObj(ho_pyramidROI, hv_WindowHandle);
                    HOperatorSet.SetDraw(hv_WindowHandle, "margin");
                    HOperatorSet.SetLineWidth(hv_WindowHandle, 1);
                    HOperatorSet.SetColor(hv_WindowHandle, "blue");
                    HOperatorSet.DispObj(ho_pyramidAllRegionXLD, hv_WindowHandle);
                    //使用完后复位窗口大小设置，使窗口适应原图片大小
                    string strErr2 = "";
                    ImageFitWindowSetPart(ho_Image, hv_WindowHandle, ref strErr2);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;
            }
            finally
            {
                ho_ROIImage.Dispose();
                ho_pyramidROIImages.Dispose();
                ho_pyramidROIs.Dispose();
                ho_pyramidROI.Dispose();
                ho_pyramidImages.Dispose();
                ho_pyramidImage.Dispose();
                ho_pyramidAllRegions.Dispose();
                ho_pyramidAllRegion.Dispose();
                ho_pyramidAllRegionXLD.Dispose();
            }
            return 0;
        }

        /************************************************
         功能：模板创建完成后，查询模板参数
          输入参数：
          * 参1 输入模板ID
          * 参2~10:要查询的模板参数
          * 增加返回错误消息字符串
           * 返回值： 成功返回0、失败返回-1
           最近更改日期:2019-4-8
        ************************************************/
        public int getModelParams(HTuple hv_modelID, ref HTuple hv_NumLevels, ref HTuple hv_AngleStart,
                    ref HTuple hv_AngleExtent, ref HTuple hv_AngleStep, ref HTuple hv_ScaleMin, ref HTuple hv_ScaleMax,
                    ref HTuple hv_ScaleStep, ref HTuple hv_Metric, ref HTuple hv_MinContrast, ref string strErrMsg)
        {
            strErrMsg = "";

            try
            {
                if (!HTupleValided(hv_modelID, ref strErrMsg))
                {
                    strErrMsg = "输入模板ID数据无效：" + strErrMsg;
                    return -1;
                }

                try
                {
                    //查询模板参数值：无论从图像还是从Xld创建的，无论是不放缩、同步放缩、异步放缩算法创建的，均可查询
                    //参2：输出金字塔等级；参3：输出最小旋转角度；参4：输出角度范围；参5：输出角度步长；参6：最小放缩比；
                    //参7：最大放缩比，参8：放缩步长,参9：输出匹配标准；参10：输出最小对比度。
                    HOperatorSet.GetShapeModelParams(hv_modelID, out hv_NumLevels, out hv_AngleStart,
                        out hv_AngleExtent, out hv_AngleStep, out hv_ScaleMin, out hv_ScaleMax,
                        out hv_ScaleStep, out hv_Metric, out hv_MinContrast);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /************************************************
         功能：模板创建完成后，重设xld创建的模板的极性(适用于xld创建的模板)
          输入参数：
          * 参1 输入用来找模板的图像
          * 参2：输入也是隐式输出,输入为已经创建好的xld模板ID，输出为更改后的
          * 参3、4、5：初次尝试找模板时找到匹配对象行列和角度
          * 参6：输入将要更改的新的极性值
         * 增加返回错误消息字符串
          * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-4-8
        ************************************************/
        public int resetMetricXld(HObject ho_Image, HTuple hv_modelID,
            HTuple hv_Row, HTuple hv_Col, HTuple hv_Angle, HTuple hv_reseMetric, ref string strErrMsg)
        {
            strErrMsg = "";

            HTuple hv_origRow = new HTuple(), hv_origCol = new HTuple();
            HTuple hv_HomMat2D = new HTuple();

            try
            {
                if (!HObjectValided(ho_Image, ref strErrMsg))
                {
                    strErrMsg = "输入模板图像无效：" + strErrMsg;
                    return -1;
                }
                if (!HTupleValided(hv_modelID, ref strErrMsg))
                {
                    strErrMsg = "输入模板ID数据无效：" + strErrMsg;
                    return -1;
                }

                try
                {
                    //重设极性(适用于从xld创建的模板)
                    HOperatorSet.GetShapeModelOrigin(hv_modelID, out hv_origRow, out hv_origCol);
                    HOperatorSet.VectorAngleToRigid(hv_origRow, hv_origCol, 0, hv_Row, hv_Col, hv_Angle, out hv_HomMat2D);
                    HOperatorSet.SetShapeModelMetric(ho_Image, hv_modelID, hv_HomMat2D, hv_reseMetric);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /************************************************
          功能：模板创建完成后，查询模板参考原点坐标
           输入参数：
           * 参1 输入模板ID
           * 参2/3:要查询的模板参考原点
           * 增加返回错误消息字符串
          * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-4-8
         ************************************************/
        public int getModelOrigin(HTuple hv_modelID, ref HTuple hv_origRow, ref HTuple hv_origCol, ref string strErrMsg)
        {
            strErrMsg = "";
            try
            {
                if (!HTupleValided(hv_modelID, ref strErrMsg))
                {
                    strErrMsg = "输入模板ID数据无效：" + strErrMsg;
                    return -1;
                }

                try
                {
                    //查询模板参考原点:无论从图像还是从Xld创建的，无论是不放缩、同步放缩、异步放缩算法创建的，均可查询
                    //模板参考原点坐标(默认为(0,0))方向默认0
                    HOperatorSet.GetShapeModelOrigin(hv_modelID, out hv_origRow, out hv_origCol);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;

        }

        /************************************************
         功能：模板创建完成后，重设模板(创建时默认参考点0，0)参考原点坐标
          输入参数：
          * 参1 输入模板ID
          * 参2/3:要查询的模板参考原点
          * 增加返回错误消息字符串
          * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-4-8
        ************************************************/
        public int setModelOrigin(HTuple hv_modelID, HTuple hv_origRow, HTuple hv_origCol, ref string strErrMsg)
        {
            strErrMsg = "";

            try
            {
                if (!HTupleValided(hv_modelID, ref strErrMsg))
                {
                    strErrMsg = "输入模板ID数据无效：" + strErrMsg;
                    return -1;
                }
                try
                {
                    //重设模板参考原点，原点向左上角移动，相当于模板轮廓向右下方移动；默认模板轮廓中心(即画模板时的ROI中心)为原点
                    HOperatorSet.SetShapeModelOrigin(hv_modelID, hv_origRow, hv_origCol);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return -1;
                }

            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;

        }

        /************************************************
         功能：模板创建完成后，重设模板最小对比度
          输入参数：
          * 参1 输入模板ID
          * 参2/3:要查询的模板参考原点
           * 增加返回错误消息字符串
          * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-4-8
        ************************************************/
        public int setModelMinContrast(HTuple hv_modelID, HTuple hv_ContrastSize, ref string strErrMsg)
        {
            strErrMsg = "";

            try
            {
                if (!HTupleValided(hv_modelID, ref strErrMsg))
                {
                    strErrMsg = "输入模板ID数据无效：" + strErrMsg;
                    return -1;
                }
                try
                {
                    HOperatorSet.SetShapeModelParam(hv_modelID, "min_contrast", hv_ContrastSize);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;

        }

        /************************************************
         功能：模板创建完成后，重设(默认为系统设置)找模板超时时间
          输入参数：
          * 参1 输入模板ID
          * 参2:输入超时时间
          * 参3：输入计时模式
          * 增加返回错误消息字符串
          * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-4-8
        ************************************************/
        public int setModelOutTime(HTuple hv_modelID, HTuple hv_outTime, HTuple hv_outTimeMode, ref string strErrMsg)
        {
            strErrMsg = "";

            try
            {
                if (!HTupleValided(hv_modelID, ref strErrMsg))
                {
                    strErrMsg = "输入模板ID数据无效：" + strErrMsg;
                    return -1;
                }
                try
                {
                    HOperatorSet.SetShapeModelParam(hv_modelID, "timeout", hv_outTime);
                    HOperatorSet.SetSystem("timer_mode", hv_outTimeMode);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;

        }

        /************************************************
         功能：模板创建完成后，设置(默认为系统设置)是否允许模板部分被遮盖
          输入参数：
          * 参1 输入模板ID
          * 参2:输入遮挡模式："false"、"true"、"system"
          * 增加返回错误消息字符串
          * 返回值： 成功返回0、失败返回-1
          最近更改日期:2019-4-8
        ************************************************/
        public int setCoverBorder(HTuple hv_modelID, HTuple hv_coverBorder, ref string strErrMsg)
        {
            strErrMsg = "";
            try
            {
                if (!HTupleValided(hv_modelID, ref strErrMsg))
                {
                    strErrMsg = "输入模板ID数据无效：" + strErrMsg;
                    return -1;
                }
                try
                {
                    HOperatorSet.SetShapeModelParam(hv_modelID, "border_shape_models", hv_coverBorder);
                    //等效：
                    //HOperatorSet.SetSystem("border_shape_models", "false");
                    //HOperatorSet.SetSystem("border_shape_models", "true");
                    //无HOperatorSet.SetSystem("border_shape_models", "system");
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;

        }

        #endregion


        #region 2维N点标定工具相关


        /************************************************
        功能：根据已知条件启点在左上角输出指定形状的轨迹节点坐标值列表
         * 已知条件：平移总节点数、总行数或总列数、基准点节点次序数(即第几个点)、基准点(x,y)、X步长、
         * 求取结果：输出指定形状的轨迹节点坐标X集合列表、Y集合列表
        输入参数：
        * 参1：平移总节点数
        * 参2：轨迹算法选择，1："弓字形"算法；2："W字形算法"（目前支持这两种）
        * 参3：优先移动的方向，1：表示优先平移X(即水平方向)；2：表示优先平移Y方向(竖直方向)
        * 参4：总行数或总列数；X优先(水平优先)时该值代表标定点行数；Y优先(竖直优先)时该值代表标定点列数
        * 参5：基准点节点次序数(即第几个点，从1开始)
        * 参6：基准点X值
        * 参7：基准点Y值 
        * 参8：X步长
        * 参9：Y步长
        * 参10：输出所有点X值集合List本身具有ref功能，不必前缀ref
        * 参11：输出所有点Y值集合List本身具有ref功能，不必前缀ref
        * 参12：输出错误消息
        * 返回值： 成功返回0、失败返回-1
        *最近更改日期:2019-05-7
        ************************************************/
        public int GetMoveShapePointXY(int sumPointNum, int iMoveShape, int iPriorityXY, int RowOrColNum, int basePointOrder, float basePointX, float basePointY, float stepX, float stepY,
            List<float> allPointX, List<float> allPointY, ref string strErrmsg)
        {
            strErrmsg = "";
            try
            {
                //(1)X优先(水平优先)时RowOrColNum代表标定点行数；
                //(2)Y优先(竖直优先)时RowOrColNum代表标定点列数.
                //(3)X优先(水平优先)时"弓字形"算法输出为"弓","W字形"算法输出为逆转90度。
                //(4)Y优先(竖直优先)时"弓字形"算法输出为对称逆转90度,"W字形"算法输出为"W"。

                //(5)"弓字形"算法:X优先(水平优先)和Y优先(竖直优先)算法相似，只是Row和Col算法对调

                switch (iMoveShape)
                {
                    case 1://"弓"形算法

                        //1.求每行有多少个节点数，即列数
                        int perRowPointNum = sumPointNum / RowOrColNum;//每行点数
                        if ((sumPointNum % RowOrColNum) > 0)//如果有余数，则每行点数加1
                        {
                            perRowPointNum++;
                        }
                        //2.求取指定位置的基准点在第几行
                        int basePointRow = basePointOrder / perRowPointNum;//在第几行
                        if ((basePointOrder % perRowPointNum) > 0)//如果有余数，行数加1
                        {
                            basePointRow++;
                        }
                        //3.求取指定位置的基准点在第几列
                        int basePointCol = 0;
                        if (basePointRow % 2 == 0)//偶数行
                        {
                            if ((basePointOrder % perRowPointNum) > 0)//不能被整除
                            {
                                basePointCol = perRowPointNum - (basePointOrder % perRowPointNum) + 1;//在第几列
                            }
                            else
                            {
                                basePointCol = 1;//在第几列
                            }

                        }
                        else if (basePointRow % 2 == 1)//奇数行  
                        {
                            if ((basePointOrder % perRowPointNum) > 0)//不能被整除
                            {
                                basePointCol = basePointOrder % perRowPointNum;//在第几列
                            }
                            else
                            {
                                basePointCol = perRowPointNum;//在第几列
                            }
                        }

                        //4.根据基准坐标、和步长，求取各个点的坐标，添加到集合输出
                        allPointX.Clear();//清空之前数据
                        allPointY.Clear();//清空之前数据
                        for (int pointOrder = 1; pointOrder <= sumPointNum; pointOrder++)
                        {
                            //当前点在第几行
                            int pointRow = pointOrder / perRowPointNum;//在第几行
                            if ((pointOrder % perRowPointNum) > 0)//如果有余数，行数加1
                            {
                                pointRow++;
                            }
                            //当前点在在第几列
                            int pointCol = 0;
                            if (pointRow % 2 == 0)//偶数行
                            {
                                if ((pointOrder % perRowPointNum) > 0)//不能被整除
                                {
                                    pointCol = perRowPointNum - (pointOrder % perRowPointNum) + 1;//在第几列
                                }
                                else
                                {
                                    pointCol = 1;//在第几列
                                }
                            }
                            else if (pointRow % 2 == 1)//奇数行  
                            {
                                if ((pointOrder % perRowPointNum) > 0)//不能被整除
                                {
                                    pointCol = pointOrder % perRowPointNum;//在第几列
                                }
                                else
                                {
                                    pointCol = perRowPointNum;//在第几列
                                }

                            }
                            float offsetColX = 0.0f;
                            float offsetRowY = 0.0f;
                            float CurColX = 0.0f;
                            float CurRowY = 0.0f;

                            switch (iPriorityXY)
                            {
                                case 1://优先水平移动，将输出"弓"形轨迹点

                                    //当前点相对于参考点偏移量：
                                    //为实现机械坐标和图像坐标系同向(需要乘上带方向的步长)，即：左上角XY递减，右下角XY递增，
                                    //比如偏移量(-2,-2)表当前点在参考点左上角，
                                    offsetColX = (pointCol - basePointCol) * stepX;
                                    offsetRowY = (pointRow - basePointRow) * stepY;

                                    break;
                                case 2://优先竖直移动，将输出弓字形对称逆转90度

                                    //当前点相对于参考点偏移量：
                                    //为实现机械坐标和图像坐标系同向(需要乘上带方向的步长)，即：左上角XY递减，右下角XY递增，
                                    //比如偏移量(-2,-2)表当前点在参考点左上角，
                                    offsetColX = (pointRow - basePointRow) * stepX;
                                    offsetRowY = (pointCol - basePointCol) * stepY;

                                    break;
                                default:
                                    strErrmsg = "选择的运动方向优先级暂不支持！";
                                    return -1;
                            }
                            CurColX = offsetColX + basePointX;
                            CurRowY = offsetRowY + basePointY;

                            //当前点坐标添加到集合中
                            allPointX.Add(CurColX);
                            allPointY.Add(CurRowY);
                        }

                        break;

                    case 2://"W"形算法

                        //1.求每行有多少个节点数，即列数
                        int perRowPointNum2 = sumPointNum / RowOrColNum;//每行点数
                        if ((sumPointNum % RowOrColNum) > 0)//如果有余数，则每行点数加1
                        {
                            perRowPointNum2++;
                        }
                        //2.求取指定位置的基准点在第几行
                        int basePointRow2 = basePointOrder / perRowPointNum2;//在第几行
                        if ((basePointOrder % perRowPointNum2) > 0)//如果有余数，行数加1
                        {
                            basePointRow2++;
                        }
                        //3.求取指定位置的基准点在第几列
                        int basePointCol2 = 0;
                        if ((basePointOrder % perRowPointNum2) > 0)//不能被整除
                        {
                            basePointCol2 = basePointOrder % perRowPointNum2;//在第几列  
                        }
                        else
                        {
                            basePointCol2 = perRowPointNum2;//在第几列
                        }

                        //4.根据基准坐标、和步长，求取各个点的坐标，添加到集合输出
                        allPointX.Clear();//清空之前数据
                        allPointY.Clear();//清空之前数据
                        for (int pointOrder = 1; pointOrder <= sumPointNum; pointOrder++)
                        {
                            //当前点在第几行
                            int pointRow2 = pointOrder / perRowPointNum2;//在第几行
                            if ((pointOrder % perRowPointNum2) > 0)//如果有余数，行数加1
                            {
                                pointRow2++;
                            }
                            //当前点在在第几列
                            int pointCol2 = 0;
                            if ((pointOrder % perRowPointNum2) > 0)//不能被整除
                            {
                                pointCol2 = pointOrder % perRowPointNum2;//在第几列  
                            }
                            else
                            {
                                pointCol2 = perRowPointNum2;//在第几列
                            }

                            float offsetColX = 0.0f;
                            float offsetRowY = 0.0f;
                            float CurColX = 0.0f;
                            float CurRowY = 0.0f;

                            switch (iPriorityXY)
                            {
                                case 1://优先水平移动，将输出"W字形"逆转90度

                                    //当前点相对于参考点偏移量：
                                    //为实现机械坐标和图像坐标系同向(需要乘上带方向的步长)，即：左上角XY递减，右下角XY递增，
                                    //比如偏移量(-2,-2)表当前点在参考点左上角，
                                    offsetColX = (pointCol2 - basePointCol2) * stepX;
                                    offsetRowY = (pointRow2 - basePointRow2) * stepY;

                                    break;
                                case 2://优先竖直移动，将输出"W"形轨迹点

                                    //当前点相对于参考点偏移量：
                                    //为实现机械坐标和图像坐标系同向(需要乘上带方向的步长)，即：左上角XY递减，右下角XY递增，
                                    //比如偏移量(-2,-2)表当前点在参考点左上角，
                                    offsetColX = (pointRow2 - basePointRow2) * stepX;
                                    offsetRowY = (pointCol2 - basePointCol2) * stepY;

                                    break;
                                default:
                                    strErrmsg = "选择的运动方向优先级暂不支持！";
                                    return -1;
                            }
                            CurColX = offsetColX + basePointX;
                            CurRowY = offsetRowY + basePointY;

                            //当前点坐标添加到集合中
                            allPointX.Add(CurColX);
                            allPointY.Add(CurRowY);
                        }

                        break;
                    default:
                        strErrmsg = "选择的轨迹算法暂不支持！";
                        return -1;
                }
            }
            catch (Exception Err)
            {
                strErrmsg = "异常原因：" + Err;
                return -1;
            }
            return 0;
        }


        /************************************************
       功能： 获取前后二维数据的转换关系矩阵，
        输入参数：
        * 参1 前第1维度，halcon变量，可以当一维数组使用
        * 参2: 前第2维度，halcon变量，可以当一维数组使用
        * 参3 后第1维度，halcon变量，可以当一维数组使用
        * 参4: 后第2维度，halcon变量，可以当一维数组使用
        * 参5：输出变换矩阵(标定结果)
        * 参6：返回错误消息
        * 返回值： 成功返回0、失败返回-1
        最近更改日期:2019-1-24
      ************************************************/
        public int HdevVectorToHomMat2d(HTuple hv_Px, HTuple hv_Py, HTuple hv_Qx, HTuple hv_Qy, ref HTuple hv_HomMat2D, ref string strErrMsg)
        {
            strErrMsg = "";

            try
            {
                try
                {
                    HOperatorSet.VectorToHomMat2d(hv_Px, hv_Py, hv_Qx, hv_Qy, out hv_HomMat2D);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    //出错
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                //出错
                return -1;
            }
            finally
            {
            }
            return 0;
        }


        /************************************************
        功能： 通过使用二维变换矩阵，求一个点的映射点
         输入参数：
         * 参1：输入变换矩阵(标定结果)
         * 参2：前第1维度，halcon变量，可以当一维数组使用
         * 参3: 前第2维度，halcon变量，可以当一维数组使用
         * 参4  输出，后第1维度，halcon变量，可以当一维数组输出
         * 参5: 输出，后第2维度，halcon变量，可以当一维数组输出
         * 参5：输出变换矩阵(标定结果)
         * 参6：返回错误消息
         * 返回值： 成功返回0、失败返回-1
         最近更改日期:2019-5-11
       ************************************************/
        public int HdevAffineTransPoint2d(HTuple hv_HomMat2D, HTuple hv_Px, HTuple hv_Py, ref HTuple hv_Qx, ref HTuple hv_Qy, ref string strErrMsg)
        {
            strErrMsg = "";
            try
            {
                try
                {
                    HOperatorSet.AffineTransPoint2d(hv_HomMat2D, hv_Px, hv_Py, out hv_Qx, out hv_Qy);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1; //出错
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1; //出错
            }
            finally
            {
            }
            return 0;
        }

        /****功能：图像偏移量-->机械偏移量，转换结果保留小数点后6位，可调机械偏移量方向
          * 参1：输入机械偏移量X方向
          * 参2：输入机械偏移量Y方向
          * 参3：标定结果矩阵
          * 参4、5：输入图像XY偏移量
          * 参6、7：输出机械偏移量XY
          * 参8：返回错误消息
          * 返回值：成功返回0，失败返回-1
          最近更改日期:2019-7-3
        ************************************************/
        public int CalibTransPoint2d(string strOrienWorldX, string strOrienWorldY, HTuple hv_HomMat2D, HTuple hv_ImgCX,
                                    HTuple hv_ImgRY, ref HTuple hv_WorldX, ref HTuple hv_WorldY, ref string strErrMsg)
        {
            try//捕获C#语法异常，比如“未将对象的引用对象的实例”此异常HalconException捕获不住
            {
                try//捕获halcon算子异常
                {
                    int affine = HdevAffineTransPoint2d(hv_HomMat2D, hv_ImgCX, hv_ImgRY, ref hv_WorldX, ref hv_WorldY, ref strErrMsg);
                    if (affine == 0)//成功
                    {
                        hv_WorldX = hv_WorldX.TupleString("#.6f");//(或worldX.TupleString(".6f"))转成四舍五入保留6位小数的字符串数字
                        HOperatorSet.TupleNumber(hv_WorldX, out hv_WorldX);//数字字符串转数字
                        hv_WorldY = hv_WorldY.TupleString("#.6f");
                        HOperatorSet.TupleNumber(hv_WorldY, out hv_WorldY);

                        //调整机械偏移量方向，使与实际机械方向相同
                        if (strOrienWorldX == "1")
                        {
                            HOperatorSet.TupleNeg(hv_WorldX, out hv_WorldX);//取反
                        }
                        else if (strOrienWorldX == "0")
                        {//不处理，默认为正
                        }
                        else
                        {
                            strErrMsg = "输入机械X调整方向不存在！";
                            return -1;//出错，中断执行
                        }

                        if (strOrienWorldY == "1")//负
                        {
                            HOperatorSet.TupleNeg(hv_WorldY, out hv_WorldY);//取反
                        }
                        else if (strOrienWorldY == "0")//正
                        { //不处理，默认为正
                        }
                        else
                        {
                            strErrMsg = "输入机械Y调整方向不存在！";
                            return -1;//出错，中断执行
                        }

                        return 0;
                    }
                    else
                    {
                        strErrMsg = "标定转换时出错！";
                        return -1;//出错，中断执行
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "错因：" + hEx;
                    return -1;//出错，中断执行
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "错因：" + Ex;
                return -1;//出错，中断执行
            }
        }


        //数据分析

        /************************************************
        功能： 一维数据直方图，即：求一维数组所有元素的最大值、最小值、均值、中值、标准差(均方差)，注意元素类型要一致
         输入参数：
         * 参1：输入一维数组，该输入不能为空数组
         * 参2：输出最小元素
         * 参3: 输出最大元素
         * 参4  输出均值
         * 参5: 输出中值
         * 参6：输出标准差
         * 参7：返回错误消息
         * 返回值： 成功返回0、失败返回-1
         最近更改日期:2019-5-14
       ************************************************/
        public int HistogramArray1D(HTuple hv_Array1D, ref HTuple hv_Min, ref HTuple hv_Max, ref HTuple hv_Mean, ref HTuple hv_Median, ref HTuple hv_Deviation, ref string strErrMsg)
        {
            strErrMsg = "";
            try
            {
                try
                {
                    //计算最大值、最小值、平均值、中值、标准差(均方差)
                    HOperatorSet.TupleMax(hv_Array1D, out hv_Max);
                    HOperatorSet.TupleMin(hv_Array1D, out hv_Min);
                    HOperatorSet.TupleMean(hv_Array1D, out hv_Mean);
                    HOperatorSet.TupleMedian(hv_Array1D, out hv_Median);
                    HOperatorSet.TupleDeviation(hv_Array1D, out hv_Deviation);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "出错:可能输入数组数据为空," + hEx;
                    return -1; //出错
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "出错:可能输入数组数据为空," + Ex;
                return -1; //出错
            }
            finally
            {
            }
            return 0;
        }

        /************************************************
        功能： 从一维数组创建一维函数(自变量X从零开始间距是1，第一Y值是0,第二是1,第三是0,往后Y值才是数组元素)
         *     并显示函数坐标系在窗口指定位置，并计算和显示数组元素总数/最大值/最小值/平均值/中值/标准差
         输入参数：
         * 参1：要显示的窗口句柄
         * 参2：输入一维数组
         * 参3: 设置字体大小
         * 参4  设置字体显示的位置行
         * 参5: 设置字体显示的位置列
         * 参6：设置函数坐标图的线宽
         * 参7: 设置设置函数曲线的颜色，比如"green"、"red",可以写[](采集默认颜色设置)
         * 参8：设置坐标X轴标签名
         * 参9: 设置坐标Y轴标签名
         * 参10：输入参数名数组(可以写[]采用默认值),
         * 比如['axes_color','style','clip','margin_bottom','margin_left','margin_top','margin_right','start_x','end_x','start_y','end_y','origin_x','origin_y']
         * {"axes_color","style","clip","margin_bottom","margin_left","margin_top","margin_right","start_x","end_x","start_y","end_y","origin_x","origin_y"}
         * 参11：设置与参数名数组对应的参数值数组(可以写[]采用默认值)，
         * 比如['blue','line','yes',250, 500, 25, 150,0,50,0,100,0,0]，
         * {"blue","line","yes","250", "500", "25", "150","0","50","0","100","0","0"}非数字参数转化字符串，数字参数转化数值
         * 原点到左边和下边的像素距离；x轴顶点到右边距离，和y轴顶点到上边距离
         * 参12：返回错误类型
         * 返回值： 成功返回0、失败返回-1
         最近更改日期:2019-5-14
       ************************************************/
        public int ShowArrayFunct1D(HTuple hv_WindowHandle, HTuple hv_Array1D, HTuple hv_FontSize, HTuple hv_FontRow, HTuple hv_FontCol, HTuple hv_LineWidth, HTuple hv_PointLineColor, string strXLabel,
            string strYLabel, string[] strParamNames, string[] strParamValues, ref string strErrMsg)
        {
            strErrMsg = "";
            HTuple hv_FuncArray1D = null, hv_Max = null, hv_Min = null, hv_Mean = null, hv_Median = null, hv_Deviation = null;
            HTuple hv_GenParamNames = new HTuple();
            HTuple hv_GenParamValues = new HTuple();
            HTuple hv_IsNumber = new HTuple();
            HTuple hv_Number = new HTuple();

            try
            {
                if ((hv_Array1D == null) || (hv_Array1D.TupleLength() < 1))
                {
                    strErrMsg = "输入一维数组无效！";
                    return -1; //出错
                }

                try
                {
                    for (int i = 0; i < strParamNames.Length; i++)
                    {
                        hv_GenParamNames[i] = strParamNames[i];//参数名，字符串
                        hv_GenParamValues[i] = strParamValues[i];//参数值，字符串、整数、实数
                        HOperatorSet.TupleIsNumber(hv_GenParamValues[i], out hv_IsNumber);//判断是否是数字，或数字字符串
                        if (hv_IsNumber[0] == 1)//如果可以转化为数字，转化为数字
                        {
                            HOperatorSet.TupleNumber(hv_GenParamValues[i], out hv_Number);
                            hv_GenParamValues[i] = hv_Number;
                        }
                    }

                    //计算最大值、最小值、平均值、中值、标准差(均方差)
                    HOperatorSet.TupleMax(hv_Array1D, out hv_Max);
                    HOperatorSet.TupleMin(hv_Array1D, out hv_Min);
                    HOperatorSet.TupleMean(hv_Array1D, out hv_Mean);
                    HOperatorSet.TupleMedian(hv_Array1D, out hv_Median);
                    HOperatorSet.TupleDeviation(hv_Array1D, out hv_Deviation);
                    //创建一维函数
                    HOperatorSet.CreateFunct1dArray(hv_Array1D, out hv_FuncArray1D);


                    //在窗口上显示函数坐标系
                    HOperatorSet.SetLineWidth(hv_WindowHandle, hv_LineWidth);//设置坐标线宽为1
                    set_display_font(hv_WindowHandle, hv_FontSize, "mono", "false", "false");
                    disp_message(hv_WindowHandle, (((((new HTuple(new HTuple("总数:") + (new HTuple(hv_Array1D.TupleLength()))) +
                       " 最小值:") + (hv_Min.TupleString(".3f")))).TupleConcat((("\n最大值:" + (hv_Max.TupleString(".3f"))) +
                       " 平均值:") + (hv_Mean.TupleString(".3f"))))).TupleConcat((("\n中值:" + (hv_Median.TupleString(".3f"))) +
                       " 标准差:") + (hv_Deviation.TupleString(".3f"))), "window", hv_FontRow, hv_FontCol, "green", "false");

                    plot_funct_1d(hv_WindowHandle, hv_FuncArray1D, strXLabel, strYLabel, hv_PointLineColor, hv_GenParamNames, hv_GenParamValues);


                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1; //出错
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1; //出错
            }
            finally
            {
            }
            return 0;
        }



        #endregion


        #region 图像拼接工具相关
        //因还没有使用暂无多语言开发
        /***功能：定义图像对：(哪个图像应该映射到哪个图像)***
         这里拼接图像的排列为：RNum行CNum列,从左到右依次拍照添加
             输入参数：
             * 参1：输入图像排列为RNum行，>0
             * 参2：输入图像排列为CNum列，>0，比如3行*4列工12张
             * 参3: 输入图像对组成规则:(先X后Y串联;先X后Y首列串联;先X后Y中间列串联;先X后Y尾列串联
             * 先Y后X串联;先Y后X首行串联;先Y后X中间行串联;先Y后X尾行串联;)
             * 参4: 输出图像对的前者图像索引集合
             * 参5：输出图像对的后者图像索引集合
             
             * 参7：返回错误消息
             * 返回值： 成功返回0、失败返回-1
             最近更改日期:2019-5-14
        ************************************************/
        public int DefImgPair(HTuple hv_RNum, HTuple hv_CNum, string strImgPairType, ref HTuple hv_From, ref HTuple hv_To, ref string strErrMsg)
        {
            strErrMsg = "";
            HTuple hv_Fx = new HTuple(), hv_Tx = new HTuple();
            HTuple hv_Fy = new HTuple(), hv_Ty = new HTuple();

            try
            {
                switch (strImgPairType)
                {
                    case "先X后Y串联":
                        {
                            //图像拼接规则：(先)每行图片和它右边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, hv_CNum - 1, 1, out hv_Fx);
                            HOperatorSet.TupleGenSequence(2, hv_CNum, 1, out hv_Tx);
                            //收集每行图像对
                            HTuple end_val66 = hv_RNum;
                            HTuple step_val66 = 1;
                            for (HTuple hv_R = 1; hv_R.Continue(end_val66, step_val66); hv_R = hv_R.TupleAdd(step_val66))
                            {
                                hv_From = hv_From.TupleConcat(hv_Fx + (hv_CNum * (hv_R - 1)));
                                hv_To = hv_To.TupleConcat(hv_Tx + (hv_CNum * (hv_R - 1)));
                            }

                            //图像拼接规则：(后)每列图片和它下边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, (hv_CNum * (hv_RNum - 2)) + 1, hv_CNum, out hv_Fy);
                            HOperatorSet.TupleGenSequence(hv_CNum + 1, (hv_CNum * (hv_RNum - 1)) + 1, hv_CNum,
                                out hv_Ty);
                            //收集每列图像对
                            HTuple end_val75 = hv_CNum;
                            HTuple step_val75 = 1;
                            for (HTuple hv_C = 1; hv_C.Continue(end_val75, step_val75); hv_C = hv_C.TupleAdd(step_val75))
                            {
                                hv_From = hv_From.TupleConcat((hv_Fy + hv_C) - 1);
                                hv_To = hv_To.TupleConcat((hv_Ty + hv_C) - 1);
                            }
                        }
                        break;
                    case "先X后Y首列串联":
                        {
                            //图像拼接规则：(先)每行图片和它右边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, hv_CNum - 1, 1, out hv_Fx);
                            HOperatorSet.TupleGenSequence(2, hv_CNum, 1, out hv_Tx);
                            //收集每行图像对
                            HTuple end_val66 = hv_RNum;
                            HTuple step_val66 = 1;
                            for (HTuple hv_R = 1; hv_R.Continue(end_val66, step_val66); hv_R = hv_R.TupleAdd(step_val66))
                            {
                                hv_From = hv_From.TupleConcat(hv_Fx + (hv_CNum * (hv_R - 1)));
                                hv_To = hv_To.TupleConcat(hv_Tx + (hv_CNum * (hv_R - 1)));
                            }

                            //图像拼接规则：(后)每列图片和它下边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, (hv_CNum * (hv_RNum - 2)) + 1, hv_CNum, out hv_Fy);
                            HOperatorSet.TupleGenSequence(hv_CNum + 1, (hv_CNum * (hv_RNum - 1)) + 1, hv_CNum,
                                out hv_Ty);
                            //收集每列图像对
                            HTuple end_val75 = 1;
                            HTuple step_val75 = 1;
                            for (HTuple hv_C = end_val75; hv_C.Continue(end_val75, step_val75); hv_C = hv_C.TupleAdd(step_val75))
                            {
                                hv_From = hv_From.TupleConcat((hv_Fy + hv_C) - 1);
                                hv_To = hv_To.TupleConcat((hv_Ty + hv_C) - 1);
                            }
                        }

                        break;
                    case "先X后Y中间列串联":
                        {
                            //图像拼接规则：(先)每行图片和它右边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, hv_CNum - 1, 1, out hv_Fx);
                            HOperatorSet.TupleGenSequence(2, hv_CNum, 1, out hv_Tx);
                            //收集每行图像对
                            HTuple end_val66 = hv_RNum;
                            HTuple step_val66 = 1;
                            for (HTuple hv_R = 1; hv_R.Continue(end_val66, step_val66); hv_R = hv_R.TupleAdd(step_val66))
                            {
                                hv_From = hv_From.TupleConcat(hv_Fx + (hv_CNum * (hv_R - 1)));
                                hv_To = hv_To.TupleConcat(hv_Tx + (hv_CNum * (hv_R - 1)));
                            }

                            //图像拼接规则：(后)每列图片和它下边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, (hv_CNum * (hv_RNum - 2)) + 1, hv_CNum, out hv_Fy);
                            HOperatorSet.TupleGenSequence(hv_CNum + 1, (hv_CNum * (hv_RNum - 1)) + 1, hv_CNum,
                                out hv_Ty);
                            //收集每列图像对

                            HTuple end_val75 = hv_CNum / 2;
                            if (hv_CNum % 2 > 0) end_val75 = end_val75 + 1;
                            HTuple step_val75 = 1;
                            for (HTuple hv_C = end_val75; hv_C.Continue(end_val75, step_val75); hv_C = hv_C.TupleAdd(step_val75))
                            {
                                hv_From = hv_From.TupleConcat((hv_Fy + hv_C) - 1);
                                hv_To = hv_To.TupleConcat((hv_Ty + hv_C) - 1);
                            }
                        }
                        break;
                    case "先X后Y尾列串联":
                        {
                            //图像拼接规则：(先)每行图片和它右边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, hv_CNum - 1, 1, out hv_Fx);
                            HOperatorSet.TupleGenSequence(2, hv_CNum, 1, out hv_Tx);
                            //收集每行图像对
                            HTuple end_val66 = hv_RNum;
                            HTuple step_val66 = 1;
                            for (HTuple hv_R = 1; hv_R.Continue(end_val66, step_val66); hv_R = hv_R.TupleAdd(step_val66))
                            {
                                hv_From = hv_From.TupleConcat(hv_Fx + (hv_CNum * (hv_R - 1)));
                                hv_To = hv_To.TupleConcat(hv_Tx + (hv_CNum * (hv_R - 1)));
                            }

                            //图像拼接规则：(后)每列图片和它下边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, (hv_CNum * (hv_RNum - 2)) + 1, hv_CNum, out hv_Fy);
                            HOperatorSet.TupleGenSequence(hv_CNum + 1, (hv_CNum * (hv_RNum - 1)) + 1, hv_CNum,
                                out hv_Ty);
                            //收集每列图像对

                            HTuple end_val75 = hv_CNum;
                            HTuple step_val75 = 1;
                            for (HTuple hv_C = hv_CNum; hv_C.Continue(end_val75, step_val75); hv_C = hv_C.TupleAdd(step_val75))
                            {
                                hv_From = hv_From.TupleConcat((hv_Fy + hv_C) - 1);
                                hv_To = hv_To.TupleConcat((hv_Ty + hv_C) - 1);
                            }
                        }
                        break;
                    case "先Y后X串联":
                        {
                            //图像拼接规则：(先)每列图片和它下边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, (hv_CNum * (hv_RNum - 2)) + 1, hv_CNum, out hv_Fy);
                            HOperatorSet.TupleGenSequence(hv_CNum + 1, (hv_CNum * (hv_RNum - 1)) + 1, hv_CNum,
                                out hv_Ty);
                            //收集每列图像对
                            HTuple end_val75 = hv_CNum;
                            HTuple step_val75 = 1;
                            for (HTuple hv_C = 1; hv_C.Continue(end_val75, step_val75); hv_C = hv_C.TupleAdd(step_val75))
                            {
                                hv_From = hv_From.TupleConcat((hv_Fy + hv_C) - 1);
                                hv_To = hv_To.TupleConcat((hv_Ty + hv_C) - 1);
                            }

                            //图像拼接规则：(后)每行图片和它右边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, hv_CNum - 1, 1, out hv_Fx);
                            HOperatorSet.TupleGenSequence(2, hv_CNum, 1, out hv_Tx);
                            //收集每行图像对
                            HTuple end_val66 = hv_RNum;
                            HTuple step_val66 = 1;
                            for (HTuple hv_R = 1; hv_R.Continue(end_val66, step_val66); hv_R = hv_R.TupleAdd(step_val66))
                            {
                                hv_From = hv_From.TupleConcat(hv_Fx + (hv_CNum * (hv_R - 1)));
                                hv_To = hv_To.TupleConcat(hv_Tx + (hv_CNum * (hv_R - 1)));
                            }
                        }
                        break;
                    case "先Y后X首行串联":
                        {
                            //图像拼接规则：(先)每列图片和它下边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, (hv_CNum * (hv_RNum - 2)) + 1, hv_CNum, out hv_Fy);
                            HOperatorSet.TupleGenSequence(hv_CNum + 1, (hv_CNum * (hv_RNum - 1)) + 1, hv_CNum,
                                out hv_Ty);
                            //收集每列图像对
                            HTuple end_val75 = hv_CNum;
                            HTuple step_val75 = 1;
                            for (HTuple hv_C = 1; hv_C.Continue(end_val75, step_val75); hv_C = hv_C.TupleAdd(step_val75))
                            {
                                hv_From = hv_From.TupleConcat((hv_Fy + hv_C) - 1);
                                hv_To = hv_To.TupleConcat((hv_Ty + hv_C) - 1);
                            }

                            //图像拼接规则：(后)每行图片和它右边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, hv_CNum - 1, 1, out hv_Fx);
                            HOperatorSet.TupleGenSequence(2, hv_CNum, 1, out hv_Tx);
                            //收集每行图像对
                            HTuple end_val66 = 1;
                            HTuple step_val66 = 1;
                            for (HTuple hv_R = 1; hv_R.Continue(end_val66, step_val66); hv_R = hv_R.TupleAdd(step_val66))
                            {
                                hv_From = hv_From.TupleConcat(hv_Fx + (hv_CNum * (hv_R - 1)));
                                hv_To = hv_To.TupleConcat(hv_Tx + (hv_CNum * (hv_R - 1)));
                            }
                        }
                        break;
                    case "先Y后X中间行串联":
                        {
                            //图像拼接规则：(先)每列图片和它下边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, (hv_CNum * (hv_RNum - 2)) + 1, hv_CNum, out hv_Fy);
                            HOperatorSet.TupleGenSequence(hv_CNum + 1, (hv_CNum * (hv_RNum - 1)) + 1, hv_CNum,
                                out hv_Ty);
                            //收集每列图像对
                            HTuple end_val75 = hv_CNum;
                            HTuple step_val75 = 1;
                            for (HTuple hv_C = 1; hv_C.Continue(end_val75, step_val75); hv_C = hv_C.TupleAdd(step_val75))
                            {
                                hv_From = hv_From.TupleConcat((hv_Fy + hv_C) - 1);
                                hv_To = hv_To.TupleConcat((hv_Ty + hv_C) - 1);
                            }

                            //图像拼接规则：(后)每行图片和它右边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, hv_CNum - 1, 1, out hv_Fx);
                            HOperatorSet.TupleGenSequence(2, hv_CNum, 1, out hv_Tx);
                            //收集每行图像对
                            HTuple end_val66 = hv_RNum / 2;
                            if (hv_RNum % 2 > 0) end_val66 = end_val66 + 1;
                            HTuple step_val66 = 1;
                            for (HTuple hv_R = end_val66; hv_R.Continue(end_val66, step_val66); hv_R = hv_R.TupleAdd(step_val66))
                            {
                                hv_From = hv_From.TupleConcat(hv_Fx + (hv_CNum * (hv_R - 1)));
                                hv_To = hv_To.TupleConcat(hv_Tx + (hv_CNum * (hv_R - 1)));
                            }
                        }
                        break;
                    case "先Y后X尾行串联":
                        {
                            //图像拼接规则：(先)每列图片和它下边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, (hv_CNum * (hv_RNum - 2)) + 1, hv_CNum, out hv_Fy);
                            HOperatorSet.TupleGenSequence(hv_CNum + 1, (hv_CNum * (hv_RNum - 1)) + 1, hv_CNum,
                                out hv_Ty);
                            //收集每列图像对
                            HTuple end_val75 = hv_CNum;
                            HTuple step_val75 = 1;
                            for (HTuple hv_C = 1; hv_C.Continue(end_val75, step_val75); hv_C = hv_C.TupleAdd(step_val75))
                            {
                                hv_From = hv_From.TupleConcat((hv_Fy + hv_C) - 1);
                                hv_To = hv_To.TupleConcat((hv_Ty + hv_C) - 1);
                            }

                            //图像拼接规则：(后)每行图片和它右边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, hv_CNum - 1, 1, out hv_Fx);
                            HOperatorSet.TupleGenSequence(2, hv_CNum, 1, out hv_Tx);
                            //收集每行图像对
                            HTuple end_val66 = hv_RNum;
                            HTuple step_val66 = 1;
                            for (HTuple hv_R = end_val66; hv_R.Continue(end_val66, step_val66); hv_R = hv_R.TupleAdd(step_val66))
                            {
                                hv_From = hv_From.TupleConcat(hv_Fx + (hv_CNum * (hv_R - 1)));
                                hv_To = hv_To.TupleConcat(hv_Tx + (hv_CNum * (hv_R - 1)));
                            }
                        }
                        break;
                    default://"先X后Y首列串联"
                        {
                            //图像拼接规则：(先)每行图片和它右边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, hv_CNum - 1, 1, out hv_Fx);
                            HOperatorSet.TupleGenSequence(2, hv_CNum, 1, out hv_Tx);
                            //收集每行图像对
                            HTuple end_val66 = hv_RNum;
                            HTuple step_val66 = 1;
                            for (HTuple hv_R = 1; hv_R.Continue(end_val66, step_val66); hv_R = hv_R.TupleAdd(step_val66))
                            {
                                hv_From = hv_From.TupleConcat(hv_Fx + (hv_CNum * (hv_R - 1)));
                                hv_To = hv_To.TupleConcat(hv_Tx + (hv_CNum * (hv_R - 1)));
                            }

                            //图像拼接规则：(后)每列图片和它下边图片组成图像对
                            HOperatorSet.TupleGenSequence(1, (hv_CNum * (hv_RNum - 2)) + 1, hv_CNum, out hv_Fy);
                            HOperatorSet.TupleGenSequence(hv_CNum + 1, (hv_CNum * (hv_RNum - 1)) + 1, hv_CNum,
                                out hv_Ty);
                            //收集每列图像对
                            HTuple end_val75 = hv_CNum;
                            HTuple step_val75 = 1;
                            for (HTuple hv_C = 1; hv_C.Continue(end_val75, step_val75); hv_C = hv_C.TupleAdd(step_val75))
                            {
                                hv_From = hv_From.TupleConcat((hv_Fy + hv_C) - 1);
                                hv_To = hv_To.TupleConcat((hv_Ty + hv_C) - 1);
                            }
                        }
                        break;
                }
                return 0;
            }
            catch (HalconException hEx)
            {
                strErrMsg = "定义图像对时出错：" + hEx;
                return -1;
            }
        }

        #endregion


        #region 找圆工具相关

        /***功能：通过“侦测点拟合”法———实现的找圆(圆弧)工具***
         * 矩形测量句柄中心在圆弧测量轴线上且垂直圆弧线;
         * 规定测量句柄分布方向由圆弧起点逆时针指向终点;
         * 测量句柄侦测方法由参数决定("由外到内":(指向圆心)，"由内到外"(背向圆心))
           输入参数：
           * 参1：输入图像，
           * 参2：是否“锚定”。即：true:根据输入前(F)和后(Q)状态生成仿射变换对检测ROI进行锚定
           * 参3、4、5、6、7、8:生成对检测ROI进行锚定的仿射变换的前(F)和后(Q)状态(位置和旋转弧度角)
           * 参9、10、11、12、13: 输入圆弧的圆心、半径、起止角(弧度)
           * 参14: 侦测方向："由外到内":(指向圆心)，"由内到外"(背向圆心)
           * 参15：表示侦测方向和分布方向的箭头大小，建议值10
           * 参16：句柄数量(卡尺数量)，建议值20
           * 参17：光滑值(光滑滤波)，建议值1.0
           * 参18：边缘振幅阈值(边缘阈值)，建议值35
           * 参19：每个小矩形测量句柄的(边缘宽)半高(卡尺高度)，建议值5
           * 参20：每个小矩形测量句柄的(测量ROI长)半宽(卡尺宽度)，建议值20
           * 参21：明暗方向(边缘极性)('all'任意、'negative'白到黑、'positive'黑到白)，
           * 参22：边缘选取类型(搜索模式)('all'边缘最强、'first'第一边缘、'last'最后边缘)
           输出参数：
           * 参23：输出圆弧轴线轮廓箭头(箭头方向代表分布方向)和小矩形句柄测量方向箭头(由圆心和圆弧中点连线表示)
           * 参24：输出每个小矩形测量句柄对应的矩形,集合
           * 参25：输出每个小矩形测量句柄抓取的边缘点(叉号),集合
         输入参数：
         * 参26: 拟合采用的算法：'ahuber', 'algebraic', 'atukey', 'geohuber', 'geometric', 'geotukey';建议值： "algebraic";
         * 参27: 参与拟合的最多点数(>3,-1表所有点都参与)；建议值-1
         * 参28：等高线端点之间被认为是“闭合”的最大距离(区分圆或圆弧)>=0，建议值0
         * 参29：拟合时忽略等高线起始点和结束点的个数>=0，建议值0
         * 参30：鲁棒加权拟合的最大迭代次数>0,建议值3
         * 参31：消除异常值(离群值)的裁剪因子>0建议值2.0(典型的:Huber是1.0,Tukey是2.0)值越小忽略的离群值越多；建议值2.0
         *  对于“huber”和“tukey”，使用稳健的误差统计量来估计与轮廓点之间的距离的标准差，而不需要离群值。
            参数ClippingFactor(标准偏差的比例因子)控制异常值的数量:为ClippingFactor选择的值越小，检测到的异常值就越多。
            在Tukey算法中，异常值被删除，而在Huber算法中，异常值只被阻尼，或者更精确地说，它们被线性加权。
            在不进行稳健加权的情况下，将距离的平方和作为误差值进行优化，即采用最小二乘法。在实践中，建议采用“Tukey”方法。
         输出参数：
         * 参32/33/34/35/36：输出圆心、半径、起始角(弧度)、终止角(弧度)
         * 参37：输出圆弧边界点的排序方式：('negative'反向的-顺时针, 'positive'正向的-逆时针)
         * 参38：是输出整圆还是输出拟合真实圆弧，true：表输出真实圆弧，false:始终输出整圆
         * 参39：输出带圆心(叉号)的圆或圆弧轮廓
         * 参40：返回错误消息
         * 返回值： 成功返回0、失败返回-1
         最近更改日期:2019-12-2
        ************************************************/
        public int FindArcCircleToolByPoints(HObject ho_Image,
            //(位置和角度)获取一个"二维齐次单位变换矩阵",用于锚定检测ROI(自动跟随)
            bool bAutoFollow, HTuple hv_FRefRow, HTuple hv_FRefCol, HTuple hv_FRefAngle,
            HTuple hv_QRefRow, HTuple hv_QRefCol, HTuple hv_QRefAngle,
            //圆弧ROI上获取mark点
            HTuple hv_CirROIRow, HTuple hv_CirROICol, HTuple hv_CirROIR, HTuple hv_CirROIStartAngle, HTuple hv_CirROIEndAngle,
            HTuple hv_StrFindOrien, HTuple hv_OrienArrowSize, HTuple hv_MeaRecNum, HTuple hv_MeaRecSigma, HTuple hv_MeaRecThreshold,
            HTuple hv_MeaRecHalfH, HTuple hv_MeaRecHalfW, HTuple hv_MeaRecTransition, HTuple hv_MeaRecSelect,
            out HObject ho_MeaCirOrienArrow, out HObject ho_AllMinMeaRec, out HObject ho_AllMeaMarkCross,
            //mark点拟合圆
            HTuple hv_Algorithm, HTuple hv_MaxNumPoints, HTuple hv_MaxClosureDist,
            HTuple hv_ClippingEndPoints, HTuple hv_Iterations, HTuple hv_ClippingFactor,
            out HTuple hv_FitCirRow, out HTuple hv_FitCirCol, out HTuple hv_FitCirR,
            out HTuple hv_FitCirStartPhi, out HTuple hv_FitCirEndPhi, out HTuple hv_FitCirPointOrder,
            bool bOutTrueArcOrCircle, out HObject ho_FitArcOrCirXld, ref string strErrMsg)
        {
            strErrMsg = "";
            HOperatorSet.GenEmptyObj(out ho_MeaCirOrienArrow);
            HOperatorSet.GenEmptyObj(out ho_AllMinMeaRec);
            HOperatorSet.GenEmptyObj(out ho_AllMeaMarkCross);
            HOperatorSet.GenEmptyObj(out ho_FitArcOrCirXld);

            hv_FitCirRow = new HTuple();
            hv_FitCirCol = new HTuple();
            hv_FitCirR = new HTuple();
            hv_FitCirStartPhi = new HTuple();
            hv_FitCirEndPhi = new HTuple();
            hv_FitCirPointOrder = new HTuple();

            try
            {
                HTuple hv_HomMat2D = new HTuple();
                HTuple hv_NewCirROIRow = new HTuple();
                HTuple hv_NewCirROICol = new HTuple();
                HTuple hv_NewCirROIR = new HTuple();
                HTuple hv_NewCirROIStartAngle = new HTuple();
                HTuple hv_NewCirROIEndAngle = new HTuple();


                HTuple[] hv_AllMeaMarkR = null;
                HTuple[] hv_AllMeaMarkC = null;
                HTuple[] hv_AllMeaMarkAmp = null;
                HTuple[] hv_AllMeaMarkDist = null;
                HTuple hv_AllFitRow = new HTuple();
                HTuple hv_AllFitCol = new HTuple();

                try
                {
                    if (bAutoFollow)
                    {
                        //1、获取一个"二维齐次单位变换矩阵"(平移、旋转、缩放(暂不支持)、映射(暂不支持))
                        //比如：可以是形状模板匹配的模板点和匹配点之间的仿射变换
                        int runResult1 = GenHomMat2d(hv_FRefRow, hv_FRefCol, hv_FRefAngle,
                            hv_QRefRow, hv_QRefCol, hv_QRefAngle, out hv_HomMat2D, ref strErrMsg);
                        if (runResult1 == 0)//成功
                        {
                            //2、对圆弧(整体对象或关键点仿射变换)自动跟随定位点
                            int runResult2 = AffineTransCircleSectorByHomMat2D(hv_HomMat2D, hv_CirROIRow, hv_CirROICol, hv_CirROIR, hv_CirROIStartAngle, hv_CirROIEndAngle,
                             out hv_NewCirROIRow, out hv_NewCirROICol, out hv_NewCirROIR, out hv_NewCirROIStartAngle, out hv_NewCirROIEndAngle, ref strErrMsg);
                            if (runResult1 != 0)//不成功
                            {
                                strErrMsg = "圆弧检测ROI自动跟随出错：" + strErrMsg;
                                return -1;
                            }
                        }
                        else
                        {
                            strErrMsg = "获取一个仿射变换矩阵出错：" + strErrMsg;
                            return -1;
                        }

                    }
                    else
                    {
                        hv_NewCirROIRow = hv_CirROIRow;
                        hv_NewCirROICol = hv_CirROICol;
                        hv_NewCirROIR = hv_CirROIR;
                        hv_NewCirROIStartAngle = hv_CirROIStartAngle;
                        hv_NewCirROIEndAngle = hv_CirROIEndAngle;
                    }


                    //3、找圆上的mark点
                    ho_MeaCirOrienArrow.Dispose(); ho_AllMinMeaRec.Dispose(); ho_AllMeaMarkCross.Dispose();
                    int runResult3 = FindFitArcCircleMarkToolByCircleSectorOfRec2MeasurePos(ho_Image,
                        hv_NewCirROIRow, hv_NewCirROICol, hv_NewCirROIR, hv_NewCirROIStartAngle, hv_NewCirROIEndAngle,
                        hv_StrFindOrien, hv_OrienArrowSize, hv_MeaRecNum, hv_MeaRecSigma, hv_MeaRecThreshold,
                        hv_MeaRecHalfH, hv_MeaRecHalfW, hv_MeaRecTransition, hv_MeaRecSelect,
                        out ho_MeaCirOrienArrow, out ho_AllMinMeaRec, out ho_AllMeaMarkCross,
                        out hv_AllMeaMarkR, out hv_AllMeaMarkC, out hv_AllMeaMarkAmp, out hv_AllMeaMarkDist, ref strErrMsg);
                    if (runResult3 == 0)//成功
                    {
                        //4、收集mark点坐标
                        //C#中GetLength(0)表获取数组第一维所有元素数，Length表获取所有维度所有元素总数
                        //C#中Length获得的是数组元素总数，比如string[] str = new string[5],
                        //即使每个元素为null或[]Length结果仍为5,这点与halcon中TupleLength()和Length不同
                        //hv_AllMeaMarkDist.Length=测量句柄数
                        for (int index = 0; index < hv_AllMeaMarkR.Length; index++)
                        {
                            //hv_AllFitRow = hv_AllFitRow.TupleConcat(hv_AllMeaMarkR[index]);
                            //或
                            HOperatorSet.TupleConcat(hv_AllFitRow, hv_AllMeaMarkR[index], out hv_AllFitRow);
                            HOperatorSet.TupleConcat(hv_AllFitCol, hv_AllMeaMarkC[index], out hv_AllFitCol);
                        }

                        //5、点拟合圆
                        ho_FitArcOrCirXld.Dispose();
                        int runResult5 = FitArcCircleByPoints(hv_AllFitRow, hv_AllFitCol,
                            hv_Algorithm, hv_MaxNumPoints, hv_MaxClosureDist,
                            hv_ClippingEndPoints, hv_Iterations, hv_ClippingFactor,
                            out hv_FitCirRow, out hv_FitCirCol, out hv_FitCirR,
                            out hv_FitCirStartPhi, out hv_FitCirEndPhi, out hv_FitCirPointOrder,
                            bOutTrueArcOrCircle, out ho_FitArcOrCirXld, ref strErrMsg);
                        if (runResult5 == 0)//成功
                        {
                        }
                        else
                        {
                            strErrMsg = "Mark点拟合圆出错：" + strErrMsg;
                            return -1;
                        }
                    }
                    else
                    {
                        strErrMsg = "侦测Mark点出错：" + strErrMsg;
                        return -1;
                    }



                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    //出错
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                //出错
                return -1;
            }
            finally
            {

            }
            return 0;
        }

        //1.画“圆弧”或“圆”检测ROI相关

        /***功能：在窗口图片上画一个类型为非阻塞“圆弧”或“圆”的绘图对象并设置默认颜色***
             输入参数：
             * 参1：输入图像
             * 参2：输入窗口句柄
             * 参3：是画圆弧还是画圆对象，ture:圆弧，false：圆
             * 参4、5、6、7、8: 输入初始圆弧的圆心、半径、起止角(弧度)
             * 参9: 输出所画的圆弧绘图对象的句柄ID
             * 参10：返回错误消息
             * 返回值： 成功返回0、失败返回-1
             最近更改日期:2019-12-7
        ************************************************/
        public int CreateDrawObjCircleSector(HObject ho_Image, HTuple hv_WindowHandle, bool bDrawArcOrCir,
            HTuple hv_CirROIRow, HTuple hv_CirROICol, HTuple hv_CirROIR, HTuple hv_CirROIStartAngle, HTuple hv_CirROIEndAngle,
            out HTuple hv_DrawCirID, ref string strErrMsg)
        {
            strErrMsg = "";
            hv_DrawCirID = new HTuple();
            try
            {
                if (!HObjectValided(ho_Image, ref strErrMsg))
                {
                    strErrMsg = "输入图像无效："+ strErrMsg;
                    return -1;
                }
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }
                try
                {
                    //创建一个类型为“圆弧”或“圆”的绘图对象并设置其颜色
                    if (bDrawArcOrCir)
                    {
                        HOperatorSet.CreateDrawingObjectCircleSector(hv_CirROIRow, hv_CirROICol, hv_CirROIR,
                     hv_CirROIStartAngle, hv_CirROIEndAngle, out hv_DrawCirID);
                    }
                    else
                    {
                        HOperatorSet.CreateDrawingObjectCircle(hv_CirROIRow, hv_CirROICol, hv_CirROIR, out hv_DrawCirID);
                    }
                    HOperatorSet.SetDrawingObjectParams(hv_DrawCirID, "color", "cyan");
                    //将绘图对象和图像附加到窗口显示
                    HOperatorSet.AttachDrawingObjectToWindow(hv_WindowHandle, hv_DrawCirID);
                    HOperatorSet.AttachBackgroundToWindow(ho_Image, hv_WindowHandle);
                    //set_display_font(hv_WindowHandle, 14, "mono", "false", "false");//这里面不能设置字体大小，否则对其他算法有影响
                    //disp_message(hv_WindowHandle, "提示:鼠标调节圆弧位置和大小", "window", 5, 5, "red", "false");//可以打印消息

                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    //出错
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                //出错
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /***功能：获取调整后的圆弧或圆绘图对象所画的圆、圆弧ROI参数******
           输入参数：
           * 参1：输入圆弧绘图对象ID
           * 参2：是画圆弧还是画圆对象，ture:圆弧，false：圆
           * 参3、4、5、6、7: 输出调整后圆弧ROI的圆心、半径、起止角(弧度),
           * 如果画的是圆，则输出圆的起始角=rad(0),终止角=rad(360)
           * 参8: 输出圆弧绘图对象调整后所画的xld圆弧/圆ROI轮廓
           * 参9：返回错误消息
           * 返回值： 成功返回0、失败返回-1
           最近更改日期:2019-12-7
        ************************************************/
        public int GetAdjustDrawObjCircleSectorParams(HTuple hv_DrawCirID, bool bDrawArcOrCir, out HTuple hv_CirROIRow, out HTuple hv_CirROICol,
            out HTuple hv_CirROIR, out HTuple hv_CirROIStartAngle, out HTuple hv_CirROIEndAngle, out HObject ho_CirROIXld, ref string strErrMsg)
        {
            strErrMsg = "";
            hv_CirROIRow = new HTuple();
            hv_CirROICol = new HTuple();
            hv_CirROIR = new HTuple();
            hv_CirROIStartAngle = new HTuple();
            hv_CirROIEndAngle = new HTuple();
            HOperatorSet.GenEmptyObj(out ho_CirROIXld);

            HTuple hv_ParamValues = new HTuple();

            try
            {
                try
                {
                    //获取调整后的圆弧参数
                    if (bDrawArcOrCir)
                    {
                        HOperatorSet.GetDrawingObjectParams(hv_DrawCirID, ((((new HTuple("row")).TupleConcat(
                  "column")).TupleConcat("radius")).TupleConcat("start_angle")).TupleConcat("end_angle"), out hv_ParamValues);
                    }
                    else
                    {
                        HOperatorSet.GetDrawingObjectParams(hv_DrawCirID, ((new HTuple("row")).TupleConcat(
                            "column")).TupleConcat("radius"), out hv_ParamValues);
                    }
                    //HOperatorSet.GetDrawingObjectIconic(out ho_CirObj, hv_DrawCirID);
                    if ((int)(new HTuple((new HTuple(hv_ParamValues.TupleLength())).TupleEqual(5))) != 0)
                    {
                        hv_CirROIRow = hv_ParamValues[0];
                        hv_CirROICol = hv_ParamValues[1];
                        hv_CirROIR = hv_ParamValues[2];
                        hv_CirROIStartAngle = hv_ParamValues[3];
                        hv_CirROIEndAngle = hv_ParamValues[4];
                        ho_CirROIXld.Dispose();
                        HOperatorSet.GenCircleContourXld(out ho_CirROIXld, hv_CirROIRow, hv_CirROICol, hv_CirROIR,
                            hv_CirROIStartAngle, hv_CirROIEndAngle, "positive", 1);
                    }
                    else if ((int)(new HTuple((new HTuple(hv_ParamValues.TupleLength())).TupleEqual(3))) != 0)
                    {
                        hv_CirROIRow = hv_ParamValues[0];
                        hv_CirROICol = hv_ParamValues[1];
                        hv_CirROIR = hv_ParamValues[2];
                        hv_CirROIStartAngle = 0;
                        hv_CirROIEndAngle = (new HTuple(360)).TupleRad();
                        ho_CirROIXld.Dispose();
                        HOperatorSet.GenCircleContourXld(out ho_CirROIXld, hv_CirROIRow, hv_CirROICol, hv_CirROIR,
                            hv_CirROIStartAngle, hv_CirROIEndAngle, "positive", 1);
                    }
                    else
                    {
                        strErrMsg = "获取圆参数为空!";
                        //出错
                        return -1;
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    //出错
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                //出错
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /***功能：清除“圆弧”或“圆”绘图对象ID******
           输入参数：
           * 参1：输入窗口句柄
           * 参2: 输入输出清空后的圆弧绘图对象ID
           * 参3：返回错误消息
           * 返回值： 成功返回0、失败返回-1
           最近更改日期:2019-12-2
        ************************************************/
        public int ClearDrawObjCircleSector(HTuple hv_WindowHandle, ref HTuple hv_DrawCirID, ref string strErrMsg)
        {
            strErrMsg = "";
            try
            {
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }
                try
                {
                    //分离绘图对象
                    HOperatorSet.DetachDrawingObjectFromWindow(hv_WindowHandle, hv_DrawCirID);
                    HOperatorSet.DetachBackgroundFromWindow(hv_WindowHandle);
                    //清除绘图对象
                    HOperatorSet.ClearDrawingObject(hv_DrawCirID);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    //出错
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                //出错
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /***功能：在窗口图片上画一个类型为阻塞“圆”***
             输入参数：
             * 参1：输入图像
             * 参2：输入窗口句柄
             * 参3：是画圆弧还是画圆对象，ture:圆弧，false：圆
             * 参4、5、6、7、8: 输入初始圆弧的圆心、半径、起止角(弧度)
             * 参9: 输出所画的圆弧绘图对象的句柄ID
             * 参10：返回错误消息
             * 返回值： 成功返回0、失败返回-1
             最近更改日期:2019-12-7
        ************************************************/
        public int DrawCircleMod(HTuple hv_WindowHandle, ref HTuple hv_CirROIRow, ref HTuple hv_CirROICol, ref HTuple hv_CirROIR,
            out HObject ho_CirROIXld, ref string strErrMsg)
        {
            strErrMsg = "";
            HOperatorSet.GenEmptyObj(out ho_CirROIXld);
            try
            {
                if (!HTupleValided(hv_WindowHandle, ref strErrMsg))
                {
                    strErrMsg = "输入窗口句柄无效：" + strErrMsg;
                    return -1;
                }
                try
                {
                    HOperatorSet.SetColor(hv_WindowHandle, "cyan");
                    HOperatorSet.DrawCircleMod(hv_WindowHandle, hv_CirROIRow, hv_CirROICol, hv_CirROIR,
                            out hv_CirROIRow, out hv_CirROICol, out hv_CirROIR);
                    //HOperatorSet.DrawCircle(hv_WindowHandle, out hv_CirRow1, out hv_CirCol1, out hv_CirR1);
                    ho_CirROIXld.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_CirROIXld, hv_CirROIRow, hv_CirROICol, hv_CirROIR,
                        0, (new HTuple(360)).TupleRad(), "positive", 1);
                    HOperatorSet.DispObj(ho_CirROIXld, hv_WindowHandle);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    //出错
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                //出错
                return -1;
            }
            finally
            {
            }
            return 0;
        }


        //2.圆弧检测ROI仿射变换相关(模板自动跟随)

        /***功能：生成一个"二维齐次单位变换矩阵"(平移、旋转、缩放(暂不支持)、映射(暂不支持))***
           * 比如：可以是形状模板匹配的模板点和匹配点之间的仿射变换
           * 参1/2/3：变换前的位置和角度
           * 参4/5/6：变换后的位置和角度
           * 参7: 输出变换矩阵
           * 参8：返回错误消息
           * 返回值： 成功返回0、失败返回-1
           最近更改日期:2019-12-2
        ************************************************/
        public int GenHomMat2d(HTuple hv_FRefRow, HTuple hv_FRefCol, HTuple hv_FRefAngle,
            HTuple hv_QRefRow, HTuple hv_QRefCol, HTuple hv_QRefAngle, out HTuple hv_HomMat2D, ref string strErrMsg)
        {
            strErrMsg = "";
            hv_HomMat2D = new HTuple();
            try
            {
                try
                {
                    //定义二维齐次单位变换矩阵
                    HOperatorSet.HomMat2dIdentity(out hv_HomMat2D);

                    //添加平移(参2/3：行列平移增量
                    HOperatorSet.HomMat2dTranslate(hv_HomMat2D, (hv_QRefRow - hv_FRefRow), (hv_QRefCol - hv_FRefCol), out hv_HomMat2D);

                    //添加旋转(参2要旋转多少角度，参3/4旋转参考基准点)
                    HOperatorSet.HomMat2dRotate(hv_HomMat2D, (hv_QRefAngle - hv_FRefAngle), hv_QRefRow, hv_QRefCol, out hv_HomMat2D);

                    ////添加放缩(参2/3行列放缩系数，参4/5放缩参考基准点)
                    //HOperatorSet.HomMat2dScale(hv_HomMat2D, hv_ScaleRow, hv_ScaleCol, hv_QRefRow, hv_QRefCol, out hv_HomMat2D);

                    ////添加反射变换(镜像,即关于(ReflectRow, ReflectCol）,(ReflectRowQ, ReflectColQ)决定的对称轴对称)
                    //HOperatorSet.HomMat2dReflect(hv_HomMat2D, hv_ReflectRowP, hv_ReflectColP, hv_ReflectRowQ, hv_ReflectColQ, out hv_HomMat2D);
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    //出错
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                //出错
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        /***功能：对圆弧(整体对象或关键点仿射变换)自动跟随定位点(基于"相同二维变换的齐次变换矩阵")***
           * 参1：输入二维变换矩阵(可平移、可以旋转、也可以缩放)
           * 参2、3、4、5、6: 输出初始圆弧的圆心、半径、起止角(弧度)
           * 参7、8、9、10、11: 输出变换后圆弧的圆心、半径、起止角(弧度)
           * 参12：返回错误消息
           * 返回值： 成功返回0、失败返回-1
           最近更改日期:2019-12-2
        ************************************************/
        public int AffineTransCircleSectorByHomMat2D(HTuple hv_HomMat2D, HTuple hv_CirROIRow, HTuple hv_CirROICol, HTuple hv_CirROIR, HTuple hv_CirROIStartAngle, HTuple hv_CirROIEndAngle,
            out HTuple hv_NewCirROIRow, out HTuple hv_NewCirROICol, out HTuple hv_NewCirROIR, out HTuple hv_NewCirROIStartAngle, out HTuple hv_NewCirROIEndAngle, ref string strErrMsg)
        {
            strErrMsg = "";
            hv_NewCirROIRow = new HTuple();
            hv_NewCirROICol = new HTuple();
            hv_NewCirROIR = new HTuple();
            hv_NewCirROIStartAngle = new HTuple();
            hv_NewCirROIEndAngle = new HTuple();
            try
            {
                try
                {

                    //**(比如扇形检测区域)检测区域的中心点(关键点)仿射变换,后重绘制检测区域

                    //起始点
                    HTuple hv_OldCirStartR = hv_CirROIRow - (hv_CirROIR * (hv_CirROIStartAngle.TupleSin()));
                    HTuple hv_OldCirStartC = hv_CirROICol + (hv_CirROIR * (hv_CirROIStartAngle.TupleCos()));
                    HTuple hv_NewCirStartR = new HTuple();
                    HTuple hv_NewCirStartC = new HTuple();
                    //终止点
                    HTuple hv_OldCirEndR = hv_CirROIRow - (hv_CirROIR * (hv_CirROIEndAngle.TupleSin()));
                    HTuple hv_OldCirEndC = hv_CirROICol + (hv_CirROIR * (hv_CirROIEndAngle.TupleCos()));
                    HTuple hv_NewCirEndR = new HTuple();
                    HTuple hv_NewCirEndC = new HTuple();

                    HOperatorSet.AffineTransPixel(hv_HomMat2D, hv_CirROIRow, hv_CirROICol, out hv_NewCirROIRow, out hv_NewCirROICol);
                    HOperatorSet.AffineTransPixel(hv_HomMat2D, hv_OldCirStartR, hv_OldCirStartC, out hv_NewCirStartR, out hv_NewCirStartC);
                    HOperatorSet.AffineTransPixel(hv_HomMat2D, hv_OldCirEndR, hv_OldCirEndC, out hv_NewCirEndR, out hv_NewCirEndC);
                    //新半径
                    HOperatorSet.DistancePp(hv_NewCirROIRow, hv_NewCirROICol, hv_NewCirStartR, hv_NewCirStartC, out hv_NewCirROIR);
                    //**半径，所以新的起始角为：
                    if ((int)(new HTuple(hv_NewCirStartR.TupleGreater(hv_NewCirROIRow))) != 0)
                    {
                        //如果在图像坐标系的3/4象限
                        if ((int)(new HTuple(hv_NewCirStartC.TupleGreater(hv_NewCirROICol))) != 0)
                        {
                            //如果在图像坐标系的4象限
                            hv_NewCirROIStartAngle = ((new HTuple(360)).TupleRad()) - (((((hv_NewCirStartC - hv_NewCirROICol) * 1.0) / hv_CirROIR)).TupleAcos()
                                );
                        }
                        else if ((int)(new HTuple(hv_NewCirStartC.TupleLess(hv_NewCirROICol))) != 0)
                        {
                            //如果在图像坐标系的3象限
                            hv_NewCirROIStartAngle = ((new HTuple(180)).TupleRad()) + (((((hv_NewCirROICol - hv_NewCirStartC) * 1.0) / hv_CirROIR)).TupleAcos()
                                );
                        }
                        else
                        {
                            //如果在图像坐标系的y正轴上(向下)
                            hv_NewCirROIStartAngle = (new HTuple(270)).TupleRad();
                        }
                    }
                    else if ((int)(new HTuple(hv_NewCirStartR.TupleLess(hv_NewCirROIRow))) != 0)
                    {
                        //如果在图像坐标系的1/2象限
                        if ((int)(new HTuple(hv_NewCirStartC.TupleGreater(hv_NewCirROICol))) != 0)
                        {
                            //如果在图像坐标系的1象限
                            hv_NewCirROIStartAngle = ((((hv_NewCirStartC - hv_NewCirROICol) * 1.0) / hv_CirROIR)).TupleAcos()
                                ;
                        }
                        else if ((int)(new HTuple(hv_NewCirStartC.TupleLess(hv_NewCirROICol))) != 0)
                        {
                            //如果在图像坐标系的2象限
                            hv_NewCirROIStartAngle = ((new HTuple(180)).TupleRad()) - (((((hv_NewCirROICol - hv_NewCirStartC) * 1.0) / hv_CirROIR)).TupleAcos()
                                );
                        }
                        else
                        {
                            //如果在图像坐标系的y负轴上(向上)
                            hv_NewCirROIStartAngle = (new HTuple(90)).TupleRad();
                        }
                    }
                    else if ((int)(new HTuple(hv_NewCirStartR.TupleEqual(hv_NewCirROIRow))) != 0)
                    {
                        //如果在图像坐标系的x轴上
                        if ((int)(new HTuple(hv_NewCirStartC.TupleGreater(hv_NewCirROICol))) != 0)
                        {
                            //如果在图像坐标系的x正轴上(向右)
                            hv_NewCirROIStartAngle = (new HTuple(0)).TupleRad();
                        }
                        else if ((int)(new HTuple(hv_NewCirStartC.TupleLess(hv_NewCirROICol))) != 0)
                        {
                            //如果在图像坐标系的x负轴上(向左)
                            hv_NewCirROIStartAngle = (new HTuple(180)).TupleRad();
                        }
                        else
                        {
                            //如果在图像坐标系原点(即起始点和圆心重合)
                            hv_NewCirROIStartAngle = (new HTuple(0)).TupleRad();
                        }
                    }

                    //**所以新的终止角为：
                    if ((int)(new HTuple(hv_NewCirEndR.TupleGreater(hv_NewCirROIRow))) != 0)
                    {
                        //如果在图像坐标系的3/4象限
                        if ((int)(new HTuple(hv_NewCirEndC.TupleGreater(hv_NewCirROICol))) != 0)
                        {
                            //如果在图像坐标系的4象限
                            hv_NewCirROIEndAngle = ((new HTuple(360)).TupleRad()) - (((((hv_NewCirEndC - hv_NewCirROICol) * 1.0) / hv_CirROIR)).TupleAcos()
                                );
                        }
                        else if ((int)(new HTuple(hv_NewCirEndC.TupleLess(hv_NewCirROICol))) != 0)
                        {
                            //如果在图像坐标系的3象限
                            hv_NewCirROIEndAngle = ((new HTuple(180)).TupleRad()) + (((((hv_NewCirROICol - hv_NewCirEndC) * 1.0) / hv_CirROIR)).TupleAcos()
                                );
                        }
                        else
                        {
                            //如果在图像坐标系的y正轴上(向下)
                            hv_NewCirROIEndAngle = (new HTuple(270)).TupleRad();
                        }
                    }
                    else if ((int)(new HTuple(hv_NewCirEndR.TupleLess(hv_NewCirROIRow))) != 0)
                    {
                        //如果在图像坐标系的1/2象限
                        if ((int)(new HTuple(hv_NewCirEndC.TupleGreater(hv_NewCirROICol))) != 0)
                        {
                            //如果在图像坐标系的1象限
                            hv_NewCirROIEndAngle = ((((hv_NewCirEndC - hv_NewCirROICol) * 1.0) / hv_CirROIR)).TupleAcos()
                                ;
                        }
                        else if ((int)(new HTuple(hv_NewCirEndC.TupleLess(hv_NewCirROICol))) != 0)
                        {
                            //如果在图像坐标系的2象限
                            hv_NewCirROIEndAngle = ((new HTuple(180)).TupleRad()) - (((((hv_NewCirROICol - hv_NewCirEndC) * 1.0) / hv_CirROIR)).TupleAcos()
                                );
                        }
                        else
                        {
                            //如果在图像坐标系的y负轴上(向上)
                            hv_NewCirROIEndAngle = (new HTuple(90)).TupleRad();
                        }
                    }
                    else if ((int)(new HTuple(hv_NewCirEndR.TupleEqual(hv_NewCirROIRow))) != 0)
                    {
                        //如果在图像坐标系的x轴上
                        if ((int)(new HTuple(hv_NewCirEndC.TupleGreater(hv_NewCirROICol))) != 0)
                        {
                            //如果在图像坐标系的x正轴上(向右)
                            hv_NewCirROIEndAngle = (new HTuple(0)).TupleRad();
                        }
                        else if ((int)(new HTuple(hv_NewCirEndC.TupleLess(hv_NewCirROICol))) != 0)
                        {
                            //如果在图像坐标系的x负轴上(向左)
                            hv_NewCirROIEndAngle = (new HTuple(180)).TupleRad();
                        }
                        else
                        {
                            //如果在图像坐标系原点(即起始点和圆心重合)
                            hv_NewCirROIEndAngle = (new HTuple(0)).TupleRad();
                        }
                    }

                    //或者
                    //HOperatorSet.AffineTransPixel(hv_HomMat2D, hv_CirROIRow, hv_CirROICol,out hv_NewCirROIRow, out hv_NewCirROICol);
                    ////新半径为：
                    //hv_NewCirROIR = hv_CirROIR;
                    ////新起始角(弧度)
                    //hv_NewCirROIStartAngle = hv_CirROIStartAngle + hv_Angle;//为模板匹配得到的模板旋转角度
                    ////新终止角(弧度)
                    //hv_NewCirROIEndAngle = hv_CirROIEndAngle + hv_Angle; ;
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    //出错
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                //出错
                return -1;
            }
            finally
            {
            }
            return 0;
        }


        //3.通过“侦测点拟合”法实现的找圆(圆弧)工具相关

        /***功能：获取用来拟合圆弧或圆的Mark点，基于沿圆弧分布的(非边缘对)矩形测量句柄的抓边工具***
         * 矩形测量句柄中心在圆弧测量轴线上且垂直圆弧线;
         * 规定测量句柄分布方向由圆弧起点逆时针指向终点;
         * 测量句柄侦测方法由参数决定("由外到内":(指向圆心)，"由内到外"(背向圆心))
           输入参数：
           * 参1：输入图像
           * 参2、3、4、5、6: 输入圆弧的圆心、半径、起止角(弧度)
           * 参7: 侦测方向："由外到内":(指向圆心)，"由内到外"(背向圆心)
           * 参8：表示侦测方向和分布方向的箭头大小，建议值10
           * 参9：句柄数量(卡尺数量)，建议值20
           * 参10：光滑值(光滑滤波)，建议值1.0
           * 参11：边缘振幅阈值(边缘阈值)，建议值35
           * 参12：每个小矩形测量句柄的(边缘宽)半高(卡尺高度)，建议值5
           * 参13：每个小矩形测量句柄的(测量ROI长)半宽(卡尺宽度)，建议值20
           * 参14：明暗方向(边缘极性)('all'任意、'negative'白到黑、'positive'黑到白)，
           * 参15：边缘选取类型(搜索模式)('all'边缘最强、'first'第一边缘、'last'最后边缘)
           输出参数：
           * 参16：输出圆弧轴线轮廓箭头(箭头方向代表分布方向)和小矩形句柄测量方向箭头(由圆心和圆弧中点连线表示)
           * 参17：输出每个小矩形测量句柄对应的矩形,集合
           * 参18：输出每个小矩形测量句柄抓取的边缘点(叉号),集合
           * 参19：输出抓取的边缘点行坐标，集合(一维数组,索引：小矩形测量句柄ID)
           * 参20：输出抓取的边缘点列坐标，集合(一维数组,索引：小矩形测量句柄ID)
           * 参21：输出抓取的边缘点振幅值，集合(一维数组,索引：小矩形测量句柄ID)
           * 参22：输出抓取的边缘点之间距离段，集合(一维数组,索引：小矩形测量句柄ID)
           * 参23：返回错误消息
           * 返回值： 成功返回0、失败返回-1
           最近更改日期:2019-12-2
        ************************************************/
        public int FindFitArcCircleMarkToolByCircleSectorOfRec2MeasurePos(HObject ho_Image,
            HTuple hv_CirROIRow, HTuple hv_CirROICol, HTuple hv_CirROIR, HTuple hv_CirROIStartAngle, HTuple hv_CirROIEndAngle,
            HTuple hv_StrFindOrien, HTuple hv_OrienArrowSize, HTuple hv_MeaRecNum, HTuple hv_MeaRecSigma, HTuple hv_MeaRecThreshold,
            HTuple hv_MeaRecHalfH, HTuple hv_MeaRecHalfW, HTuple hv_MeaRecTransition, HTuple hv_MeaRecSelect,
            out HObject ho_MeaCirOrienArrow, out HObject ho_AllMinMeaRec, out HObject ho_AllMeaMarkCross,
            out HTuple[] hv_AllMeaMarkR, out HTuple[] hv_AllMeaMarkC, out HTuple[] hv_AllMeaMarkAmp, out HTuple[] hv_AllMeaMarkDist, ref string strErrMsg)
        {
            strErrMsg = "";
            HObject ho_MeaCirOrienArrow1 = null;
            HOperatorSet.GenEmptyObj(out ho_MeaCirOrienArrow1);
            HObject ho_MeaCirOrienArrow2 = null;
            HOperatorSet.GenEmptyObj(out ho_MeaCirOrienArrow2);
            HObject ho_MinMeaRec = null;
            HOperatorSet.GenEmptyObj(out ho_MinMeaRec);
            HObject ho_MeaMarkCross = null;
            HOperatorSet.GenEmptyObj(out ho_MeaMarkCross);

            //输出
            HOperatorSet.GenEmptyObj(out ho_MeaCirOrienArrow);
            HOperatorSet.GenEmptyObj(out ho_AllMinMeaRec);
            HOperatorSet.GenEmptyObj(out ho_AllMeaMarkCross);

            //输出数值结果,集合(二维数组,第一个元素代表测量句柄索引,halcon无法定义二维数组)
            hv_AllMeaMarkR = new HTuple[hv_MeaRecNum];//每个为null,不是[],tuple变量每个元素就是一个一维数组
            hv_AllMeaMarkC = new HTuple[hv_MeaRecNum];
            hv_AllMeaMarkAmp = new HTuple[hv_MeaRecNum];
            hv_AllMeaMarkDist = new HTuple[hv_MeaRecNum];
            //将每个为null,初始化为[]对象
            for (int index = 0; index < hv_MeaRecNum.I; index++)
            {
                hv_AllMeaMarkR[index] = new HTuple();
                hv_AllMeaMarkC[index] = new HTuple();
                hv_AllMeaMarkAmp[index] = new HTuple();
                hv_AllMeaMarkDist[index] = new HTuple();
            }


            try
            {
                if (!HObjectValided(ho_Image, ref strErrMsg))
                {
                    strErrMsg = "输入图像无效："+ strErrMsg;
                    return -1;
                }

                try
                {

                    //**(1)求输入图像宽、高
                    HTuple hv_Width = new HTuple();
                    HTuple hv_Height = new HTuple();
                    HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
                    //**(2)生成：圆弧轴线轮廓箭头(箭头方向代表分布方向)和小矩形句柄测量方向箭头(由圆心和圆弧中点连线表示)
                    ho_MeaCirOrienArrow.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_MeaCirOrienArrow, hv_CirROIRow, hv_CirROICol,
                        hv_CirROIR, hv_CirROIStartAngle, hv_CirROIEndAngle, "positive", 1);
                    //(由中心和半径及终止角)计算圆弧线终点坐标(在终点坐标点生成箭头)
                    HTuple hv_RStart1 = hv_CirROIRow - (hv_CirROIR * (((hv_CirROIEndAngle - 0.05)).TupleSin()));
                    HTuple hv_CStart1 = hv_CirROICol + (hv_CirROIR * (((hv_CirROIEndAngle - 0.05)).TupleCos()));
                    HTuple hv_REnd1 = hv_CirROIRow - (hv_CirROIR * (hv_CirROIEndAngle.TupleSin()));
                    HTuple hv_CEnd1 = hv_CirROICol + (hv_CirROIR * (hv_CirROIEndAngle.TupleCos()));
                    ho_MeaCirOrienArrow1.Dispose();
                    gen_arrow_contour_xld(out ho_MeaCirOrienArrow1, hv_RStart1, hv_CStart1, hv_REnd1,
                        hv_CEnd1, hv_OrienArrowSize, hv_OrienArrowSize);
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_MeaCirOrienArrow, ho_MeaCirOrienArrow1, out ExpTmpOutVar_0
                            );
                        ho_MeaCirOrienArrow.Dispose();
                        ho_MeaCirOrienArrow = ExpTmpOutVar_0;
                    }
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.UnionAdjacentContoursXld(ho_MeaCirOrienArrow, out ExpTmpOutVar_0,
                            20, 2, "attr_keep");
                        ho_MeaCirOrienArrow.Dispose();
                        ho_MeaCirOrienArrow = ExpTmpOutVar_0;
                    }
                    //生成每个MinMeaRec小矩形测量句柄测量方向箭头(由圆心和圆弧中点连线表示)
                    HTuple hv_RStart2 = new HTuple();
                    HTuple hv_CStart2 = new HTuple();
                    HTuple hv_REnd2 = new HTuple();
                    HTuple hv_CEnd2 = new HTuple();
                    if ((int)(new HTuple(hv_StrFindOrien.TupleEqual("0"))) != 0)// "0":由外到内
                    {
                        if ((int)(new HTuple(hv_CirROIEndAngle.TupleGreater(hv_CirROIStartAngle))) != 0)
                        {
                            hv_RStart2 = hv_CirROIRow - ((hv_CirROIR + 15) * ((((hv_CirROIStartAngle + hv_CirROIEndAngle) / 2.0)).TupleSin()
                                ));
                            hv_CStart2 = hv_CirROICol + ((hv_CirROIR + 15) * ((((hv_CirROIStartAngle + hv_CirROIEndAngle) / 2.0)).TupleCos()
                               ));
                        }
                        else
                        {
                            hv_RStart2 = hv_CirROIRow - ((hv_CirROIR + 15) * (((((((new HTuple(360)).TupleRad()
                               ) + hv_CirROIStartAngle) + hv_CirROIEndAngle) / 2.0)).TupleSin()));
                            hv_CStart2 = hv_CirROICol + ((hv_CirROIR + 15) * (((((((new HTuple(360)).TupleRad()
                               ) + hv_CirROIStartAngle) + hv_CirROIEndAngle) / 2.0)).TupleCos()));
                        }
                        hv_REnd2 = hv_CirROIRow.Clone();
                        hv_CEnd2 = hv_CirROICol.Clone();
                    }
                    else if ((int)(new HTuple(hv_StrFindOrien.TupleEqual("1"))) != 0)//"1":由内到外
                    {
                        hv_RStart2 = hv_CirROIRow.Clone();
                        hv_CStart2 = hv_CirROICol.Clone();
                        if ((int)(new HTuple(hv_CirROIEndAngle.TupleGreater(hv_CirROIStartAngle))) != 0)
                        {
                            hv_REnd2 = hv_CirROIRow - ((hv_CirROIR + 15) * ((((hv_CirROIStartAngle + hv_CirROIEndAngle) / 2.0)).TupleSin()
                                ));
                            hv_CEnd2 = hv_CirROICol + ((hv_CirROIR + 15) * ((((hv_CirROIStartAngle + hv_CirROIEndAngle) / 2.0)).TupleCos()
                                ));
                        }
                        else
                        {
                            hv_REnd2 = hv_CirROIRow - ((hv_CirROIR + 15) * (((((((new HTuple(360)).TupleRad()) + hv_CirROIStartAngle) + hv_CirROIEndAngle) / 2.0)).TupleSin()
                                ));
                            hv_CEnd2 = hv_CirROICol + ((hv_CirROIR + 15) * (((((((new HTuple(360)).TupleRad()) + hv_CirROIStartAngle) + hv_CirROIEndAngle) / 2.0)).TupleCos()
                                ));
                        }

                    }
                    ho_MeaCirOrienArrow2.Dispose();
                    gen_arrow_contour_xld(out ho_MeaCirOrienArrow2, hv_RStart2, hv_CStart2, hv_REnd2,
                        hv_CEnd2, hv_OrienArrowSize, hv_OrienArrowSize);
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_MeaCirOrienArrow, ho_MeaCirOrienArrow2, out ExpTmpOutVar_0
                            );
                        ho_MeaCirOrienArrow.Dispose();
                        ho_MeaCirOrienArrow = ExpTmpOutVar_0;
                    }

                    //**(3)成N个MinMeaRec小测量句柄，分布方向：圆弧起点指向终点，测量方向：StrFindOrien确定
                    HTuple end_val252 = hv_MeaRecNum - 1;
                    HTuple step_val252 = 1;
                    for (HTuple hv_MeaRecId = 0; hv_MeaRecId.Continue(end_val252, step_val252); hv_MeaRecId = hv_MeaRecId.TupleAdd(step_val252))
                    {
                        //**计算每个MinMeaRec小矩形测量句柄的中心位置和方向
                        //测量句柄间隔多少弧度
                        HTuple hv_StepAngle = 0.01;
                        if ((int)(new HTuple(hv_CirROIEndAngle.TupleGreater(hv_CirROIStartAngle))) != 0)
                        {
                            hv_StepAngle = (hv_CirROIEndAngle - hv_CirROIStartAngle) / hv_MeaRecNum;
                        }
                        else
                        {
                            hv_StepAngle = (((new HTuple(360)).TupleRad()) + (hv_CirROIEndAngle - hv_CirROIStartAngle)) / hv_MeaRecNum;
                        }
                        //小矩形测量句柄中心位置
                        HTuple hv_MinMeaRecCentR = hv_CirROIRow - (hv_CirROIR * ((((hv_CirROIStartAngle + (hv_StepAngle / 2.0)) + (hv_StepAngle * hv_MeaRecId))).TupleSin()
                            ));
                        HTuple hv_MinMeaRecCentC = hv_CirROICol + (hv_CirROIR * ((((hv_CirROIStartAngle + (hv_StepAngle / 2.0)) + (hv_StepAngle * hv_MeaRecId))).TupleCos()
                            ));
                        //小矩形测量句柄方向
                        HTuple hv_MinMeaRecPhi = 0;
                        if ((int)(new HTuple(hv_StrFindOrien.TupleEqual("0"))) != 0)// "0":由外到内
                        {
                            hv_MinMeaRecPhi = ((hv_CirROIStartAngle + (hv_StepAngle / 2.0)) + (hv_StepAngle * hv_MeaRecId)) + ((new HTuple(180)).TupleRad()
                                );
                        }
                        else if ((int)(new HTuple(hv_StrFindOrien.TupleEqual("1"))) != 0)// "1":由内到外
                        {
                            hv_MinMeaRecPhi = (hv_CirROIStartAngle + (hv_StepAngle / 2.0)) + (hv_StepAngle * hv_MeaRecId);
                        }
                        //每个MinMeaRec小测量句柄的半宽、半高
                        HTuple hv_MinMeaRecHalfH = hv_MeaRecHalfH.Clone();
                        HTuple hv_MinMeaRecHalfW = hv_MeaRecHalfW.Clone();
                        //**抓取边缘mark点
                        ho_MinMeaRec.Dispose();
                        HOperatorSet.GenRectangle2ContourXld(out ho_MinMeaRec, hv_MinMeaRecCentR,
                            hv_MinMeaRecCentC, hv_MinMeaRecPhi, hv_MinMeaRecHalfW, hv_MinMeaRecHalfH);
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ConcatObj(ho_AllMinMeaRec, ho_MinMeaRec, out ExpTmpOutVar_0);
                            ho_AllMinMeaRec.Dispose();
                            ho_AllMinMeaRec = ExpTmpOutVar_0;
                        }
                        HTuple hv_MeasureHandle = new HTuple();
                        HTuple hv_MinMeaRecMarkR = new HTuple();
                        HTuple hv_MinMeaRecMarkC = new HTuple();
                        HTuple hv_MinMeaRecMarkAmp = new HTuple();
                        HTuple hv_MinMeaRecMarkDist = new HTuple();
                        //插值算法：'nearest_neighbor'、'bilinear'、'bicubic'
                        HOperatorSet.GenMeasureRectangle2(hv_MinMeaRecCentR, hv_MinMeaRecCentC, hv_MinMeaRecPhi,
                            hv_MinMeaRecHalfW, hv_MinMeaRecHalfH, hv_Width, hv_Height, "nearest_neighbor",
                            out hv_MeasureHandle);
                        HOperatorSet.MeasurePos(ho_Image, hv_MeasureHandle, hv_MeaRecSigma, hv_MeaRecThreshold,
                            hv_MeaRecTransition, hv_MeaRecSelect, out hv_MinMeaRecMarkR, out hv_MinMeaRecMarkC,
                            out hv_MinMeaRecMarkAmp, out hv_MinMeaRecMarkDist);
                        HOperatorSet.CloseMeasure(hv_MeasureHandle);
                        //针对每一个测量句柄，生成并收集测得的Mark
                        ho_MeaMarkCross.Dispose();
                        HOperatorSet.GenCrossContourXld(out ho_MeaMarkCross, hv_MinMeaRecMarkR, hv_MinMeaRecMarkC,
                            6, 0.785398);
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ConcatObj(ho_AllMeaMarkCross, ho_MeaMarkCross, out ExpTmpOutVar_0
                                );
                            ho_AllMeaMarkCross.Dispose();
                            ho_AllMeaMarkCross = ExpTmpOutVar_0;
                        }
                        //记录结果到一维数组中(第一个元素代表测量句柄索引)


                        //*MinMeaRecMarkR, MinMeaRecMarkC, MinMeaRecMarkAmp,MinMeaRecMarkDist--->
                        //*AllMeaMarkR:=[],AllMeaMarkC:=[],AllMeaMarkAmp:=[],AllMeaMarkDist:=[]
                        HOperatorSet.TupleConcat(hv_AllMeaMarkR[hv_MeaRecId], hv_MinMeaRecMarkR, out hv_AllMeaMarkR[hv_MeaRecId]);
                        HOperatorSet.TupleConcat(hv_AllMeaMarkC[hv_MeaRecId], hv_MinMeaRecMarkC, out hv_AllMeaMarkC[hv_MeaRecId]);
                        HOperatorSet.TupleConcat(hv_AllMeaMarkAmp[hv_MeaRecId], hv_MinMeaRecMarkAmp, out hv_AllMeaMarkAmp[hv_MeaRecId]);
                        HOperatorSet.TupleConcat(hv_AllMeaMarkDist[hv_MeaRecId], hv_MinMeaRecMarkDist, out hv_AllMeaMarkDist[hv_MeaRecId]);

                        //或者
                        //hv_AllMeaMarkR[hv_MeaRecId] = hv_MinMeaRecMarkR;
                        //hv_AllMeaMarkC[hv_MeaRecId] = hv_MinMeaRecMarkC;
                        //hv_AllMeaMarkAmp[hv_MeaRecId] = hv_MinMeaRecMarkAmp;
                        //hv_AllMeaMarkDist[hv_MeaRecId] = hv_MinMeaRecMarkDist;
                    }

                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    //出错
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                //出错
                return -1;
            }
            finally
            {
                ho_MeaCirOrienArrow1.Dispose();
                ho_MeaCirOrienArrow2.Dispose();
                ho_MinMeaRec.Dispose();
                ho_MeaMarkCross.Dispose();
            }
            return 0;
        }

        /***功能：根据坐标点拟合圆(点生成轮廓线，轮廓线拟合圆)***
         输入参数：
         * 参1、2：所有参与拟合点的行列坐标
         * 参3: 拟合采用的算法：'ahuber', 'algebraic', 'atukey', 'geohuber', 'geometric', 'geotukey';建议值： "algebraic";
         * 参4: 参与拟合的最多点数(>3,-1表所有点都参与)；建议值-1
         * 参5：等高线端点之间被认为是“闭合”的最大距离(区分圆或圆弧)>=0，建议值0
         * 参6：拟合时忽略等高线起始点和结束点的个数>=0，建议值0
         * 参7：鲁棒加权拟合的最大迭代次数>0,建议值3
         * 参8：消除异常值(离群值)的裁剪因子>0建议值2.0(典型的:Huber是1.0,Tukey是2.0)值越小忽略的离群值越多；建议值2.0
         *  对于“huber”和“tukey”，使用稳健的误差统计量来估计与轮廓点之间的距离的标准差，而不需要离群值。
            参数ClippingFactor(标准偏差的比例因子)控制异常值的数量:为ClippingFactor选择的值越小，检测到的异常值就越多。
            在Tukey算法中，异常值被删除，而在Huber算法中，异常值只被阻尼，或者更精确地说，它们被线性加权。
            在不进行稳健加权的情况下，将距离的平方和作为误差值进行优化，即采用最小二乘法。在实践中，建议采用“Tukey”方法。
         输出参数：
         * 参9/10/11/12/13：输出圆心、半径、起始角(弧度)、终止角(弧度)
         * 参14：输出圆弧边界点的排序方式：('negative'反向的-顺时针, 'positive'正向的-逆时针)
         * 参15：是输出整圆还是输出拟合真实圆弧，true：表输出真实圆弧，false:始终输出整圆
         * 参16：输出带圆心(叉号)的圆或圆弧轮廓
         * 参17：返回错误消息
         * 返回值： 成功返回0、失败返回-1
         最近更改日期:2019-12-2
        ************************************************/
        public int FitArcCircleByPoints(HTuple hv_AllFitRow, HTuple hv_AllFitCol,
            HTuple hv_Algorithm, HTuple hv_MaxNumPoints, HTuple hv_MaxClosureDist,
            HTuple hv_ClippingEndPoints, HTuple hv_Iterations, HTuple hv_ClippingFactor,
            out HTuple hv_FitCirRow, out HTuple hv_FitCirCol, out HTuple hv_FitCirR,
            out HTuple hv_FitCirStartPhi, out HTuple hv_FitCirEndPhi, out HTuple hv_FitCirPointOrder,
            bool bOutTrueArcOrCircle, out HObject ho_FitArcOrCirXld, ref string strErrMsg)
        {
            strErrMsg = "";

            hv_FitCirRow = new HTuple();
            hv_FitCirCol = new HTuple();
            hv_FitCirR = new HTuple();
            hv_FitCirStartPhi = new HTuple();
            hv_FitCirEndPhi = new HTuple();
            hv_FitCirPointOrder = new HTuple();

            HObject ho_FitContour = null;
            HOperatorSet.GenEmptyObj(out ho_FitContour);
            HObject ho_FitCirCenterCross = null;
            HOperatorSet.GenEmptyObj(out ho_FitCirCenterCross);

            HOperatorSet.GenEmptyObj(out ho_FitArcOrCirXld);

            try
            {
                try
                {
                    if ((hv_AllFitRow.Length < 1) || (hv_AllFitCol.Length < 1))
                    {
                        strErrMsg = "参与拟合点不存在！";
                        //出错
                        return -1;
                    }

                    //点生成轮廓
                    ho_FitContour.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_FitContour, hv_AllFitRow, hv_AllFitCol);
                    //**轮廓拟合圆:参2：拟合采用的算法('ahuber', 'algebraic', 'atukey', 'geohuber', 'geometric', 'geotukey')
                    //参3：参与拟合的最多点数(>3,-1表所有点都参与);参4：等高线端点之间被认为是“闭合”的最大距离(区分圆或圆弧)>=0。
                    //参5：拟合时忽略等高线起始点和结束点的个数>=0。参6：鲁棒加权拟合的最大迭代次数>0,建议值3。
                    //参7：消除异常值(离群值)的裁剪因子>0建议值2.0(1.0, 1.5, 2.0, 2.5, 3.0)(典型的:Huber是1.0,Tukey是2.0)值越小忽略的离群值越多。
                    //对于“huber”和“tukey”，使用稳健的误差统计量来估计与轮廓点之间的距离的标准差，而不需要离群值。
                    //参数ClippingFactor(标准偏差的比例因子)控制异常值的数量:为ClippingFactor选择的值越小，检测到的异常值就越多。
                    //在Tukey算法中，异常值被删除，而在Huber算法中，异常值只被阻尼，或者更精确地说，它们被线性加权。
                    //在不进行稳健加权的情况下，将距离的平方和作为误差值进行优化，即采用最小二乘法。在实践中，建议采用“Tukey”方法。
                    //参8、9、10、11、12：输出圆心、半径、起始角(弧度)、终止角(弧度)
                    //参13：输出圆弧边界点的排序方式：('negative'反向的-顺时针, 'positive'正向的-逆时针)
                    HOperatorSet.FitCircleContourXld(ho_FitContour, hv_Algorithm, hv_MaxNumPoints,
                        hv_MaxClosureDist, hv_ClippingEndPoints, hv_Iterations, hv_ClippingFactor,
                        out hv_FitCirRow, out hv_FitCirCol, out hv_FitCirR, out hv_FitCirStartPhi,
                        out hv_FitCirEndPhi, out hv_FitCirPointOrder);
                    if ((int)(new HTuple((new HTuple(hv_FitCirRow.TupleLength())).TupleGreater(
                        0))) != 0)
                    {
                        if (bOutTrueArcOrCircle)//true:输出原始圆弧
                        {
                            ho_FitArcOrCirXld.Dispose();
                            HOperatorSet.GenCircleContourXld(out ho_FitArcOrCirXld, hv_FitCirRow, hv_FitCirCol,
                                hv_FitCirR, hv_FitCirStartPhi, hv_FitCirEndPhi, hv_FitCirPointOrder, 1);
                        }
                        else//false:圆弧修改成整圆输出
                        {
                            ho_FitArcOrCirXld.Dispose();
                            HOperatorSet.GenCircleContourXld(out ho_FitArcOrCirXld, hv_FitCirRow, hv_FitCirCol,
                                hv_FitCirR, (new HTuple(0)).TupleRad(), (new HTuple(360)).TupleRad(), hv_FitCirPointOrder, 1);
                        }

                        ho_FitCirCenterCross.Dispose();
                        HOperatorSet.GenCrossContourXld(out ho_FitCirCenterCross, hv_FitCirRow, hv_FitCirCol, 10, 0.785398);
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ConcatObj(ho_FitArcOrCirXld, ho_FitCirCenterCross, out ExpTmpOutVar_0);
                            ho_FitArcOrCirXld.Dispose();
                            ho_FitArcOrCirXld = ExpTmpOutVar_0;
                        }
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    //出错
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                //出错
                return -1;
            }
            finally
            {
                ho_FitContour.Dispose();
                ho_FitCirCenterCross.Dispose();
            }
            return 0;
        }

        #endregion



        #region 测量定位相关---暂存

        /*(取边缘对)制作环形测量工具之4---输入原始图像(可能有其他干扰)、测量目标为暗色或亮、不区分测量正反向、取边缘对。
         */
        public void measurePairs_brightDarkObj_Wholeimage(HObject ho_measObjImage, out HObject ho_measPathCir,
             out HObject ho_AllMeasureRec, out HObject ho_AllEdgeStart, out HObject ho_AllEdgeEnd,
             HTuple hv_measPathCirR, HTuple hv_measPathCirRow, HTuple hv_measPathCirCol,
             HTuple hv_measRecNum, HTuple hv_startPhi, HTuple hv_endPhi, HTuple hv_sigma,
             HTuple hv_minThresholdAmp, HTuple hv_transition, HTuple hv_ROI_W, HTuple hv_ROI_H,
             HTuple hv_transPixeMul, HTuple hv_ignoreMinDist, HTuple hv_ignoreMaxDist, out HTuple hv_HomeRow,
             out HTuple hv_HomeCol, out HTuple hv_EndRow, out HTuple hv_EndCol, out HTuple hv_AllDist,
             out HTuple hv_MaxDist, out HTuple hv_MinDist, out HTuple hv_MeanDist, out HTuple hv_MedianDist,
             out HTuple hv_DeviationDist)
        {

            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_measureRec = null, ho_EdgeStart = null;
            HObject ho_EdgeEnd = null;

            // Local control variables 

            HTuple hv_ObjWidth = null, hv_ObjHeight = null;
            HTuple hv_ID = null, hv_Angle = new HTuple(), hv_MeasureHandle = new HTuple();
            HTuple hv_RowEdgeFirst = new HTuple(), hv_ColumnEdgeFirst = new HTuple();
            HTuple hv_AmplitudeFirst = new HTuple(), hv_RowEdgeSecond = new HTuple();
            HTuple hv_ColumnEdgeSecond = new HTuple(), hv_AmplitudeSecond = new HTuple();
            HTuple hv_InDistance = new HTuple(), hv_OutDistance = new HTuple();
            HTuple hv_OneDist = new HTuple(), hv_RowStart = new HTuple();
            HTuple hv_ColStart = new HTuple(), hv_RowEnd = new HTuple();
            HTuple hv_ColEnd = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_measPathCir);
            HOperatorSet.GenEmptyObj(out ho_AllMeasureRec);
            HOperatorSet.GenEmptyObj(out ho_AllEdgeStart);
            HOperatorSet.GenEmptyObj(out ho_AllEdgeEnd);
            HOperatorSet.GenEmptyObj(out ho_measureRec);
            HOperatorSet.GenEmptyObj(out ho_EdgeStart);
            HOperatorSet.GenEmptyObj(out ho_EdgeEnd);
            hv_MaxDist = new HTuple();
            hv_MinDist = new HTuple();
            hv_MeanDist = new HTuple();
            hv_MedianDist = new HTuple();
            hv_DeviationDist = new HTuple();
            try
            {
                ho_AllMeasureRec.Dispose();
                HOperatorSet.GenEmptyObj(out ho_AllMeasureRec);
                ho_AllEdgeStart.Dispose();
                HOperatorSet.GenEmptyObj(out ho_AllEdgeStart);
                ho_AllEdgeEnd.Dispose();
                HOperatorSet.GenEmptyObj(out ho_AllEdgeEnd);
                hv_HomeRow = new HTuple();
                hv_HomeCol = new HTuple();
                hv_EndRow = new HTuple();
                hv_EndCol = new HTuple();
                hv_AllDist = new HTuple();

                //测量目标图像的宽和高
                HOperatorSet.GetImageSize(ho_measObjImage, out hv_ObjWidth, out hv_ObjHeight);
                //测量的"轨迹圆"
                ho_measPathCir.Dispose();
                HOperatorSet.GenCircleContourXld(out ho_measPathCir, hv_measPathCirRow, hv_measPathCirCol,
                    hv_measPathCirR, hv_startPhi.TupleRad(), hv_endPhi.TupleRad(), "positive",
                    1);
                //**画测量句柄
                HTuple end_val14 = hv_measRecNum - 1;
                HTuple step_val14 = 1;
                for (hv_ID = 0; hv_ID.Continue(end_val14, step_val14); hv_ID = hv_ID.TupleAdd(step_val14))
                {
                    hv_Angle = hv_startPhi + (((hv_endPhi - hv_startPhi) / hv_measRecNum) * hv_ID);
                    HOperatorSet.GenMeasureRectangle2(hv_measPathCirRow - (hv_measPathCirR * (((hv_Angle.TupleRad()
                        )).TupleSin())), hv_measPathCirCol + (hv_measPathCirR * (((hv_Angle.TupleRad()
                        )).TupleCos())), hv_Angle.TupleRad(), hv_ROI_W, hv_ROI_H, hv_ObjWidth,
                        hv_ObjHeight, "nearest_neighbor", out hv_MeasureHandle);
                    ho_measureRec.Dispose();
                    HOperatorSet.GenRectangle2ContourXld(out ho_measureRec, hv_measPathCirRow - (hv_measPathCirR * (((hv_Angle.TupleRad()
                        )).TupleSin())), hv_measPathCirCol + (hv_measPathCirR * (((hv_Angle.TupleRad()
                        )).TupleCos())), hv_Angle.TupleRad(), hv_ROI_W, hv_ROI_H);
                    HOperatorSet.MeasurePairs(ho_measObjImage, hv_MeasureHandle, hv_sigma, hv_minThresholdAmp,
                        hv_transition, "all", out hv_RowEdgeFirst, out hv_ColumnEdgeFirst, out hv_AmplitudeFirst,
                        out hv_RowEdgeSecond, out hv_ColumnEdgeSecond, out hv_AmplitudeSecond,
                        out hv_InDistance, out hv_OutDistance);
                    HOperatorSet.CloseMeasure(hv_MeasureHandle);
                    //得到全部测量矩形
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_AllMeasureRec, ho_measureRec, out ExpTmpOutVar_0
                            );
                        ho_AllMeasureRec.Dispose();
                        ho_AllMeasureRec = ExpTmpOutVar_0;
                    }
                    if ((int)(new HTuple((new HTuple(hv_InDistance.TupleLength())).TupleEqual(
                        1))) != 0)
                    {
                        hv_OneDist = hv_InDistance.TupleSum();
                    }
                    else
                    {
                        hv_OneDist = (hv_InDistance.TupleSum()) + (hv_OutDistance.TupleSum());
                    }
                    //转换为实际距离后取小数点后3位
                    hv_OneDist = hv_OneDist * hv_transPixeMul;
                    hv_OneDist = hv_OneDist.TupleString(".3f");
                    HOperatorSet.TupleNumber(hv_OneDist, out hv_OneDist);
                    //测量有效结果值及边缘线坐标和边缘线：如果值异常(超限),或为空，则舍弃
                    if ((int)((new HTuple((new HTuple((new HTuple(hv_OneDist.TupleLess(hv_ignoreMinDist))).TupleOr(
                        new HTuple(hv_OneDist.TupleGreater(hv_ignoreMaxDist))))).TupleOr(new HTuple((new HTuple(hv_InDistance.TupleLength()
                        )).TupleEqual(0))))).TupleNot()) != 0)
                    {

                        hv_AllDist = hv_AllDist.TupleConcat(hv_OneDist);

                        hv_HomeRow = hv_HomeRow.TupleConcat(hv_RowEdgeFirst.TupleSelect(0));
                        hv_HomeCol = hv_HomeCol.TupleConcat(hv_ColumnEdgeFirst.TupleSelect(0));

                        hv_EndRow = hv_EndRow.TupleConcat(hv_RowEdgeSecond.TupleSelect((new HTuple(hv_RowEdgeSecond.TupleLength()
                            )) - 1));
                        hv_EndCol = hv_EndCol.TupleConcat(hv_ColumnEdgeSecond.TupleSelect((new HTuple(hv_ColumnEdgeSecond.TupleLength()
                            )) - 1));

                        //画第一个边缘线
                        hv_RowStart = (hv_RowEdgeFirst.TupleSelect(0)) + (hv_ROI_H * (((((hv_Angle + 90)).TupleRad()
                            )).TupleSin()));
                        hv_ColStart = (hv_ColumnEdgeFirst.TupleSelect(0)) - (hv_ROI_H * (((((hv_Angle + 90)).TupleRad()
                            )).TupleCos()));
                        hv_RowEnd = (hv_RowEdgeFirst.TupleSelect(0)) - (hv_ROI_H * (((((hv_Angle + 90)).TupleRad()
                            )).TupleSin()));
                        hv_ColEnd = (hv_ColumnEdgeFirst.TupleSelect(0)) + (hv_ROI_H * (((((hv_Angle + 90)).TupleRad()
                            )).TupleCos()));
                        ho_EdgeStart.Dispose();
                        HOperatorSet.GenContourPolygonXld(out ho_EdgeStart, hv_RowStart.TupleConcat(
                            hv_RowEnd), hv_ColStart.TupleConcat(hv_ColEnd));
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ConcatObj(ho_AllEdgeStart, ho_EdgeStart, out ExpTmpOutVar_0
                                );
                            ho_AllEdgeStart.Dispose();
                            ho_AllEdgeStart = ExpTmpOutVar_0;
                        }
                        //画最后一个边缘线
                        hv_RowStart = (hv_RowEdgeSecond.TupleSelect((new HTuple(hv_RowEdgeSecond.TupleLength()
                            )) - 1)) + (hv_ROI_H * (((((hv_Angle + 90)).TupleRad())).TupleSin()));
                        hv_ColStart = (hv_ColumnEdgeSecond.TupleSelect((new HTuple(hv_ColumnEdgeSecond.TupleLength()
                            )) - 1)) - (hv_ROI_H * (((((hv_Angle + 90)).TupleRad())).TupleCos()));
                        hv_RowEnd = (hv_RowEdgeSecond.TupleSelect((new HTuple(hv_RowEdgeSecond.TupleLength()
                            )) - 1)) - (hv_ROI_H * (((((hv_Angle + 90)).TupleRad())).TupleSin()));
                        hv_ColEnd = (hv_ColumnEdgeSecond.TupleSelect((new HTuple(hv_ColumnEdgeSecond.TupleLength()
                            )) - 1)) + (hv_ROI_H * (((((hv_Angle + 90)).TupleRad())).TupleCos()));
                        ho_EdgeEnd.Dispose();
                        HOperatorSet.GenContourPolygonXld(out ho_EdgeEnd, hv_RowStart.TupleConcat(
                            hv_RowEnd), hv_ColStart.TupleConcat(hv_ColEnd));
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ConcatObj(ho_AllEdgeEnd, ho_EdgeEnd, out ExpTmpOutVar_0);
                            ho_AllEdgeEnd.Dispose();
                            ho_AllEdgeEnd = ExpTmpOutVar_0;
                        }
                    }
                }
                if ((int)(new HTuple((new HTuple(hv_AllDist.TupleLength())).TupleNotEqual(0))) != 0)
                {
                    //所有尺寸最大值：
                    HOperatorSet.TupleMax(hv_AllDist, out hv_MaxDist);
                    //最小值：
                    HOperatorSet.TupleMin(hv_AllDist, out hv_MinDist);
                    //平均值：
                    HOperatorSet.TupleMean(hv_AllDist, out hv_MeanDist);
                    //中值
                    HOperatorSet.TupleMedian(hv_AllDist, out hv_MedianDist);
                    //标准差：
                    HOperatorSet.TupleDeviation(hv_AllDist, out hv_DeviationDist);
                }
                else
                {
                    hv_MaxDist = new HTuple();
                    hv_MinDist = new HTuple();
                    hv_MeanDist = new HTuple();
                    hv_MedianDist = new HTuple();
                    hv_DeviationDist = new HTuple();

                }

                ho_measureRec.Dispose();
                ho_EdgeStart.Dispose();
                ho_EdgeEnd.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_measureRec.Dispose();
                ho_EdgeStart.Dispose();
                ho_EdgeEnd.Dispose();

                throw HDevExpDefaultException;
            }
        }

        /*(取边缘对)制作环形测量工具之3---测量时输入处理后目标图像(已经排除了其他干扰)、测量目标为暗色或亮、不区分测量正反向、取边缘对。
         */
        public void measurePairs_brightDarkObj_ROIimage(HObject ho_measObjImage, HObject ho_loopRegion,
            out HObject ho_ObjImage, out HObject ho_measPathCir, out HObject ho_AllMeasureRec,
            out HObject ho_AllEdgeStart, out HObject ho_AllEdgeEnd, HTuple hv_dilLoopR,
            HTuple hv_fillThreshold, HTuple hv_measPathCirR, HTuple hv_measPathCirRow, HTuple hv_measPathCirCol,
            HTuple hv_measRecNum, HTuple hv_startPhi, HTuple hv_endPhi, HTuple hv_sigma,
            HTuple hv_minThresholdAmp, HTuple hv_transition, HTuple hv_ROI_W, HTuple hv_ROI_H,
            HTuple hv_transPixeMul, HTuple hv_ignoreMinDist, HTuple hv_ignoreMaxDist, out HTuple hv_HomeRow,
            out HTuple hv_HomeCol, out HTuple hv_EndRow, out HTuple hv_EndCol, out HTuple hv_AllDist,
            out HTuple hv_MaxDist, out HTuple hv_MinDist, out HTuple hv_MeanDist, out HTuple hv_MedianDist,
            out HTuple hv_DeviationDist)
        {

            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_LoopObj, ho_ImageSour, ho_measureRec = null;
            HObject ho_EdgeStart = null, ho_EdgeEnd = null;

            // Local control variables 

            HTuple hv_ObjWidth = null, hv_ObjHeight = null;
            HTuple hv_ID = null, hv_Angle = new HTuple(), hv_MeasureHandle = new HTuple();
            HTuple hv_RowEdgeFirst = new HTuple(), hv_ColumnEdgeFirst = new HTuple();
            HTuple hv_AmplitudeFirst = new HTuple(), hv_RowEdgeSecond = new HTuple();
            HTuple hv_ColumnEdgeSecond = new HTuple(), hv_AmplitudeSecond = new HTuple();
            HTuple hv_InDistance = new HTuple(), hv_OutDistance = new HTuple();
            HTuple hv_OneDist = new HTuple(), hv_RowStart = new HTuple();
            HTuple hv_ColStart = new HTuple(), hv_RowEnd = new HTuple();
            HTuple hv_ColEnd = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ObjImage);
            HOperatorSet.GenEmptyObj(out ho_measPathCir);
            HOperatorSet.GenEmptyObj(out ho_AllMeasureRec);
            HOperatorSet.GenEmptyObj(out ho_AllEdgeStart);
            HOperatorSet.GenEmptyObj(out ho_AllEdgeEnd);
            HOperatorSet.GenEmptyObj(out ho_LoopObj);
            HOperatorSet.GenEmptyObj(out ho_ImageSour);
            HOperatorSet.GenEmptyObj(out ho_measureRec);
            HOperatorSet.GenEmptyObj(out ho_EdgeStart);
            HOperatorSet.GenEmptyObj(out ho_EdgeEnd);
            hv_MaxDist = new HTuple();
            hv_MinDist = new HTuple();
            hv_MeanDist = new HTuple();
            hv_MedianDist = new HTuple();
            hv_DeviationDist = new HTuple();
            try
            {

                //获得图像感兴趣测量区域
                ho_LoopObj.Dispose();
                HOperatorSet.DilationCircle(ho_loopRegion, out ho_LoopObj, hv_dilLoopR);
                //erosion_circle (loopRegion, LoopObj, 8)
                //方法1：
                //get_domain (Image, ImageRegion)
                //difference (ImageRegion, LoopObj, notLoopObj)
                //copy_image (Image, ObjImage)
                //overpaint_region (ObjImage, notLoopObj, 0, 'fill')
                //或paint_region (notLoopObj, Image, ObjImage, 255, 'fill')
                //方法2：
                ho_ImageSour.Dispose();
                HOperatorSet.ReduceDomain(ho_measObjImage, ho_LoopObj, out ho_ImageSour);
                ho_ObjImage.Dispose();
                HOperatorSet.GenImageProto(ho_measObjImage, out ho_ObjImage, hv_fillThreshold);
                HOperatorSet.OverpaintGray(ho_ObjImage, ho_ImageSour);

                ho_AllMeasureRec.Dispose();
                HOperatorSet.GenEmptyObj(out ho_AllMeasureRec);
                ho_AllEdgeStart.Dispose();
                HOperatorSet.GenEmptyObj(out ho_AllEdgeStart);
                ho_AllEdgeEnd.Dispose();
                HOperatorSet.GenEmptyObj(out ho_AllEdgeEnd);
                hv_HomeRow = new HTuple();
                hv_HomeCol = new HTuple();
                hv_EndRow = new HTuple();
                hv_EndCol = new HTuple();
                hv_AllDist = new HTuple();

                //测量目标图像的宽和高
                HOperatorSet.GetImageSize(ho_ObjImage, out hv_ObjWidth, out hv_ObjHeight);
                //测量的"轨迹圆"
                ho_measPathCir.Dispose();
                HOperatorSet.GenCircleContourXld(out ho_measPathCir, hv_measPathCirRow, hv_measPathCirCol,
                    hv_measPathCirR, hv_startPhi.TupleRad(), hv_endPhi.TupleRad(), "positive",
                    1);
                //**画测量句柄
                HTuple end_val29 = hv_measRecNum - 1;
                HTuple step_val29 = 1;
                for (hv_ID = 0; hv_ID.Continue(end_val29, step_val29); hv_ID = hv_ID.TupleAdd(step_val29))
                {
                    hv_Angle = hv_startPhi + (((hv_endPhi - hv_startPhi) / hv_measRecNum) * hv_ID);
                    HOperatorSet.GenMeasureRectangle2(hv_measPathCirRow - (hv_measPathCirR * (((hv_Angle.TupleRad()
                        )).TupleSin())), hv_measPathCirCol + (hv_measPathCirR * (((hv_Angle.TupleRad()
                        )).TupleCos())), hv_Angle.TupleRad(), hv_ROI_W, hv_ROI_H, hv_ObjWidth,
                        hv_ObjHeight, "nearest_neighbor", out hv_MeasureHandle);
                    ho_measureRec.Dispose();
                    HOperatorSet.GenRectangle2ContourXld(out ho_measureRec, hv_measPathCirRow - (hv_measPathCirR * (((hv_Angle.TupleRad()
                        )).TupleSin())), hv_measPathCirCol + (hv_measPathCirR * (((hv_Angle.TupleRad()
                        )).TupleCos())), hv_Angle.TupleRad(), hv_ROI_W, hv_ROI_H);
                    HOperatorSet.MeasurePairs(ho_ObjImage, hv_MeasureHandle, hv_sigma, hv_minThresholdAmp,
                        hv_transition, "all", out hv_RowEdgeFirst, out hv_ColumnEdgeFirst, out hv_AmplitudeFirst,
                        out hv_RowEdgeSecond, out hv_ColumnEdgeSecond, out hv_AmplitudeSecond,
                        out hv_InDistance, out hv_OutDistance);
                    HOperatorSet.CloseMeasure(hv_MeasureHandle);
                    //得到全部测量矩形
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_AllMeasureRec, ho_measureRec, out ExpTmpOutVar_0
                            );
                        ho_AllMeasureRec.Dispose();
                        ho_AllMeasureRec = ExpTmpOutVar_0;
                    }
                    if ((int)(new HTuple((new HTuple(hv_InDistance.TupleLength())).TupleEqual(
                        1))) != 0)
                    {
                        hv_OneDist = hv_InDistance.TupleSum();
                    }
                    else
                    {
                        hv_OneDist = (hv_InDistance.TupleSum()) + (hv_OutDistance.TupleSum());
                    }
                    //转换为实际距离后取小数点后3位
                    hv_OneDist = hv_OneDist * hv_transPixeMul;
                    hv_OneDist = hv_OneDist.TupleString(".3f");
                    HOperatorSet.TupleNumber(hv_OneDist, out hv_OneDist);
                    //测量有效结果值及边缘线坐标和边缘线：如果值异常(超限),或为空，则舍弃
                    if ((int)((new HTuple((new HTuple((new HTuple(hv_OneDist.TupleLess(hv_ignoreMinDist))).TupleOr(
                        new HTuple(hv_OneDist.TupleGreater(hv_ignoreMaxDist))))).TupleOr(new HTuple((new HTuple(hv_InDistance.TupleLength()
                        )).TupleEqual(0))))).TupleNot()) != 0)
                    {

                        hv_AllDist = hv_AllDist.TupleConcat(hv_OneDist);

                        hv_HomeRow = hv_HomeRow.TupleConcat(hv_RowEdgeFirst.TupleSelect(0));
                        hv_HomeCol = hv_HomeCol.TupleConcat(hv_ColumnEdgeFirst.TupleSelect(0));

                        hv_EndRow = hv_EndRow.TupleConcat(hv_RowEdgeSecond.TupleSelect((new HTuple(hv_RowEdgeSecond.TupleLength()
                            )) - 1));
                        hv_EndCol = hv_EndCol.TupleConcat(hv_ColumnEdgeSecond.TupleSelect((new HTuple(hv_ColumnEdgeSecond.TupleLength()
                            )) - 1));

                        //画第一个边缘线
                        hv_RowStart = (hv_RowEdgeFirst.TupleSelect(0)) + (hv_ROI_H * (((((hv_Angle + 90)).TupleRad()
                            )).TupleSin()));
                        hv_ColStart = (hv_ColumnEdgeFirst.TupleSelect(0)) - (hv_ROI_H * (((((hv_Angle + 90)).TupleRad()
                            )).TupleCos()));
                        hv_RowEnd = (hv_RowEdgeFirst.TupleSelect(0)) - (hv_ROI_H * (((((hv_Angle + 90)).TupleRad()
                            )).TupleSin()));
                        hv_ColEnd = (hv_ColumnEdgeFirst.TupleSelect(0)) + (hv_ROI_H * (((((hv_Angle + 90)).TupleRad()
                            )).TupleCos()));
                        ho_EdgeStart.Dispose();
                        HOperatorSet.GenContourPolygonXld(out ho_EdgeStart, hv_RowStart.TupleConcat(
                            hv_RowEnd), hv_ColStart.TupleConcat(hv_ColEnd));
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ConcatObj(ho_AllEdgeStart, ho_EdgeStart, out ExpTmpOutVar_0
                                );
                            ho_AllEdgeStart.Dispose();
                            ho_AllEdgeStart = ExpTmpOutVar_0;
                        }
                        //画最后一个边缘线
                        hv_RowStart = (hv_RowEdgeSecond.TupleSelect((new HTuple(hv_RowEdgeSecond.TupleLength()
                            )) - 1)) + (hv_ROI_H * (((((hv_Angle + 90)).TupleRad())).TupleSin()));
                        hv_ColStart = (hv_ColumnEdgeSecond.TupleSelect((new HTuple(hv_ColumnEdgeSecond.TupleLength()
                            )) - 1)) - (hv_ROI_H * (((((hv_Angle + 90)).TupleRad())).TupleCos()));
                        hv_RowEnd = (hv_RowEdgeSecond.TupleSelect((new HTuple(hv_RowEdgeSecond.TupleLength()
                            )) - 1)) - (hv_ROI_H * (((((hv_Angle + 90)).TupleRad())).TupleSin()));
                        hv_ColEnd = (hv_ColumnEdgeSecond.TupleSelect((new HTuple(hv_ColumnEdgeSecond.TupleLength()
                            )) - 1)) + (hv_ROI_H * (((((hv_Angle + 90)).TupleRad())).TupleCos()));
                        ho_EdgeEnd.Dispose();
                        HOperatorSet.GenContourPolygonXld(out ho_EdgeEnd, hv_RowStart.TupleConcat(
                            hv_RowEnd), hv_ColStart.TupleConcat(hv_ColEnd));
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ConcatObj(ho_AllEdgeEnd, ho_EdgeEnd, out ExpTmpOutVar_0);
                            ho_AllEdgeEnd.Dispose();
                            ho_AllEdgeEnd = ExpTmpOutVar_0;
                        }
                    }
                }
                if ((int)(new HTuple((new HTuple(hv_AllDist.TupleLength())).TupleNotEqual(0))) != 0)
                {
                    //所有尺寸最大值：
                    HOperatorSet.TupleMax(hv_AllDist, out hv_MaxDist);
                    //最小值：
                    HOperatorSet.TupleMin(hv_AllDist, out hv_MinDist);
                    //平均值：
                    HOperatorSet.TupleMean(hv_AllDist, out hv_MeanDist);
                    //中值
                    HOperatorSet.TupleMedian(hv_AllDist, out hv_MedianDist);
                    //标准差：
                    HOperatorSet.TupleDeviation(hv_AllDist, out hv_DeviationDist);
                }
                else
                {
                    hv_MaxDist = new HTuple();
                    hv_MinDist = new HTuple();
                    hv_MeanDist = new HTuple();
                    hv_MedianDist = new HTuple();
                    hv_DeviationDist = new HTuple();

                }

                ho_LoopObj.Dispose();
                ho_ImageSour.Dispose();
                ho_measureRec.Dispose();
                ho_EdgeStart.Dispose();
                ho_EdgeEnd.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_LoopObj.Dispose();
                ho_ImageSour.Dispose();
                ho_measureRec.Dispose();
                ho_EdgeStart.Dispose();
                ho_EdgeEnd.Dispose();

                throw HDevExpDefaultException;
            }
        }

        /*(非边缘对)制作环形测量工具之2---输入原始图像(已经排除了其他干扰,但手动控制的周围像素，振幅大可能也会引入干扰，基本无用，所以采用边缘对最佳)、
            测量目标为暗色或亮色、不区分测量正反向、非边缘对。*/
        public void measurePosAll_brightDarkObj_ROIimage(HObject ho_measObjImage, HObject ho_loopRegion,
            out HObject ho_ObjImage, out HObject ho_measPathCir, out HObject ho_AllMeasureRec,
            out HObject ho_AllEdgeStart, out HObject ho_AllEdgeEnd, HTuple hv_dilLoopR,
            HTuple hv_fillThreshold, HTuple hv_measPathCirR, HTuple hv_measPathCirRow, HTuple hv_measPathCirCol,
            HTuple hv_measRecNum, HTuple hv_startPhi, HTuple hv_endPhi, HTuple hv_sigma,
            HTuple hv_minThresholdAmp, HTuple hv_ROI_W, HTuple hv_ROI_H, HTuple hv_transPixeMul,
            HTuple hv_ignoreMinDist, HTuple hv_ignoreMaxDist, out HTuple hv_HomeRow, out HTuple hv_HomeCol,
            out HTuple hv_EndRow, out HTuple hv_EndCol, out HTuple hv_AllDist, out HTuple hv_MaxDist,
            out HTuple hv_MinDist, out HTuple hv_MeanDist, out HTuple hv_MedianDist, out HTuple hv_DeviationDist)
        {

            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_LoopObj, ho_ImageSour, ho_measureRec = null;
            HObject ho_EdgeStart = null, ho_EdgeEnd = null;

            // Local control variables 

            HTuple hv_ObjWidth = null, hv_ObjHeight = null;
            HTuple hv_ID = null, hv_Angle = new HTuple(), hv_MeasureHandle = new HTuple();
            HTuple hv_RowEdge = new HTuple(), hv_ColumnEdge = new HTuple();
            HTuple hv_Amplitude = new HTuple(), hv_Distance = new HTuple();
            HTuple hv_OneDist = new HTuple(), hv_RowStart = new HTuple();
            HTuple hv_ColStart = new HTuple(), hv_RowEnd = new HTuple();
            HTuple hv_ColEnd = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ObjImage);
            HOperatorSet.GenEmptyObj(out ho_measPathCir);
            HOperatorSet.GenEmptyObj(out ho_AllMeasureRec);
            HOperatorSet.GenEmptyObj(out ho_AllEdgeStart);
            HOperatorSet.GenEmptyObj(out ho_AllEdgeEnd);
            HOperatorSet.GenEmptyObj(out ho_LoopObj);
            HOperatorSet.GenEmptyObj(out ho_ImageSour);
            HOperatorSet.GenEmptyObj(out ho_measureRec);
            HOperatorSet.GenEmptyObj(out ho_EdgeStart);
            HOperatorSet.GenEmptyObj(out ho_EdgeEnd);
            hv_MaxDist = new HTuple();
            hv_MinDist = new HTuple();
            hv_MeanDist = new HTuple();
            hv_MedianDist = new HTuple();
            hv_DeviationDist = new HTuple();
            try
            {

                //获得图像感兴趣测量区域
                ho_LoopObj.Dispose();
                HOperatorSet.DilationCircle(ho_loopRegion, out ho_LoopObj, hv_dilLoopR);
                //erosion_circle (loopRegion, LoopObj, 8)
                //方法1：
                //get_domain (Image, ImageRegion)
                //difference (ImageRegion, LoopObj, notLoopObj)
                //copy_image (Image, ObjImage)
                //overpaint_region (ObjImage, notLoopObj, 0, 'fill')
                //或paint_region (notLoopObj, Image, ObjImage, 255, 'fill')
                //方法2：
                ho_ImageSour.Dispose();
                HOperatorSet.ReduceDomain(ho_measObjImage, ho_LoopObj, out ho_ImageSour);
                ho_ObjImage.Dispose();
                HOperatorSet.GenImageProto(ho_measObjImage, out ho_ObjImage, hv_fillThreshold);
                HOperatorSet.OverpaintGray(ho_ObjImage, ho_ImageSour);


                ho_AllMeasureRec.Dispose();
                HOperatorSet.GenEmptyObj(out ho_AllMeasureRec);
                ho_AllEdgeStart.Dispose();
                HOperatorSet.GenEmptyObj(out ho_AllEdgeStart);
                ho_AllEdgeEnd.Dispose();
                HOperatorSet.GenEmptyObj(out ho_AllEdgeEnd);
                hv_HomeRow = new HTuple();
                hv_HomeCol = new HTuple();
                hv_EndRow = new HTuple();
                hv_EndCol = new HTuple();
                hv_AllDist = new HTuple();

                //测量目标图像的宽和高
                HOperatorSet.GetImageSize(ho_ObjImage, out hv_ObjWidth, out hv_ObjHeight);
                //测量的"轨迹圆"
                ho_measPathCir.Dispose();
                HOperatorSet.GenCircleContourXld(out ho_measPathCir, hv_measPathCirRow, hv_measPathCirCol,
                    hv_measPathCirR, hv_startPhi.TupleRad(), hv_endPhi.TupleRad(), "positive",
                    1);
                //**画测量句柄
                HTuple end_val30 = hv_measRecNum - 1;
                HTuple step_val30 = 1;
                for (hv_ID = 0; hv_ID.Continue(end_val30, step_val30); hv_ID = hv_ID.TupleAdd(step_val30))
                {
                    hv_Angle = hv_startPhi + (((hv_endPhi - hv_startPhi) / hv_measRecNum) * hv_ID);
                    HOperatorSet.GenMeasureRectangle2(hv_measPathCirRow - (hv_measPathCirR * (((hv_Angle.TupleRad()
                        )).TupleSin())), hv_measPathCirCol + (hv_measPathCirR * (((hv_Angle.TupleRad()
                        )).TupleCos())), hv_Angle.TupleRad(), hv_ROI_W, hv_ROI_H, hv_ObjWidth,
                        hv_ObjHeight, "nearest_neighbor", out hv_MeasureHandle);
                    ho_measureRec.Dispose();
                    HOperatorSet.GenRectangle2ContourXld(out ho_measureRec, hv_measPathCirRow - (hv_measPathCirR * (((hv_Angle.TupleRad()
                        )).TupleSin())), hv_measPathCirCol + (hv_measPathCirR * (((hv_Angle.TupleRad()
                        )).TupleCos())), hv_Angle.TupleRad(), hv_ROI_W, hv_ROI_H);
                    HOperatorSet.MeasurePos(ho_ObjImage, hv_MeasureHandle, hv_sigma, hv_minThresholdAmp,
                        "all", "all", out hv_RowEdge, out hv_ColumnEdge, out hv_Amplitude, out hv_Distance);
                    HOperatorSet.CloseMeasure(hv_MeasureHandle);
                    //得到全部测量矩形
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_AllMeasureRec, ho_measureRec, out ExpTmpOutVar_0
                            );
                        ho_AllMeasureRec.Dispose();
                        ho_AllMeasureRec = ExpTmpOutVar_0;
                    }
                    hv_OneDist = hv_Distance.TupleSum();
                    //转换为实际距离后取小数点后3位
                    hv_OneDist = hv_OneDist * hv_transPixeMul;
                    hv_OneDist = hv_OneDist.TupleString(".3f");
                    HOperatorSet.TupleNumber(hv_OneDist, out hv_OneDist);
                    //测量有效结果值及边缘线坐标和边缘线：如果值异常(超限),或为空，则舍弃
                    if ((int)((new HTuple((new HTuple((new HTuple(hv_OneDist.TupleLess(hv_ignoreMinDist))).TupleOr(
                        new HTuple(hv_OneDist.TupleGreater(hv_ignoreMaxDist))))).TupleOr(new HTuple((new HTuple(hv_Distance.TupleLength()
                        )).TupleEqual(0))))).TupleNot()) != 0)
                    {

                        hv_AllDist = hv_AllDist.TupleConcat(hv_OneDist);

                        hv_HomeRow = hv_HomeRow.TupleConcat(hv_RowEdge.TupleSelect(0));
                        hv_HomeCol = hv_HomeCol.TupleConcat(hv_ColumnEdge.TupleSelect(0));

                        hv_EndRow = hv_EndRow.TupleConcat(hv_RowEdge.TupleSelect((new HTuple(hv_RowEdge.TupleLength()
                            )) - 1));
                        hv_EndCol = hv_EndCol.TupleConcat(hv_ColumnEdge.TupleSelect((new HTuple(hv_ColumnEdge.TupleLength()
                            )) - 1));

                        //画第一个边缘线
                        hv_RowStart = (hv_RowEdge.TupleSelect(0)) + (hv_ROI_H * (((((hv_Angle + 90)).TupleRad()
                            )).TupleSin()));
                        hv_ColStart = (hv_ColumnEdge.TupleSelect(0)) - (hv_ROI_H * (((((hv_Angle + 90)).TupleRad()
                            )).TupleCos()));
                        hv_RowEnd = (hv_RowEdge.TupleSelect(0)) - (hv_ROI_H * (((((hv_Angle + 90)).TupleRad()
                            )).TupleSin()));
                        hv_ColEnd = (hv_ColumnEdge.TupleSelect(0)) + (hv_ROI_H * (((((hv_Angle + 90)).TupleRad()
                            )).TupleCos()));
                        ho_EdgeStart.Dispose();
                        HOperatorSet.GenContourPolygonXld(out ho_EdgeStart, hv_RowStart.TupleConcat(
                            hv_RowEnd), hv_ColStart.TupleConcat(hv_ColEnd));
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ConcatObj(ho_AllEdgeStart, ho_EdgeStart, out ExpTmpOutVar_0
                                );
                            ho_AllEdgeStart.Dispose();
                            ho_AllEdgeStart = ExpTmpOutVar_0;
                        }
                        //画最后一个边缘线
                        hv_RowStart = (hv_RowEdge.TupleSelect((new HTuple(hv_RowEdge.TupleLength()
                            )) - 1)) + (hv_ROI_H * (((((hv_Angle + 90)).TupleRad())).TupleSin()));
                        hv_ColStart = (hv_ColumnEdge.TupleSelect((new HTuple(hv_ColumnEdge.TupleLength()
                            )) - 1)) - (hv_ROI_H * (((((hv_Angle + 90)).TupleRad())).TupleCos()));
                        hv_RowEnd = (hv_RowEdge.TupleSelect((new HTuple(hv_RowEdge.TupleLength()
                            )) - 1)) - (hv_ROI_H * (((((hv_Angle + 90)).TupleRad())).TupleSin()));
                        hv_ColEnd = (hv_ColumnEdge.TupleSelect((new HTuple(hv_ColumnEdge.TupleLength()
                            )) - 1)) + (hv_ROI_H * (((((hv_Angle + 90)).TupleRad())).TupleCos()));
                        ho_EdgeEnd.Dispose();
                        HOperatorSet.GenContourPolygonXld(out ho_EdgeEnd, hv_RowStart.TupleConcat(
                            hv_RowEnd), hv_ColStart.TupleConcat(hv_ColEnd));
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ConcatObj(ho_AllEdgeEnd, ho_EdgeEnd, out ExpTmpOutVar_0);
                            ho_AllEdgeEnd.Dispose();
                            ho_AllEdgeEnd = ExpTmpOutVar_0;
                        }
                    }
                }
                if ((int)(new HTuple((new HTuple(hv_AllDist.TupleLength())).TupleNotEqual(0))) != 0)
                {
                    //所有尺寸最大值：
                    HOperatorSet.TupleMax(hv_AllDist, out hv_MaxDist);
                    //最小值：
                    HOperatorSet.TupleMin(hv_AllDist, out hv_MinDist);
                    //平均值：
                    HOperatorSet.TupleMean(hv_AllDist, out hv_MeanDist);
                    //中值
                    HOperatorSet.TupleMedian(hv_AllDist, out hv_MedianDist);
                    //标准差：
                    HOperatorSet.TupleDeviation(hv_AllDist, out hv_DeviationDist);
                }
                else
                {
                    hv_MaxDist = new HTuple();
                    hv_MinDist = new HTuple();
                    hv_MeanDist = new HTuple();
                    hv_MedianDist = new HTuple();
                    hv_DeviationDist = new HTuple();
                }

                ho_LoopObj.Dispose();
                ho_ImageSour.Dispose();
                ho_measureRec.Dispose();
                ho_EdgeStart.Dispose();
                ho_EdgeEnd.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_LoopObj.Dispose();
                ho_ImageSour.Dispose();
                ho_measureRec.Dispose();
                ho_EdgeStart.Dispose();
                ho_EdgeEnd.Dispose();

                throw HDevExpDefaultException;
            }
        }

        /*(非边缘对)制作环形测量工具之1---输入原始图像(可能有其他干扰)、测量目标为暗色或亮色、不区分测量正反向、非边缘对。
         */
        public void measurePosAll_brightDarkObj_Wholeimage(HObject ho_measObjImage, out HObject ho_measPathCir,
            out HObject ho_AllMeasureRec, out HObject ho_AllEdgeStart, out HObject ho_AllEdgeEnd,
            HTuple hv_measPathCirR, HTuple hv_measPathCirRow, HTuple hv_measPathCirCol,
            HTuple hv_measRecNum, HTuple hv_startPhi, HTuple hv_endPhi, HTuple hv_sigma,
            HTuple hv_minThresholdAmp, HTuple hv_ROI_W, HTuple hv_ROI_H, HTuple hv_transPixeMul,
            HTuple hv_ignoreMinDist, HTuple hv_ignoreMaxDist, out HTuple hv_HomeRow, out HTuple hv_HomeCol,
            out HTuple hv_EndRow, out HTuple hv_EndCol, out HTuple hv_AllDist, out HTuple hv_MaxDist,
            out HTuple hv_MinDist, out HTuple hv_MeanDist, out HTuple hv_MedianDist, out HTuple hv_DeviationDist)
        {

            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_measureRec = null, ho_EdgeStart = null;
            HObject ho_EdgeEnd = null;

            // Local control variables 

            HTuple hv_ObjWidth = null, hv_ObjHeight = null;
            HTuple hv_ID = null, hv_Angle = new HTuple(), hv_MeasureHandle = new HTuple();
            HTuple hv_RowEdge = new HTuple(), hv_ColumnEdge = new HTuple();
            HTuple hv_Amplitude = new HTuple(), hv_Distance = new HTuple();
            HTuple hv_OneDist = new HTuple(), hv_RowStart = new HTuple();
            HTuple hv_ColStart = new HTuple(), hv_RowEnd = new HTuple();
            HTuple hv_ColEnd = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_measPathCir);
            HOperatorSet.GenEmptyObj(out ho_AllMeasureRec);
            HOperatorSet.GenEmptyObj(out ho_AllEdgeStart);
            HOperatorSet.GenEmptyObj(out ho_AllEdgeEnd);
            HOperatorSet.GenEmptyObj(out ho_measureRec);
            HOperatorSet.GenEmptyObj(out ho_EdgeStart);
            HOperatorSet.GenEmptyObj(out ho_EdgeEnd);
            hv_MaxDist = new HTuple();
            hv_MinDist = new HTuple();
            hv_MeanDist = new HTuple();
            hv_MedianDist = new HTuple();
            hv_DeviationDist = new HTuple();
            try
            {
                ho_AllMeasureRec.Dispose();
                HOperatorSet.GenEmptyObj(out ho_AllMeasureRec);
                ho_AllEdgeStart.Dispose();
                HOperatorSet.GenEmptyObj(out ho_AllEdgeStart);
                ho_AllEdgeEnd.Dispose();
                HOperatorSet.GenEmptyObj(out ho_AllEdgeEnd);
                hv_HomeRow = new HTuple();
                hv_HomeCol = new HTuple();
                hv_EndRow = new HTuple();
                hv_EndCol = new HTuple();
                hv_AllDist = new HTuple();

                //测量目标图像的宽和高
                HOperatorSet.GetImageSize(ho_measObjImage, out hv_ObjWidth, out hv_ObjHeight);
                //测量的"轨迹圆"
                ho_measPathCir.Dispose();
                HOperatorSet.GenCircleContourXld(out ho_measPathCir, hv_measPathCirRow, hv_measPathCirCol,
                    hv_measPathCirR, hv_startPhi.TupleRad(), hv_endPhi.TupleRad(), "positive",
                    1);
                //**画测量句柄
                HTuple end_val14 = hv_measRecNum - 1;
                HTuple step_val14 = 1;
                for (hv_ID = 0; hv_ID.Continue(end_val14, step_val14); hv_ID = hv_ID.TupleAdd(step_val14))
                {
                    hv_Angle = hv_startPhi + (((hv_endPhi - hv_startPhi) / hv_measRecNum) * hv_ID);
                    HOperatorSet.GenMeasureRectangle2(hv_measPathCirRow - (hv_measPathCirR * (((hv_Angle.TupleRad()
                        )).TupleSin())), hv_measPathCirCol + (hv_measPathCirR * (((hv_Angle.TupleRad()
                        )).TupleCos())), hv_Angle.TupleRad(), hv_ROI_W, hv_ROI_H, hv_ObjWidth,
                        hv_ObjHeight, "nearest_neighbor", out hv_MeasureHandle);
                    ho_measureRec.Dispose();
                    HOperatorSet.GenRectangle2ContourXld(out ho_measureRec, hv_measPathCirRow - (hv_measPathCirR * (((hv_Angle.TupleRad()
                        )).TupleSin())), hv_measPathCirCol + (hv_measPathCirR * (((hv_Angle.TupleRad()
                        )).TupleCos())), hv_Angle.TupleRad(), hv_ROI_W, hv_ROI_H);
                    HOperatorSet.MeasurePos(ho_measObjImage, hv_MeasureHandle, hv_sigma, hv_minThresholdAmp,
                        "all", "all", out hv_RowEdge, out hv_ColumnEdge, out hv_Amplitude, out hv_Distance);
                    HOperatorSet.CloseMeasure(hv_MeasureHandle);
                    //得到全部测量矩形
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_AllMeasureRec, ho_measureRec, out ExpTmpOutVar_0
                            );
                        ho_AllMeasureRec.Dispose();
                        ho_AllMeasureRec = ExpTmpOutVar_0;
                    }
                    hv_OneDist = hv_Distance.TupleSum();
                    //转换为实际距离后取小数点后3位
                    hv_OneDist = hv_OneDist * hv_transPixeMul;
                    hv_OneDist = hv_OneDist.TupleString(".3f");
                    HOperatorSet.TupleNumber(hv_OneDist, out hv_OneDist);
                    //测量有效结果值及边缘线坐标和边缘线：如果值异常(超限),或为空，则舍弃
                    if ((int)((new HTuple((new HTuple((new HTuple(hv_OneDist.TupleLess(hv_ignoreMinDist))).TupleOr(
                        new HTuple(hv_OneDist.TupleGreater(hv_ignoreMaxDist))))).TupleOr(new HTuple((new HTuple(hv_Distance.TupleLength()
                        )).TupleEqual(0))))).TupleNot()) != 0)
                    {

                        hv_AllDist = hv_AllDist.TupleConcat(hv_OneDist);

                        hv_HomeRow = hv_HomeRow.TupleConcat(hv_RowEdge.TupleSelect(0));
                        hv_HomeCol = hv_HomeCol.TupleConcat(hv_ColumnEdge.TupleSelect(0));

                        hv_EndRow = hv_EndRow.TupleConcat(hv_RowEdge.TupleSelect((new HTuple(hv_RowEdge.TupleLength()
                            )) - 1));
                        hv_EndCol = hv_EndCol.TupleConcat(hv_ColumnEdge.TupleSelect((new HTuple(hv_ColumnEdge.TupleLength()
                            )) - 1));

                        //画第一个边缘线
                        hv_RowStart = (hv_RowEdge.TupleSelect(0)) + (hv_ROI_H * (((((hv_Angle + 90)).TupleRad()
                            )).TupleSin()));
                        hv_ColStart = (hv_ColumnEdge.TupleSelect(0)) - (hv_ROI_H * (((((hv_Angle + 90)).TupleRad()
                            )).TupleCos()));
                        hv_RowEnd = (hv_RowEdge.TupleSelect(0)) - (hv_ROI_H * (((((hv_Angle + 90)).TupleRad()
                            )).TupleSin()));
                        hv_ColEnd = (hv_ColumnEdge.TupleSelect(0)) + (hv_ROI_H * (((((hv_Angle + 90)).TupleRad()
                            )).TupleCos()));
                        ho_EdgeStart.Dispose();
                        HOperatorSet.GenContourPolygonXld(out ho_EdgeStart, hv_RowStart.TupleConcat(
                            hv_RowEnd), hv_ColStart.TupleConcat(hv_ColEnd));
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ConcatObj(ho_AllEdgeStart, ho_EdgeStart, out ExpTmpOutVar_0
                                );
                            ho_AllEdgeStart.Dispose();
                            ho_AllEdgeStart = ExpTmpOutVar_0;
                        }
                        //画最后一个边缘线
                        hv_RowStart = (hv_RowEdge.TupleSelect((new HTuple(hv_RowEdge.TupleLength()
                            )) - 1)) + (hv_ROI_H * (((((hv_Angle + 90)).TupleRad())).TupleSin()));
                        hv_ColStart = (hv_ColumnEdge.TupleSelect((new HTuple(hv_ColumnEdge.TupleLength()
                            )) - 1)) - (hv_ROI_H * (((((hv_Angle + 90)).TupleRad())).TupleCos()));
                        hv_RowEnd = (hv_RowEdge.TupleSelect((new HTuple(hv_RowEdge.TupleLength()
                            )) - 1)) - (hv_ROI_H * (((((hv_Angle + 90)).TupleRad())).TupleSin()));
                        hv_ColEnd = (hv_ColumnEdge.TupleSelect((new HTuple(hv_ColumnEdge.TupleLength()
                            )) - 1)) + (hv_ROI_H * (((((hv_Angle + 90)).TupleRad())).TupleCos()));
                        ho_EdgeEnd.Dispose();
                        HOperatorSet.GenContourPolygonXld(out ho_EdgeEnd, hv_RowStart.TupleConcat(
                            hv_RowEnd), hv_ColStart.TupleConcat(hv_ColEnd));
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ConcatObj(ho_AllEdgeEnd, ho_EdgeEnd, out ExpTmpOutVar_0);
                            ho_AllEdgeEnd.Dispose();
                            ho_AllEdgeEnd = ExpTmpOutVar_0;
                        }
                    }
                }
                if ((int)(new HTuple((new HTuple(hv_AllDist.TupleLength())).TupleNotEqual(0))) != 0)
                {
                    //所有尺寸最大值：
                    HOperatorSet.TupleMax(hv_AllDist, out hv_MaxDist);
                    //最小值：
                    HOperatorSet.TupleMin(hv_AllDist, out hv_MinDist);
                    //平均值：
                    HOperatorSet.TupleMean(hv_AllDist, out hv_MeanDist);
                    //中值
                    HOperatorSet.TupleMedian(hv_AllDist, out hv_MedianDist);
                    //标准差：
                    HOperatorSet.TupleDeviation(hv_AllDist, out hv_DeviationDist);
                }
                else
                {
                    hv_MaxDist = new HTuple();
                    hv_MinDist = new HTuple();
                    hv_MeanDist = new HTuple();
                    hv_MedianDist = new HTuple();
                    hv_DeviationDist = new HTuple();

                }

                ho_measureRec.Dispose();
                ho_EdgeStart.Dispose();
                ho_EdgeEnd.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_measureRec.Dispose();
                ho_EdgeStart.Dispose();
                ho_EdgeEnd.Dispose();

                throw HDevExpDefaultException;
            }
        }

        #endregion

        #region 测量定位相关---基于矩形测量句柄的(非边缘对、边缘对)抓边工具


        /***功能：根据设置显示对象(region/xld/image)统一接口 ***
           输入参数：
           * 参1 输入要显示的对象（单元素或多元素）
           * 参2：显示窗口句柄
           * 参3：输入是否显示对象*ture=1,false=0,部分或全部元素为[]则默认设置元素值为0
           * 参4：输入显示对象轮廓线宽设置(比如1)，部分或全部元素为[]则不设置默认值，也不执行对应设置
           * 参5：输入显示对象颜色设置，部分或全部元素为[]则不设置默认值，也不执行对应设置 
           * 参6：输入显示(区域)对象填充方式设置("margin"、"fill")，部分或全部元素为[]则不设置默认值，也不执行对应设置
           * 参7返回值： 成功返回0，异常出错返回-1  
           最近更改日期:2019-7-16
        *****************************************************/
        public int ShowObj(HObject ho_ShowObj, HTuple hv_WindowHandle,
            HTuple hv_IsShowObj, HTuple hv_ShowLineW, HTuple hv_ShowColor, HTuple hv_ShowDrawType, ref string strErrMsg)
        {
            strErrMsg = "";

            /**********临时对象参数*********/
            HObject ho_Obj = null;
            HOperatorSet.GenEmptyObj(out ho_Obj);
            try
            {
                try
                {
                    //*******执行显示对象******************
                    HTuple hv_ObjNum = new HTuple();
                    HOperatorSet.CountObj(ho_ShowObj, out hv_ObjNum);
                    HTuple hv_IsShowObjNum = new HTuple(hv_IsShowObj.TupleLength());
                    HTuple hv_ShowLineWNum = new HTuple(hv_ShowLineW.TupleLength());
                    HTuple hv_ShowColorNum = new HTuple(hv_ShowColor.TupleLength());
                    HTuple hv_ShowDrawTypeNum = new HTuple(hv_ShowDrawType.TupleLength());

                    if ((int)(new HTuple(hv_ObjNum.TupleEqual(0))) != 0)//如果显示对象为空
                    {
                        strErrMsg = "显示对象为空对象！";
                        return -1;//直接返回
                    }
                    else
                    {
                        //**如果"是否显示对象"变量全部为[],则默认设置为：每个对象全都不显示(0)
                        if ((int)(new HTuple(hv_IsShowObjNum.TupleEqual(0))) != 0)
                        {
                            HOperatorSet.TupleGenConst(hv_ObjNum, 0, out hv_IsShowObj);
                        }
                        //**如果线宽、颜色、填充类型变量全部为[],则不执行设置，也不设置默认值
                        //if (ShowLineWNum==0)
                        //tuple_gen_const (ObjNum, 1, ShowLineW)
                        //endif

                        //if (ShowColorNum==0)
                        //tuple_gen_const (ObjNum, 'red', ShowColor)
                        //endif

                        //if (ShowDrawTypeNum==0)
                        //tuple_gen_const (ObjNum, 'margin', ShowDrawType)
                        //endif

                        //**如果是"是否显示对象"变量部分为[],则部分为[]的变量默认设置为：不显示(0)对象
                        HTuple hv_NewIsShowObj = new HTuple();
                        if ((int)((new HTuple((new HTuple(0)).TupleLess(hv_IsShowObjNum))).TupleAnd(
                            new HTuple(hv_IsShowObjNum.TupleLess(hv_ObjNum)))) != 0)
                        {
                            HOperatorSet.TupleGenConst(hv_ObjNum - hv_IsShowObjNum, 0, out hv_NewIsShowObj);
                            hv_IsShowObj = hv_IsShowObj.TupleConcat(hv_NewIsShowObj);
                        }
                        //**如果线宽、颜色、填充类型变量元素部分为[]或不存在,则为[]或不存在的元素不执行设置，也不设置默认值
                        //if ((0<ShowLineWNum)and(ShowLineWNum<ObjNum))
                        //tuple_gen_const ((ObjNum-ShowLineWNum), 1, NewShowLineW)
                        //ShowLineW := [ShowLineW,NewShowLineW]
                        //endif

                        //if ((0<ShowColorNum)and(ShowColorNum<ObjNum))
                        //tuple_gen_const ((ObjNum-ShowColorNum), 'red', NewShowColor)
                        //ShowColor := [ShowColor,NewShowColor]
                        //endif

                        //if ((0<ShowDrawTypeNum)and(ShowDrawTypeNum<ObjNum))
                        //tuple_gen_const ((ObjNum-ShowDrawTypeNum), 'margin', NewShowDrawType)
                        //ShowDrawType := [ShowDrawType,NewShowDrawType]
                        //endif


                        HTuple end_val327 = hv_ObjNum - 1;
                        HTuple step_val327 = 1;
                        HTuple hv_i = new HTuple();
                        for (hv_i = 0; hv_i.Continue(end_val327, step_val327); hv_i = hv_i.TupleAdd(step_val327))
                        {
                            //设置线宽，如果值存在则设置
                            if ((int)(new HTuple(hv_i.TupleLess(hv_ShowLineWNum))) != 0)
                            {
                                HOperatorSet.SetLineWidth(hv_WindowHandle, hv_ShowLineW.TupleSelect(hv_i));
                            }
                            //设置颜色，如果值存在则设置
                            if ((int)(new HTuple(hv_i.TupleLess(hv_ShowColorNum))) != 0)
                            {
                                HOperatorSet.SetColor(hv_WindowHandle, hv_ShowColor.TupleSelect(hv_i));
                            }
                            //设置填充方式，如果值存在则设置
                            if ((int)(new HTuple(hv_i.TupleLess(hv_ShowDrawTypeNum))) != 0)
                            {
                                HOperatorSet.SetDraw(hv_WindowHandle, hv_ShowDrawType.TupleSelect(hv_i));
                            }
                            //显示对象， 由前面设置，值必存在，根据值来执行是否显示对象
                            if ((int)(hv_IsShowObj.TupleSelect(hv_i)) != 0)
                            {
                                ho_Obj.Dispose();
                                HOperatorSet.SelectObj(ho_ShowObj, out ho_Obj, hv_i + 1);
                                HOperatorSet.DispObj(ho_Obj, hv_WindowHandle);
                            }
                        }
                    }


                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            finally
            {
                ho_Obj.Dispose();
            }
            return 0;
        }

        /**********功能：由离散点来拟合直线**********
         输入参数：
           * 参1/2: 输入点集行列值
           * 参3：输入拟合算法：tukey、huber、regression、gauss、drop,默认"tukey"
           * 参4：输入参与拟合最多点数，默认-1，-1表示全部参与,该值>=2
           * 参5：输入拟合轮廓起终点之间忽略(裁剪)点数，默认0，该值>=0
           * 参6：输入循环计算(迭代)次数，默认5，该值>=0
           * 参7：输入离群值忽略(剪切)因子,越小忽略越多，默认2.0,该值>0
           * 参8：输出，拟合得到的直线(以xld形式显示)
           * 参9~12：输出，拟合得到直线起点和终点坐标
         * 参13返回值： 成功返回0，异常出错返回-1  
         最近更改日期:2019-7-16
      *****************************************************/
        public int PointsFitLine(HTuple hv_PRows, HTuple hv_PCols, HTuple hv_Algorithm, HTuple hv_MaxNumPoints, HTuple hv_ClipNumPoints, HTuple hv_ForNum, HTuple hv_ClipFactor,
           ref HObject ho_XldLine, ref HTuple hv_LineRowBegin, ref HTuple hv_LineColBegin, ref HTuple hv_LineRowEnd, ref HTuple hv_LineColEnd, ref string strErrMsg)
        {
            strErrMsg = "";

            /*****输出参数初始化******/

            //拟合得到直线起点和终点坐标
            hv_LineRowBegin = new HTuple();
            hv_LineColBegin = new HTuple();
            hv_LineRowEnd = new HTuple();
            hv_LineColEnd = new HTuple();

            /**********临时对象参数*********/
            HObject ho_Contour = null;
            HOperatorSet.GenEmptyObj(out ho_Contour);

            try
            {
                try
                {
                    //***********************点集来拟合直线工具***********************
                    HTuple hv_Nr = new HTuple();
                    HTuple hv_Nc = new HTuple();
                    HTuple hv_Dist = new HTuple();

                    ho_Contour.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_Contour, hv_PRows, hv_PCols);
                    HOperatorSet.FitLineContourXld(ho_Contour, hv_Algorithm, hv_MaxNumPoints, hv_ClipNumPoints,
                        hv_ForNum, hv_ClipFactor, out hv_LineRowBegin, out hv_LineColBegin, out hv_LineRowEnd,
                        out hv_LineColEnd, out hv_Nr, out hv_Nc, out hv_Dist);
                    if (ho_XldLine != null)//避免垃圾内存泄漏
                        ho_XldLine.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_XldLine, hv_LineRowBegin.TupleConcat(
                        hv_LineRowEnd), hv_LineColBegin.TupleConcat(hv_LineColEnd));
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            finally
            {
                ho_Contour.Dispose();
            }
            return 0;
        }


        #region (固定矩形、圆形区域范围内)沿中轴线(直线)方向

        //(10.基础)矩形区域(1条)中轴线抓边工具(边缘对)----用途:抓边-->输出坐标、振幅、边距
        //(基础功能，可用于测距离、找中点、找任意角(交)点、直角角(交)点、拟合直线...)


        //(10.基础)矩形区域(1条)中轴线抓边工具(非边缘对)----用途:抓边-->输出坐标、振幅、边距
        //(基础功能，可用于测距离、找中点、找任意角(交)点、直角角(交)点、拟合直线...)


        //(11.拓展)矩形区域(1条)中轴线方向抓边工具(非边缘对)----用途：拟合1条直线-->输出直线坐标、xld直线

        //(12.拓展)矩形区域(2条)中轴线方向抓边工具(非边缘对)----用途：找直角角点-->输出角点坐标


        /***功能：矩形区域中心相互垂直的最多两条抓边工具(非边缘对)---主要用途找直角的角点***
           输入参数：
           * 参1 输入图像变量
           * 参2：是否使用抓边对象2,如果置ture表示启用抓边对象2；false表不使用对象2仅使用抓边对象1.
           * 参3~9：矩形区域ROIRec：中心坐标、弧度角、矩形半宽、半高、
             * 小测量句柄方向箭头大小、测量对象1和对象2的分布方向箭头大小
           * 参10~16：句柄数量、光滑值、边缘振幅阈值、小测量句柄的(边缘宽)半高、明暗方向("all"、"negative"、"positive")、
             * 边缘选取("all"、"first"、"last")、测量句柄方向,ture:相对MinMeaRec1分布方向逆时针转90度方向，否则顺时针90度方向
           * 参17~23：同上
           *参24~33：输出感兴趣区域ROIRec轮廓、抓边对象1的MinMeaRec1分布方向箭头、每个MinMeaRec1小测量句柄矩形集合、
             *每个MinMeaRec1小测量句柄矩形矩形的方向箭头集合、每个MinMeaRec1小测量句柄抓取的边缘集合,30~33输出数值结果集合
           参34~42：抓边对象2的MinMeaRec2分布方向箭头、每个MinMeaRec2小测量句柄矩形集合、
             *每个MinMeaRec2小测量句柄矩形矩形的方向箭头集合、每个MinMeaRec2小测量句柄抓取的边缘集合,39~42输出数值结果集合
           * 参43返回值： 成功返回0，异常出错返回-1  
           最近更改日期:2019-7-16
        *****************************************************/
        public int RecMeaPosMaxTwo90AngleEdge(HObject ho_Image, bool isUseObj2,
            HTuple hv_RecCentR, HTuple hv_RecCentC, HTuple hv_RecRadPhi, HTuple hv_RecHalfW, HTuple hv_RecHalfH, HTuple hv_MeaRecOrienArrowSize, HTuple hv_ObjOrienArrowSize,
            HTuple hv_MeaRec1Num, HTuple hv_MeaRec1Sigma, HTuple hv_MeaRec1Threshold, HTuple hv_MeaRec1HalfH, HTuple hv_MeaRec1Transition, HTuple hv_MeaRec1Select, HTuple hv_MeaRec1Orien,
            HTuple hv_MeaRec2Num, HTuple hv_MeaRec2Sigma, HTuple hv_MeaRec2Threshold, HTuple hv_MeaRec2HalfH, HTuple hv_MeaRec2Transition, HTuple hv_MeaRec2Select, HTuple hv_MeaRec2Orien,
            ref HObject ho_ROIRec, ref HObject ho_Obj1OrienArrow, ref HObject ho_MeaRec1, ref HObject ho_MeaRec1OrienArrow, ref HObject ho_MeaRec1Mark,
            ref HTuple[] hv_MeaRec1EdgeR, ref HTuple[] hv_MeaRec1EdgeC, ref HTuple[] hv_MeaRec1Amp, ref HTuple[] hv_MeaRec1Dist,
            ref HObject ho_Obj2OrienArrow, ref HObject ho_MeaRec2, ref HObject ho_MeaRec2OrienArrow, ref HObject ho_MeaRec2Mark,
            ref HTuple[] hv_MeaRec2EdgeR, ref HTuple[] hv_MeaRec2EdgeC, ref HTuple[] hv_MeaRec2Amp, ref HTuple[] hv_MeaRec2Dist, ref string strErrMsg)
        {

            strErrMsg = "";

            //*******************输出参数初始化******************
            //输出感兴趣区域ROIRec轮廓
            HOperatorSet.GenEmptyObj(out ho_ROIRec);
            //**抓边对象1:输出参数
            //抓边对象1的MinMeaRec1分布方向箭头,单个
            HOperatorSet.GenEmptyObj(out ho_Obj1OrienArrow);
            //每个MinMeaRec1小测量句柄矩形,集合
            HOperatorSet.GenEmptyObj(out ho_MeaRec1);
            //每个MinMeaRec1小测量句柄矩形矩形的方向箭头,集合
            HOperatorSet.GenEmptyObj(out ho_MeaRec1OrienArrow);
            //每个MinMeaRec1小测量句柄抓取的边缘,集合
            HOperatorSet.GenEmptyObj(out ho_MeaRec1Mark);
            //输出数值结果,集合(二维数组,第一个元素代表测量句柄索引,halcon无法定义二维数组)
            hv_MeaRec1EdgeR = new HTuple[hv_MeaRec1Num];//每个为null,不是[]
            hv_MeaRec1EdgeC = new HTuple[hv_MeaRec1Num];
            hv_MeaRec1Amp = new HTuple[hv_MeaRec1Num];
            hv_MeaRec1Dist = new HTuple[hv_MeaRec1Num];
            //将每个为null,初始化为[]对象
            for (int index = 0; index < hv_MeaRec1Num.I; index++)
            {
                hv_MeaRec1EdgeR[index] = new HTuple();
                hv_MeaRec1EdgeC[index] = new HTuple();
                hv_MeaRec1Amp[index] = new HTuple();
                hv_MeaRec1Dist[index] = new HTuple();
            }


            //**抓边对象2:输出参数
            //抓边对象2的MinMeaRec2分布方向箭头,单个
            HOperatorSet.GenEmptyObj(out ho_Obj2OrienArrow);
            //每个MinMeaRec2小测量句柄矩形，集合
            HOperatorSet.GenEmptyObj(out ho_MeaRec2);
            //每个MinMeaRec2小测量句柄矩形矩形的方向箭头，集合
            HOperatorSet.GenEmptyObj(out ho_MeaRec2OrienArrow);
            HOperatorSet.GenEmptyObj(out ho_MeaRec2Mark);
            //输出数值结果,集合(二维数组,第一个元素代表测量句柄索引,halcon无法定义二维数组)
            hv_MeaRec2EdgeR = new HTuple[hv_MeaRec2Num];//每个为null,不是[]
            hv_MeaRec2EdgeC = new HTuple[hv_MeaRec2Num];
            hv_MeaRec2Amp = new HTuple[hv_MeaRec2Num];
            hv_MeaRec2Dist = new HTuple[hv_MeaRec2Num];
            //将每个为null,初始化为[]对象
            for (int index = 0; index < hv_MeaRec2Num.I; index++)
            {
                hv_MeaRec2EdgeR[index] = new HTuple();
                hv_MeaRec2EdgeC[index] = new HTuple();
                hv_MeaRec2Amp[index] = new HTuple();
                hv_MeaRec2Dist[index] = new HTuple();
            }

            /**********临时对象参数*********/
            HObject ho_MinMeaRec1 = null;
            HOperatorSet.GenEmptyObj(out ho_MinMeaRec1);
            HObject ho_MinMeaRec1OrienArrow = null;
            HOperatorSet.GenEmptyObj(out ho_MinMeaRec1OrienArrow);
            HObject ho_MinMeaRec1Marks = null;
            HOperatorSet.GenEmptyObj(out ho_MinMeaRec1Marks);

            HObject ho_MinMeaRec2 = null;
            HOperatorSet.GenEmptyObj(out ho_MinMeaRec2);
            HObject ho_MinMeaRec2OrienArrow = null;
            HOperatorSet.GenEmptyObj(out ho_MinMeaRec2OrienArrow);
            HObject ho_MinMeaRec2Marks = null;
            HOperatorSet.GenEmptyObj(out ho_MinMeaRec2Marks);

            try
            {
                try
                {
                    //**(1)求输入图像宽、高
                    HTuple hv_Width = new HTuple();
                    HTuple hv_Height = new HTuple();
                    HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
                    //**(2)生成感兴趣区域ROIRec轮廓
                    ho_ROIRec.Dispose();
                    HOperatorSet.GenRectangle2ContourXld(out ho_ROIRec, hv_RecCentR, hv_RecCentC,
                        hv_RecRadPhi, hv_RecHalfW, hv_RecHalfH);
                    //**(3)生成MinMeaRec1分布方向箭头，也即ROIRec方向箭头
                    HTuple hv_Obj1RadPhi = hv_RecRadPhi.Clone();
                    HTuple hv_RecRStart1 = hv_RecCentR + (hv_RecHalfW * (hv_Obj1RadPhi.TupleSin()));
                    HTuple hv_RecCStart1 = hv_RecCentC - (hv_RecHalfW * (hv_Obj1RadPhi.TupleCos()));
                    HTuple hv_RecREnd1 = hv_RecCentR - (hv_RecHalfW * (hv_Obj1RadPhi.TupleSin()));
                    HTuple hv_RecCEnd1 = hv_RecCentC + (hv_RecHalfW * (hv_Obj1RadPhi.TupleCos()));
                    ho_Obj1OrienArrow.Dispose();
                    gen_arrow_contour_xld(out ho_Obj1OrienArrow, hv_RecRStart1, hv_RecCStart1,
                        hv_RecREnd1, hv_RecCEnd1, hv_ObjOrienArrowSize, hv_ObjOrienArrowSize);
                    //**(4)成N个MinMeaRec1小测量句柄，规定MinMeaRec1分布方向与ROIRec方向同向
                    HTuple hv_MeaRec1Id = new HTuple();
                    HTuple end_val153 = hv_MeaRec1Num - 1;
                    HTuple step_val153 = 1;
                    for (hv_MeaRec1Id = 0; hv_MeaRec1Id.Continue(end_val153, step_val153); hv_MeaRec1Id = hv_MeaRec1Id.TupleAdd(step_val153))
                    {
                        //每个MinMeaRec1小测量句柄的中心位置和方向
                        HTuple hv_MinMeaRec1CentR = hv_RecRStart1 - (((hv_Obj1RadPhi.TupleSin()) * (hv_RecHalfW / hv_MeaRec1Num)) * ((2.0 * hv_MeaRec1Id) + 1));
                        HTuple hv_MinMeaRec1CentC = hv_RecCStart1 + (((hv_Obj1RadPhi.TupleCos()) * (hv_RecHalfW / hv_MeaRec1Num)) * ((2.0 * hv_MeaRec1Id) + 1));
                        HTuple hv_MinMeaRec1Phi = new HTuple();
                        if ((int)(hv_MeaRec1Orien) != 0)
                        {
                            hv_MinMeaRec1Phi = hv_Obj1RadPhi + ((new HTuple(90)).TupleRad());
                        }
                        else
                        {
                            hv_MinMeaRec1Phi = hv_Obj1RadPhi - ((new HTuple(90)).TupleRad());
                        }
                        //每个MinMeaRec1小测量句柄的半宽、半高
                        //半宽等于ROIRec半高
                        HTuple hv_MinMeaRec1HalfW = hv_RecHalfH.Clone();
                        //可自由设置
                        HTuple hv_MinMeaRec1HalfH = hv_MeaRec1HalfH.Clone();
                        //每个MinMeaRec1小测量句柄方向箭头的起点和终点
                        HTuple hv_MinMeaRec1RStart = hv_MinMeaRec1CentR + (hv_MinMeaRec1HalfW * (hv_MinMeaRec1Phi.TupleSin()
                            ));
                        HTuple hv_MinMeaRec1CStart = hv_MinMeaRec1CentC - (hv_MinMeaRec1HalfW * (hv_MinMeaRec1Phi.TupleCos()
                            ));
                        HTuple hv_MinMeaRec1REnd = hv_MinMeaRec1CentR - (hv_MinMeaRec1HalfW * (hv_MinMeaRec1Phi.TupleSin()
                            ));
                        HTuple hv_MinMeaRec1CEnd = hv_MinMeaRec1CentC + (hv_MinMeaRec1HalfW * (hv_MinMeaRec1Phi.TupleCos()
                            ));

                        ho_MinMeaRec1.Dispose();
                        HOperatorSet.GenRectangle2ContourXld(out ho_MinMeaRec1, hv_MinMeaRec1CentR,
                            hv_MinMeaRec1CentC, hv_MinMeaRec1Phi, hv_MinMeaRec1HalfW, hv_MinMeaRec1HalfH);
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ConcatObj(ho_MeaRec1, ho_MinMeaRec1, out ExpTmpOutVar_0);
                            ho_MeaRec1.Dispose();
                            ho_MeaRec1 = ExpTmpOutVar_0;
                        }
                        ho_MinMeaRec1OrienArrow.Dispose();
                        gen_arrow_contour_xld(out ho_MinMeaRec1OrienArrow, hv_MinMeaRec1RStart, hv_MinMeaRec1CStart,
                            hv_MinMeaRec1REnd, hv_MinMeaRec1CEnd, hv_MeaRecOrienArrowSize, hv_MeaRecOrienArrowSize);
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ConcatObj(ho_MeaRec1OrienArrow, ho_MinMeaRec1OrienArrow, out ExpTmpOutVar_0
                                );
                            ho_MeaRec1OrienArrow.Dispose();
                            ho_MeaRec1OrienArrow = ExpTmpOutVar_0;
                        }

                        HTuple hv_MeasureHandle = new HTuple();
                        HTuple hv_MinMeaRec1EdgeR = new HTuple();
                        HTuple hv_MinMeaRec1EdgeC = new HTuple();
                        HTuple hv_MinMeaRec1Amp = new HTuple();
                        HTuple hv_MinMeaRec1Dist = new HTuple();
                        HOperatorSet.GenMeasureRectangle2(hv_MinMeaRec1CentR, hv_MinMeaRec1CentC,
                            hv_MinMeaRec1Phi, hv_MinMeaRec1HalfW, hv_MinMeaRec1HalfH, hv_Width, hv_Height,
                            "nearest_neighbor", out hv_MeasureHandle);
                        HOperatorSet.MeasurePos(ho_Image, hv_MeasureHandle, hv_MeaRec1Sigma, hv_MeaRec1Threshold,
                            hv_MeaRec1Transition, hv_MeaRec1Select, out hv_MinMeaRec1EdgeR, out hv_MinMeaRec1EdgeC,
                            out hv_MinMeaRec1Amp, out hv_MinMeaRec1Dist);
                        HOperatorSet.CloseMeasure(hv_MeasureHandle);

                        //**针对每一个测量句柄，生成并收集测得的边缘线轮廓
                        HTuple hv_Num = (((new HTuple(hv_MinMeaRec1EdgeR.TupleLength())).TupleConcat(new HTuple(hv_MinMeaRec1EdgeC.TupleLength())))).TupleMin();
                        HTuple end_val184 = hv_Num - 1;
                        HTuple step_val184 = 1;
                        HTuple hv_i = new HTuple();
                        for (hv_i = 0; hv_i.Continue(end_val184, step_val184); hv_i = hv_i.TupleAdd(step_val184))
                        {
                            HTuple hv_RowStart = (hv_MinMeaRec1EdgeR.TupleSelect(hv_i)) + (hv_MinMeaRec1HalfH * (((hv_MinMeaRec1Phi - ((new HTuple(90)).TupleRad()
                                ))).TupleSin()));
                            HTuple hv_ColStart = (hv_MinMeaRec1EdgeC.TupleSelect(hv_i)) - (hv_MinMeaRec1HalfH * (((hv_MinMeaRec1Phi - ((new HTuple(90)).TupleRad()
                                ))).TupleCos()));
                            HTuple hv_RowEnd = (hv_MinMeaRec1EdgeR.TupleSelect(hv_i)) - (hv_MinMeaRec1HalfH * (((hv_MinMeaRec1Phi - ((new HTuple(90)).TupleRad()
                                ))).TupleSin()));
                            HTuple hv_ColEnd = (hv_MinMeaRec1EdgeC.TupleSelect(hv_i)) + (hv_MinMeaRec1HalfH * (((hv_MinMeaRec1Phi - ((new HTuple(90)).TupleRad()
                                ))).TupleCos()));
                            ho_MinMeaRec1Marks.Dispose();
                            HOperatorSet.GenContourPolygonXld(out ho_MinMeaRec1Marks, hv_RowStart.TupleConcat(
                                hv_RowEnd), hv_ColStart.TupleConcat(hv_ColEnd));
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.ConcatObj(ho_MeaRec1Mark, ho_MinMeaRec1Marks, out ExpTmpOutVar_0
                                    );
                                ho_MeaRec1Mark.Dispose();
                                ho_MeaRec1Mark = ExpTmpOutVar_0;
                            }
                        }

                        //记录结果，到二维数组中(第一个元素代表测量句柄索引)
                        hv_MeaRec1EdgeR[hv_MeaRec1Id] = hv_MinMeaRec1EdgeR.Clone();
                        hv_MeaRec1EdgeC[hv_MeaRec1Id] = hv_MinMeaRec1EdgeC.Clone();
                        hv_MeaRec1Amp[hv_MeaRec1Id] = hv_MinMeaRec1Amp.Clone();
                        hv_MeaRec1Dist[hv_MeaRec1Id] = hv_MinMeaRec1Dist.Clone();
                    }

                    if (isUseObj2)//如果使用抓边对象2
                    {
                        try
                        {
                            //**(5)生成MinMeaRec2分布方向箭头，也即相对ROIRec方向顺时针转90度(垂直)
                            HTuple hv_Obj2RadPhi = hv_Obj1RadPhi - ((new HTuple(90)).TupleRad());
                            HTuple hv_RecRStart2 = hv_RecCentR + (hv_RecHalfH * (hv_Obj2RadPhi.TupleSin()));
                            HTuple hv_RecCStart2 = hv_RecCentC - (hv_RecHalfH * (hv_Obj2RadPhi.TupleCos()));
                            HTuple hv_RecREnd2 = hv_RecCentR - (hv_RecHalfH * (hv_Obj2RadPhi.TupleSin()));
                            HTuple hv_RecCEnd2 = hv_RecCentC + (hv_RecHalfH * (hv_Obj2RadPhi.TupleCos()));
                            ho_Obj2OrienArrow.Dispose();
                            gen_arrow_contour_xld(out ho_Obj2OrienArrow, hv_RecRStart2, hv_RecCStart2,
                                hv_RecREnd2, hv_RecCEnd2, hv_ObjOrienArrowSize, hv_ObjOrienArrowSize);
                            //**(6)成N个MinMeaRec2小测量句柄，规定MinMeaRec2分布方向为相对MinMeaRec1分布方向顺时针转90度(垂直)
                            HTuple hv_MeaRec2Id = new HTuple();
                            HTuple end_val200 = hv_MeaRec2Num - 1;
                            HTuple step_val200 = 1;
                            for (hv_MeaRec2Id = 0; hv_MeaRec2Id.Continue(end_val200, step_val200); hv_MeaRec2Id = hv_MeaRec2Id.TupleAdd(step_val200))
                            {
                                //每个MinMeaRec2小测量句柄的中心位置和方向
                                HTuple hv_MinMeaRec2CentR = hv_RecRStart2 - (((hv_Obj2RadPhi.TupleSin()) * (hv_RecHalfH / hv_MeaRec2Num)) * ((2.0 * hv_MeaRec2Id) + 1));
                                HTuple hv_MinMeaRec2CentC = hv_RecCStart2 + (((hv_Obj2RadPhi.TupleCos()) * (hv_RecHalfH / hv_MeaRec2Num)) * ((2.0 * hv_MeaRec2Id) + 1));
                                HTuple hv_MinMeaRec2Phi = new HTuple();
                                if ((int)(hv_MeaRec2Orien) != 0)
                                {
                                    hv_MinMeaRec2Phi = hv_Obj2RadPhi + ((new HTuple(90)).TupleRad());
                                }
                                else
                                {
                                    hv_MinMeaRec2Phi = hv_Obj2RadPhi - ((new HTuple(90)).TupleRad());
                                }
                                //每个MinMeaRec2小测量句柄的半宽、半高
                                //半宽等于ROIRec半宽
                                HTuple hv_MinMeaRec2HalfW = hv_RecHalfW.Clone();
                                //可自由设置
                                HTuple hv_MinMeaRec2HalfH = hv_MeaRec2HalfH.Clone();

                                //每个MinMeaRec2小测量句柄方向箭头的起点和终点
                                HTuple hv_MinMeaRec2RStart = hv_MinMeaRec2CentR + (hv_MinMeaRec2HalfW * (hv_MinMeaRec2Phi.TupleSin()
                                    ));
                                HTuple hv_MinMeaRec2CStart = hv_MinMeaRec2CentC - (hv_MinMeaRec2HalfW * (hv_MinMeaRec2Phi.TupleCos()
                                    ));
                                HTuple hv_MinMeaRec2REnd = hv_MinMeaRec2CentR - (hv_MinMeaRec2HalfW * (hv_MinMeaRec2Phi.TupleSin()
                                    ));
                                HTuple hv_MinMeaRec2CEnd = hv_MinMeaRec2CentC + (hv_MinMeaRec2HalfW * (hv_MinMeaRec2Phi.TupleCos()
                                    ));
                                ho_MinMeaRec2.Dispose();
                                HOperatorSet.GenRectangle2ContourXld(out ho_MinMeaRec2, hv_MinMeaRec2CentR,
                                    hv_MinMeaRec2CentC, hv_MinMeaRec2Phi, hv_MinMeaRec2HalfW, hv_MinMeaRec2HalfH);
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.ConcatObj(ho_MeaRec2, ho_MinMeaRec2, out ExpTmpOutVar_0);
                                    ho_MeaRec2.Dispose();
                                    ho_MeaRec2 = ExpTmpOutVar_0;
                                }
                                ho_MinMeaRec2OrienArrow.Dispose();
                                gen_arrow_contour_xld(out ho_MinMeaRec2OrienArrow, hv_MinMeaRec2RStart, hv_MinMeaRec2CStart,
                                    hv_MinMeaRec2REnd, hv_MinMeaRec2CEnd, hv_MeaRecOrienArrowSize, hv_MeaRecOrienArrowSize);
                                {
                                    HObject ExpTmpOutVar_0;
                                    HOperatorSet.ConcatObj(ho_MeaRec2OrienArrow, ho_MinMeaRec2OrienArrow, out ExpTmpOutVar_0
                                        );
                                    ho_MeaRec2OrienArrow.Dispose();
                                    ho_MeaRec2OrienArrow = ExpTmpOutVar_0;
                                }

                                HTuple hv_MeasureHandle = new HTuple();
                                HTuple hv_MinMeaRec2EdgeR = new HTuple();
                                HTuple hv_MinMeaRec2EdgeC = new HTuple();
                                HTuple hv_MinMeaRec2Amp = new HTuple();
                                HTuple hv_MinMeaRec2Dist = new HTuple();
                                HOperatorSet.GenMeasureRectangle2(hv_MinMeaRec2CentR, hv_MinMeaRec2CentC,
                                    hv_MinMeaRec2Phi, hv_MinMeaRec2HalfW, hv_MinMeaRec2HalfH, hv_Width, hv_Height,
                                    "nearest_neighbor", out hv_MeasureHandle);
                                HOperatorSet.MeasurePos(ho_Image, hv_MeasureHandle, hv_MeaRec2Sigma, hv_MeaRec2Threshold,
                                    hv_MeaRec2Transition, hv_MeaRec2Select, out hv_MinMeaRec2EdgeR, out hv_MinMeaRec2EdgeC,
                                    out hv_MinMeaRec2Amp, out hv_MinMeaRec2Dist);
                                HOperatorSet.CloseMeasure(hv_MeasureHandle);

                                //**针对每一个测量句柄，生成并收集测得的边缘线轮廓
                                HTuple hv_Num = (((new HTuple(hv_MinMeaRec2EdgeR.TupleLength())).TupleConcat(new HTuple(hv_MinMeaRec2EdgeC.TupleLength()
                                    )))).TupleMin();
                                HTuple end_val231 = hv_Num - 1;
                                HTuple step_val231 = 1;
                                HTuple hv_i = new HTuple();
                                for (hv_i = 0; hv_i.Continue(end_val231, step_val231); hv_i = hv_i.TupleAdd(step_val231))
                                {
                                    HTuple hv_RowStart = (hv_MinMeaRec2EdgeR.TupleSelect(hv_i)) + (hv_MinMeaRec2HalfH * (((hv_MinMeaRec2Phi - ((new HTuple(90)).TupleRad()
                                        ))).TupleSin()));
                                    HTuple hv_ColStart = (hv_MinMeaRec2EdgeC.TupleSelect(hv_i)) - (hv_MinMeaRec2HalfH * (((hv_MinMeaRec2Phi - ((new HTuple(90)).TupleRad()
                                        ))).TupleCos()));
                                    HTuple hv_RowEnd = (hv_MinMeaRec2EdgeR.TupleSelect(hv_i)) - (hv_MinMeaRec2HalfH * (((hv_MinMeaRec2Phi - ((new HTuple(90)).TupleRad()
                                        ))).TupleSin()));
                                    HTuple hv_ColEnd = (hv_MinMeaRec2EdgeC.TupleSelect(hv_i)) + (hv_MinMeaRec2HalfH * (((hv_MinMeaRec2Phi - ((new HTuple(90)).TupleRad()
                                        ))).TupleCos()));
                                    ho_MinMeaRec2Marks.Dispose();
                                    HOperatorSet.GenContourPolygonXld(out ho_MinMeaRec2Marks, hv_RowStart.TupleConcat(
                                        hv_RowEnd), hv_ColStart.TupleConcat(hv_ColEnd));
                                    {
                                        HObject ExpTmpOutVar_0;
                                        HOperatorSet.ConcatObj(ho_MeaRec2Mark, ho_MinMeaRec2Marks, out ExpTmpOutVar_0
                                            );
                                        ho_MeaRec2Mark.Dispose();
                                        ho_MeaRec2Mark = ExpTmpOutVar_0;
                                    }
                                }

                                //记录结果，到二维数组中(第一个元素代表测量句柄索引)
                                hv_MeaRec2EdgeR[hv_MeaRec2Id] = hv_MinMeaRec2EdgeR.Clone();
                                hv_MeaRec2EdgeC[hv_MeaRec2Id] = hv_MinMeaRec2EdgeC.Clone();
                                hv_MeaRec2Amp[hv_MeaRec2Id] = hv_MinMeaRec2Amp.Clone();
                                hv_MeaRec2Dist[hv_MeaRec2Id] = hv_MinMeaRec2Dist.Clone();
                            }
                        }
                        catch (HalconException hEx)
                        {
                            strErrMsg = "对象2抓边运行出错，原因：" + hEx;
                            return -1;
                        }

                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            finally
            {
                ho_MinMeaRec1.Dispose();
                ho_MinMeaRec1OrienArrow.Dispose();
                ho_MinMeaRec1Marks.Dispose();
                ho_MinMeaRec2.Dispose();
                ho_MinMeaRec2OrienArrow.Dispose();
                ho_MinMeaRec2Marks.Dispose();
            }
            return 0;
        }

        /***功能：找边缘点集、点集拟合直线、(垂直直线交点)直角角点---(利用矩形垂直双向抓边+离散点拟合直线）***
           *******输入参数：
           * 参1 输入图像变量
           * 参2：输入是否拟合直线,ture表示执行拟合直线,false表不执行直线拟合仅取边缘点集。
           * 参3：输入是否使用抓边对象2,ture表示启用抓边对象2,false表不使用,当执行拟合直线且启用对象2时则执行找直角角点
           * 参4~10：输入矩形区域ROIRec：中心坐标、弧度角、矩形半宽、半高、
             * 小测量句柄方向箭头大小、测量对象1和对象2的分布方向箭头大小
           * 参11~17：输入句柄数量、光滑值、边缘振幅阈值、小测量句柄的(边缘宽)半高、明暗方向("all"、"negative"、"positive")、
             * 边缘选取("all"、"first"、"last")、测量句柄方向,ture:相对MinMeaRec1分布方向逆时针转90度方向，否则顺时针90度方向
           * 参18~24：输入同上
          *参25~29：
          *输入拟合算法：tukey、huber、regression、gauss、drop,默认"tukey"
          *输入参与拟合最多点数，默认-1，-1表示全部参与,该值>=2
          *输入拟合轮廓起终点之间忽略(裁剪)点数，默认0，该值>=0
          *输入循环计算(迭代)次数，默认5，该值>=0
          *输入离群值忽略(剪切)因子,越小忽略越多，默认2.0,该值>0
          *参30~34：同上
          ***********输出参数：
         *参35~48：输出感兴趣区域ROIRec轮廓、抓边对象1的MinMeaRec1分布方向箭头、每个MinMeaRec1小测量句柄矩形集合、
             *每个MinMeaRec1小测量句柄矩形矩形的方向箭头集合、每个MinMeaRec1小测量句柄抓取的边缘集合,输出数值结果集合，
             *输出，拟合得到的直线1(以xld形式显示)、拟合得到直线1起点和终点坐标。
         *参49~61：抓边对象2的MinMeaRec2分布方向箭头、每个MinMeaRec2小测量句柄矩形集合、
             *每个MinMeaRec2小测量句柄矩形矩形的方向箭头集合、每个MinMeaRec2小测量句柄抓取的边缘集合,输出数值结果集合，
             *输出，拟合得到的直线2(以xld形式显示)、拟合得到直线2起点和终点坐标。
         *参62~64：输出，交点坐标和是否重叠标志，
         *当交点Row:=[],Col:=[]时两直线平行,当IsOverlapping:=1时两线共线,此时一定Row:=[],Col:=[]
         *参65：strErrMsg返回错误消息
         *返回值： 成功返回0，异常出错返回-1  
          最近更改日期:2019-7-17
        *****************************************************/
        public int FindRecMeaPosMaxTwo90AngleEdgeOrLineOrCrossPoint(HObject ho_Image, bool isFitLine, bool isUseObj2,
            HTuple hv_RecCentR, HTuple hv_RecCentC, HTuple hv_RecRadPhi, HTuple hv_RecHalfW, HTuple hv_RecHalfH, HTuple hv_MeaRecOrienArrowSize, HTuple hv_ObjOrienArrowSize,
            HTuple hv_MeaRec1Num, HTuple hv_MeaRec1Sigma, HTuple hv_MeaRec1Threshold, HTuple hv_MeaRec1HalfH, HTuple hv_MeaRec1Transition, HTuple hv_MeaRec1Select, HTuple hv_MeaRec1Orien,
            HTuple hv_MeaRec2Num, HTuple hv_MeaRec2Sigma, HTuple hv_MeaRec2Threshold, HTuple hv_MeaRec2HalfH, HTuple hv_MeaRec2Transition, HTuple hv_MeaRec2Select, HTuple hv_MeaRec2Orien,
            HTuple hv_Algorithm1, HTuple hv_MaxNumPoints1, HTuple hv_ClipNumPoints1, HTuple hv_ForNum1, HTuple hv_ClipFactor1,
            HTuple hv_Algorithm2, HTuple hv_MaxNumPoints2, HTuple hv_ClipNumPoints2, HTuple hv_ForNum2, HTuple hv_ClipFactor2,
            ref HObject ho_ROIRec, ref HObject ho_Obj1OrienArrow, ref HObject ho_MeaRec1, ref HObject ho_MeaRec1OrienArrow, ref HObject ho_MeaRec1Mark,
            ref HTuple[] hv_MeaRec1EdgeR, ref HTuple[] hv_MeaRec1EdgeC, ref HTuple[] hv_MeaRec1Amp, ref HTuple[] hv_MeaRec1Dist,
            ref HObject ho_XldLine1, ref HTuple hv_LineRowBegin1, ref HTuple hv_LineColBegin1, ref HTuple hv_LineRowEnd1, ref HTuple hv_LineColEnd1,
            ref HObject ho_Obj2OrienArrow, ref HObject ho_MeaRec2, ref HObject ho_MeaRec2OrienArrow, ref HObject ho_MeaRec2Mark,
            ref HTuple[] hv_MeaRec2EdgeR, ref HTuple[] hv_MeaRec2EdgeC, ref HTuple[] hv_MeaRec2Amp, ref HTuple[] hv_MeaRec2Dist,
            ref HObject ho_XldLine2, ref HTuple hv_LineRowBegin2, ref HTuple hv_LineColBegin2, ref HTuple hv_LineRowEnd2, ref HTuple hv_LineColEnd2,
            //直线1、2的交点:当交点Row:=[]、Col:=[]时两直线平行，当IsOverlapping:=1时两线共线，此时一定Row:=[]、Col:=[]
            ref HTuple hv_CrossPointR, ref HTuple hv_CrossPointC, ref HTuple hv_IsOverlapping,
            ref string strErrMsg)
        {
            strErrMsg = "";

            //*******************输出参数初始化******************
            HOperatorSet.GenEmptyObj(out ho_ROIRec);//输出感兴趣区域ROIRec轮廓
            //**抓边对象1:输出参数
            HOperatorSet.GenEmptyObj(out ho_Obj1OrienArrow);//抓边对象1的MinMeaRec1分布方向箭头,单个
            HOperatorSet.GenEmptyObj(out ho_MeaRec1);//每个MinMeaRec1小测量句柄矩形,集合
            HOperatorSet.GenEmptyObj(out ho_MeaRec1OrienArrow);//每个MinMeaRec1小测量句柄矩形矩形的方向箭头,集合
            HOperatorSet.GenEmptyObj(out ho_MeaRec1Mark);//每个MinMeaRec1小测量句柄抓取的边缘,集合
            //**抓边对象2:输出参数
            HOperatorSet.GenEmptyObj(out ho_Obj2OrienArrow);//抓边对象2的MinMeaRec2分布方向箭头,单个
            HOperatorSet.GenEmptyObj(out ho_MeaRec2);//每个MinMeaRec2小测量句柄矩形，集合
            HOperatorSet.GenEmptyObj(out ho_MeaRec2OrienArrow);//每个MinMeaRec2小测量句柄矩形矩形的方向箭头,集合
            HOperatorSet.GenEmptyObj(out ho_MeaRec2Mark);//每个MinMeaRec2小测量句柄矩形矩形的方向箭头，集合
            //**拟合直线1、2、
            HOperatorSet.GenEmptyObj(out ho_XldLine1);
            hv_LineRowBegin1 = new HTuple();
            hv_LineColBegin1 = new HTuple();
            hv_LineRowEnd1 = new HTuple();
            hv_LineColEnd1 = new HTuple();
            HOperatorSet.GenEmptyObj(out ho_XldLine2);
            hv_LineRowBegin2 = new HTuple();
            hv_LineColBegin2 = new HTuple();
            hv_LineRowEnd2 = new HTuple();
            hv_LineColEnd2 = new HTuple();
            //**交点
            hv_IsOverlapping = new HTuple();
            hv_CrossPointR = new HTuple();
            hv_CrossPointC = new HTuple();

            try
            {
                try
                {
                    int runResult = RecMeaPosMaxTwo90AngleEdge(ho_Image, isUseObj2,
                            hv_RecCentR, hv_RecCentC, hv_RecRadPhi, hv_RecHalfW, hv_RecHalfH, hv_MeaRecOrienArrowSize, hv_ObjOrienArrowSize,
                            hv_MeaRec1Num, hv_MeaRec1Sigma, hv_MeaRec1Threshold, hv_MeaRec1HalfH, hv_MeaRec1Transition, hv_MeaRec1Select, hv_MeaRec1Orien,
                            hv_MeaRec2Num, hv_MeaRec2Sigma, hv_MeaRec2Threshold, hv_MeaRec2HalfH, hv_MeaRec2Transition, hv_MeaRec2Select, hv_MeaRec2Orien,
                            ref ho_ROIRec, ref ho_Obj1OrienArrow, ref ho_MeaRec1, ref ho_MeaRec1OrienArrow, ref ho_MeaRec1Mark,
                            ref hv_MeaRec1EdgeR, ref hv_MeaRec1EdgeC, ref hv_MeaRec1Amp, ref hv_MeaRec1Dist,
                            ref ho_Obj2OrienArrow, ref ho_MeaRec2, ref ho_MeaRec2OrienArrow, ref ho_MeaRec2Mark,
                            ref hv_MeaRec2EdgeR, ref hv_MeaRec2EdgeC, ref hv_MeaRec2Amp, ref hv_MeaRec2Dist, ref strErrMsg);
                    if (runResult == 0)//成功
                    {
                        if (isFitLine)//是否执行拟合直线
                        {
                            /******收集对象1的点,然后拟合直线1*****/
                            HTuple hv_PRows1 = new HTuple();
                            HTuple hv_PCols1 = new HTuple();
                            //C#中GetLength(0)表获取数组第一维所有元素数，Length表获取所有维度所有元素总数
                            //C#中Length获得的是数组元素总数，比如string[] str = new string[5],
                            //即使每个元素为null或[]Length结果仍为5,这点与halcon中TupleLength()不同
                            for (int index = 0; index < hv_MeaRec1EdgeR.Length; index++)
                            {
                                hv_PRows1 = hv_PRows1.TupleConcat(hv_MeaRec1EdgeR[index]);
                                hv_PCols1 = hv_PCols1.TupleConcat(hv_MeaRec1EdgeC[index]);
                            }
                            //根据对象1的点，拟合直线1
                            int runLine1 = PointsFitLine(hv_PRows1, hv_PCols1, hv_Algorithm1, hv_MaxNumPoints1, hv_ClipNumPoints1, hv_ForNum1, hv_ClipFactor1,
                                ref ho_XldLine1, ref hv_LineRowBegin1, ref hv_LineColBegin1, ref hv_LineRowEnd1, ref hv_LineColEnd1, ref strErrMsg);
                            if (runLine1 == -1)//失败
                            {
                                strErrMsg = "直线1拟合出错！";
                                return -1;
                            }
                        }

                        if (isUseObj2 && isFitLine)//如果使用对象2，且执行拟合直线
                        {
                            /******收集对象2的点,然后拟合直线2*****/
                            HTuple hv_PRows2 = new HTuple();
                            HTuple hv_PCols2 = new HTuple();
                            //C#中GetLength(0)表获取数组第一维所有元素数，Length表获取所有维度所有元素总数
                            //C#中Length获得的是数组元素总数，比如string[] str = new string[5],
                            //即使每个元素为null或[]Length结果仍为5,这点与halcon中TupleLength()不同
                            for (int index = 0; index < hv_MeaRec2EdgeR.Length; index++)
                            {
                                hv_PRows2 = hv_PRows2.TupleConcat(hv_MeaRec2EdgeR[index]);
                                hv_PCols2 = hv_PCols2.TupleConcat(hv_MeaRec2EdgeC[index]);
                            }
                            //根据对象2的点，拟合直线2
                            int runLine2 = PointsFitLine(hv_PRows2, hv_PCols2, hv_Algorithm2, hv_MaxNumPoints2, hv_ClipNumPoints2, hv_ForNum2, hv_ClipFactor2,
                                ref ho_XldLine2, ref hv_LineRowBegin2, ref hv_LineColBegin2, ref hv_LineRowEnd2, ref hv_LineColEnd2, ref strErrMsg);
                            if (runLine2 == 0)//成功
                            {
                                //求直线1和直线2的交点坐标
                                try
                                {
                                    //当交点Row:=[]、Col:=[]时两直线平行，当IsOverlapping:=1时两线共线，此时一定Row:=[]、Col:=[]
                                    //等效,intersection_ll已经过时了，只是为了向后兼容才提供的。新应用程序应该使用工具/几何章节的intersection_lines操作符。
                                    HOperatorSet.IntersectionLines(hv_LineRowBegin1, hv_LineColBegin1, hv_LineRowEnd1, hv_LineColEnd1,
                                      hv_LineRowBegin2, hv_LineColBegin2, hv_LineRowEnd2, hv_LineColEnd2,
                                      out hv_CrossPointR, out hv_CrossPointC, out hv_IsOverlapping);
                                }
                                catch (HalconException hEx)
                                {
                                    strErrMsg = "找直线1和直线2的交点出错！原因：" + hEx;
                                    return -1;
                                }
                            }
                            else if (runLine2 == -1)//失败
                            {
                                strErrMsg = "直线2拟合出错！";
                                return -1;
                            }
                        }
                    }
                    else if (runResult == -1)//失败
                    {
                        strErrMsg = "矩形内取垂直边缘点集运行出错！原因：" + strErrMsg;
                        return -1;
                    }
                }
                catch (HalconException hEx)
                {
                    strErrMsg = "" + hEx;
                    return -1;
                }
            }
            catch (Exception Ex)
            {
                strErrMsg = "" + Ex;
                return -1;
            }
            finally
            {
            }
            return 0;
        }

        //(20.基础)圆形区域(1条)中轴线抓边工具(边缘对)----用途:抓边-->输出坐标、振幅、边距
        //(基础功能，可用于测距离、找中点、找任意角(交)点、直角角(交)点、拟合直线...)


        //(21.拓展)圆形区域(2条)中轴线方向抓边工具(非边缘对)----用途：找任意夹角角点-->输出角点坐标



        #endregion



        #region (自由设置高度范围内)沿轮廓线(直线、圆弧...)方向


        #endregion




        #endregion

        #region 测量定位相关---基于弧形测量句柄的(非边缘对、边缘对)抓边工具


        #endregion

        #endregion

    }
}
