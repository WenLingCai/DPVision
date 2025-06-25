using DPVision.Model.Tool;
using System.Windows.Controls;


namespace DPVToolImageEnhanceUI
{
    public partial class DPVToolFindCicleLiteUI : UserControl, IToolUI
    {
        public string ToolType => "DPVToolFindCicle";
        public string UIVariant => "Lite";
        public DPVToolFindCicleLiteUI()
        {
           
        }

        public void SetTool(ITool tool)
        {
           
        }

        public object GetControl() => this;
    }
}