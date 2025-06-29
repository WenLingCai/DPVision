using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using Example;
using log4net.Config;
using log4net;
using Microsoft.EntityFrameworkCore;
using DPVision.Views;
using LiveCharts.Wpf;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using DPVision.ViewModels;


namespace DPVision
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private static Mutex _mutex = null;
        const string appName = "DPVision";
        bool createdNew;
        protected override System.Windows.Window CreateShell()
        {
            _mutex = new Mutex(true, appName, out createdNew);
            if (!createdNew)
            {
                //应用程序已经在运行！当前的执行退出。
                //Application.Current.Shutdown();
            }
            return Container.Resolve<MainView>();
        }

      


        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MainView, MainViewModel>();

        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            // 指定模块dll存放插件目录
            return new DirectoryModuleCatalog() { ModulePath = @".\Modules" };
        }



    }

}
