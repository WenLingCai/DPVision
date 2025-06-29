using DPVision.Model.Flow;
using DPVision.Model.Tool;
using System.Windows.Controls;


namespace DPVFlowCaliView
{
    public partial class DPVFlowCaliView : UserControl, IFlowUI
    {
        public string FlowType => "DPVFlowCaliView";
        public string FlowName => "DPVFlowCaliView";
        public DPVFlowCaliView()
        {
            
        }

        public void SetTool(IFlowBase tool)
        {
      
        }

        public object GetControl() => this;
    }
}