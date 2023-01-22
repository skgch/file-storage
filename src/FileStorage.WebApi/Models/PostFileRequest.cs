using System.ComponentModel.DataAnnotations;

namespace FileStorage.WebApi.Models;

public class PostFileRequest
{
    [Required]
    public IFormFile File { get; set; }
}