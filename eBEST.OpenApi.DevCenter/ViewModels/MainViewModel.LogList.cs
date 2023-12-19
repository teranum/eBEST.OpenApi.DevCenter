using App.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBEST.OpenApi.DevCenter.Models;
using System.Text;
using System.Windows;

namespace eBEST.OpenApi.DevCenter.ViewModels;

internal partial class MainViewModel
{
    public const int MAX_LOG_COUNT = 1000;
    public IList<TabListData> TabListDatas { get; set; }

    private TabListData? _selectedTabListData;
    public TabListData? SelectedTabListData
    {
        get => _selectedTabListData;
        set
        {
            if (value != _selectedTabListData)
            {
                _selectedTabListData = value;
                if (_selectedTabListData != null && _selectedTabListData.Id != 0)
                {
                    _selectedTabListData.Id = 1;
                }
                OnPropertyChanged(nameof(SelectedTabListData));
            }
        }
    }

    // 싱글라인 추가
    private void OutputLog(LogKind kind, string message, bool AutoClear = true)
    {
        var listData = TabListDatas[(int)kind];
        string dt_msg = DateTime.Now.ToString("HH:mm:ss.fff : ") + message;
        if (AutoClear && listData.Items.Count > MAX_LOG_COUNT) listData.Items.Clear();
        listData.Items.Add(dt_msg);
        if (listData != SelectedTabListData)
            listData.Id = 4;
        else listData.Id = 1;
    }

    // 멀티라인 추가
    private void OutputLog(LogKind kind, IList<string> messages)
    {
        var listData = TabListDatas[(int)kind];
        foreach (var item in messages)
        {
            listData.Items.Add(item);
        }
        if (listData != SelectedTabListData)
            listData.Id = 4;
        else listData.Id = 1;
    }

    // 로그지우기
    private void OutputLogClear(LogKind kind)
    {
        var listData = TabListDatas[(int)kind];
        listData.Items.Clear();
    }

    // 리스트 더블클릭 : 최근조회TR
    private void ListBox_MouseDoubleClick(string Text)
    {
        if (SelectedTabListData != null && SelectedTabListData.Name.Equals("최근조회TR"))
        {
            var vals = Text.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (vals.Length > 2)
            {
                string code = vals[vals.Length-2].Trim();
                if (_modelClasses.TryGetValue(code, out var modelClass))
                {
                    SelectPanelType(modelClass);
                }
            }
        }
    }

    private string? _doubleClickedItem;
    public string? DoubleClickedItem
    {
        get => _doubleClickedItem;
        set
        {
            if (!string.Equals(_doubleClickedItem, value))
            {
                _doubleClickedItem = value;
                if (_doubleClickedItem != null) ListBox_MouseDoubleClick(_doubleClickedItem);
            }
        }
    }

    [RelayCommand]
    void Logs_Menu_Copy()
    {
        if (SelectedTabListData is not null)
        {
            var lines = SelectedTabListData.Items;
            StringBuilder stringBuilder = new();
            foreach (string line in lines)
            {
                stringBuilder.AppendLine(line);
            }
            string sAll = stringBuilder.ToString();
            if (sAll.Length > 0)
            {
                Clipboard.SetText(sAll);
            }
        }
    }

    [RelayCommand]
    void Logs_Menu_Clear()
    {
        if (SelectedTabListData is not null)
        {
            SelectedTabListData.Items.Clear();
            SelectedTabListData.Id = 0;
        }
    }

    [RelayCommand]
    void Logs_Menu_AllClear()
    {
        foreach (var data in TabListDatas)
        {
            data.Items.Clear();
            data.Id = 0;
        }
    }

    [ObservableProperty]
    string? _selectedLogListItem;

    [RelayCommand]
    void Logs_Menu_RemoveBroad()
    {
        if (_openApi.Connected == false)
        {
            return;
        }
        if (SelectedTabListData != null && SelectedTabListData.Name.Equals("OnRealData"))
        {
            if (SelectedLogListItem != null)
            {
                int nTrCodeTextIndex = SelectedLogListItem.IndexOf("TrCode=");
                if (nTrCodeTextIndex != -1)
                {
                    string tr_code = SelectedLogListItem.Substring(nTrCodeTextIndex + "TrCode=".Length, 3);
                    if (tr_code.Length == 3)
                    {
                        int nKeyTextIndex = SelectedLogListItem.IndexOf("Key=");
                        if (nKeyTextIndex != -1)
                        {
                            string tr_key = SelectedLogListItem.Substring(nKeyTextIndex + "Key=".Length).Split(',')[0].Trim();
                            _ = _openApi.RemoveRealtimeRequest(tr_code, tr_key);
                        }
                    }
                }
            }
        }
    }
}

