using DPVision.Common;
using DPVision.Model.Flow;
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
    public class FlowFactory
    {

        private static readonly Dictionary<string, IFlowBase> _flows = new Dictionary<string, IFlowBase>();
        private static readonly Dictionary<string, Type> _flowTypes = new Dictionary<string, Type>();
        private static readonly Dictionary<string, Type> FlowUIs = new Dictionary<string, Type>();

        public static void LoadUIPlugins(string pluginDir)
        {
            foreach (var dll in Directory.GetFiles(pluginDir, "*.dll"))
            {
                var asm = Assembly.LoadFrom(dll);
                foreach (var t in asm.GetTypes())
                {
                    if (typeof(IFlowUI).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
                    {
                        var inst = (IFlowUI)Activator.CreateInstance(t);
                        FlowUIs[inst.FlowType] = t;
                    }
                }
            }
        }

       
         

        // 获取UI实例
        public static IFlowUI CreateFlowUI(string flowType)
        {
            if (FlowUIs.TryGetValue(flowType, out var t))
                return (IFlowUI)Activator.CreateInstance(t);
            return null;
        }
        public static void LoadTool(string sPath)
        {
           
            _flows.Clear();
         
           _flowTypes.Clear();


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

                                if (typeof(IFlowBase).IsAssignableFrom(type) && !type.IsAbstract)
                                {
                                    var inst = (IFlowBase)Activator.CreateInstance(type);
                                  
                                    FieldInfo FlowType = type.GetField("FlowType");
                                    if (FlowType != null)
                                    {
                                        if (FlowType.GetValue(type) != null)
                                        {
                                            Register(FlowType.GetValue(type).ToString(), inst);
                                            RegisterType(FlowType.GetValue(type).ToString(), type);
                                         
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
        public static void Register(string flowType, IFlowBase executor)
        {
            _flows[flowType] = executor;
        }
        public static void RegisterType(string flowType, Type type)
        {
            _flowTypes[flowType] = type;
        }
        public static IFlowBase GetFlow(string flowType)
        {
            return _flows.TryGetValue(flowType, out var exec) ? exec : null;
        }
        public static IFlowBase CreateFlow(string flowType, string sToolName = "")
        {
            IFlowBase result;
            object obj;
            if (sToolName == "")
            {
                obj = Activator.CreateInstance(_flowTypes[flowType]);
            }
            else
            {
                object[] args = new object[]
                         {
                                sToolName
                         };
                obj = Activator.CreateInstance(_flowTypes[flowType], args);
            }
            if (obj != null)
            {
                result = obj as IFlowBase;
                return result;
            }
            result = null;
            return result;
       
        }

    }

}
