using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Trackademy.Application.MaterialServices.Models;

public class MaterialAddModel
{
    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(255, ErrorMessage = "Название не может быть длиннее 255 символов")]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "Описание не может быть длиннее 1000 символов")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Группа обязательна")]
    public Guid GroupId { get; set; }
    
    [Required(ErrorMessage = "Файл обязателен")]
    public IFormFile File { get; set; } = null!;
}
