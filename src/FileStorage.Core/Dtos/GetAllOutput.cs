namespace FileStorage.Core.Dtos;

public class GetAllOutput
{
    public string Id { get; set; }
    public string FileName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}