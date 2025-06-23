using DPVision.Model.Tool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DPVision.Core
{
    public static class ToolRegistry
    {
        private static readonly Dictionary<string, ITool> _executors = new Dictionary<string, ITool>();

        /// <summary>
        /// 扫描并加载插件DLL目录
        /// </summary>
        public static void LoadTools(string pluginDir)
        {
            foreach (var dll in Directory.GetFiles(pluginDir, "*.dll"))
            {
                try
                {
                    var asm = Assembly.LoadFrom(dll);
                    foreach (var type in asm.GetTypes())
                    {
                        if (typeof(ITool).IsAssignableFrom(type) && !type.IsAbstract)
                        {
                            var inst = (ITool)Activator.CreateInstance(type);
                            Register(inst.ToolType, inst);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PluginLoader] 加载{dll}失败: {ex.Message}");
                }
            }
        }

        public static void Register(string toolType, ITool executor)
        {
            _executors[toolType] = executor;
        }

        public static ITool GetExecutor(string toolType) =>
            _executors.TryGetValue(toolType, out var exec) ? exec : null;
  
    }
}
