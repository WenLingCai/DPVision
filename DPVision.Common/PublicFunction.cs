using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DPVision.Common
{
   public class PublicFunction
    {

        /// <summary>
        /// 根据名称获取控件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static object GetWidgetByName(string name, object obj)
        {
            FieldInfo fieldInfo = obj.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(obj);
            }
            return null;
        }


        /// <summary>
        /// 根据名称获取控件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetWidgetByName<T>(string name,T control, object obj)
        {
            FieldInfo fieldInfo = obj.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (fieldInfo != null)
            {
                if(fieldInfo.GetValue(obj)!=null)
                {
                    control = (T)fieldInfo.GetValue(obj);
                }
            }
            return control;
        }



        /// <summary>
        /// 判断字符串是否为数值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumeric(string str) //接收一个string类型的参数,保存到str里
        {
            if (str == null || str.Length == 0)    //验证这个参数是否为空
                return false;                           //是，就返回False
            ASCIIEncoding ascii = new ASCIIEncoding();//new ASCIIEncoding 的实例
            byte[] bytestr = ascii.GetBytes(str);         //把string类型的参数保存到数组里

            foreach (byte c in bytestr)                   //遍历这个数组里的内容
            {
                if (c < 48 || c > 57)                          //判断是否为数字
                {
                    return false;                              //不是，就返回False
                }
            }
            return true;                                        //是，就返回True
        }

    }
}
