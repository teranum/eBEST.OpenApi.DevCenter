using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBEST.OpenApi.DevCenter.Models;
using eBEST.OpenApi.Models;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace eBEST.OpenApi.DevCenter.ViewModels;

internal partial class MainViewModel
{
    [ObservableProperty] string _req_path = string.Empty;
    [ObservableProperty] string _req_tr_cd = string.Empty;
    [ObservableProperty] string _req_tr_cont = string.Empty;
    [ObservableProperty] string _req_tr_cont_key = string.Empty;
    [ObservableProperty] string _res_tr_cd = string.Empty;
    [ObservableProperty] string _res_tr_cont = string.Empty;
    [ObservableProperty] string _res_tr_cont_key = string.Empty;

    private string _save_tr_cont = string.Empty;
    private string _save_tr_cont_key = string.Empty;

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
            OutputLog(LogKind.LOGS, "로그인 후 사용하세요");
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

            Stopwatch rd_timer = Stopwatch.StartNew();
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
                nameValueCollection.Add(p.Name, record_value);
        }
        string jsonbody = JsonSerializer.Serialize(nameValueCollection);

        string path = pathAttribute.Path;
        string tr_code = pathAttribute.TRCode.Length > 0 ? pathAttribute.TRCode : _selectedPanelType.Name;

        Stopwatch timer = Stopwatch.StartNew();
        (string out_tr_cd, string out_tr_cont, string out_tr_cont_key, string jsonResponse) = await _openApi.GetDataWithJsonString(path, tr_code, tr_cont, tr_cont_key, jsonbody).ConfigureAwait(true);
        timer.Stop();

        OutputLog(LogKind.LOGS, $"{_selectedPanelType.Name} TR 요청 완료: time(ms) = {timer.Elapsed.TotalMilliseconds}");
        OutputLog(LogKind.최근조회TR, $"{_selectedPanelType.Name} : {pathAttribute.Description}");

        RequestText = jsonbody;
        ResponseText = jsonResponse;
        Req_path = path;
        Req_tr_cd = tr_code;
        Req_tr_cont = tr_cont;
        Req_tr_cont_key = tr_cont_key;
        Res_tr_cd = out_tr_cd;
        Res_tr_cont = out_tr_cont;
        Res_tr_cont_key = out_tr_cont_key;

        _save_tr_cont = out_tr_cont;
        _save_tr_cont_key = out_tr_cont_key;
        QueryNextCommand.NotifyCanExecuteChanged();

        var resultBody = JsonSerializer.Deserialize(jsonResponse, _selectedPanelType, _jsonOptions);

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
            }
            var blockDescName = p.Name + (bArray ? " : OCCURS" : string.Empty);
            newOutBlockDatas.Add(new BlockRecord(p.Name, $"{blockDescName} ({newList.Count})", newList));
        }
        OutBlockDatas = newOutBlockDatas;
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

            if (pathAttribute.Key.Equals("account"))
            {
                sb_wss.AppendLine($"// [실시간 계좌응답 요청] {trName} : {pathAttribute.Description}");
                sb_wss.AppendLine($"_openApi.AddAccountRealtimeRequest(\"{trName}\", \"\");");
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
                sb_wss.AppendLine($"_openApi.AddRealtimeRequest(\"{trName}\", \"{inblock_forst_data}\");");
            }
            sb_wss.AppendLine();
            sb_wss.AppendLine($"// OnRealtimeEvent 이벤트");
            sb_wss.AppendLine("JsonSerializerOptions _jsonOptions = new() { NumberHandling = JsonNumberHandling.AllowReadingFromString };");
            sb_wss.AppendLine($"if (e.TrCode.Equals(\"{trName}\"))");
            sb_wss.AppendLine("{");
            sb_wss.AppendLine($"\t{trName}OutBlock? outBlockData = JsonSerializer.Deserialize<{trName}OutBlock>(e.RealtimeBody, _jsonOptions);");
            sb_wss.AppendLine($"\tif (outBlockData is not null)");
            sb_wss.AppendLine("\t{");
            sb_wss.AppendLine($"\t\t// {trName}OutBlock 데이터 처리");
            sb_wss.AppendLine("\t}");
            sb_wss.AppendLine("}");
            EquipText = sb_wss.ToString();

            return;
        }

        var inBlockProperties = _selectedPanelType.GetProperties().Where(m => m.Name.Contains("InBlock"));
        if (!inBlockProperties.Any()) return;

        var outBlockProperties = _selectedPanelType.GetProperties().Where(m => m.Name.Contains("OutBlock"));
        if (!outBlockProperties.Any()) return;

        Dictionary<string, object?> nameValueCollection = [];
        List<(bool bArray, List<string> values)> string_values = [];
        foreach (var p in inBlockProperties)
        {
            bool bArray = p.PropertyType.Name.Contains("[]");
            var blockDescName = p.Name + (bArray ? " : OCCURS" : string.Empty);
            var blockRecord = InBlockDatas.FirstOrDefault(m => m.DescName.Equals(blockDescName));
            if (blockRecord == null) continue;
            if (blockRecord.BlockDatas.Count == 0) continue;

            var param_strings = new List<string>();

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

            string_values.Add((bArray, param_strings));

            nameValueCollection.Add(p.Name, record_value);
        }

        StringBuilder sb = new();
        sb.AppendLine($"// [요청] {_selectedPanelType.Name} : {pathAttribute.Description}");
        sb.AppendLine($"{_selectedPanelType.Name} tr_data = new()");
        sb.AppendLine("\t{");
        int nInBlockIndex = 0;
        foreach (var name_value in nameValueCollection)
        {
            var bArray = string_values[nInBlockIndex].bArray;
            sb.Append($"\t\t{name_value.Key} = ");
            if (bArray) sb.Append("[");
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
        sb.AppendLine("await _openApi.GetTRData(tr_data); // or _openApi.GetTRData(tr_data).Wait();");

        var first_outblock = outBlockProperties.First();
        sb.AppendLine($"if (tr_data.{first_outblock.Name} is null)");
        sb.AppendLine("{");
        sb.AppendLine("\t// 오류 처리");
        sb.AppendLine("\tDebug.WriteLine(tr_data.rsp_cd.Length > 0 ? $\"{tr_data.rsp_cd}-{tr_data.rsp_msg}\" : _openApi.LastErrorMessage);");
        sb.AppendLine("\treturn;");
        sb.AppendLine("}");
        sb.AppendLine($"// tr_data.{first_outblock.Name} 데이터 처리");

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
        sb.AppendLine("// #pragma warning disable IDE1006");
        sb.AppendLine();
        sb.AppendLine("namespace eBEST.OpenApi.Models;");
        sb.AppendLine();
        sb.AppendLine($"// {tType.Name} : {pathAttribute.Description}");

        StringBuilder sb_param = new();
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
                    var typeName = param.PropertyType.Name switch
                    {
                        "String" => "string",
                        "Int32" => "int",
                        "Double" => "double",
                        "Int64" => "long",
                        _ => param.PropertyType.Name
                    };
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

        EquipText = sb.ToString();


    }
}

