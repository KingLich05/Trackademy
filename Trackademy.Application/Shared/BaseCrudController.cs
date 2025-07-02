using Microsoft.AspNetCore.Mvc;

namespace Trackademy.Application.Shared;

[ApiController]
[Route("api/[controller]")]
public class BaseCrudController<T, TDto> : ControllerBase
    where T : class
    where TDto : class
{
    protected readonly IBaseService<T, TDto> _service;

    protected BaseCrudController(IBaseService<T, TDto> service)
    {
        _service = service;
    }

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

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] TDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public virtual async Task<IActionResult> Update(Guid id, [FromBody] TDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

}