using FileStorage.Core.Dtos;
using Moq;

namespace FileStorage.Core.Test;

[Parallelizable]
public class FileStorageUseCaseTest
{
    [Test]
    public void Save_SavesFile()
    {
        // Arrange
        var mock = new Mock<IFileStorageService>();
        var useCase = new FileStorageUseCase(mock.Object);

        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms);
        sw.Write("This is a test file.");

        var input = new SaveInput
        {
            FileName = "valid.text",
            Content = ms,
        };

        // Act
        var output = useCase.Save(input);

        // Assert
        mock.Verify(x => x.Save(It.IsAny<FileEntity>()), Times.Once);
        Assert.That(output.Id, Has.Length.EqualTo(32));
        Assert.That(output.Result, Is.EqualTo(FileStorageUseCaseResult.Success));
    }

    [Test]
    public void Save_DoesNotSaveInValidFile()
    {
        // Arrange
        var mock = new Mock<IFileStorageService>();
        var useCase = new FileStorageUseCase(mock.Object);

        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms);
        sw.Write("This is a test file.");

        var input = new SaveInput
        {
            FileName = "12345678901234567890123456789012345678901234567890" +
                       "12345678901234567890123456789012345678901234567890" +
                       "12345678901234567890123456789012345678901234567890" +
                       "12345678901234567890123456789012345678901234567890" +
                       "1.txt",
            Content = ms
        };

        // Act
        var output = useCase.Save(input);

        // Assert
        mock.Verify(x => x.Save(It.IsAny<FileEntity>()), Times.Never);
        Assert.That(output.Result, Is.EqualTo(FileStorageUseCaseResult.InvalidFileError));
    }

    [Test]
    public void Delete_DeletesFile()
    {
        // Arrange
        var mock = new Mock<IFileStorageService>();
        var useCase = new FileStorageUseCase(mock.Object);

        var id = Guid.NewGuid().ToString("N");

        // Act
        var output = useCase.Delete(id);

        // Assert
        mock.Verify(x => x.Delete(id), Times.Once);
        Assert.That(output.Result, Is.EqualTo(FileStorageUseCaseResult.Success));
    }

    [Test]
    public void Delete_FailsWhenFileNotFound()
    {
        // Arrange
        var mock = new Mock<IFileStorageService>();
        mock.Setup(x => x.Delete(It.IsAny<string>()))
            .Throws<FileNotFoundException>();

        var useCase = new FileStorageUseCase(mock.Object);

        var id = Guid.NewGuid().ToString("N");

        // Act
        var output = useCase.Delete(id);

        // Assert
        mock.Verify(x => x.Delete(id), Times.Once);
        Assert.That(output.Result, Is.EqualTo(FileStorageUseCaseResult.FileNotFoundError));
    }

    [Test]
    public void GetAll_Test()
    {
        // Arrange
        var id = Guid.NewGuid().ToString("N");
        var fileName = "test1.csv";
        var createdAt = DateTimeOffset.Now;
        var updatedAt = createdAt + TimeSpan.FromDays(1);

        var mock = new Mock<IFileStorageService>();
        mock.Setup(x => x.GetAll())
            .Returns(new List<FileEntity>
            {
                new FileEntity
                {
                    Id = id,
                    FileName = fileName,
                    CreatedAt = createdAt,
                    UpdatedAt = updatedAt,
                }
            });

        var useCase = new FileStorageUseCase(mock.Object);

        // Act
        var outputList = useCase.GetAll();

        // Assert
        mock.Verify(x => x.GetAll(), Times.Once);
        var output = outputList.First();
        Assert.That(output.Id, Is.EqualTo(id));
        Assert.That(output.FileName, Is.EqualTo(fileName));
        Assert.That(output.CreatedAt, Is.EqualTo(createdAt));
        Assert.That(output.UpdatedAt, Is.EqualTo(updatedAt));
    }
}