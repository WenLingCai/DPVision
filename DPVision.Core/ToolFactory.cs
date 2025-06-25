using DPVision.Common;
using DPVision.Model.Tool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DPVision.Core
{
    public class ToolFactory
    {

        private static readonly Dictionary<string, ITool> _tools = new Dictionary<string, ITool>();
        private static readonly Dictionary<string, Type> _toolTypes = new Dictionary<string, Type>();
        private static readonly Dictionary<(string ToolType, string UIVariant), Type> ToolUIs = new Dictionary<(string ToolType, string UIVariant), Type>();

        public static void LoadUIPlugins(string pluginDir)
        {
            foreach (var dll in Directory.GetFiles(pluginDir, "*.dll"))
            {
                var asm = Assembly.LoadFrom(dll);
                foreach (var t in asm.GetTypes())
                {
                    if (typeof(IToolUI).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
                    {
                        var inst = (IToolUI)Activator.CreateInstance(t);
                        ToolUIs[(inst.ToolType, inst.UIVariant)] = t;
                    }
                }
            }
        }

        public List<string> GetAvailableUIVariants(string toolType)
        {
           return ToolUIs.Keys.Where(k => k.ToolType == toolType).Select(k => k.UIVariant).ToList();
        }
         

        // 获取UI实例
        public static IToolUI CreateToolUI(string toolType, string uiVariant="Lite")
        {
            if (ToolUIs.TryGetValue((toolType, uiVariant), out var t))
                return (IToolUI)Activator.CreateInstance(t);
            return null;
        }
        public static void LoadTool(string sPath)
        {
           
            _tools.Clear();
         
           _toolTypes.Clear();


            DirectoryInfo directoryInfo = new DirectoryInfo(sPath);
            FileInfo[] files = directoryInfo.GetFiles();
            FileInfo[] array = files;
            int i = 0;
            while (i < array.Length)
            {
                FileInfo fileInfo = array[i];
                try
                {
                    string a = fileInfo.Extension.ToUpper();
                    bool flag = a != ".DLL";
                    if (!flag)
                    {
                        Assembly assembly = Assembly.LoadFile(fileInfo.FullName);
        
                        string mark = DllHelper.GetAssemblyTrademark(assembly);
              
                        if (assembly != null)
                        {
                            Type[] types = assembly.GetTypes();
                            for (int j = 0; j < types.Length; j++)
                            {
                                Type type = types[j];

                                if (typeof(ITool).IsAssignableFrom(type) && !type.IsAbstract)
                                {
                                    var inst = (ITool)Activator.CreateInstance(type);
                                  
                                    FieldInfo ToolType = type.GetField("ToolType");
                                    if (ToolType != null)
                                    {
                                        if (ToolType.GetValue(type) != null)
                                        {
                                            Register(ToolType.GetValue(type).ToString(), inst);
                                            RegisterType(ToolType.GetValue(type).ToString(), type);
                                         
                                        }
                                    }
                             

                                }
                            }
                        }


                    }
                }
                catch (Exception err)
                {
                }
                i++;
                continue;
            }
        }
        public static void Register(string toolType, ITool executor)
        {
            _tools[toolType] = executor;
        }
        public static void RegisterType(string toolType, Type type)
        {
            _toolTypes[toolType] = type;
        }
        public static ITool GetTool(string toolType)
        {
            return _tools.TryGetValue(toolType, out var exec) ? exec : null;
        }
        public static ITool CreateTool(string toolType,string sToolName = "")
        {
            ITool result;
            object obj;
            if (sToolName == "")
            {
                obj = Activator.CreateInstance(_toolTypes[toolType]);
            }
            else
            {
                object[] args = new object[]
                         {
                                sToolName
                         };
                obj = Activator.CreateInstance(_toolTypes[toolType], args);
            }
            if (obj != null)
            {
                result = obj as ITool;
                return result;
            }
            result = null;
            return result;
       
        }

    }

}
