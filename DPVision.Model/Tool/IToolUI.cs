
namespace DPVision.Model.Tool
{
    public interface IToolUI
    {
        void SetViewModel(object viewModel);
        object GetControl();
    }
}