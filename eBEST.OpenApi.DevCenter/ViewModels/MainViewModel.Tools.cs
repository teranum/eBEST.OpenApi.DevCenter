using CommunityToolkit.Mvvm.Input;

namespace eBEST.OpenApi.DevCenter.ViewModels;

internal partial class MainViewModel
{
    public string MenuCustomizeHeaderText { get; } = "리소스";
    public List<string> MenuCustomizeItems { get; } =
    [
        "이베스트 OpenApi 홈페이지",
        "DevCenter 오픈소스",
    ];

    internal void OnDataGridCellEditEnding()
    {
        MakeEquipText();
    }

    [RelayCommand]
    void MenuCustomize(string text)
    {
        if (text.Equals("이베스트 OpenApi 홈페이지"))
        {
            var sInfo = new System.Diagnostics.ProcessStartInfo("https://openapi.ebestsec.co.kr/intro")
            {
                UseShellExecute = true,
            };
            System.Diagnostics.Process.Start(sInfo);
        }
        else if (text.Equals("DevCenter 오픈소스"))
        {
            var sInfo = new System.Diagnostics.ProcessStartInfo("https://github.com/teranum/eBEST.OpenApi.DevCenter")
            {
                UseShellExecute = true,
            };
            System.Diagnostics.Process.Start(sInfo);
        }
    }

}

