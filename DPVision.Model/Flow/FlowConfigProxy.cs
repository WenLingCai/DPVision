using System;
using System.Collections.Generic;
using System.Text;

namespace DPVision.Model.Flow
{
    [Serializable]
    public class FlowConfigProxy
    {
        public string FlowType { get; set; }
        public string FlowName { get; set; }
        public List<ToolConfigProxy> Tools { get; set; } = new List<ToolConfigProxy>();
    }
}
