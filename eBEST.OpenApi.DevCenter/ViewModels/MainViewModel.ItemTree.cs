using CommunityToolkit.Mvvm.ComponentModel;
using eBEST.OpenApi.DevCenter.Models;
using eBEST.OpenApi.Models;
using System.ComponentModel;
using System.Reflection;

namespace eBEST.OpenApi.DevCenter.ViewModels;

internal partial class MainViewModel
{
    [ObservableProperty] string _tRName = "";
    [ObservableProperty] IList<BlockRecord>? _inBlockDatas;
    [ObservableProperty] IList<BlockRecord>? _outBlockDatas;

    public List<TabTreeData>? TabTreeDatas { get; protected set; }
    public TabTreeData? SelectedTabTreeData { get; set; }
    private IdTextItem? _selectedTreeItem;
    public IdTextItem? SelectedTreeItem
    {
        get => _selectedTreeItem;
        set
        {
            if (_selectedTreeItem == value) return;
            _selectedTreeItem = value;
            TreeView_SelectedItemChanged(_selectedTreeItem);
        }
    }

    private readonly Dictionary<string, Type> _modelClasses = [];
    private readonly Dictionary<string, Type> _blockRecords = [];

    void LoadTrDatas()
    {
        const int image_Group = 1;
        const int image_subGroup = 3;

        var 업종 = new IdTextItem(image_Group, "업종");
        {
            var group = 업종;

            var 시세 = new IdTextItem(image_subGroup, "시세");
            GetChildTrData(시세, "/indtp/market-data");
            var 차트 = new IdTextItem(image_subGroup, "차트");
            GetChildTrData(차트, "/indtp/chart");

            group.AddChild(시세);
            group.AddChild(차트);
            group.IsExpanded = true;
        }
        var 주식 = new IdTextItem(image_Group, "주식");
        {
            var group = 주식;

            var 시세 = new IdTextItem(image_subGroup, "시세");
            GetChildTrData(시세, "/stock/market-data");
            var 거래원 = new IdTextItem(image_subGroup, "거래원");
            GetChildTrData(거래원, "/stock/exchange");
            var 투자정보 = new IdTextItem(image_subGroup, "투자정보");
            GetChildTrData(투자정보, "/stock/investinfo");
            var 프로그램 = new IdTextItem(image_subGroup, "프로그램");
            GetChildTrData(프로그램, "/stock/program");
            var 투자자 = new IdTextItem(image_subGroup, "투자자");
            GetChildTrData(투자자, "/stock/investor");
            var 외인_기관 = new IdTextItem(image_subGroup, "외인/기관");
            GetChildTrData(외인_기관, "/stock/frgr-itt");
            var ELW = new IdTextItem(image_subGroup, "ELW");
            GetChildTrData(ELW, "/stock/elw");
            var ETF = new IdTextItem(image_subGroup, "ETF");
            GetChildTrData(ETF, "/stock/etf");
            var 섹터 = new IdTextItem(image_subGroup, "섹터");
            GetChildTrData(섹터, "/stock/sector");
            var 종목검색 = new IdTextItem(image_subGroup, "종목검색");
            GetChildTrData(종목검색, "/stock/item-search");
            var 상위종목 = new IdTextItem(image_subGroup, "상위종목");
            GetChildTrData(상위종목, "/stock/high-item");
            var 차트 = new IdTextItem(image_subGroup, "차트");
            GetChildTrData(차트, "/stock/chart");
            var etc = new IdTextItem(image_subGroup, "기타");
            GetChildTrData(etc, "/stock/etc");
            var 계좌 = new IdTextItem(image_subGroup, "계좌");
            GetChildTrData(계좌, "/stock/accno");
            var 주문 = new IdTextItem(image_subGroup, "주문");
            GetChildTrData(주문, "/stock/order");

            group.AddChild(시세);
            group.AddChild(거래원);
            group.AddChild(투자정보);
            group.AddChild(프로그램);
            group.AddChild(투자자);
            group.AddChild(외인_기관);
            group.AddChild(섹터);
            group.AddChild(종목검색);
            group.AddChild(상위종목);
            group.AddChild(차트);
            group.AddChild(etc);
            group.AddChild(계좌);
            group.AddChild(주문);
            group.IsExpanded = true;
        }
        var 선물옵션 = new IdTextItem(image_Group, "선물/옵션");
        {
            var group = 선물옵션;

            var 시세 = new IdTextItem(image_subGroup, "시세");
            GetChildTrData(시세, "/futureoption/market-data");
            var 투자자 = new IdTextItem(image_subGroup, "투자자");
            GetChildTrData(투자자, "/futureoption/investor");
            var 차트 = new IdTextItem(image_subGroup, "차트");
            GetChildTrData(차트, "/futureoption/chart");
            var 계좌 = new IdTextItem(image_subGroup, "계좌");
            GetChildTrData(계좌, "/futureoption/accno");
            var 주문 = new IdTextItem(image_subGroup, "주문");
            GetChildTrData(주문, "/futureoption/order");
            var etc = new IdTextItem(image_subGroup, "기타");
            GetChildTrData(etc, "/futureoption/etc");

            group.AddChild(시세);
            group.AddChild(투자자);
            group.AddChild(차트);
            group.AddChild(계좌);
            group.AddChild(주문);
            group.AddChild(etc);
            group.IsExpanded = true;
        }
        var 해외선물 = new IdTextItem(image_Group, "해외선물");
        {
            var group = 해외선물;

            var 시세 = new IdTextItem(image_subGroup, "시세");
            GetChildTrData(시세, "/overseas-futureoption/market-data");
            var 계좌 = new IdTextItem(image_subGroup, "계좌");
            GetChildTrData(계좌, "/overseas-futureoption/accno");
            var 주문 = new IdTextItem(image_subGroup, "주문");
            GetChildTrData(주문, "/overseas-futureoption/order");
            var 차트 = new IdTextItem(image_subGroup, "차트");
            GetChildTrData(차트, "/overseas-futureoption/chart");

            group.AddChild(시세);
            group.AddChild(계좌);
            group.AddChild(주문);
            group.AddChild(차트);
            group.IsExpanded = true;
        }
        var 기타 = new IdTextItem(image_Group, "기타");
        {
            var group = 기타;

            var 시간조회 = new IdTextItem(image_subGroup, "시간조회");
            GetChildTrData(시간조회, "/etc/time-search");

            group.AddChild(시간조회);
            group.IsExpanded = true;
        }

        var 실시간업종 = new IdTextItem(image_Group, "업종");
        {
            var group = 실시간업종;

            GetChildTrData(group, "/websocket/indtp");

            group.IsExpanded = true;
        }
        var 실시간주식 = new IdTextItem(image_Group, "주식");
        {
            var group = 실시간주식;

            GetChildTrData(group, "/websocket/stock");

            group.IsExpanded = true;
        }
        var 실시간선물옵션 = new IdTextItem(image_Group, "선물옵션");
        {
            var group = 실시간선물옵션;

            GetChildTrData(group, "/websocket/futureoption");

            group.IsExpanded = true;
        }
        var 실시간ELW = new IdTextItem(image_Group, "ELW");
        {
            var group = 실시간ELW;

            GetChildTrData(group, "/websocket/elw");

            group.IsExpanded = true;
        }
        var 실시간해외선물 = new IdTextItem(image_Group, "해외선물");
        {
            var group = 실시간해외선물;

            GetChildTrData(group, "/websocket/overseas-futureoption");

            group.IsExpanded = true;
        }
        var 실시간기타 = new IdTextItem(image_Group, "기타");
        {
            var group = 실시간기타;

            GetChildTrData(group, "/websocket/etc");

            group.IsExpanded = true;
        }

        var 실시간_시세_투자정보 = new IdTextItem(image_Group, "실시간 시세 투자정보");
        {
            var group = 실시간_시세_투자정보;

            GetChildTrData(group, "/websocket/investinfo");

            group.IsExpanded = true;
        }


        var Tr목록 = new TabTreeData(1, "Tr목록")
        {
            OrgItems = [업종, 주식, 선물옵션, 해외선물, 기타],
        };

        var Real목록 = new TabTreeData(2, "Real목록")
        {
            OrgItems = [실시간업종, 실시간주식, 실시간선물옵션, 실시간ELW, 실시간해외선물, 실시간기타, 실시간_시세_투자정보],
        };

        var CustomList = new TabTreeData(0, "사용자기능")
        {
            OrgItems = [new IdTextItem(image_Group, "API정보(개발중...)"),],
        };

        TabTreeDatas = [Tr목록, Real목록, CustomList];
        SelectedTabTreeData = Tr목록;
    }

