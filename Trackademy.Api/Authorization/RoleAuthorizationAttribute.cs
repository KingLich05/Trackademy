using Microsoft.AspNetCore.Authorization;
using Trackademy.Domain.Enums;

namespace Trackademy.Api.Authorization;

/// <summary>
/// Атрибут авторизации на основе минимальной роли пользователя.
/// Роли выстроены в иерархии: Owner > Administrator > Teacher > Student
/// </summary>
public class RoleAuthorizationAttribute : AuthorizeAttribute
{
    public RoleAuthorizationAttribute(RoleEnum minimumRole)
    {
        // Строим список ролей, начиная с указанной минимальной роли и выше по иерархии
        var allowedRoles = new List<string>();
        
        // Добавляем роли в зависимости от минимальной роли
        switch (minimumRole)
        {
            case RoleEnum.Student:
                allowedRoles.Add("Student");
                goto case RoleEnum.Teacher;
            case RoleEnum.Teacher:
                allowedRoles.Add("Teacher");
                goto case RoleEnum.Administrator;
            case RoleEnum.Administrator:
                allowedRoles.Add("Administrator");
                goto case RoleEnum.Owner;
            case RoleEnum.Owner:
                allowedRoles.Add("Owner");
                break;
        }
        
        Roles = string.Join(",", allowedRoles);
    }
}