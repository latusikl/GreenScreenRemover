using System.Windows;
using Caliburn.Micro;
using GreenScreen.ViewModels;

namespace GreenScreen
{
    class Bootstrapper: BootstrapperBase
    {
        public Bootstrapper()
        {
                Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>();
        }
    }
}
