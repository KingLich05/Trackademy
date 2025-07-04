using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Trackademy.Application.Shared;

public class BaseService<T, TDto> : IBaseService<T, TDto>
    where T : class
    where TDto : class
{
    private readonly DbContext _context;
    private readonly IMapper _mapper;
    private readonly DbSet<T> _dbSet;

    public BaseService(DbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
        _dbSet = _context.Set<T>();
    }

    public virtual async Task<IEnumerable<TDto>> GetAllAsync()
    {
        var entities = await _dbSet.ToListAsync();
        return _mapper.Map<IEnumerable<TDto>>(entities);
    }

    public virtual async Task<TDto?> GetByIdAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        return entity is null ? null : _mapper.Map<TDto>(entity);
    }

    public virtual async Task<TDto> CreateAsync(TDto dto)
    {
        var entity = _mapper.Map<T>(dto);
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<TDto>(entity);
    }

    public virtual async Task<bool> UpdateAsync(Guid id, TDto dto)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity is null) return false;

        _mapper.Map(dto, entity);
        _context.Update(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity is null) return false;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}