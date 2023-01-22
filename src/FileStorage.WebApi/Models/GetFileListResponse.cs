using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace FileStorage.WebApi.Models;

public class GetFileListResponse
{
    [SwaggerSchema("Cursor to get next page.")]
    public string NextCursor { get; set; }

    [SwaggerSchema("List of files.")]
    [Required]
    public IList<GetFileListResponseItem> Items { get; set; }
}

public class GetFileListResponseItem
{
    [Required]
    public string Id { get; set; }
    [Required]
    public string FileName { get; set; }
    [Required]
    public DateTimeOffset CreatedAt { get; set; }
    [Required]
    public DateTimeOffset UpdatedAt { get; set; }
}