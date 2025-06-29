using DPVision.Model.ROI;
using DPVision.Model.Tool;
using System;
using System.Collections.Generic;
using System.Text;

namespace DPVision.Model.Flow
{
    public interface IFlowUI
    {
        string FlowType { get; }
        string FlowName { get; }

        void SetTool(IFlowBase tool);
        object GetControl();


    }
}
