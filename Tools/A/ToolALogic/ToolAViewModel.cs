using DPVision.Model.Tool;
using System;


namespace ToolA
{
    public class ToolAViewModel : ITool
    {
        public string Name => "ToolA";
        public object Parameters { get; private set; }
        public event EventHandler ParametersChanged;

        public ToolAViewModel()
        {
            Parameters = new ToolAParams();
        }

        public void Execute()
        {
            var p = Parameters as ToolAParams;
            p.Output = $"Hello12, {p.Input}";
            ParametersChanged?.Invoke(this, EventArgs.Empty); // 通知UI刷新
        }
    }
}