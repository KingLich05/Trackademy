using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Trackademy.Application.Shared.Models;

namespace Trackademy.Application.Shared.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PagedRequest pagedRequest)
    {
        return await query.ToPagedResultAsync(pagedRequest.PageNumber, pagedRequest.PageSize);
    }

    public static async Task<PagedResult<TDto>> ToPagedResultAsync<TEntity, TDto>(
        this IQueryable<TEntity> query,
        int pageNumber,
        int pageSize,
        IMapper mapper)
    {
        var totalCount = await query.CountAsync();
        var entities = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = mapper.Map<List<TDto>>(entities);

        return new PagedResult<TDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public static async Task<PagedResult<TDto>> ToPagedResultAsync<TEntity, TDto>(
        this IQueryable<TEntity> query,
        PagedRequest pagedRequest,
        IMapper mapper)
    {
        return await query.ToPagedResultAsync<TEntity, TDto>(
            pagedRequest.PageNumber,
            pagedRequest.PageSize,
            mapper);
    }
}