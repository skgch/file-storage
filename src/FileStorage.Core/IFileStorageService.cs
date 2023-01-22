namespace FileStorage.Core;

public interface IFileStorageService
{
    public void Save(FileEntity file);
    public void Delete(string id);
    public IEnumerable<FileEntity> GetAll();
}