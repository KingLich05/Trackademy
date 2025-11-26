using AutoMapper;
using Trackademy.Application.MaterialServices.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.MaterialServices;

public class MaterialProfile : Profile
{
    public MaterialProfile()
    {
        CreateMap<Material, MaterialDto>()
            .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Group != null ? src.Group.Name : string.Empty))
            .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src => src.UploadedBy != null ? src.UploadedBy.FullName : string.Empty));
    }
}
