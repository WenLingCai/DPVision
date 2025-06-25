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
                Application.Current.Shutdown();
            }
            return Container.Resolve<MainWindow>();
        }

      


        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {


        }

      


    }

}
