using Trackademy.Application.SubjectServices.Models;
using Trackademy.Domain.Enums;

namespace Trackademy.Application.GroupServices.Models;

public class GroupsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Level { get; set; }
    public SubjectMinimalViewModel Subject { get; set; }
    public List<UserMinimalViewModel> Students { get; set; }
    public PaymentType PaymentType { get; set; }
    public decimal MonthlyPrice { get; set; }
    public DateTime? CourseEndDate { get; set; }
}