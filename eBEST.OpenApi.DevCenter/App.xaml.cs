using App.Helpers;
using CommunityToolkit.Mvvm.DependencyInjection;
using eBEST.OpenApi.DevCenter.ViewModels;
using eBEST.OpenApi.DevCenter.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;

namespace eBEST.OpenApi.DevCenter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Ioc.Default.ConfigureServices(
                new ServiceCollection()

                //Services
                .AddSingleton<IAppRegistry>(new AppRegistry("teranum"))
                .AddSingleton<MainViewModel>()

                .BuildServiceProvider()
                );

            Startup += (sender, args) =>
            {
                var mainView = new MainView();
                mainView.MouseDown += (sender, e) =>
                {
                    if (e.ChangedButton == MouseButton.Left)
                        mainView.DragMove();
                };
                mainView.DataContext = Ioc.Default.GetRequiredService<MainViewModel>();
                mainView.Show();
            };
        }
    }

}
