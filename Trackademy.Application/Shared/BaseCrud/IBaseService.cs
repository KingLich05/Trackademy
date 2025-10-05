namespace Trackademy.Application.Shared.BaseCrud;

public interface IBaseService<T, TDto, TAddDto, TUpdateDto>
{
    Task<IEnumerable<TDto>> GetAllAsync();
    Task<TDto?> GetByIdAsync(Guid id);

    Task<Guid> CreateAsync(TAddDto dto);

    Task<Guid> UpdateAsync(Guid id, TUpdateDto dto);

    Task<bool> DeleteAsync(Guid id);
}