
namespace DPVision.Model.Tool
{
    public interface IToolUI
    {
        string ToolType { get; }
         string UIVariant { get; }
        void SetTool(ITool tool);
        object GetControl();
    }
}