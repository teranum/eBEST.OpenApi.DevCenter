using App.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBEST.OpenApi.DevCenter.Helpers;
using eBEST.OpenApi.DevCenter.Models;
using eBEST.OpenApi.DevCenter.Views;
using eBEST.OpenApi.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;

namespace eBEST.OpenApi.DevCenter.ViewModels
{
    internal partial class MainViewModel : ObservableObject
    {
        private readonly string _appVersion;
        private List<GithubTagInfo>? _releaseTags;

        [ObservableProperty] string _title = "eBEST OpenApi DevCenter";
        [ObservableProperty] string _statusText = "Ready";

        [ObservableProperty] string _statusUrl = string.Empty;
        [ObservableProperty] string _statusUrlText = string.Empty;


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
            var assemblyName = Application.ResourceAssembly.GetName();
            _appVersion = $"{assemblyName.Version!.Major}.{assemblyName.Version.Minor}";
            _title = $"{assemblyName.Name} v{_appVersion}";

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

            _ = CheckVersionAsync();

            LoadTrDatas();

            LoadToolsData();
        }

        private async Task CheckVersionAsync()
        {
            // 깃헙에서 최신 버전 정보 가져오기

            _releaseTags = await GetGithubRepoTagInfos("teranum", "eBEST.OpenApi.DevCenter").ConfigureAwait(true);
            if (_releaseTags != null && _releaseTags.Count > 0)
            {
                var lastTag = _releaseTags[0];
                if (string.Equals(lastTag.tag_name, _appVersion))
                {
                    StatusText = "최신 버전입니다.";
                }
                else
                {
                    StatusUrl = lastTag.html_url;
                    StatusText = $"새로운 버전({lastTag.tag_name})이 있습니다.";
                }
            }
        }

        [RelayCommand]
        static void Hyperlink_RequestNavigate(Uri url)
        {
            var sInfo = new System.Diagnostics.ProcessStartInfo(url.AbsoluteUri)
            {
                UseShellExecute = true,
            };
            System.Diagnostics.Process.Start(sInfo);
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
            // 기존 키 로딩
            string AppKey = _appRegistry.GetValue("InitData", "AppKey", string.Empty);
            string SecretKey = _appRegistry.GetValue("InitData", "SecretKey", string.Empty);
            bool IsRememberKey = _appRegistry.GetValue("InitData", "IsRememberKey", DefValue: false);

            var keyWindow = new KeySetting
            {
                AppKey = AppKey,
                SecretKey = SecretKey,
                IsRememberKey = IsRememberKey,
            };

            if (keyWindow.ShowDialog() == true)
            {
                AppKey = keyWindow.AppKey;
                SecretKey = keyWindow.SecretKey;
                IsRememberKey = keyWindow.IsRememberKey;

                if (IsRememberKey)
                {
                    _appRegistry.SetValue("InitData", "AppKey", AppKey);
                    _appRegistry.SetValue("InitData", "SecretKey", SecretKey);
                    _appRegistry.SetValue("InitData", "IsRememberKey", IsRememberKey);
                }
                else
                {
                    _appRegistry.DeleteValue("InitData", "AppKey");
                    _appRegistry.DeleteValue("InitData", "SecretKey");
                    _appRegistry.DeleteValue("InitData", "IsRememberKey");
                }
                _ = _openApi.ConnectAsync(AppKey, SecretKey);
            }
        }
        bool CanLogin() => !_openApi.Connected;
        [RelayCommand] static void MenuExit() => System.Windows.Application.Current.Shutdown();

        [RelayCommand]
        void Menu_Version()
        {
            // 버젼 정보
            if (_releaseTags != null && _releaseTags.Count != 0)
            {
                var versionView = new VersionView(_releaseTags);
                versionView.ShowDialog();
            }
        }

        static Task<List<GithubTagInfo>?> GetGithubRepoTagInfos(string Username, string Repository)
        {
            // 깃헙 릴리즈 태그에서 가져오기
            HttpClient client = new();
            var pih = ProductInfoHeaderValue.Parse(Repository);
            client.DefaultRequestHeaders.UserAgent.Add(pih);
            return client.GetFromJsonAsync<List<GithubTagInfo>>($"https://api.github.com/repos/{Username}/{Repository}/releases");
        }

    }
}
