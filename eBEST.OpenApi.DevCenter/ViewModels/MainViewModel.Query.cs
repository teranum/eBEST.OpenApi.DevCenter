using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBEST.OpenApi.DevCenter.Models;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace eBEST.OpenApi.DevCenter.ViewModels;

internal partial class MainViewModel
{
    [ObservableProperty] string _req_path = string.Empty;
    [ObservableProperty] string _req_tr_cd = string.Empty;
    [ObservableProperty] string _req_tr_cont = string.Empty;
    [ObservableProperty] string _req_tr_cont_key = string.Empty;
    [ObservableProperty] string _req_JsonText = string.Empty;
    [ObservableProperty] string _req_Time = "요청시간 : ";
    [ObservableProperty] string _res_tr_cd = string.Empty;
    [ObservableProperty] string _res_tr_cont = string.Empty;
    [ObservableProperty] string _res_tr_cont_key = string.Empty;
    [ObservableProperty] string _res_JsonText = string.Empty;
    [ObservableProperty] string _res_Time = "응답시간 : ";

    private string _save_tr_cont = string.Empty;
    private string _save_tr_cont_key = string.Empty;
    private string _save_cts_date = string.Empty;
    private string _save_cts_time = string.Empty;

    [RelayCommand]
    void Query()
    {
        _ = QueryProp("N", string.Empty);
    }

    [RelayCommand(CanExecute = nameof(CanQueryNext))]
    void QueryNext()
    {
        _ = QueryProp(_save_tr_cont, _save_tr_cont_key);
    }

    private bool CanQueryNext() => _save_tr_cont.Equals("Y");

