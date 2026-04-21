using System.Net.Http.Json;

namespace Apartment_Manager.Api.Infrastructure;

public class SupabaseStorageService(HttpClient http, IConfiguration config)
{
    private readonly string _url = (config["Supabase:Url"]
        ?? throw new InvalidOperationException("Supabase:Url not configured")).TrimEnd('/');

    private readonly string _serviceKey = config["Supabase:ServiceKey"]
        ?? throw new InvalidOperationException("Supabase:ServiceKey not configured");

    private const string Bucket = "apartments";

    public async Task<string> UploadAsync(string path, Stream stream, string contentType)
    {
        using var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"{_url}/storage/v1/object/{Bucket}/{path}");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _serviceKey);
        request.Content = content;

        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return $"{_url}/storage/v1/object/public/{Bucket}/{path}";
    }

    public async Task DeleteAsync(string publicUrl)
    {
        var prefix = $"{_url}/storage/v1/object/public/{Bucket}/";
        if (!publicUrl.StartsWith(prefix)) return;

        var path = publicUrl[prefix.Length..];

        using var request = new HttpRequestMessage(HttpMethod.Delete,
            $"{_url}/storage/v1/object/{Bucket}");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _serviceKey);
        request.Content = JsonContent.Create(new { prefixes = new[] { path } });

        await http.SendAsync(request);
    }
}
