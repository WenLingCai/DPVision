using DPVision.Model.Tool;
using System.Windows.Controls;


namespace DPVToolTemplateMatchUI
{
    public partial class DPVToolTemplateMatchView : UserControl, IToolUI
    {
        public string ToolType => "DPVToolTemplateMatch";
        public string UIVariant => "Full";
        public DPVToolTemplateMatchView()
        {
       
        }

        public void SetTool(ITool tool)
        {
          
        }

        public object GetControl() => this;
    }
}