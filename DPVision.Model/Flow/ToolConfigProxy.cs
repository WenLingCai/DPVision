using System;
using System.Collections.Generic;
using System.Text;

namespace DPVision.Model.Flow
{
    [Serializable]
    public class ToolConfigProxy
    {
        public string ToolType { get; set; }     // 如 "FlowPlugins.MyToolConfig, FlowPlugins"
        public string ToolName { get; set; }
        public object Parameters { get; set; }
    }
}
