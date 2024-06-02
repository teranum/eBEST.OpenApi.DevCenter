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
        private readonly string _appVersion;
        private GithupRepoHelper _githupRepoHelper;
        private IList<GithupRepoHelper.GithubTagInfo>? _releaseTags;

        [ObservableProperty] string _title;
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

        [ObservableProperty] LANG_TYPE _langType = LANG_TYPE.CSHARP;

        public IEnumerable<NotifyProfile> MenuLoginProfiles { get; }

        public MainViewModel(IAppRegistry appRegistry)
        {
            var assemblyName = Application.ResourceAssembly.GetName();
            _appVersion = $"{assemblyName.Version!.Major}.{assemblyName.Version.Minor}.{assemblyName.Version.Build}";
            _title = $"LS증권 OpenApi DevCenter v{_appVersion}";

            _appRegistry = appRegistry;
            _githupRepoHelper = new("teranum", "eBEST.OpenApi.DevCenter");
            _mainWindow = Application.Current.MainWindow;
            _openApi = new();
            _openApi.OnConnectEvent += OpenApi_OnConnectEvent;
            _openApi.OnMessageEvent += OpenApi_OnMessageEvent;
            _openApi.OnRealtimeEvent += OpenApi_OnRealtimeEvent;
            InitialJsonOptions();

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

            if (Enum.TryParse(_appRegistry.GetValue(session, nameof(LangType), nameof(LANG_TYPE.CSHARP)), out LANG_TYPE savedLangType))
            {
                LangType = savedLangType;
            }

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

                _appRegistry.SetValue(session, nameof(LangType), LangType.ToString());

                SaveToolsData();
            };

            // 로그 리스트 설정
            TabListDatas = [];
            string[] logKinds = Enum.GetNames(typeof(LogKind));
            foreach (string logKind in logKinds)
            {
                TabListDatas.Add(new(logKind));
            }
            SelectedTabListData = TabListDatas[0];

            OutputLog(LogKind.LOGS, "Application Start");

            var model_and_blocks = ModelsHelper.GetModelClasses();
            foreach (var type in model_and_blocks)
            {
                if (type.BaseType != null)
                {
                    if (type.BaseType.Name.Equals("TrBase"))
                        _modelClasses.Add(type.Name, type);
                    else
                        _blockRecords.Add(type.Name, type);
                }
            }

            // 프로필 로딩
            const int MAX_PROFILE = 8;
            MenuLoginProfiles = Enumerable.Range(1, MAX_PROFILE).Select(i =>
            {
                string section = $"Profile{i}";
                string profileName = _appRegistry.GetValue(section, "Name", section);
                return new NotifyProfile(i, profileName);
            });


            _ = CheckVersionAsync();

            LoadTrDatas();

            LoadToolsData();

            LoadUserCustomItems();
        }

        private async Task CheckVersionAsync()
        {
            // 깃헙에서 최신 버전 정보 가져오기

            _releaseTags = await _githupRepoHelper.GetTagInfosAsync();
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
        async Task MenuLogin(NotifyProfile profile)
        {
            // 기존 키 로딩
            int id = profile.Id;
            string section = $"Profile{id}";
            string ProfileName = profile.Name;

            string AppKey = StringCipher.Decrypt(_appRegistry.GetValue(section, "AppKey", string.Empty));
            string SecretKey = StringCipher.Decrypt(_appRegistry.GetValue(section, "SecretKey", string.Empty));
            bool IsRememberKey = _appRegistry.GetValue(section, "IsRememberKey", DefValue: false);

            var keyWindow = new KeySetting(ProfileName, AppKey, SecretKey)
            {
                IsRememberKey = IsRememberKey,
            };

            if (keyWindow.ShowDialog() == true)
            {
                ProfileName = keyWindow.ProfileName;
                AppKey = keyWindow.AppKey;
                SecretKey = keyWindow.SecretKey;
                IsRememberKey = keyWindow.IsRememberKey;

                if (string.IsNullOrEmpty(ProfileName) || string.IsNullOrEmpty(AppKey) || string.IsNullOrEmpty(SecretKey))
                {
                    OutputLog(LogKind.LOGS, "프로필명, AppKey, SecretKey를 입력해주세요.");
                    return;
                }

                // 동일한 프로필명이 있는지 체크
                if ( !ProfileName.Equals(profile.Name))
                {
                    foreach (var item in MenuLoginProfiles)
                    {
                        if (item.Name.Equals(ProfileName))
                        {
                            OutputLog(LogKind.LOGS, "이미 동일한 프로필명이 있습니다.");
                            return;
                        }
                    }

                    _appRegistry.SetValue(section, "Name", ProfileName);
                    profile.Name = ProfileName;
                }

                if (IsRememberKey)
                {
                    _appRegistry.SetValue(section, "AppKey", StringCipher.Encrypt(AppKey));
                    _appRegistry.SetValue(section, "SecretKey", StringCipher.Encrypt(SecretKey));
                }
                else
                {
                    _appRegistry.DeleteValue(section, "AppKey");
                    _appRegistry.DeleteValue(section, "SecretKey");
                }
                _appRegistry.SetValue(section, "IsRememberKey", IsRememberKey);

                OutputLog(LogKind.LOGS, $"로그인 요청중...({ProfileName})");
                if (await _openApi.ConnectAsync(AppKey, SecretKey))
                {
                    StatusText = "로그인 성공";
                }
                else
                {
                    StatusText = "로그인 실패";
                }
            }

            MenuLogoutCommand.NotifyCanExecuteChanged();
        }
        bool CanLogin() => !_openApi.Connected;

        [RelayCommand(CanExecute = nameof(CanLogout))]
        async Task MenuLogout()
        {
            await _openApi.CloseAsync();
            StatusText = _openApi.LastErrorMessage;

            OutputLog(LogKind.LOGS, "로그아웃");

            MenuLoginCommand.NotifyCanExecuteChanged();
        }
        bool CanLogout() => _openApi.Connected;

        [RelayCommand] static void MenuExit() => System.Windows.Application.Current.Shutdown();

        [RelayCommand]
        void MenuMacAddrSetting()
        {
            // 맥주소 설정
            var mac_addr_window = new MacAddressInputWindow
            {
                PrefixAddress = _openApi.MacAddress,
                NewAddress = _appRegistry.GetValue("InitData", "MacAddress", string.Empty),
            };
            if (mac_addr_window.ShowDialog() == true)
            {
                string new_address = mac_addr_window.NewAddress;
                _openApi.MacAddress = new_address;
                _appRegistry.SetValue("InitData", "MacAddress", new_address);
            }
        }

        internal partial class NotifyProfile(int id, string name) : ObservableObject
        {
            public int Id = id;
            [ObservableProperty] string _name = name;
        }
    }
}
