using Backoffice.Application.Common.Models;

namespace Backoffice.Application.Services.Interfaces;

/// <summary>
/// Generic servis arayüzü - temel CRUD işlemleri
/// </summary>
public interface IGenericService<TKey,TDto, TCreateUpdateDto>
{
    Task<TDto?> GetByIdAsync(TKey id);
    Task<List<TDto>> GetAllAsync();
    Task<PaginatedList<TDto>> GetPagedAsync(int pageIndex, int pageSize, string? searchTerm = null);
    Task<PaginatedList<TDto>> GetPagedWithFilterAsync(FilterModel filter, int pageIndex, int pageSize);
    Task<TKey> CreateAsync(TCreateUpdateDto createDto);
    Task UpdateAsync(TCreateUpdateDto updateDto);
    Task DeleteAsync(TKey id);
}