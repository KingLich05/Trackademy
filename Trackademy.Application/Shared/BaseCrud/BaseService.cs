using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Domain.hz;

namespace Trackademy.Application.Shared.BaseCrud;

public class BaseService<T, TDto, TAddDto> : IBaseService<T, TDto, TAddDto>
    where T : Entity
    where TDto : class
    where TAddDto : class
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

    public virtual async Task<TDto> CreateAsync(TAddDto dto)
    {
        var entity = _mapper.Map<T>(dto); 
        entity.Id = Guid.NewGuid();
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<TDto>(entity);
    }

    public virtual async Task<bool> UpdateAsync(Guid id, TAddDto dto)
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