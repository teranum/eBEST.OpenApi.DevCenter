using App.Helpers;
using eBEST.OpenApi.DevCenter.ViewModels;
using eBEST.OpenApi.DevCenter.Views;
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
            Startup += (sender, args) =>
            {
                var mainView = new MainView();
                mainView.MouseDown += (sender, e) =>
                {
                    if (e.ChangedButton == MouseButton.Left)
                        mainView.DragMove();
                };
                mainView.DataContext = new MainViewModel(new AppRegistry("teranum"));
                mainView.Show();
            };
        }
    }

}
