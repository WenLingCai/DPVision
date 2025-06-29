using DPVision.ImageViewEx.ViewModels;
using DPVision.ImageViewEx.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPVision.ImageViewEx
{
    public class ImageViewExModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 可选：初始化时的逻辑
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DPVImageView, DPVImageViewModel>("DPVImageView");
        }
    }
}
