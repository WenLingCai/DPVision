using DPVision.Model.Tool;
using System.Windows.Controls;


namespace DPVToolImageSourceUI
{
    public partial class DPVToolFindCicleView : UserControl, IToolUI
    {
        public string ToolType => "DPVToolFindCicle";
        public string UIVariant => "Full";
        public DPVToolFindCicleView()
        {
            
        }

        public void SetTool(ITool tool)
        {
      
        }

        public object GetControl() => this;
    }
}