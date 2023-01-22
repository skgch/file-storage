using FileStorage.Core;
using FileStorage.Core.Dtos;
using FileStorage.WebApi.Controllers;
using FileStorage.WebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FileStorage.WebApi.Test;

[Parallelizable]
public class FilesControllerTest
{
    [Test]
    public void PostFile_Returns200()
    {
        // Arrange
        var mock = new Mock<IFileStorageUseCase>();
        var id = Guid.NewGuid().ToString("N");
        mock.Setup(x => x.Save(It.IsAny<SaveInput>())).Returns(new SaveOutput
            { Result = FileStorageUseCaseResult.Success, Id = id });
        var controller = new FilesController(mock.Object);

        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms);
        sw.Write("This is a test file.");

        var request = new PostFileRequest
        {
            File = new FormFile(ms, 0, ms.Length, "File", "test.txt")
        };

        // Act
        var result = controller.PostFile(request) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(200));

        var response = result.Value as PostFileResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Id, Is.EqualTo(id));

        mock.Verify(x => x.Save(It.IsAny<SaveInput>()), Times.Once);
    }

    [Test]
    public void PostFile_Returns400WhenFileNameIsTooLong()
    {
        // Arrange
        var mock = new Mock<IFileStorageUseCase>();
        var controller = new FilesController(mock.Object);

        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms);
        sw.Write("This is a test file.");

        const string fileName =
            "12345678901234567890123456789012345678901234567890" +
            "12345678901234567890123456789012345678901234567890" +
            "12345678901234567890123456789012345678901234567890" +
            "12345678901234567890123456789012345678901234567890" +
            "1.txt";

        var request = new PostFileRequest
        {
            File = new FormFile(ms, 0, ms.Length, "File", fileName)
        };

        // Act
        var result = controller.PostFile(request) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(400));

        var response = result.Value as ErrorResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Message, Is.EqualTo("File name must not be longer than 100."));

        mock.Verify(x => x.Save(It.IsAny<SaveInput>()), Times.Never);
    }

    [Test]
    public void PostFile_Returns400WhenFileSizeIsTooLarge()
    {
        // Arrange
        var mock = new Mock<IFileStorageUseCase>();
        var controller = new FilesController(mock.Object);

        using var ms = new MemoryStream();
        ms.Seek(5 * 1024 * 1024, SeekOrigin.Begin);
        ms.WriteByte(0);

        var request = new PostFileRequest
        {
            File = new FormFile(ms, 0, ms.Length, "File", "test.txt")
        };

        // Act
        var result = controller.PostFile(request) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(400));

        var response = result.Value as ErrorResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Message, Is.EqualTo("File size must not be larger than 5MB."));

        mock.Verify(x => x.Save(It.IsAny<SaveInput>()), Times.Never);
    }

    [Test]
    public void PostFile_Returns400WhenUseCaseError()
    {
        // Arrange
        var mock = new Mock<IFileStorageUseCase>();
        mock.Setup(x => x.Save(It.IsAny<SaveInput>()))
            .Returns(new SaveOutput { Result = FileStorageUseCaseResult.InvalidFileError });
        var controller = new FilesController(mock.Object);

        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms);
        sw.Write("This is a test file.");

        var request = new PostFileRequest
        {
            File = new FormFile(ms, 0, ms.Length, "File", "test.txt")
        };

        // Act
        var result = controller.PostFile(request) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(400));

        var response = result.Value as ErrorResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Message, Is.EqualTo("File is invalid."));

        mock.Verify(x => x.Save(It.IsAny<SaveInput>()), Times.Once);
    }

    [Test]
    public void DeleteFile_Returns204()
    {
        // Arrange
        var mock = new Mock<IFileStorageUseCase>();
        mock.Setup(x => x.Delete(It.IsAny<string>()))
            .Returns(new DeleteOutput { Result = FileStorageUseCaseResult.Success });
        var controller = new FilesController(mock.Object);

        // Act
        var id = Guid.NewGuid().ToString("N");
        var result = controller.DeleteFile(id) as NoContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(204));

        mock.Verify(x => x.Delete(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void DeleteFile_Returns404WhenFileNotFound()
    {
        // Arrange
        var mock = new Mock<IFileStorageUseCase>();
        mock.Setup(x => x.Delete(It.IsAny<string>()))
            .Returns(new DeleteOutput { Result = FileStorageUseCaseResult.FileNotFoundError });
        var controller = new FilesController(mock.Object);

        // Act
        var id = Guid.NewGuid().ToString("N");
        var result = controller.DeleteFile(id) as NotFoundObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(404));
        
        var response = result.Value as ErrorResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Message, Is.EqualTo("File does not exist."));

        mock.Verify(x => x.Delete(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void GetFileList_Returns200()
    {
        // Arrange
        var id = Guid.NewGuid().ToString("N");
        var fileName = "test.csv";
        var createdAt = DateTimeOffset.Now;
        var updatedAt = createdAt + TimeSpan.FromDays(1);
        
        var mock = new Mock<IFileStorageUseCase>();
        mock.Setup(x => x.GetAll())
            .Returns(new List<GetAllOutput>
            {
                new GetAllOutput
                {
                    Id = id,
                    FileName = fileName,
                    CreatedAt = createdAt,
                    UpdatedAt = updatedAt
                }
            });
        
        var controller = new FilesController(mock.Object);
        
        // Act
        var result = controller.GetFileList(null) as OkObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(200));
        
        var response = result.Value as GetFileListResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.NextCursor, Is.Null);
        Assert.That(response.Items.Count, Is.EqualTo(1));
        Assert.That(response.Items[0].Id, Is.EqualTo(id));
        Assert.That(response.Items[0].FileName, Is.EqualTo(fileName));
        Assert.That(response.Items[0].CreatedAt, Is.EqualTo(createdAt));
        Assert.That(response.Items[0].UpdatedAt, Is.EqualTo(updatedAt));

        mock.Verify(x => x.GetAll(), Times.Once);
    }
    
    [Test]
    [TestCase("1674371236440")]
    [TestCase("1674371236440_")]
    [TestCase("notlong_7048f7b889394333b22029a7a8f24bce")]
    [TestCase("_7048f7b889394333b22029a7a8f24bce")]
    public void GetFileList_Returns400WhenCursorIsInvalid(string invalidCursor)
    {
        // Arrange
        var mock = new Mock<IFileStorageUseCase>();
        var controller = new FilesController(mock.Object);
        
        // Act
        var result = controller.GetFileList("invalid_cursor") as BadRequestObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(400));
        
        var response = result.Value as ErrorResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Message, Is.EqualTo("Cursor is invalid."));

        mock.Verify(x => x.GetAll(), Times.Never);
    }
    
    [Test]
    public void GetFileList_ReturnsNextCursorWhenNextPageExists()
    {
        // Arrange
        var files = Enumerable.Range(0, 101)
            .Select(x => new GetAllOutput
            {
                Id = Guid.NewGuid().ToString("N"),
                FileName = $"test_{x}.txt",
                CreatedAt = DateTimeOffset.Now + TimeSpan.FromMilliseconds(x),
                UpdatedAt = DateTimeOffset.Now + TimeSpan.FromMilliseconds(x)
            }).ToList();
        
        var mock = new Mock<IFileStorageUseCase>();
        mock.Setup(x => x.GetAll())
            .Returns(files);
        var controller = new FilesController(mock.Object);
        
        // Act
        var result = controller.GetFileList(null) as OkObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(200));
        
        var response = result.Value as GetFileListResponse;
        Assert.That(response, Is.Not.Null);

        var last = response!.Items.Last();
        Assert.That(response.NextCursor, Is.EqualTo(last.CreatedAt.ToUnixTimeMilliseconds() + "_" + last.Id));
        Assert.That(response.Items.Count, Is.EqualTo(100));

        mock.Verify(x => x.GetAll(), Times.Once);
    }
    
    [Test]
    public void GetFileList_ShouldNotReturnNextCursorWhenNextPageDoesExist()
    {
        // Arrange
        var files = Enumerable.Range(0, 100)
            .Select(x => new GetAllOutput
            {
                Id = Guid.NewGuid().ToString("N"),
                FileName = $"test_{x}.txt",
                CreatedAt = DateTimeOffset.Now + TimeSpan.FromMilliseconds(x),
                UpdatedAt = DateTimeOffset.Now + TimeSpan.FromMilliseconds(x)
            }).ToList();
        
        var mock = new Mock<IFileStorageUseCase>();
        mock.Setup(x => x.GetAll())
            .Returns(files);
        var controller = new FilesController(mock.Object);
        
        // Act
        var result = controller.GetFileList(null) as OkObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(200));
        
        var response = result.Value as GetFileListResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.NextCursor, Is.Null);
        Assert.That(response.Items.Count, Is.EqualTo(100));

        mock.Verify(x => x.GetAll(), Times.Once);
    }
}