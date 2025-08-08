using Microsoft.AspNetCore.Mvc;
using Trackademy.Application.Shared.BaseCrud;

namespace Trackademy.Api.BaseController;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseCrudController<T, TDto, TAddDto> : Controller
    where T : class
    where TDto : class
    where TAddDto : class
{
    protected readonly IBaseService<T, TDto, TAddDto> _service;

    protected BaseCrudController(IBaseService<T, TDto, TAddDto> service)
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

    [HttpPost("create")]
    public virtual async Task<IActionResult> Create([FromBody] TAddDto dto)
    {
        await _service.CreateAsync(dto);
        return Ok();
    }

    [HttpPut("{id}")]
    public virtual async Task<IActionResult> Update(Guid id, [FromBody] TAddDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        if (!updated)
            return NotFound();

        return NoContent();
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