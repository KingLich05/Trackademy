using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Domain.Common;

namespace Trackademy.Application.Shared.BaseCrud;

public class BaseService<T, TDto, TAddDto, TUpdateDto> : IBaseService<T, TDto, TAddDto, TUpdateDto>
    where T : Entity
    where TDto : class
    where TAddDto : class
    where TUpdateDto : class
{
    protected readonly DbContext _context;
    protected readonly IMapper _mapper;
    protected readonly DbSet<T> _dbSet;

    public BaseService(DbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
        _dbSet = _context.Set<T>();
    }

    public virtual async Task<IEnumerable<TDto>> GetAllAsync()
    {
        var entities = await _dbSet.AsNoTracking().ToListAsync();
        return _mapper.Map<IEnumerable<TDto>>(entities);
    }

    public virtual async Task<TDto?> GetByIdAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        return entity is null ? null : _mapper.Map<TDto>(entity);
    }

    public virtual async Task<Guid> CreateAsync(TAddDto dto)
    {
        var entity = _mapper.Map<T>(dto); 
        entity.Id = Guid.NewGuid();
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }

   public virtual async Task<Guid> UpdateAsync(Guid id, TUpdateDto dto)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity is null) return Guid.Empty;

        _mapper.Map(dto, entity);
        _context.Update(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
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