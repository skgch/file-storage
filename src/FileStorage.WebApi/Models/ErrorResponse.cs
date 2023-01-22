using System.ComponentModel.DataAnnotations;

namespace FileStorage.WebApi.Models;

public class ErrorResponse
{
    [Required]
    public string Message { get; set; }
}