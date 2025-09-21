namespace Trackademy.Application.GroupServices.Models;

public class GroupsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Level { get; set; }
    public string SubjectName { get; set; }
    public List<UserMinimalViewModel> Students { get; set; }
}