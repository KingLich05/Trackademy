namespace Trackademy.Application.Shared.BaseCrud;

public interface IBaseService<T, TDto, TAddDto>
{
    Task<IEnumerable<TDto>> GetAllAsync();
    Task<TDto?> GetByIdAsync(Guid id);
    Task<TDto> CreateAsync(TAddDto dto);
    Task<bool> UpdateAsync(Guid id, TAddDto dto);
    Task<bool> DeleteAsync(Guid id);
}