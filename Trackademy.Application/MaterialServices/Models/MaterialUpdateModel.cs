using System.ComponentModel.DataAnnotations;

namespace Trackademy.Application.MaterialServices.Models;

public class MaterialUpdateModel
{
    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(255, ErrorMessage = "Название не может быть длиннее 255 символов")]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "Описание не может быть длиннее 1000 символов")]
    public string? Description { get; set; }
}
