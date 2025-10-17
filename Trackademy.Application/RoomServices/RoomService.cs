using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Persistance;
using Trackademy.Application.RoomServices.Models;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Application.Shared.Exception;
using Trackademy.Application.Shared.Extensions;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.RoomServices;

public class RoomService : 
    BaseService<Room, RoomDto, RoomAddModel, RoomUpdateModel>,
    IRoomService
{
    public RoomService(TrackademyDbContext context, IMapper mapper) 
        : base(context, mapper)
    {
    }

    public async Task<PagedResult<RoomDto>> GetAllAsync(GetRoomsRequest request)
    {
        var trackademyContext = (TrackademyDbContext)_context;
        var query = trackademyContext.Rooms
            .Where(x => x.OrganizationId == request.OrganizationId);

        return await query.ToPagedResultAsync<Room, RoomDto>(
            request.PageNumber,
            request.PageSize,
            _mapper);
    }

    public override async Task<Guid> CreateAsync(RoomAddModel dto)
    {
        if (dto.Capacity == 0)
        {
            throw new ConflictException($"Кабинет не должен быть с нулевой вместимостью.");
        }

        var trackademyContext = (TrackademyDbContext)_context;
        var isExists = await trackademyContext.Rooms.AnyAsync(r =>
            r.OrganizationId == dto.OrganizationId &&
            r.Name.ToLower() == dto.Name.ToLower());

        if (isExists)
        {
            throw new ConflictException($"Кабинет с названием '{dto.Name}' уже существует в этой организации.");
        }

        return await base.CreateAsync(dto);
    }

    public override async Task<Guid> UpdateAsync(Guid id, RoomUpdateModel dto)
    {
        var trackademyContext = (TrackademyDbContext)_context;
        var entity = await trackademyContext.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        if (entity is null)
            return Guid.Empty;

        if (dto.Capacity == 0)
        {
            throw new ConflictException($"Кабинет не должен быть с нулевой вместимостью.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Name) &&
            !dto.Name.Equals(entity.Name, StringComparison.OrdinalIgnoreCase))
        {
            var trackademyContext2 = (TrackademyDbContext)_context;
            var isExists = await trackademyContext2.Rooms.AnyAsync(r =>
                r.OrganizationId == entity.OrganizationId &&
                r.Name.ToLower() == dto.Name.ToLower() &&
                r.Id != entity.Id);

            if (isExists)
            {
                throw new ConflictException($"Кабинет с названием '{dto.Name}' уже существует в этой организации.");
            }
        }
        
        return await base.UpdateAsync(id, dto);
    }
}