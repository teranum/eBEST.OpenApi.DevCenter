using eBEST.OpenApi.DevCenter.Models;
using eBEST.OpenApi.Events;
using eBEST.OpenApi.Models;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eBEST.OpenApi.DevCenter.ViewModels;

internal partial class MainViewModel
{
    private readonly JsonSerializerOptions _jsonOptions = new() { NumberHandling = JsonNumberHandling.AllowReadingFromString };
    private void OpenApi_OnConnectEvent(object? sender, EBestOnConnectEventArgs e)
    {
        StatusUrl = string.Empty;
        if (e.Ok)
        {
            StatusText = "Connected";
            OutputLog(LogKind.LOGS, "로그인 성공" + (_openApi.ServerType == EBestOpenApi.SERVER_TYPE.모의투자 ? "(모의서버)" : "(실서버)"));
        }
        else
        {
            StatusText = "Disconnected";
            OutputLog(LogKind.LOGS, $"로그인 실패 : {e.Msg}");
        }

        MenuLoginCommand.NotifyCanExecuteChanged();
    }

    private void OpenApi_OnRealtimeEvent(object? sender, EBestOnRealtimeEventArgs e)
    {
        if (_modelClasses.TryGetValue(e.TrCode, out var modelClass))
        {
            PathAttribute? pathAttribute = modelClass.GetCustomAttribute<PathAttribute>();
            if (pathAttribute != null && pathAttribute.Key.Equals("account"))
            {
                // OutBlock 프로퍼티 찾는다
                var outblock_prop = modelClass.GetProperties().FirstOrDefault(x => x.Name.Contains("OutBlock"));
                if (outblock_prop != null)
                {
                    var block_type = outblock_prop.PropertyType;
                    var data = JsonSerializer.Deserialize(e.RealtimeBody, block_type, _jsonOptions);
                    if (data != null)
                    {
                        OutputLog(LogKind.실시간계좌응답, $"TrCode={e.TrCode}, Key={e.Key}");
                        List<string> list = [];
                        foreach (var prop in block_type.GetProperties())
                        {
                            var value = prop.GetValue(data);
                            if (value != null)
                            {
                                if (value.GetType() == typeof(string))
                                    list.Add($"{prop.Name}=\"{value}\"");
                                else
                                    list.Add($"{prop.Name}={value}");
                            }
                        }
                        OutputLog(LogKind.실시간계좌응답, list);
                    }
                    else
                        OutputLog(LogKind.실시간계좌응답, $"TrCode={e.TrCode}, Key={e.Key}, {e.RealtimeBody}");

                    return;
                }
            }
        }
        OutputLog(LogKind.실시간시세응답, $"TrCode={e.TrCode}, Key={e.Key}, {e.RealtimeBody}");
    }

    private void OpenApi_OnMessageEvent(object? sender, EBestOnMessageEventArgs e)
    {
        OutputLog(LogKind.OnMessage, e.Message);
    }

}