    async Task QueryProp(string tr_cont, string tr_cont_key)
    {
        //MakeEquipText();

        if (!_openApi.Connected)
        {
            OutputLog(LogKind.LOGS, "로그인 후 요청해 주세요");
            return;
        }

        if (_selectedPanelType == null || InBlockDatas == null || OutBlockDatas == null) return;

        PathAttribute? pathAttribute = _selectedPanelType.GetCustomAttribute<PathAttribute>();
        if (pathAttribute == null || pathAttribute.Path.Contains("websocket"))
        {
            // 실시간 시세 조회 요청
            string trCode = _selectedPanelType.Name;
            if (InBlockDatas.Count == 0)
                return;
            if (InBlockDatas[0].BlockDatas.Count == 0)
                return;

            var record_value = InBlockDatas[0].BlockDatas[0];

            var valueType = record_value.GetType();
            var parameters = valueType.GetProperties();
            foreach (var param in parameters)
            {
                var dd = param.GetValue(record_value);
                if (dd == null) continue;
                if (param.PropertyType == typeof(string))
                {
                    _appRegistry.SetValue(valueType.Name, param.Name!, dd as string);
                    continue;
                }

                if (param.PropertyType == typeof(int))
                {
                    _appRegistry.SetValue(valueType.Name, param.Name!, (int)dd);
                    continue;
                }

                if (param.PropertyType == typeof(double))
                {
                    _appRegistry.SetValue(valueType.Name, param.Name!, (double)dd);
                    continue;
                }

                if (param.PropertyType == typeof(long))
                {
                    _appRegistry.SetValue(valueType.Name, param.Name!, (long)dd);
                    continue;
                }
            }

            string rd = string.Empty;
            if (parameters.Length > 0 && parameters[0].GetValue(record_value) is string param_value)
            {
                rd = param_value;
            }

            bool isAccount = pathAttribute != null && pathAttribute.Key.Equals("account");
            Stopwatch rd_timer = Stopwatch.StartNew();
            if (isAccount)
                await _openApi.AddAccountRealtimeRequest(trCode).ConfigureAwait(true);
            else
                await _openApi.AddRealtimeRequest(trCode, rd).ConfigureAwait(true);
            rd_timer.Stop();
            OutputLog(LogKind.LOGS, $"{_selectedPanelType.Name} 실시간 시세 요청 완료: time(ms) = {rd_timer.Elapsed.TotalMilliseconds}");
            return;
        }

        var inBlockProperties = _selectedPanelType.GetProperties().Where(m => m.Name.Contains("InBlock"));
        if (!inBlockProperties.Any()) return;

        var outBlockProperties = _selectedPanelType.GetProperties().Where(m => m.Name.Contains("OutBlock"));
        if (!outBlockProperties.Any()) return;

        Dictionary<string, object?> nameValueCollection = [];
        foreach (var p in inBlockProperties)
        {
            bool bArray = p.PropertyType.Name.Contains("[]");
            var blockDescName = p.Name + (bArray ? " : OCCURS" : string.Empty);
            var blockRecord = InBlockDatas.FirstOrDefault(m => m.DescName.Equals(blockDescName));
            if (blockRecord == null) continue;
            if (blockRecord.BlockDatas.Count == 0) continue;

            var record_value = blockRecord.BlockDatas[0];

            // 일단 세이빙
            var recordType = record_value.GetType();
            var parameters = recordType.GetProperties();
            foreach (var param in parameters)
            {
                var dd = param.GetValue(record_value);
                if (dd == null) continue;
                if (param.PropertyType == typeof(string))
                {
                    string val = (string)dd;
                    _appRegistry.SetValue(recordType.Name, param.Name!, val);
                    continue;
                }
                else if (param.PropertyType == typeof(int))
                {
                    int val = (int)dd;
                    _appRegistry.SetValue(recordType.Name, param.Name!, val);
                    continue;
                }
                else if (param.PropertyType == typeof(double))
                {
                    double val = (double)dd;
                    _appRegistry.SetValue(recordType.Name, param.Name!, val);
                    continue;
                }
                else if (param.PropertyType == typeof(long))
                {
                    long val = (long)dd;
                    _appRegistry.SetValue(recordType.Name, param.Name!, dd);
                    continue;
                }
            }

            if (bArray)
                nameValueCollection.Add(p.Name, blockRecord.BlockDatas);
            else
            {
                // for next tr_cont
                if (tr_cont.Equals("Y"))
                {
                    List<object?> value_list = [];
                    foreach (var param in parameters)
                    {
                        var prop_value = param.GetValue(record_value);
                        if (param.Name.Equals("cts_date")) prop_value = _save_cts_date;
                        else if (param.Name.Equals("cts_time")) prop_value = _save_cts_time;
                        value_list.Add(prop_value);
                    }
                    var r4 = Activator.CreateInstance(recordType, [.. value_list]);
                    nameValueCollection.Add(p.Name, r4);
                }
                else
                    nameValueCollection.Add(p.Name, record_value);
            }
        }
        string jsonbody = JsonSerializer.Serialize(nameValueCollection);

        string path = pathAttribute.Path;
        string tr_code = pathAttribute.TRCode.Length > 0 ? pathAttribute.TRCode : _selectedPanelType.Name;

        OutputLog(LogKind.LOGS, $"TR 요청 : {tr_code}, {pathAttribute.Description}");
        Req_Time = $"요청시간 : {DateTime.Now:HH:mm:ss.fff}";

        (string out_tr_cd, string out_tr_cont, string out_tr_cont_key, string jsonResponse) = await _openApi.GetDataWithJsonString(path, tr_code, tr_cont, tr_cont_key, jsonbody).ConfigureAwait(true);

        Res_Time = $"응답시간 : {DateTime.Now:HH:mm:ss.fff}";


        OutputLog(LogKind.최근조회TR, $"{_selectedPanelType.Name} : {pathAttribute.Description}");

        Req_path = path;
        Req_tr_cd = tr_code;
        Req_tr_cont = tr_cont;
        Req_tr_cont_key = tr_cont_key;
        Req_JsonText = jsonbody;
        Res_tr_cd = out_tr_cd;
        Res_tr_cont = out_tr_cont;
        Res_tr_cont_key = out_tr_cont_key;
        Res_JsonText = jsonResponse;

        _save_tr_cont = out_tr_cont;
        _save_tr_cont_key = out_tr_cont_key;
        QueryNextCommand.NotifyCanExecuteChanged();

        var resultBody = JsonSerializer.Deserialize(jsonResponse, _selectedPanelType, _jsonOptions);
        _save_cts_date = string.Empty;
        _save_cts_time = string.Empty;

        List<BlockRecord> newOutBlockDatas = [];
        foreach (var p in outBlockProperties)
        {
            bool bArray = false;
            var data = p.GetValue(resultBody);
            var newList = new List<object>();
            if (data == null) continue;
            if (data is IEnumerable listData)
            {
                bArray = true;
                foreach (var item in listData)
                {
                    newList.Add(item);
                }
            }
            else
            {
                newList.Add(data);
                // for next tr_cont
                if (_save_tr_cont.Equals("Y"))
                {
                    Type dataType = data.GetType();
                    if (dataType.GetProperty("cts_date") is PropertyInfo prop_cts_date)
                        _save_cts_date = (string)prop_cts_date.GetValue(data)!;
                    if (dataType.GetProperty("cts_time") is PropertyInfo prop_cts_time)
                        _save_cts_time = (string)prop_cts_time.GetValue(data)!;
                }
            }
            var blockDescName = p.Name + (bArray ? " : OCCURS" : string.Empty);
            newOutBlockDatas.Add(new BlockRecord(p.Name, $"{blockDescName} ({newList.Count})", newList));
        }
        OutBlockDatas = newOutBlockDatas;
    }

