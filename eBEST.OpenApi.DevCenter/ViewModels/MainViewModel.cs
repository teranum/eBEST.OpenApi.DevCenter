using App.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBEST.OpenApi.DevCenter.Helpers;
using eBEST.OpenApi.DevCenter.Models;
using eBEST.OpenApi.DevCenter.Views;
using eBEST.OpenApi.Models;
using System.Windows;

namespace eBEST.OpenApi.DevCenter.ViewModels
{
    internal partial class MainViewModel : ObservableObject
    {

        [ObservableProperty] string _title = "eBEST OpenApi DevCenter";
        [ObservableProperty] string _statusText = "Ready";

        [ObservableProperty] GridLength _tabTreeWidth;
        [ObservableProperty] GridLength _tabListHeight;
        [ObservableProperty] GridLength _propertyWidth;

        [ObservableProperty] string _equipText = "";
        [ObservableProperty] int _equipHeight = 220;

        private readonly IAppRegistry _appRegistry;
        private readonly Window _mainWindow;
        private readonly EBestOpenApi _openApi;

        public MainViewModel(IAppRegistry appRegistry)
        {
            _appRegistry = appRegistry;
            _mainWindow = Application.Current.MainWindow;
            _openApi = new();
            _openApi.OnConnectEvent += OpenApi_OnConnectEvent;
            _openApi.OnMessageEvent += OpenApi_OnMessageEvent;
            _openApi.OnRealtimeEvent += OpenApi_OnRealtimeEvent;

            // 메인 윈도우 설정값 로딩
            string session = _mainWindow.GetType().Name;
            int Left = _appRegistry.GetValue(session, "Left", 0);
            int Top = _appRegistry.GetValue(session, "Top", 0);
            int Width = _appRegistry.GetValue(session, "Width", 1250);
            int Height = _appRegistry.GetValue(session, "Height", 760);
            bool TopMost = _appRegistry.GetValue(session, "TopMost", 0) != 0;

            TabTreeWidth = new(_appRegistry.GetValue(session, nameof(TabTreeWidth), 410));
            TabListHeight = new(_appRegistry.GetValue(session, nameof(TabListHeight), 150));
            PropertyWidth = new(_appRegistry.GetValue(session, nameof(PropertyWidth), 270));

            if (Left != 0) _mainWindow.Left = Left;
            if (Top != 0) _mainWindow.Top = Top;
            if (Width != 0) _mainWindow.Width = Width;
            if (Height != 0) _mainWindow.Height = Height;
            _mainWindow.Topmost = TopMost;

            _mainWindow.Closed += (s, e) =>
            {
                _appRegistry.SetValue(session, "Left", (int)_mainWindow!.Left);
                _appRegistry.SetValue(session, "Top", (int)_mainWindow.Top);
                _appRegistry.SetValue(session, "Width", (int)_mainWindow.Width);
                _appRegistry.SetValue(session, "Height", (int)_mainWindow.Height);
                _appRegistry.SetValue(session, "TopMost", _mainWindow.Topmost ? 1 : 0);

                _appRegistry.SetValue(session, nameof(TabTreeWidth), (int)TabTreeWidth.Value);
                _appRegistry.SetValue(session, nameof(TabListHeight), (int)TabListHeight.Value);
                _appRegistry.SetValue(session, nameof(PropertyWidth), (int)PropertyWidth.Value);

                SaveToolsData();
            };

            // 로그 리스트 설정
            TabListDatas = new List<TabListData>();
            string[] logKinds = Enum.GetNames(typeof(LogKind));
            foreach (string logKind in logKinds)
            {
                TabListDatas.Add(new(logKind));
            }
            SelectedTabListData = TabListDatas[0];

            OutputLog(LogKind.LOGS, "Application Start");

            var models = ModelsHelper.GetModelClasses();
            foreach (var model in models)
            {
                _modelClasses.Add(model.Name, model);
            }
            var records = ModelsHelper.GetBlockRecords();
            foreach (var record in records)
            {
                _blockRecords.Add(record.Name, record);
            }


            LoadTrDatas();

            LoadToolsData();
        }

        // OpenApi.Models reference 용으로 사용
        async void GetData()
        {
            t1102 주식현재가 = new()
            {
                t1102InBlock = new("005930"),
            };
            await _openApi.GetTRData(주식현재가).ConfigureAwait(true);
            if (주식현재가.t1102OutBlock != null)
            {
                StatusText = 주식현재가.t1102OutBlock.hname;
            }
        }

        // 메뉴
        [RelayCommand(CanExecute = nameof(CanLogin))]
        void MenuLogin()
        {
            string AppKey = string.Empty;
            string SecretKey = string.Empty;
#if DEBUG
            // 개발용에서는 간편 키보관 허용
            AppKey = _appRegistry.GetValue("InitData", "AppKey", "");
            SecretKey = _appRegistry.GetValue("InitData", "SecretKey", "");
#endif
            var keyWindow = new AppKey();
            keyWindow.textAppKey.Text = AppKey;
            keyWindow.textSecretKey.Text = SecretKey;
            if (keyWindow.ShowDialog() == true)
            {
                AppKey = keyWindow.textAppKey.Text;
                SecretKey = keyWindow.textSecretKey.Text;
#if DEBUG
                // 개발용에서는 간편 키보관 허용
                _appRegistry.SetValue("InitData", "AppKey", AppKey);
                _appRegistry.SetValue("InitData", "SecretKey", SecretKey);
#endif
                _ = _openApi.ConnectAsync(AppKey, SecretKey);
            }
        }
        bool CanLogin() => _openApi.Connected == false;
        [RelayCommand] void MenuExit() => System.Windows.Application.Current.Shutdown();
    }
}
