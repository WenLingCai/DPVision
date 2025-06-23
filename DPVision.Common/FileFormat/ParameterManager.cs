using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace DPVision.Common.FileFormat
{

    public class ParameterManager
    {
        private readonly Dictionary<string, string> parameters = new Dictionary<string, string>();

        // 读基础类型
        public T Get<T>(string key, T defaultValue = default)
        {
            if (parameters.TryGetValue(key, out var value))
            {
                try
                {
                    // 针对常用类型做特殊处理
                    if (typeof(T).IsEnum)
                        return (T)Enum.Parse(typeof(T), value, true);
                    if (typeof(T) == typeof(bool))
                        return (T)(object)bool.Parse(value);
                    if (typeof(T) == typeof(int))
                        return (T)(object)int.Parse(value, CultureInfo.InvariantCulture);
                    if (typeof(T) == typeof(float))
                        return (T)(object)float.Parse(value, CultureInfo.InvariantCulture);
                    if (typeof(T) == typeof(double))
                        return (T)(object)double.Parse(value, CultureInfo.InvariantCulture);
                    if (typeof(T) == typeof(DateTime))
                        return (T)(object)DateTime.Parse(value, CultureInfo.InvariantCulture);
                    // 其它类型
                    return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
                }
                catch { return defaultValue; }
            }
            return defaultValue;
        }

        // 写
        public void Set<T>(string key, T value)
        {
            try
            {
                if (value == null)
                    parameters[key] = "";
                else if (value is IFormattable formattable)
                    parameters[key] = formattable.ToString(null, CultureInfo.InvariantCulture);
                else
                    parameters[key] = value.ToString();
            }
            catch
            {

            }
        
        }

        /// <summary>
        /// 将字典转为XmlElement（根为Parameters，每个Parameter为子节点）
        /// </summary>
        public XmlElement DictionaryToXmlElement(string nodename)
        {
            XmlDocument doc =new XmlDocument();
            var root = doc.CreateElement(nodename);
            foreach (var kv in parameters)
            {
                var elem = doc.CreateElement("Parameter");
                elem.SetAttribute("Name", kv.Key);
                elem.SetAttribute("Value", kv.Value ?? "");
                root.AppendChild(elem);
            }
            return root;
        }

        /// <summary>
        /// 从XmlNodeList还原为字典（支持/Parameters/Parameter结构）
        /// </summary>
        public Dictionary<string, string> XmlNodeListToDictionary(XmlNodeList nodeList, string nodename)
        {
            
            var dict = new Dictionary<string, string>();
            foreach (XmlElement elem in nodeList)
            {
                
                if (elem.Name == nodename)
                {
                    string name = elem.GetAttribute("Name");
                    string value = elem.GetAttribute("Value");
                    dict[name] = value;
                }
            }
            return dict;
        }

        /// <summary>
        /// 从XmlElement根节点直接还原为字典
        /// </summary>
        public Dictionary<string, string> XmlElementToDictionary(XmlElement root, string nodename)
        {
            return XmlNodeListToDictionary(root.SelectNodes("Parameter"), nodename);
        }

        // 可选：索引器调用
        public string this[string key]
        {
            get => parameters.TryGetValue(key, out var v) ? v : null;
            set => parameters[key] = value;
        }

        // 可选：获取所有参数副本
        public Dictionary<string, string> ToDictionary() => new Dictionary<string, string>(parameters);
    }
}