    [RelayCommand]
    async Task QueryRequestAsync()
    {
        // 요청 데이터로 직접 TR 요청
        if (!_openApi.Connected)
        {
            OutputLog(LogKind.LOGS, "로그인 후 요청해 주세요");
            return;
        }

        // 입력 파라메터 간단 검사
        if (Req_path.Length == 0 || Req_tr_cd.Length == 0)
        {
            OutputLog(LogKind.LOGS, "path, tr_cd 는 필수 입력입니다.");
            return;
        }

        OutputLog(LogKind.LOGS, $"전문 요청 : path={Req_path}, tr_cd={Req_tr_cd}");

        Req_Time = $"요청시간 : {DateTime.Now:HH:mm:ss.fff}";

        (Res_tr_cd, Res_tr_cont, Res_tr_cont_key, Res_JsonText) = await _openApi.GetDataWithJsonString(Req_path, Req_tr_cd, Req_tr_cont, Req_tr_cont_key, Req_JsonText).ConfigureAwait(true);

        Res_Time = $"응답시간 : {DateTime.Now:HH:mm:ss.fff}";

        if (_modelClasses.TryGetValue(Req_tr_cd, out var modelClass))
        {
            SelectPanelType(modelClass);
        }
    }

    void MakeEquipText()
    {
        if (_selectedPanelType == null || InBlockDatas == null) return;
        var trName = _selectedPanelType.Name;
        PathAttribute? pathAttribute = _selectedPanelType.GetCustomAttribute<PathAttribute>();
        if (pathAttribute == null) return;
        bModelSrcMode = false;
        if (pathAttribute.Path.Contains("websocket"))
        {
            StringBuilder sb_wss = new();
            if (LangType == LANG_TYPE.CSHARP)
            {
                if (pathAttribute.Key.Equals("account"))
                {
                    sb_wss.AppendLine($"// [실시간 계좌응답 요청] {trName} : {pathAttribute.Description}");
                    sb_wss.AppendLine($"api.AddAccountRealtimeRequest(\"{trName}\");");
                }
                else
                {
                    // InBlock 프로퍼티 찾는다
                    var in_record = InBlockDatas[0].BlockDatas[0];
                    var inblock_props = in_record.GetType().GetProperties();
                    if (inblock_props.Length == 0) return;
                    var inblock_prop_first = inblock_props[0];
                    var inblock_forst_data = inblock_prop_first.GetValue(in_record);
                    sb_wss.AppendLine($"// [실시간 시세 요청] {trName} : {pathAttribute.Description}");
                    sb_wss.AppendLine($"await api.AddRealtimeRequest(\"{trName}\", \"{inblock_forst_data}\");");
                }
                sb_wss.AppendLine();
                sb_wss.AppendLine($"// OnRealtimeEvent 이벤트");
                sb_wss.AppendLine("JsonSerializerOptions _jsonOptions = new() { NumberHandling = JsonNumberHandling.AllowReadingFromString };");
                sb_wss.AppendLine($"if (e.TrCode.Equals(\"{trName}\"))");
                sb_wss.AppendLine("{");
                sb_wss.AppendLine($"\tvar outBlockData = e.RealtimeBody.Deserialize<{trName}OutBlock>(_jsonOptions);");
                sb_wss.AppendLine($"\tif (outBlockData is not null)");
                sb_wss.AppendLine("\t{");
                sb_wss.AppendLine($"\t\t// {trName}OutBlock 데이터 처리");
                sb_wss.AppendLine("\t}");
                sb_wss.AppendLine("}");
            }
            else if (LangType == LANG_TYPE.PYTHON)
            {
                sb_wss.Append(
                    $$"""
                    import asyncio
                    import ebest
                    from app_keys import appkey, appsecretkey # app_keys.py 파일에 appkey, appsecretkey 변수를 정의하고 사용하세요
                    
                    async def main():
                        api=ebest.OpenApi()
                        api.on_realtime = on_realtime # 실시간 이벤트 핸들러 등록
                        if not await api.login(appkey, appsecretkey): return print(f"연결실패: {api.last_message}")

                    
                    """);

                if (pathAttribute.Key.Equals("account"))
                {
                    sb_wss.AppendLine($"    # [실시간 계좌응답 요청] {trName} : {pathAttribute.Description}");
                    sb_wss.AppendLine($"    await api.add_realtime(\"{trName}\", \"\")");
                }
                else
                {
                    sb_wss.AppendLine($"    # [실시간 시세 요청] {trName} : {pathAttribute.Description}");
                    // InBlock 프로퍼티 찾는다
                    if (InBlockDatas.Count > 0)
                    {
                        var in_record = InBlockDatas[0].BlockDatas[0];
                        var inblock_props = in_record.GetType().GetProperties();
                        if (inblock_props.Length == 0) return;
                        var inblock_prop_first = inblock_props[0];
                        var inblock_forst_data = inblock_prop_first.GetValue(in_record);
                        sb_wss.AppendLine($"    await api.add_realtime(\"{trName}\", \"{inblock_forst_data}\")");
                    }
                }

                sb_wss.Append(
                    $$"""
                        
                        ... # 다른 작업 수행
                        await api.close()


                    def on_realtime(api:ebest.OpenApi, trcode, key, realtimedata):
                        if trcode == "{{trName}}":
                            print(f"{trcode}: {key}, {realtimedata}")

                    asyncio.run(main())
                    
                    """);
            }

            EquipText = sb_wss.ToString();

            return;
        }

        var inBlockProperties = _selectedPanelType.GetProperties().Where(m => m.Name.Contains("InBlock"));
        if (!inBlockProperties.Any()) return;

        var outBlockProperties = _selectedPanelType.GetProperties().Where(m => m.Name.Contains("OutBlock"));
        if (!outBlockProperties.Any()) return;

        Dictionary<string, object?> nameValueCollection = [];
        List<(bool bArray, List<string> names, List<string> values)> string_values = [];
        foreach (var p in inBlockProperties)
        {
            bool bArray = p.PropertyType.Name.Contains("[]");
            var blockDescName = p.Name + (bArray ? " : OCCURS" : string.Empty);
            var blockRecord = InBlockDatas.FirstOrDefault(m => m.DescName.Equals(blockDescName));
            if (blockRecord == null) continue;
            if (blockRecord.BlockDatas.Count == 0) continue;

            var name_strings = new List<string>();
            var param_strings = new List<string>();

            var record_value = blockRecord.BlockDatas[0];

            // 일단 세이빙
            var recordType = record_value.GetType();
            var parameters = recordType.GetProperties();
            foreach (var param in parameters)
            {
                var dd = param.GetValue(record_value);
                if (dd == null) continue;
                name_strings.Add(param.Name);
                if (param.PropertyType == typeof(string))
                {
                    string val = (string)dd;
                    param_strings.Add("\"" + val + "\"");
                    continue;
                }
                else if (param.PropertyType == typeof(int))
                {
                    int val = (int)dd;
                    param_strings.Add(val.ToString());
                    continue;
                }
                else if (param.PropertyType == typeof(double))
                {
                    double val = (double)dd;
                    param_strings.Add(val.ToString());
                    continue;
                }
                else if (param.PropertyType == typeof(long))
                {
                    long val = (long)dd;
                    param_strings.Add(val.ToString());
                    continue;
                }
            }

            string_values.Add((bArray, name_strings, param_strings));

            nameValueCollection.Add(p.Name, record_value);
        }

        StringBuilder sb = new();
        if (LangType == LANG_TYPE.CSHARP)
        {
            sb.AppendLine($"// [요청] {trName} : {pathAttribute.Description}");
            sb.AppendLine($"{trName} tr_data = new()");
            sb.AppendLine("\t{");
            int nInBlockIndex = 0;
            foreach (var name_value in nameValueCollection)
            {
                var bArray = string_values[nInBlockIndex].bArray;
                sb.Append($"\t\t{name_value.Key} = ");
                if (bArray) sb.Append('[');
                sb.Append("new(");
                var inblock_values = string_values[nInBlockIndex].values;
                for (int i = 0; i < inblock_values.Count; i++)
                {
                    if (i != 0) sb.Append(", ");
                    sb.Append($"{inblock_values[i]}");
                }
                sb.Append("),");
                if (bArray) sb.Append(" ],");
                sb.AppendLine("");
                nInBlockIndex++;
            }
            sb.AppendLine("\t};");
            sb.AppendLine("await api.GetTRData(tr_data);");

            var first_outblock = outBlockProperties.First();
            sb.AppendLine($"if (tr_data.{first_outblock.Name} is null)");
            sb.AppendLine("{");
            sb.AppendLine("\t// 오류 처리");
            sb.AppendLine("\tprint(tr_data.rsp_cd.Length > 0 ? $\"{tr_data.rsp_cd}-{tr_data.rsp_msg}\" : api.LastErrorMessage);");
            sb.AppendLine("\treturn;");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine($"// tr_data.{first_outblock.Name} 데이터 처리");
        }
        else if (LangType == LANG_TYPE.PYTHON)
        {
            sb.AppendLine(
                $$"""
                    import asyncio
                    import ebest
                    from app_keys import appkey, appsecretkey # app_keys.py 파일에 appkey, appsecretkey 변수를 정의하고 사용하세요
                    
                    async def main():
                        api=ebest.OpenApi()
                        if not await api.login(appkey, appsecretkey): return print(f"연결실패: {api.last_message}")

                        # [요청] {{trName}} : {{pathAttribute.Description}}
                        request = {
                    """);
            int nInBlockIndex = 0;
            foreach (var name_value in nameValueCollection)
            {
                var bArray = string_values[nInBlockIndex].bArray;
                sb.Append($"        \"{name_value.Key}\": ");
                if (bArray) sb.Append('[');
                else sb.AppendLine("{");
                var inblock_values = string_values[nInBlockIndex].values;
                var inblock_names = string_values[nInBlockIndex].names;
                for (int i = 0; i < inblock_values.Count; i++)
                {
                    sb.AppendLine($"            \"{inblock_names[i]}\": {inblock_values[i]},");
                }
                if (bArray) sb.Append(" ],");
                sb.AppendLine($$"""        },""");
                nInBlockIndex++;
            }
            sb.Append(
                $$"""
                        }
                        response = await api.request("{{trName}}", request)
                        if not response: return print(f"요청실패: {api.last_message}")
                    
                        print(response.body)

                        ... # 다른 작업 수행
                        await api.close()

                    asyncio.run(main())
                    
                    """);
        }

        EquipText = sb.ToString();
    }

