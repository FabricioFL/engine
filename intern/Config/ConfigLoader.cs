using System.Text.Json;

namespace Engine.Config;

public static class ConfigLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = ConfigJsonContext.Default
    };

    public static T Load<T>(string relativePath) where T : class, new()
    {
        string fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);

        if (!File.Exists(fullPath))
        {
            // Also check from working directory (development)
            fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
        }

        if (!File.Exists(fullPath))
            return new T();

        var json = File.ReadAllBytes(fullPath);
        return JsonSerializer.Deserialize<T>(json, Options) ?? new T();
    }
}
