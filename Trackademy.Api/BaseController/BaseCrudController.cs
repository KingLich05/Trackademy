using Microsoft.AspNetCore.Mvc;
using Trackademy.Application.Shared.BaseCrud;

namespace Trackademy.Api.BaseController;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseCrudController<T, TDto, TAddDto, TUpdateDto> : ControllerBase
    where T : class
    where TDto : class
    where TAddDto : class
    where TUpdateDto : class
{
    protected readonly IBaseService<T, TDto, TAddDto, TUpdateDto> _service;

    protected BaseCrudController(IBaseService<T, TDto, TAddDto, TUpdateDto> service)
        => _service = service;

    [HttpGet]
    public virtual async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null)
            return NotFound();
        return Ok(item);
    }

    [HttpPost("create")]
    public virtual async Task<IActionResult> Create([FromBody] TAddDto dto)
    {
        var id = await _service.CreateAsync(dto);
        
        if (id == Guid.Empty) return NotFound();

        return Ok(id);
    }

    [HttpPut("{id}")]
    public virtual async Task<IActionResult> Update(Guid id, [FromBody] TUpdateDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);

        if (updated == Guid.Empty) return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await _service.DeleteAsync(id);
        if (!isDeleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}