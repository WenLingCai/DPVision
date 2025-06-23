using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace DPVision.Common
{
    /// <summary>
    /// ini文件操作类
    /// </summary>
    public class IniHelper
    {
        /// <summary>
        /// ini文件存储路径
        /// </summary>
        public string Filepath;

        /// <summary>
        /// 类的构造函数，传递INI文件名 
        /// </summary>
        /// <param name="input">输入文件名</param>
        public IniHelper(string input)
        {
            if (input != "")
            {
                Filepath = input;
            }
            else
            {
               
            }
        }

        #region "操作"

        #region "读"
        /// 读取INI文件
        /// </summary>
        /// <param name="section">段落</param>
        /// <param name="key">键</param>
        /// <returns>返回的键值</returns>
        public static string ReadValue(string section, string key, string path, string defautvalue)
        {
            StringBuilder temp = new StringBuilder(1024);
            int i = GetPrivateProfileString(section, key, "", temp, 1024, path);
            string[] s = temp.ToString().Split(new char[] { '%' });
            if (s[0] != "")
            {
                return s[0].ToString();
            }
            else
            {
                //if (defautvalue != null && defautvalue.Length > 0)
                //{
                //    SetContentValue(filename, Section, key, defautvalue);
                //}
                return defautvalue;
            }
        }
        /// 读取INI文件
        /// </summary>
        /// <param name="Section">段，格式[]</param>
        /// <param name="Key">键</param>
        /// <returns>返回byte类型的section组或键值组</returns>
        public static byte[] IniReadValues(string section, string key, string path)
        {
            byte[] temp = new byte[255];
            int i = GetPrivateProfileString(section, key, "", temp, 255, path);
            return temp;
        }



        public static Dictionary<string, string> ReadSection(string section, string path)
        {
            var result = new Dictionary<string, string>();
            byte[] buffer = new byte[2048]; // 缓冲区大小

            // 读取节点下的所有键值对
            int length = GetPrivateProfileSection(section, buffer, buffer.Length, path);

            if (length > 0)
            {
                string[] keyValuePairs = Encoding.Unicode.GetString(buffer, 0, length).Split('\0');
                foreach (var pair in keyValuePairs)
                {
                    if (!string.IsNullOrEmpty(pair))
                    {
                        string[] parts = pair.Split(new[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            result[parts[0]] = parts[1];
                        }
                    }
                }
            }

            return result;
        }
        #endregion

        #region "写"
        /// 写INI文件
        /// </summary>
        /// <param name="section">段落</param>
        /// <param name="key">键</param>
        /// <param name="iValue">值</param>
        public static void WriteValue(string section, string key, string value,string path)
        {
            WritePrivateProfileString(section, key, value, path);
        }

        #endregion

        #region "删除"
        /// <summary>
        /// 删除指定字段
        /// </summary>
        /// <param name="sectionName">段落</param>
        /// <param name="keyName">键</param>
        public static void iniDelete(string sectionName, string keyName, string path)
        {
            WritePrivateProfileString(sectionName, keyName, null, path);
        }
        /// <summary>
        /// 删除字段重载
        /// </summary>
        /// <param name="sectionName">段落</param>
        public void iniDelete(string sectionName)
        {
            WritePrivateProfileString(sectionName, null, null, this.Filepath);
        }
        /// <summary>
        /// 删除ini文件下所有段落
        /// </summary>
        public void ClearAllSection(string path)
        {
            WriteValue(null, null, null, path);
        }
        /// <summary>
        /// 删除ini文件下personal段落下的所有键
        /// </summary>
        /// <param name="Section"></param>
        public void ClearSection(string Section, string path)
        {
            WriteValue(Section, null, null, path);
        }
        #endregion
        #endregion

        #region "API"
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, byte[] retVal, int size, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileSection(string section, byte[] retVal, int size, string filePath);
        #endregion
    }
}
