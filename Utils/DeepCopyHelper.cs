using System.Text.Json;
using System.Text.Json.Serialization;

public static class DeepCopyHelper
{
    public static T DeepCopy<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        return JsonSerializer.Deserialize<T>(json,options);
    }

}
