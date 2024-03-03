using App.Helpers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace eBEST.OpenApi.DevCenter.Views
{
    /// <summary>
    /// Interaction logic for VersionView.xaml
    /// </summary>
    public partial class VersionView : Window
    {
        public IList<GithupRepoHelper.GithubTagInfo> TagInfos { get; }

        public VersionView(IList<GithupRepoHelper.GithubTagInfo> tagInfos)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            TagInfos = tagInfos;

            this.DataContext = this;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var sInfo = new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true,
            };
            Process.Start(sInfo);
            e.Handled = true;
        }
    }
}
