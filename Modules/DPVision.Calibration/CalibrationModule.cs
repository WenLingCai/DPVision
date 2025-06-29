using DPVision.Calibration.ViewModels;
using DPVision.Calibration.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPVision.Calibration
{
    public class CalibrationModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 可选：初始化时的逻辑
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DPVCaliView, DPVCaliViewModel>("DPVCaliView");
        }
    }
}
