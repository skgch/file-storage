using FileStorage.Core;
using FileStorage.Core.Dtos;
using FileStorage.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FileStorage.WebApi.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageUseCase _useCase;

    public FilesController(IFileStorageUseCase useCase)
    {
        _useCase = useCase;
    }

    /// <summary>
    /// Upload a file.
    /// </summary>
    [SwaggerResponse(200, "File was uploaded.", Type = typeof(PostFileResponse))]
    [SwaggerResponse(400, "File name is longer than 100 or size is lager than 5MB.", Type = typeof(ErrorResponse))]
    [Produces("application/json")]
    [HttpPost]
    public IActionResult PostFile([FromForm] PostFileRequest request)
    {
        if (request.File.FileName.Length > 100)
            return BadRequest(new ErrorResponse { Message = "File name must not be longer than 100." });

        if (request.File.Length > 5 * 1024 * 1024)
            return BadRequest(new ErrorResponse { Message = "File size must not be larger than 5MB." });

        using var stream = request.File.OpenReadStream();

        var output = _useCase.Save(new SaveInput
        {
            Content = stream,
            FileName = request.File.FileName,
        });

        if (output.Result == FileStorageUseCaseResult.InvalidFileError)
            return BadRequest(new ErrorResponse { Message = "File is invalid." });

        return Ok(new PostFileResponse { Id = output.Id });
    }

    /// <summary>
    /// Delete a file.
    /// </summary>
    [SwaggerResponse(204, "File was deleted.")]
    [SwaggerResponse(404, "File does not exist.", typeof(ErrorResponse))]
    [Produces("application/json")]
    [HttpDelete("{id}")]
    public IActionResult DeleteFile([SwaggerParameter("File id.")] string id)
    {
        var output = _useCase.Delete(id);

        if (output.Result == FileStorageUseCaseResult.FileNotFoundError)
            return NotFound(new ErrorResponse { Message = "File does not exist." });

        return NoContent();
    }

    /// <summary>
    /// Get list of files.
    /// </summary>
    /// <remarks>
    /// Get next page by specifying cursor which is returned by previous page.
    /// Max page size is 100.
    /// </remarks>
    [SwaggerResponse(200, Type = typeof(GetFileListResponse))]
    [SwaggerResponse(400, "Cursor is invalid", Type = typeof(ErrorResponse))]
    [Produces("application/json")]
    [HttpGet]
    public IActionResult GetFileList(string cursor)
    {
        // Parse cursor into Unix milli seconds and file id for pagination.
        long createdAtMilliSec = default;
        string id = default;
        if (cursor != null)
        {
            var timeAndId = cursor.Split("_", StringSplitOptions.RemoveEmptyEntries);
            if (timeAndId.Length != 2)
                return BadRequest(new ErrorResponse { Message = "Cursor is invalid." });

            if (!long.TryParse(timeAndId[0], out createdAtMilliSec))
                return BadRequest(new ErrorResponse { Message = "Cursor is invalid." });

            if (Guid.TryParse(timeAndId[0], out _))
                return BadRequest(new ErrorResponse { Message = "Cursor is invalid." });

            id = timeAndId[0];
        }
        
        const int pageSize = 100;

        var files = _useCase.GetAll()
            .Select(f => new GetFileListResponseItem
            {
                Id = f.Id,
                FileName = f.FileName,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            })
            .Where(f => cursor == null 
                        || f.CreatedAt.ToUnixTimeMilliseconds() > createdAtMilliSec
                        || (f.CreatedAt.ToUnixTimeMilliseconds() == createdAtMilliSec && string.CompareOrdinal(f.Id, id) > 0))
            .OrderBy(f => f.CreatedAt)
            .ThenBy(f => f.Id)
            .Take(pageSize + 1)
            .ToList();

        var response = new GetFileListResponse
        {
            Items = files.Take(pageSize).ToList()
        };

        var last = response.Items.Last();
        if (files.Count > pageSize)
            response.NextCursor = last.CreatedAt.ToUnixTimeMilliseconds() + "_" + last.Id;

        return Ok(response);
    }
}