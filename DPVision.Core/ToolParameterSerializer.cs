using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace DPVision.Core
{
    public static class ToolParameterSerializer
    {
        private static Type[] GetAllKnownTypes()
        {
            var allTypes = new List<Type>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] tArr;
                try { tArr = asm.GetTypes(); }
                catch { continue; }
                foreach (var t in tArr)
                {
                    if (t.IsClass && !t.IsAbstract)
                        allTypes.Add(t);
                }
            }
            return allTypes.ToArray();
        }

        public static string ToXml(object obj)
        {
            var knownTypes = GetAllKnownTypes();
            var serializer = new XmlSerializer(obj.GetType(), knownTypes);
            using (var sw = new StringWriter())
            {
                serializer.Serialize(sw, obj);
                return sw.ToString();
            }
        }

        public static object FromXml(string xml, Type t)
        {
            var knownTypes = GetAllKnownTypes();
            var serializer = new XmlSerializer(t, knownTypes);
            using (var sr = new StringReader(xml))
            {
                return serializer.Deserialize(sr);
            }
        }
    }
}
