using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.OrganizationServices.Models;
using Trackademy.Application.Persistance;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Application.Shared.Exception;

namespace Trackademy.Application.OrganizationServices;

public class OrganizationService : 
    BaseService<Domain.Users.Organization, OrganizationDto, OrganizationAddModel, OrganizationAddModel>,
    IOrganizationService
{
    public OrganizationService(TrackademyDbContext context, IMapper mapper) 
        : base(context, mapper)
    {
    }

    public override async Task<Guid> CreateAsync(OrganizationAddModel dto)
    {
        var trackademyContext = (TrackademyDbContext)_context;
        var existingOrganization = await trackademyContext.Organizations
            .FirstOrDefaultAsync(o => o.Address.ToLower() == dto.Address.ToLower());
        
        if (existingOrganization != null)
        {
            throw new ConflictException("Данный адрес уже имеется в базе");
        }

        return await base.CreateAsync(dto);
    }

    public override async Task<Guid> UpdateAsync(Guid id, OrganizationAddModel dto)
    {
        var trackademyContext = (TrackademyDbContext)_context;
        var existingOrganization = await trackademyContext.Organizations
            .FirstOrDefaultAsync(o => o.Address.ToLower() == dto.Address.ToLower() && o.Id != id);
        
        if (existingOrganization != null)
        {
            throw new ConflictException("Данный адрес уже имеется в базе");
        }

        return await base.UpdateAsync(id, dto);
    }
}