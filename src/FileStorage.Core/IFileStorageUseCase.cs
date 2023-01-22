using FileStorage.Core.Dtos;

namespace FileStorage.Core;

public interface IFileStorageUseCase
{
    public SaveOutput Save(SaveInput input);
    public DeleteOutput Delete(string id);
    public IList<GetAllOutput> GetAll();
}