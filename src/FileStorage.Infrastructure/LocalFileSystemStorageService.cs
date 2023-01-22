using System.Text.RegularExpressions;
using FileStorage.Core;
using Microsoft.Extensions.Configuration;

namespace FileStorage.Infrastructure;

public class LocalFileSystemStorageService : IFileStorageService
{
    private readonly string _dir;
    private static readonly Regex FileNameRegex = new(@"([0-9,a-z]{32})_(.*)");

    public LocalFileSystemStorageService(IConfiguration configuration)
    {
        _dir = configuration.GetValue<string>("FileSystemStorage:Directory");
    }

    public void Save(FileEntity file)
    {
        var filePath = Path.Combine(_dir, $"{file.Id}_{file.FileName}");

        using (var fileStream = File.Create(filePath))
            file.Content.CopyTo(fileStream);

        file.Content.Seek(0, SeekOrigin.Begin);
    }

    public void Delete(string id)
    {
        var path = Directory.EnumerateFiles(_dir, $"{id}_*").FirstOrDefault();

        if (path == null)
            throw new FileNotFoundException();

        File.Delete(path);
    }

    public IEnumerable<FileEntity> GetAll()
    {
        return new DirectoryInfo(_dir).GetFiles()
            .Where(f => FileNameRegex.IsMatch(f.Name))
            .Select(f =>
            {
                var match = FileNameRegex.Match(f.Name);
                return new FileEntity
                {
                    Id = match.Groups[1].Value,
                    FileName = match.Groups[2].Value,
                    CreatedAt = (DateTimeOffset)f.CreationTime,
                    UpdatedAt = (DateTimeOffset)f.LastWriteTime
                };
            });
    }
}