using System.Windows;

namespace eBEST.OpenApi.DevCenter.Views
{
    /// <summary>
    /// Interaction logic for AppKey.xaml
    /// </summary>
    public partial class AppKey : Window
    {
        public AppKey()
        {
            InitializeComponent();

            Owner = Application.Current.MainWindow;
            Topmost = Owner.Topmost;
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
