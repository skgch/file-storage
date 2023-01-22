namespace FileStorage.Core.Dtos;

public class SaveInput
{
    public string FileName { get; set; }
    public Stream Content { get; set; }
}