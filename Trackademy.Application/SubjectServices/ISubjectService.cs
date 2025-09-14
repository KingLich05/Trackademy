using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Application.Shared.Models;
using Trackademy.Application.SubjectServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.SubjectServices;

public interface ISubjectService : IBaseService<Subject, SubjectDto, SubjectAddModel>
{
    Task<IEnumerable<SubjectDto>> GetAllAsync(RequestIdOrganization request);
}