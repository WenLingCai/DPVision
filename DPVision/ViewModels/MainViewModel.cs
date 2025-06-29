using Prism.Commands;
using Prism.Mvvm;


namespace DPVision.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        public DelegateCommand<string> NavigateCommand { get; }

        public MainViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            NavigateCommand = new DelegateCommand<string>(Navigate);
            // 默认显示标定
            Navigate("DPVCaliView");
        }

        private void Navigate(string viewName)
        {
            if (!string.IsNullOrEmpty(viewName))
                _regionManager.RequestNavigate("MainRegion", viewName);
        }
    }
}