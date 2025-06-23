using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPVision.Common
{
    /// <summary>
    /// 十六进制 和string转换
    /// </summary>
    public class HexTool
    {
        #region HexToByte
        /// <summary>
        /// method to convert hex string into a byte array msg 格式为 68 74 74 70 3A 2F 2F 77 77 
        /// </summary>
        /// <param name="msg">string to convert</param>
        /// <returns>a byte array</returns>
        public static byte[] HexToByte(string msg)
        {
            //remove any spaces from the string
            msg = msg.Replace(" ", "");
            //create a byte array the length of the
            //divided by 2 (Hex is 2 characters in length)
            byte[] comBuffer = new byte[msg.Length / 2];
            //loop through the length of the provided string
            for (int i = 0; i < msg.Length; i += 2)
                //convert each set of 2 characters to a byte
                //and add to the array
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            //return the array
            return comBuffer;
        }
        #endregion

        /// <summary>
        /// C#将IP地址转为长整形
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static long IpToNumber(string ip)
        {
            string[] arr = ip.Split('.');
            return 256 * 256 * 256 * long.Parse(arr[0]) + 256 * 256 * long.Parse(arr[1]) + 256 * long.Parse(arr[2]) + long.Parse(arr[3]);
        }
        public static bool IsIpAddress(string ip)
        {
            System.Net.IPAddress ipAddress = null;
            bool isValidIp = System.Net.IPAddress.TryParse(ip, out ipAddress);
            return isValidIp;
        }
        public static string StrToHexStr(string mStr) //返回处理后的十六进制字符串
        {
            string str=  BitConverter.ToString(
            ASCIIEncoding.Default.GetBytes(mStr)).Replace("-", " ");

            return str;
        } /* StrToHex */


        public static string HexStrToStr(string mHex) // 返回十六进制代表的字符串
        {
            try
            {
                mHex = mHex.Replace(" ", "");
                if (mHex.Length <= 0) return "";
                byte[] vBytes = new byte[mHex.Length / 2];
                for (int i = 0; i < mHex.Length; i += 2)
                    if (!byte.TryParse(mHex.Substring(i, 2), NumberStyles.HexNumber, null, out vBytes[i / 2]))
                        vBytes[i / 2] = 0;
                return ASCIIEncoding.Default.GetString(vBytes);
            }
            catch (Exception)
            {
                 Debug.WriteLine($"无法将十六进制的[{mHex}]转换为string");
                return "";
            }
      
        } /* HexToStr */

        #region ByteToHex
        /// <summary>
        /// method to convert a byte array into a hex string
        /// </summary>
        /// <param name="comByte">byte array to convert</param>
        /// <returns>a hex string</returns>
        public static string ByteToHex(byte[] comByte)
        {
            //create a new StringBuilder object
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            //loop through each byte in the array
            foreach (byte data in comByte)
                //convert the byte to a string and add to the stringbuilder
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
            //return the converted value
            return builder.ToString().ToUpper();
        }
        #endregion



        /// <summary>
        /// 16进制字符转换为字符串
        /// </summary>
        /// <param name="hs"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string HexStringToString(string hs, Encoding encode)
        {
            string strTemp = "";
            byte[] b = new byte[hs.Length / 2];
            for (int i = 0; i < hs.Length / 2; i++)
            {
                strTemp = hs.Substring(i * 2, 2);
                b[i] = Convert.ToByte(strTemp, 16);
            }
            //按照指定编码将字节数组变为字符串
            return encode.GetString(b);
        }
        /// <summary>
        /// 字符串转换为16进制字符
        /// </summary>
        /// <param name="s"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string StringToHexString(string s, Encoding encode)
        {
            byte[] b = encode.GetBytes(s);//按照指定编码将string编程字节数组
            string result = string.Empty;
            for (int i = 0; i < b.Length; i++)
            {
                result += b[i].ToString("X2");
            }
            return result;
        }
        public static byte[] strToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += "";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }

        public static string ByteToHexStr(byte[] buffer)
        {
            string Str = string.Empty;
            for (int i = 0; i < buffer.Length; i++)
            {
                Str += buffer[i].ToString("X2");
            }
            return Str;
        }
        private static List<char> hexCharList = new List<char>()
            {
                '0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'
            };
        public static byte[] HexStringToBytes(string hex)
        {
            hex = hex.ToUpper();

            MemoryStream ms = new MemoryStream();

            for (int i = 0; i < hex.Length; i++)
            {
                if ((i + 1) < hex.Length)
                {
                    if (hexCharList.Contains(hex[i]) && hexCharList.Contains(hex[i + 1]))
                    {
                        // 这是一个合格的字节数据
                        ms.WriteByte((byte)(hexCharList.IndexOf(hex[i]) * 16 + hexCharList.IndexOf(hex[i + 1])));
                        i++;
                    }
                }
            }

            byte[] result = ms.ToArray();
            ms.Dispose();
            return result;
        }
        public static string ByteToHexString(byte[] InBytes, char segment)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte InByte in InBytes)
            {
                if (segment == 0) sb.Append(string.Format("{0:X2}", InByte));
                else sb.Append(string.Format("{0:X2}{1}", InByte, segment));
            }

            if (segment != 0 && sb.Length > 1 && sb[sb.Length - 1] == segment)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

    }
}
