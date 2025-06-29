using DPVision.Model.Flow;
using DPVision.Model.Tool;
using System.Windows.Controls;


namespace DPVFlowMarkView
{
    public partial class DPVFlowMarkView : UserControl, IFlowUI
    {
        public string FlowType => "DPVFlowMarkView";
        public string FlowName => "DPVFlowMarkView";
        public DPVFlowMarkView()
        {
            
        }

        public void SetTool(IFlowBase tool)
        {
      
        }

        public object GetControl() => this;
    }
}