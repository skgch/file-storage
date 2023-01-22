using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace FileStorage.WebApi.Models;

[SwaggerSchema(Required = new[] { "Id" })]
public class PostFileResponse
{
    [SwaggerSchema("Uploaded file's id.")]
    [Required]
    public string Id { get; set; } = null!;
}