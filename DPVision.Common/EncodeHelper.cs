﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DPVision.Common
{
    public class EncodeHelper
    { 
        /// <summary>转换文件格式为其他编码，比如：Unicode编码  
       /// </summary> 
       /// <param name=“data“></param> 
       /// <returns></returns>      
        private static bool IsUTF8Bytes(byte[] data)
        {
            // //判断是否是不带 BOM 的 UTF8 格式 
            int charByteCounter = 1; //计算当前正分析的字符应还有的字节数 
            byte curByte; //当前分析的字节. 
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前 
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X 
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1 
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }
        /// <summary>
        /// 通过给定的文件流，判断文件的编码类型 
        /// </summary> 
        /// <param name=“fs“>文件流</param> 
        /// <returns>文件的编码类型</returns> 
        public static System.Text.Encoding GetType(FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM 
            Encoding reVal = Encoding.Default;

            try
            {
                BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
                int i;
                int.TryParse(fs.Length.ToString(), out i);
                byte[] ss = r.ReadBytes(i);
                if (ss.Length > 2)
                {
                    if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
                    {
                        reVal = Encoding.UTF8;
                    }
                    else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
                    {
                        reVal = Encoding.BigEndianUnicode;
                    }
                    else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
                    {
                        reVal = Encoding.Unicode;
                    }
                }
                r.Close();
            }
            catch { };
            return reVal;

        }
        public static System.Text.Encoding GetType(string FILE_NAME)
        {
            FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read);
            Encoding r = GetType(fs);
            fs.Close();
            return r;
        }
        /// <summary>
        /// 文件编码转换
        /// </summary>
        /// <param name="sourceFile">源文件</param>
        /// <param name="destFile">目标文件，如果为空，则覆盖源文件</param>
        /// <param name="targetEncoding">目标编码</param>
        static void ConvertFileEncoding(string sourceFile, string destFile, System.Text.Encoding targetEncoding)
        {
            destFile = string.IsNullOrEmpty(destFile) ? sourceFile : destFile;
            System.IO.File.WriteAllText(destFile,
                  System.IO.File.ReadAllText(sourceFile, GetType(sourceFile)),
            targetEncoding);
        }
        //入口：转换文件格式为Unicode编码,防止中文乱码
        public void ConVertToUnicode(string fileFullName)
        {
            if (File.Exists(fileFullName))
            {
                if (GetType(fileFullName) != System.Text.Encoding.Unicode)
                {
                    ConvertFileEncoding(fileFullName, null, Encoding.Unicode);
                }
            }
        }
    }
}
