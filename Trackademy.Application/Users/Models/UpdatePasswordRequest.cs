using System.ComponentModel.DataAnnotations;

namespace Trackademy.Application.Users.Models;

public class UpdatePasswordRequest
{
    [Required(ErrorMessage = "ID студента обязателен")]
    public Guid StudentId { get; set; }

    [Required(ErrorMessage = "Текущий пароль обязателен")]
    [MinLength(6, ErrorMessage = "Текущий пароль должен содержать минимум 6 символов")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Новый пароль обязателен")]
    [MinLength(6, ErrorMessage = "Новый пароль должен содержать минимум 6 символов")]
    public string NewPassword { get; set; } = string.Empty;
}