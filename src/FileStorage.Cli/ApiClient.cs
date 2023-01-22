using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FileStorage.Cli;

public interface IApiClient
{
    public Task<string> UploadAsync(FileInfo file);
    public Task DeleteAsync(string id);
    public Task<FileList> GetListAsync(string? nextCursor);
}

internal class ApiClient : IApiClient
{
    private static readonly string BaseUrl = Environment.GetEnvironmentVariable("FS_API_BASE_URL") ?? "http://localhost:5000";
    
    public async Task<string> UploadAsync(FileInfo file)
    {
        await using var fileStream = file.OpenRead();
        var content = new MultipartFormDataContent
        {
            { new StreamContent(fileStream), "file", file.Name }
        };

        using var httpClient = new HttpClient();
        using var response = await httpClient.PostAsync($"{BaseUrl}/api/files", content);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            return ((string)JsonNode.Parse(json)!["id"])!;
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
            throw new InvalidOperationException("File is invalid.");

        throw new Exception("Unknown error occurred.");
    }

    public async Task DeleteAsync(string id)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.DeleteAsync($"{BaseUrl}/api/files/{id}");

        if (response.StatusCode == HttpStatusCode.NoContent)
            return;

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new FileNotFoundException();

        throw new Exception("Unknown error occurred.");
    }

    public async Task<FileList> GetListAsync(string? cursor)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync($"{BaseUrl}/api/files?cursor={cursor}");

        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception("Unknown error occurred.");

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<FileList>(json,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
    }
}

public class FileList
{
    public string? NextCursor { get; set; }
    public IList<FileListItem> Items { get; set; } = null!;
}

public class FileListItem
{
    public string Id { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}