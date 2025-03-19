using Backoffice.Application.Common.Models;
using Backoffice.Application.DTOs.Security;

namespace Backoffice.Application.Services.Interfaces;

public interface IIpFilterService
{
    Task<List<IpFilterDto>> GetAllIpFiltersAsync();
    Task<PaginatedList<IpFilterDto>> GetPagedIpFiltersAsync(int pageIndex, int pageSize, string? searchTerm = null);
    Task<IpFilterDto?> GetIpFilterByIdAsync(int id);
    Task<Result<int>> CreateIpFilterAsync(CreateUpdateIpFilterDto dto);
    Task<Result> UpdateIpFilterAsync(CreateUpdateIpFilterDto dto);
    Task<Result> DeleteIpFilterAsync(int id);
    Task<Result> ToggleIpFilterStatusAsync(int id);
    
    // IP adresi kontrolü için
    Task<bool> IsIpAllowedAsync(string ipAddress);
}