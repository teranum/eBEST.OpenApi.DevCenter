using System.Windows;

namespace eBEST.OpenApi.DevCenter.Views
{
    /// <summary>
    /// Interaction logic for MacAddressInputWindow.xaml
    /// </summary>
    public partial class MacAddressInputWindow : Window
    {
        public MacAddressInputWindow()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e) => DialogResult = true;

        public string PrefixAddress { get => text_MacAddressPrefix.Text; set => text_MacAddressPrefix.Text = value; }
        public string NewAddress { get => text_MacAddress.Text; set => text_MacAddress.Text = value; }

    }
}
