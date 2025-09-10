using Trackademy.Api.BaseController;
using Trackademy.Application.SubjectServices;
using Trackademy.Application.SubjectServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Api.Controllers.SubjectDir;

public class SubjectController(ISubjectService service) :
    BaseCrudController<Subject, SubjectDto, SubjectAddModel>(service);