    bool bModelSrcMode = false;
    [RelayCommand]
    void ModelSrc()
    {
        if (_selectedPanelType == null) return;
        Type tType = _selectedPanelType!;

        PathAttribute? pathAttribute = tType.GetCustomAttribute<PathAttribute>();
        if (pathAttribute == null) return;

        if (bModelSrcMode)
        {
            MakeEquipText();
            return;
        }
        bModelSrcMode = true;

        var block_props = tType.GetProperties();
        var inblock_props = block_props.Where(m => m.Name.Contains("InBlock"));
        var outblock_props = block_props.Where(m => m.Name.Contains("OutBlock"));

        StringBuilder sb = new();
        StringBuilder sb_param = new();
        if (LangType == LANG_TYPE.CSHARP)
        {
            sb.AppendLine($"// {tType.Name} : {pathAttribute.Description}");

            foreach (var block in block_props)
            {
                if (!block.Name.Contains("InBlock") && !block.Name.Contains("OutBlock")) continue;
                sb_param.Clear();

                if (_blockRecords.TryGetValue(block.Name, out var blockRecord))
                {
                    var parameters = blockRecord.GetProperties();
                    foreach (var param in parameters)
                    {
                        if (sb_param.Length > 0) sb_param.Append(", ");
                        var typeName = GetParameterTypeName(param.PropertyType);
                        sb_param.Append($"{typeName} {param.Name}");
                    }
                    sb.AppendLine($"public record {block.Name}({sb_param});");
                }

            }


            sb.AppendLine();
            sb.AppendLine($"[Path(\"{pathAttribute.Path}\")]");
            sb.AppendLine($"public class {tType.Name} : TrBase");
            sb.AppendLine("{");
            sb.AppendLine("\t// 요청");
            foreach (var block in inblock_props)
            {
                sb.AppendLine("\t" + $$$"""public {{{block.PropertyType.Name}}}? {{{block.Name}}} { get; set; }""");
            }
            sb.AppendLine();
            sb.AppendLine("\t// 응답");
            foreach (var block in outblock_props)
            {
                sb.AppendLine("\t" + $$$"""public {{{block.PropertyType.Name}}}? {{{block.Name}}} { get; set; }""");
            }
            sb.AppendLine("}");
            sb.AppendLine();
        }
        else if (LangType == LANG_TYPE.PYTHON)
        {
            foreach (var block in block_props)
            {
                if (!block.Name.Contains("InBlock") && !block.Name.Contains("OutBlock")) continue;
                sb_param.Clear();

                if (_blockRecords.TryGetValue(block.Name, out var blockRecord))
                {
                    var parameters = blockRecord.GetConstructors()[0].GetParameters();//.GetProperties();
                    sb.AppendLine($"class {block.Name}:");
                    foreach (var param in parameters)
                    {
                        var typeName = param.ParameterType.Name switch
                        {
                            "String" => "str",
                            "Int32" => "int",
                            "Double" => "float",
                            "Int64" => "int",
                            _ => param.ParameterType.Name
                        };
                        BlockFieldAttribute? descriptionAttribute = param.GetCustomAttribute<BlockFieldAttribute>();
                        sb.AppendLine($"    {param.Name}: {typeName}");
                        if (descriptionAttribute != null)
                        {
                            var desc_text = descriptionAttribute.Description;
                            desc_text = desc_text.Replace("\tdouble", "\tnumber");
                            desc_text = desc_text.Replace("\tlong", "\tnumber");
                            desc_text = desc_text.Replace("\tint", "\tnumber");
                            sb.AppendLine($"    \"\"\" {descriptionAttribute.Description} : {typeName} ({descriptionAttribute.Size})\"\"\"");
                        }
                        sb.AppendLine();
                    }
                }
            }
        }

        EquipText = sb.ToString();


    }

