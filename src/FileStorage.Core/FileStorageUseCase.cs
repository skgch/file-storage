using FileStorage.Core.Dtos;

namespace FileStorage.Core;

public class FileStorageUseCase : IFileStorageUseCase
{
    private readonly IFileStorageService _fileStorageService;

    public FileStorageUseCase(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public SaveOutput Save(SaveInput input)
    {
        var file = new FileEntity
        {
            Id = Guid.NewGuid().ToString("N"),
            FileName = input.FileName,
            Content = input.Content
        };

        if (!file.IsValid())
            return new SaveOutput { Result = FileStorageUseCaseResult.InvalidFileError };

        _fileStorageService.Save(file);
        return new SaveOutput { Id = file.Id, Result = FileStorageUseCaseResult.Success };
    }

    public DeleteOutput Delete(string id)
    {
        try
        {
            _fileStorageService.Delete(id);
            return new DeleteOutput { Result = FileStorageUseCaseResult.Success };
        }
        catch (FileNotFoundException)
        {
            return new DeleteOutput { Result = FileStorageUseCaseResult.FileNotFoundError };
        }
    }

    public IList<GetAllOutput> GetAll()
    {
        return _fileStorageService.GetAll()
            .Select(f => new GetAllOutput
            {
                Id = f.Id,
                FileName = f.FileName,
                CreatedAt = (DateTimeOffset)f.CreatedAt,
                UpdatedAt = (DateTimeOffset)f.UpdatedAt
            })
            .ToList();
    }
}