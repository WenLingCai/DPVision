using Prism.Ioc;
using Prism.DryIoc;
using System.Windows;

namespace PrismDryIocDemo
{
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 依赖注册位置
        }
    }
}