namespace Trackademy.Application.Shared.Models;

public class PagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    public PagedRequest()
    {
    }

    public PagedRequest(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber > 0 ? pageNumber : 1;
        PageSize = pageSize > 0 && pageSize <= 100 ? pageSize : 10; // Ограничиваем максимальный размер страницы
    }
}