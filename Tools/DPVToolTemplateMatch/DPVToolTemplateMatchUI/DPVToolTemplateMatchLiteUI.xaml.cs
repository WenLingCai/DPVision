using DPVision.Model.Tool;
using DPVToolTemplateMatchUI;
using System.Windows;
using System.Windows.Controls;


namespace DPVToolFindCircleUI
{
    public partial class DPVToolTemplateMatchLiteUI : UserControl, IToolUI
    {
     
        public string ToolType => "DPVToolTemplateMatch";
        public string UIVariant => "Lite";
        public DPVToolTemplateMatchLiteUI()
        {
     
        }
        private ITool tool;
        public void SetTool(ITool tool)
        {
            this.tool = tool;
        }

        public object GetControl() => this;

        private void MoreButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // ��ȫ����UI
            var fullUI = new DPVToolTemplateMatchView();
            fullUI.SetTool(this.tool);
            var wnd = new Window
            {
                Title = "�߼���������",
                Content = fullUI,
                Width = 350,
                Height = 200,
                Owner = Window.GetWindow(this)
            };
            wnd.ShowDialog();
        }
    }
}