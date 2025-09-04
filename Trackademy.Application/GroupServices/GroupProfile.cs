using System.Text.RegularExpressions;
using AutoMapper;
using Trackademy.Application.GroupServices.Models;

namespace Trackademy.Application.GroupServices;

public class GroupProfile : Profile
{
    public GroupProfile()
    {
        CreateMap<Group, GroupsTdo>();
    }
}