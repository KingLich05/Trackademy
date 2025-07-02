namespace Trackademy.Application.Shared;

public interface IBaseService<T, TDto>
{
    Task<IEnumerable<TDto>> GetAllAsync();
    Task<TDto?> GetByIdAsync(Guid id);
    Task<TDto> CreateAsync(TDto dto);
    Task<bool> UpdateAsync(Guid id, TDto dto);
    Task<bool> DeleteAsync(Guid id);
}