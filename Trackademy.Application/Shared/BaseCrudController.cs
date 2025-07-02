using Microsoft.AspNetCore.Mvc;

namespace Trackademy.Application.Shared;

[ApiController]
[Route("api/[controller]")]
public class BaseCrudController<T, TDto> : ControllerBase
    where T : class
    where TDto : class
{
    
}