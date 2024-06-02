using System.Windows;

namespace eBEST.OpenApi.DevCenter.Views
{
    /// <summary>
    /// Interaction logic for KeySetting.xaml
    /// </summary>
    public partial class KeySetting : Window
    {
        public KeySetting(string profileName, string appKey, string appSecretKey)
        {
            InitializeComponent();

            Owner = Application.Current.MainWindow;
            Topmost = Owner.Topmost;

            Title = $"키 설정 - {profileName}";

            textProfileName.Text = profileName;
            textAppKey.Text = appKey;
            textSecretKey.Text = appSecretKey;
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void check_RememberKey_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = check_RememberKey.IsChecked ?? false;
            if (isChecked)
            {
                // 키 기억 경고문 띄우면서 다시 한번 확인
                if (MessageBox.Show("키를 기억하면 다른 사람이 이 컴퓨터에서 이 프로그램을 실행할 때 키가 노출될 수 있습니다.\n계속하시겠습니까?", "경고", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    check_RememberKey.IsChecked = false;
                    return;
                }
            }
        }

        public string ProfileName
        {
            get => textProfileName.Text;
            set => textProfileName.Text = value;
        }

        public string AppKey
        {
            get => textAppKey.Text;
            set => textAppKey.Text = value;
        }

        public string SecretKey
        {
            get => textSecretKey.Text;
            set => textSecretKey.Text = value;
        }

        public bool IsRememberKey
        {
            get => check_RememberKey.IsChecked ?? false;
            set => check_RememberKey.IsChecked = value;
        }
    }
}
