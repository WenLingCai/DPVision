using System;
using ToolContracts;

namespace ToolB
{
    public class ToolBViewModel : ITool
    {
        public string Name => "ToolB";
        public object Parameters { get; private set; }
        public event EventHandler ParametersChanged;

        public ToolBViewModel()
        {
            Parameters = new ToolBParams();
        }

        public void Execute()
        {
            var p = Parameters as ToolBParams;
            p.Output = $"Hello, {p.Input}";
            ParametersChanged?.Invoke(this, EventArgs.Empty); // 通知UI刷新
        }
    }
}