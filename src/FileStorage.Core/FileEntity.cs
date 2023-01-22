namespace FileStorage.Core;

public class FileEntity
{
    public string Id { get; set; }
    public string FileName { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Stream Content { get; set; }

    public bool IsValid()
        => FileName.Length <= 200 && Content.Length <= 5 * 1024 * 1024;
}