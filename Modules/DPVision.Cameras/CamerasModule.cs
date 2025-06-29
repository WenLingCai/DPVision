using DPVision.Cameras.ViewModels;
using DPVision.Cameras.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPVision.Cameras
{
    public class CamerasModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 可选：初始化时的逻辑
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DPVCamerasView, DPVCamerasViewModel>("DPVCamerasView");
        }
    }
}
