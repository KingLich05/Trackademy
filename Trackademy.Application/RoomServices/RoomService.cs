using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Persistance;
using Trackademy.Application.RoomServices.Models;
using Trackademy.Application.Shared.BaseCrud;
using Trackademy.Application.Shared.Models;
using Trackademy.Domain.Users;

namespace Trackademy.Application.RoomServices;

public class RoomService : 
    BaseService<Room, RoomDto, RoomAddModel, RoomUpdateModel>,
    IRoomService
{
    private TrackademyDbContext _context;
    private readonly IMapper _mapper;
    public RoomService(TrackademyDbContext context, IMapper mapper) 
        : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RoomDto>> GetAllAsync(RequestIdOrganization request)
    {
        var rooms = await _context.Rooms
            .Where(x => x.OrganizationId == request.OrganizationId)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<RoomDto>>(rooms);
    }

    public override async Task<Guid> CreateAsync(RoomAddModel dto)
    {
        if (dto.Capacity == 0)
        {
            return Guid.Empty;
        }

        return await base.CreateAsync(dto);
    }
}