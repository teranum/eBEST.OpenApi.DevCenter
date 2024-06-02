using eBEST.OpenApi.DevCenter.Models;
using eBEST.OpenApi.Events;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eBEST.OpenApi.DevCenter.ViewModels;
internal sealed class Int32Converter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var sval = reader.GetString();
            return int.TryParse(sval, out int lval) ? lval : 0;
        }

        return reader.GetInt32();
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
internal sealed class Int64Converter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var sval = reader.GetString();
            return long.TryParse(sval, out long lval) ? lval : 0L;
        }

        return reader.GetInt64();
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
internal sealed class DoubleConverter : JsonConverter<double>
{
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var sval = reader.GetString();
            return double.TryParse(sval, out double lval) ? lval : 0L;
        }

        return reader.GetInt64();
    }

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

internal partial class MainViewModel
{
    private readonly JsonSerializerOptions _jsonOptions = new() { NumberHandling = JsonNumberHandling.AllowReadingFromString };

    void InitialJsonOptions()
    {
        _jsonOptions.Converters.Add(new Int32Converter());
        _jsonOptions.Converters.Add(new Int64Converter());
        _jsonOptions.Converters.Add(new DoubleConverter());
    }

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
                    object? data = null;
                    try
                    {
                        data = e.RealtimeBody.Deserialize(block_type, _jsonOptions);
                    }
                    catch (Exception)
                    {
                        data = null;
                    }
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

