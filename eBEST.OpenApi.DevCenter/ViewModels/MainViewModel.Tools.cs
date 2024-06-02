using CommunityToolkit.Mvvm.Input;
using eBEST.OpenApi.DevCenter.Views;

namespace eBEST.OpenApi.DevCenter.ViewModels;

internal partial class MainViewModel
{
    public string MenuCustomizeHeaderText { get; } = "About";
    public List<string> MenuCustomizeItems { get; } =
    [
        "버젼정보",
        "LS증권 OpenApi 홈페이지",
        //"DevCenter 오픈소스",
    ];

    internal void OnDataGridCellEditEnding()
    {
        MakeEquipText();
    }

    [RelayCommand]
    void MenuCustomize(string text)
    {
        if (text.Equals("버젼정보"))
        {
            // 버젼 정보
            if (_releaseTags != null && _releaseTags.Count != 0)
            {
                var versionView = new VersionView(_releaseTags);
                versionView.ShowDialog();
            }
        }
        else if (text.Equals("LS증권 OpenApi 홈페이지"))
        {
            var sInfo = new System.Diagnostics.ProcessStartInfo("https://openapi.ls-sec.co.kr/intro")
            {
                UseShellExecute = true,
            };
            System.Diagnostics.Process.Start(sInfo);
        }
        //else if (text.Equals("DevCenter 오픈소스"))
        //{
        //    var sInfo = new System.Diagnostics.ProcessStartInfo("https://github.com/teranum/eBEST.OpenApi.DevCenter")
        //    {
        //        UseShellExecute = true,
        //    };
        //    System.Diagnostics.Process.Start(sInfo);
        //}
    }

}