    partial void OnLangTypeChanged(LANG_TYPE oldValue, LANG_TYPE newValue) => MakeEquipText();

    void LoadToolsData()
    {
        string section = "TestBed";
        Req_path = _appRegistry.GetValue(section, "path", "/indtp/market-data");
        Req_tr_cd = _appRegistry.GetValue(section, "tr_cd", "t8424");
        Req_tr_cont = "N";
        string defaultJsonText = "{\r\n  \"t8424InBlock\": {\r\n    \"gubun1\": \"1\"\r\n  }\r\n}";
        Req_JsonText = _appRegistry.GetValue(section, "JsonText", defaultJsonText);

        if (Req_JsonText.Equals(defaultJsonText))
        {
            // 처음으로 앱을 작동 시켰다면
            _appRegistry.SetValue("t8424InBlock", "gubun1", "1");
        }

        if (_modelClasses.TryGetValue(Req_tr_cd, out var modelClass))
        {
            SelectPanelType(modelClass);
        }
    }

    void SaveToolsData()
    {
        string section = "TestBed";
        _appRegistry.SetValue(section, "path", Req_path);
        _appRegistry.SetValue(section, "tr_cd", Req_tr_cd);
        _appRegistry.SetValue(section, "JsonText", Req_JsonText);
    }

    [RelayCommand]
    static void DataCopy(object block)
    {
        if (block is BlockRecord blockRecord)
        {
            var datas = blockRecord.BlockDatas;
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($";{blockRecord.Name}");
            if (datas.Count > 0)
            {
                var properties = datas[0].GetType().GetProperties();
                stringBuilder.Append(';');
                foreach (var prop in properties)
                {
                    stringBuilder.Append($"{prop.Name}\t");
                }
                stringBuilder.AppendLine();
                foreach (var data in datas)
                {
                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(data);
                        if (value != null)
                        {
                            stringBuilder.Append($"{value}\t");
                        }
                    }
                    stringBuilder.AppendLine();
                }
            }
            Clipboard.SetText(stringBuilder.ToString());
        }
    }
}