    private void GetChildTrData(IdTextItem owner, string url)
    {
        foreach (var modelClass in _modelClasses)
        {
            var modle = modelClass.Value;
            PathAttribute? pathAttribute = modle.GetCustomAttribute<PathAttribute>();
            if (pathAttribute != null && pathAttribute.Path.Equals(url))
            {
                string name = modle.Name;
                if (pathAttribute.Description.Length > 0)
                {
                    name += " : " + pathAttribute.Description;
                }
                var item = new IdTextItem(14, name) { Tag = modle, };
                GetInOutProps(item);
                owner.AddChild(item);
            }
        }
    }

    private void GetInOutProps(IdTextItem owner)
    {
        Type type = (Type)owner.Tag!;
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in props)
        {
            bool bArray = prop.PropertyType.Name.EndsWith("[]");
            string blockName = prop.PropertyType.Name;
            if (bArray)
            {
                blockName = blockName[..^2];
            }
            if (_blockRecords.TryGetValue(blockName, out Type? recordType))
            {
                var item = new IdTextItem(4, (bArray ? prop.Name + " - OCCURS" : prop.Name)) { Tag = recordType, };
                bool bInputBlock = blockName.Contains("InBlock");
                GeRecordProps(item, bInputBlock ? 20 : 19);
                item.IsExpanded = true;
                owner.AddChild(item);
            }
        }
    }

    private static void GeRecordProps(IdTextItem owner, int imageId)
    {
        Type type = (Type)owner.Tag!;
        var paramsInfo = type.GetConstructors()[0].GetParameters();
        foreach (var paramInfo in paramsInfo)
        {
            string name = paramInfo.Name!;
            DescriptionAttribute? descriptionAttribute = paramInfo.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute != null)
            {
                var words = descriptionAttribute.Description.Split('\t');
                if (words.Length >= 3)
                {
                    name += " : " + words[0] + "    " + words[1] + "(" + words[2] + ")";
                }
            }
            var item = new IdTextItem(imageId, name) { Tag = paramInfo, };
            owner.AddChild(item);
        }
    }


    private void TreeView_SelectedItemChanged(IdTextItem? selectedItem)
    {
        if (SelectedTabTreeData is null) return;
        if (selectedItem is null) return;

        if (selectedItem.Id == 14)
        {
            SelectPanelType((Type)selectedItem.Tag!);
        }
    }


    Type? _selectedPanelType;
    void SelectPanelType(Type trType)
    {
        if (trType == _selectedPanelType) return;

        PathAttribute? pathAttribute = trType.GetCustomAttribute<PathAttribute>();
        if (pathAttribute == null)
        {
            return;
        }

        _save_tr_cont = string.Empty;
        _save_tr_cont_key = string.Empty;
        QueryNextCommand.NotifyCanExecuteChanged();

        string trTitle = $"{trType.Name} : {pathAttribute.Description}";
        if (pathAttribute.Key.Length > 0)
        {
            trTitle += $" [{pathAttribute.Key}]";
            trTitle = trTitle.Replace("[account]", "[계좌]");
        }
        TRName = trTitle;

        _selectedPanelType = trType;

        var props = trType.GetProperties();
        List<BlockRecord> inBlockDatas = [];
        List<BlockRecord> outBlockDatas = [];
        foreach (var prop in props)
        {
            var propName = prop.PropertyType.Name;
            bool bArray = propName.EndsWith("[]");
            string blockName = propName;
            if (bArray)
            {
                blockName = blockName[..^2];
            }
            if (_blockRecords.TryGetValue(blockName, out Type? recordType))
            {
                var blockDescName = blockName + (bArray ? " : OCCURS" : string.Empty);
                if (blockName.Contains("InBlock"))
                {
                    var parameters = recordType.GetConstructors()[0].GetParameters();
                    var paramObjects = new List<object>();
                    foreach (var param in parameters)
                    {
                        if (param.ParameterType == typeof(string))
                        {
                            paramObjects.Add(_appRegistry.GetValue(recordType.Name, param.Name!, ""));
                            continue;
                        }

                        if (param.ParameterType == typeof(int))
                        {
                            paramObjects.Add(_appRegistry.GetValue(recordType.Name, param.Name!, 0));
                            continue;
                        }

                        if (param.ParameterType == typeof(double))
                        {
                            paramObjects.Add(_appRegistry.GetValue(recordType.Name, param.Name!, 0.0));
                            continue;
                        }

                        if (param.ParameterType == typeof(long))
                        {
                            paramObjects.Add(_appRegistry.GetValue(recordType.Name, param.Name!, 0L));
                            continue;
                        }
                    }
                    var newBlockData = Activator.CreateInstance(recordType, [.. paramObjects]);
                    if (newBlockData != null)
                    {
                        inBlockDatas.Add(new(blockName, blockDescName, [newBlockData,]));
                    }
                }
                else if (blockName.Contains("OutBlock"))
                {
                    outBlockDatas.Add(new(blockName, blockDescName, []));
                }
            }
        }
        InBlockDatas = inBlockDatas;
        OutBlockDatas = outBlockDatas;

        MakeEquipText();
    }
}
