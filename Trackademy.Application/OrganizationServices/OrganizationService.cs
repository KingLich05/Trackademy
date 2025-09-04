using AutoMapper;
using Trackademy.Application.OrganizationServices.Models;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared.BaseCrud;

namespace Trackademy.Application.OrganizationServices;

public class OrganizationService : 
    BaseService<Domain.Users.Organization, OrganizationDto, OrganizationAddModel>,
    IOrganizationService
{
    public OrganizationService(TrackademyDbContext context, IMapper mapper) 
        : base(context, mapper)
    {
    }
}