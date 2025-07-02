using Microsoft.EntityFrameworkCore;

namespace Trackademy.Application.Shared;

public class BaseService<T, TDto> : IBaseService<T, TDto> where T : class, IEntity where TDto : class
{
    private readonly DbContext _context;
    // private readonly IMapper _mapper; // AutoMapper для маппинга между Entity и DTO
    private readonly DbSet<T> _dbSet;

    public BaseService(DbContext context)
    {
        _context = context;
        // _mapper = mapper;
        _dbSet = _context.Set<T>();
    }

    public async Task<IEnumerable<TDto>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
         // _mapper.Map<IEnumerable<TDto>>(entities);
    }

    public async Task<TDto?> GetByIdAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        // return entity == null ? null : _mapper.Map<TDto>(entity);
    }

    public async Task<TDto> CreateAsync(TDto dto)
    {
        var entity = _mapper.Map<T>(dto);
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<TDto>(entity);
    }

    public async Task<bool> UpdateAsync(Guid id, TDto dto)
    {
        var entity = await _dbSet.FindAsync(id);
        // if (entity == null) return false;

        // _mapper.Map(dto, entity);
        _context.Update(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null) return false;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}