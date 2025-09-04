using AutoMapper;
using Trackademy.Application.OrganizationServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.OrganizationServices;

public class OrganizationProfile : Profile
{
    public OrganizationProfile()
    {
        CreateMap<Organization, OrganizationDto>();
        CreateMap<OrganizationAddModel, Organization>().ForMember(x => x.Users, opt => opt.Ignore());
    }
}