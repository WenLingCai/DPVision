using DPVision.Flow.ViewModels;
using DPVision.Flow.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPVision.Flow
{
    public class FlowModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 可选：初始化时的逻辑
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DPVFlowView, DPVFlowViewModel>("DPVFlowView");
        }
    }
}
