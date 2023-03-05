using System.Text.Json;

namespace Dapr.Core.Extensions;

public static class ObjectExtensions
{
    public static string? TrySerializeToJson(this object obj)
    {
        if (obj == null) return null;
        try
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            // ignore
            return null;
        }
    }
}
