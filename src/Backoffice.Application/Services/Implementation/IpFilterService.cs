using System.Linq.Expressions;
using System.Net;
using AutoMapper;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Application.Common.Models;
using Backoffice.Application.DTOs.Security;
using Backoffice.Application.Services.Interfaces;
using Backoffice.Domain.Entities.Security;
using Backoffice.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Backoffice.Application.Services.Implementation;

public class IpFilterService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<IpFilterService> logger)
    : IIpFilterService
{
    public async Task<List<IpFilterDto>> GetAllIpFiltersAsync()
    {
        var repository = unitOfWork.Repository<IpFilter, int>();
        var ipFilters = await repository.GetAllAsync();
        return mapper.Map<List<IpFilterDto>>(ipFilters);
    }

    public async Task<PaginatedList<IpFilterDto>> GetPagedIpFiltersAsync(int pageIndex, int pageSize, string? searchTerm = null)
    {
        var repository = unitOfWork.Repository<IpFilter, int>();
        
        var entities = await repository.GetPagedAsync(
            pageIndex,
            pageSize,
            predicate: string.IsNullOrEmpty(searchTerm) ? null : BuildSearchPredicate(searchTerm),
            orderBy: q => q.OrderByDescending(x => x.CreatedAt));

        var dtos = mapper.Map<List<IpFilterDto>>(entities.Items);

        return new PaginatedList<IpFilterDto>(
            dtos,
            entities.TotalCount,
            entities.PageIndex,
            entities.PageSize);
    }

    public async Task<IpFilterDto?> GetIpFilterByIdAsync(int id)
    {
        var repository = unitOfWork.Repository<IpFilter, int>();
        var ipFilter = await repository.GetByIdAsync(id);
        return ipFilter == null ? null : mapper.Map<IpFilterDto>(ipFilter);
    }

    public async Task<Result<int>> CreateIpFilterAsync(CreateUpdateIpFilterDto dto)
    {
        try
        {
            // IP formatını doğrula
            if (!IsValidIpOrCidr(dto.IpAddress))
            {
                return Result<int>.Failure(["IP adresi veya CIDR formatı geçersiz."]);
            }
            
            var repository = unitOfWork.Repository<IpFilter, int>();
            
            // Aynı IP adresi zaten var mı kontrol et
            var existingIpFilter = await repository.GetWithIncludeStringAsync(
                predicate: x => x.IpAddress == dto.IpAddress);
                
            if (existingIpFilter.Any())
            {
                return Result<int>.Failure(["Bu IP adresi zaten tanımlı."]);
            }
            
            var entity = mapper.Map<IpFilter>(dto);
            await repository.AddAsync(entity);
            await unitOfWork.SaveChangesAsync();
            
            logger.LogInformation("IP filtresi oluşturuldu: {IpAddress} ({FilterType})", 
                entity.IpAddress, entity.FilterType);
            
            return Result<int>.Success(entity.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "IP filtresi oluşturulurken hata: {Message}", ex.Message);
            return Result<int>.Failure(["IP filtresi oluşturulurken bir hata oluştu."]);
        }
    }

    public async Task<Result> UpdateIpFilterAsync(CreateUpdateIpFilterDto dto)
    {
        try
        {
            if (dto.Id == null)
            {
                return Result.Failure(["IP filtresi ID'si gerekli."]);
            }
            
            // IP formatını doğrula
            if (!IsValidIpOrCidr(dto.IpAddress))
            {
                return Result.Failure(["IP adresi veya CIDR formatı geçersiz."]);
            }
            
            var repository = unitOfWork.Repository<IpFilter, int>();
            
            // Mevcut IP filtresini getir
            var entity = await repository.GetByIdAsync(dto.Id.Value);
            
            if (entity == null)
            {
                return Result.Failure([$"ID: {dto.Id} olan IP filtresi bulunamadı."]);
            }
            
            // Aynı IP adresi başka bir kayıtta var mı kontrol et
            if (entity.IpAddress != dto.IpAddress)
            {
                var existingIpFilter = await repository.GetWithIncludeStringAsync(
                    predicate: x => x.IpAddress == dto.IpAddress && x.Id != dto.Id);
                
                if (existingIpFilter.Any())
                {
                    return Result.Failure(["Bu IP adresi zaten başka bir filtrede tanımlı."]);
                }
            }
            
            // Değişiklikleri uygula
            mapper.Map(dto, entity);
            
            await repository.UpdateAsync(entity);
            await unitOfWork.SaveChangesAsync();
            
            logger.LogInformation("IP filtresi güncellendi: {IpAddress} ({FilterType})", 
                entity.IpAddress, entity.FilterType);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "IP filtresi güncellenirken hata: {Message}", ex.Message);
            return Result.Failure(["IP filtresi güncellenirken bir hata oluştu."]);
        }
    }

    public async Task<Result> DeleteIpFilterAsync(int id)
    {
        try
        {
            var repository = unitOfWork.Repository<IpFilter, int>();
            
            var entity = await repository.GetByIdAsync(id);
            
            if (entity == null)
            {
                return Result.Failure([$"ID: {id} olan IP filtresi bulunamadı."]);
            }
            
            await repository.DeleteAsync(entity);
            await unitOfWork.SaveChangesAsync();
            
            logger.LogInformation("IP filtresi silindi: {IpAddress}", entity.IpAddress);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "IP filtresi silinirken hata: {Message}", ex.Message);
            return Result.Failure(["IP filtresi silinirken bir hata oluştu."]);
        }
    }

    public async Task<Result> ToggleIpFilterStatusAsync(int id)
    {
        try
        {
            var repository = unitOfWork.Repository<IpFilter, int>();
            
            var entity = await repository.GetByIdAsync(id);
            
            if (entity == null)
            {
                return Result.Failure([$"ID: {id} olan IP filtresi bulunamadı."]);
            }
            
            entity.IsActive = !entity.IsActive;
            await repository.UpdateAsync(entity);
            await unitOfWork.SaveChangesAsync();
            
            logger.LogInformation("IP filtresi durumu değiştirildi: {IpAddress}, Aktif: {IsActive}", 
                entity.IpAddress, entity.IsActive);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "IP filtresi durumu değiştirilirken hata: {Message}", ex.Message);
            return Result.Failure(["IP filtresi durumu değiştirilirken bir hata oluştu."]);
        }
    }

    public async Task<bool> IsIpAllowedAsync(string ipAddress)
    {
        // IP adresini parse et
        if (!IPAddress.TryParse(ipAddress, out IPAddress? parsedIpAddress))
        {
            logger.LogWarning("Geçersiz IP adresi formatı: {IpAddress}", ipAddress);
            return false;
        }

        var repository = unitOfWork.Repository<IpFilter, int>();
        
        // Aktif filtreleri getir
        var activeFilters = await repository.GetWithIncludeStringAsync(
            predicate: x => x.IsActive);
            
        // Filtreleme Kuralları:
        // 1. Hiç filtre yoksa veya aktif filtre yoksa, tüm IP adreslerine izin ver
        if (!activeFilters.Any())
        {
            return true;
        }
        
        // 2. Dışlama (Deny) listesindeki bir adres varsa, engelle
        foreach (var filter in activeFilters.Where(f => f.FilterType == FilterType.Deny))
        {
            if (IpMatchesCidr(parsedIpAddress, filter.IpAddress))
            {
                logger.LogInformation("IP adresi engellendi: {IpAddress} (eşleşme: {FilterIp})", 
                    ipAddress, filter.IpAddress);
                return false;
            }
        }
        
        // 3. İzin verme (Allow) listesi varsa ve hiçbir adresle eşleşmiyorsa, engelle
        var allowFilters = activeFilters.Where(f => f.FilterType == FilterType.Allow).ToList();
        if (allowFilters.Any())
        {
            foreach (var filter in allowFilters)
            {
                if (IpMatchesCidr(parsedIpAddress, filter.IpAddress))
                {
                    return true;
                }
            }
            
            // İzin listesi var ama hiçbir adresle eşleşme yoksa engelle
            logger.LogInformation("IP adresi engellendi: {IpAddress} (izin listesinde değil)", ipAddress);
            return false;
        }
        
        // 4. İzin listesi yoksa ve engelleme listesinde değilse, izin ver
        return true;
    }

    // Yardımcı Metodlar
    
    private Expression<Func<IpFilter, bool>> BuildSearchPredicate(string searchTerm)
    {
        return x => x.IpAddress.Contains(searchTerm) || 
                   x.Description.Contains(searchTerm);
    }
    
    private bool IsValidIpOrCidr(string ipAddress)
    {
        // Basit CIDR adresi kontrolü (örn: 192.168.1.0/24)
        if (ipAddress.Contains('/'))
        {
            var parts = ipAddress.Split('/');
            if (parts.Length != 2)
                return false;
                
            // IP kısmını doğrula
            if (!IPAddress.TryParse(parts[0], out _))
                return false;
                
            // CIDR prefixi doğrula (0-32 arası)
            if (!int.TryParse(parts[1], out int cidrPrefix) || cidrPrefix < 0 || cidrPrefix > 32)
                return false;
                
            return true;
        }
        
        // Normal IP adresi kontrolü
        return IPAddress.TryParse(ipAddress, out _);
    }
    
    private bool IpMatchesCidr(IPAddress ipAddress, string cidrNotation)
    {
        try
        {
            // CIDR notasyonu (örn: 192.168.1.0/24)
            if (cidrNotation.Contains('/'))
            {
                var parts = cidrNotation.Split('/');
                var networkAddress = IPAddress.Parse(parts[0]);
                var prefixLength = int.Parse(parts[1]);
                
                // IP adreslerini byte dizisine dönüştür
                var ipBytes = ipAddress.GetAddressBytes();
                var networkBytes = networkAddress.GetAddressBytes();
                
                // IP adreslerini karşılaştır (IPv4 için)
                if (ipBytes.Length == 4) // IPv4
                {
                    // Ağ maskelemesi yap
                    var mask = CreateMask(prefixLength);
                    
                    for (int i = 0; i < 4; i++)
                    {
                        if ((ipBytes[i] & mask[i]) != (networkBytes[i] & mask[i]))
                            return false;
                    }
                    
                    return true;
                }
            }
            // Tam IP eşleşmesi kontrol et
            else if (IPAddress.TryParse(cidrNotation, out var exactIp))
            {
                return ipAddress.Equals(exactIp);
            }
            
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CIDR eşleşmesi kontrolünde hata: {Message}", ex.Message);
            return false;
        }
    }
    
    private byte[] CreateMask(int prefixLength)
    {
        var mask = new byte[4];
        
        // Prefix uzunluğuna göre maskeyi oluştur
        for (int i = 0; i < 4; i++)
        {
            if (prefixLength >= 8)
            {
                mask[i] = 255;
                prefixLength -= 8;
            }
            else if (prefixLength > 0)
            {
                mask[i] = (byte)(255 - (1 << (8 - prefixLength)) + 1);
                prefixLength = 0;
            }
            else
            {
                mask[i] = 0;
            }
        }
        
        return mask;
    }
}