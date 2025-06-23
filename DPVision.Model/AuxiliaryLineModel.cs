using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPVision.Model
{
    public class AuxiliaryLineModel
    {

        public bool bDrawCrossFlg = false;              //是否画辅助十字中心
        public int iCrossWidth = 3;                     //十字中心线宽(整数)范围(1<=Width <= 2000)

        public Color CrossColor= Color.White;
        public float fPointSize = 5;                    //刻度点大小
        public float fFontSize = 9;                     //刻度值字体大小
        public float fMMInterval = 1;                   //刻度值间距,单位mm
        public int iGetMMPerPixType = 0;                //获取"单位像素距离"的方式,0:手动输入，1：九点标定数据
        public float fMMPerPix = 0.02f;                 //"单位像素距离"mm/pix

        public bool bDrawCircleFlg = false;             //是否画辅助圆
        public float fCirR1 = 1;                        //圆形1半径，单位mm
        public float fCirR2 = 1;                        //圆形2半径，单位mm

        public bool bDrawRectangleFlg = false;          //是否画辅助矩形
        public float fRecW = 1;                         //矩形宽，单位mm
        public float fRecH = 1;                         //矩形高，单位mm
    }
}
