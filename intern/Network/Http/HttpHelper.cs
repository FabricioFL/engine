using System.Text;
using System.Text.Json;

namespace Engine.Network.Http;

public static class HttpHelper
{
    private static readonly HttpClient Client = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    public static async Task<string> GetAsync(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<T?> GetJsonAsync<T>(string url)
    {
        var json = await GetAsync(url);
        return JsonSerializer.Deserialize<T>(json);
    }

    public static async Task<string> PostAsync(string url, string body, string contentType = "application/json")
    {
        var content = new StringContent(body, Encoding.UTF8, contentType);
        var response = await Client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<string> PostJsonAsync<T>(string url, T payload)
    {
        var json = JsonSerializer.Serialize(payload);
        return await PostAsync(url, json);
    }

    public static async Task<string> PutAsync(string url, string body, string contentType = "application/json")
    {
        var content = new StringContent(body, Encoding.UTF8, contentType);
        var response = await Client.PutAsync(url, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<string> DeleteAsync(string url)
    {
        var response = await Client.DeleteAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<byte[]> DownloadBytesAsync(string url)
    {
        return await Client.GetByteArrayAsync(url);
    }
}
