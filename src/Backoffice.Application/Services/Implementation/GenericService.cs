using System.Linq.Expressions;
using AutoMapper;
using Backoffice.Application.Common.Interfaces;
using Backoffice.Application.Common.Models;
using Backoffice.Application.Services.Interfaces;
using Backoffice.Domain.Entities.Common;
using Backoffice.Domain.Exceptions;

namespace Backoffice.Application.Services.Implementation;

/// <summary>
/// Generic servis sınıfı - temel CRUD işlemlerini uygular
/// </summary>
public abstract class GenericService<TEntity, TKey, TDto, TCreateUpdateDto> : IGenericService<TKey, TDto, TCreateUpdateDto>
    where TEntity : BaseEntity<TKey>
    where TKey : IEquatable<TKey>
    where TDto : class
    where TCreateUpdateDto : class
{
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly IMapper Mapper;
    protected readonly IGenericRepository<TEntity, TKey> Repository;

    protected GenericService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        UnitOfWork = unitOfWork;
        Mapper = mapper;
        Repository = UnitOfWork.Repository<TEntity, TKey>();
    }

    public virtual async Task<TDto?> GetByIdAsync(TKey id)
    {
        var entity = await Repository.GetByIdAsync(id);

        return entity == null ? null : Mapper.Map<TDto>(entity);
    }

    public virtual async Task<List<TDto>> GetAllAsync()
    {
        var entities = await Repository.GetAllAsync();
        return Mapper.Map<List<TDto>>(entities);
    }

    public virtual async Task<PaginatedList<TDto>> GetPagedAsync(int pageIndex, int pageSize, string? searchTerm = null)
    {
        var entities = await Repository.GetPagedAsync(
            pageIndex,
            pageSize,
            predicate: string.IsNullOrEmpty(searchTerm) ? null : GetSearchPredicate(searchTerm),
            orderBy: GetDefaultOrdering());

        var dtos = Mapper.Map<List<TDto>>(entities.Items);

        return new PaginatedList<TDto>(
            dtos,
            entities.TotalCount,
            entities.PageIndex,
            entities.PageSize);
    }

    public virtual async Task<PaginatedList<TDto>> GetPagedWithFilterAsync(FilterModel filter, int pageIndex, int pageSize)
    {
        var entities = await Repository.GetPagedWithFilterAsync(
            filter,
            pageIndex,
            pageSize,
            orderBy: GetDefaultOrdering());

        var dtos = Mapper.Map<List<TDto>>(entities.Items);

        return new PaginatedList<TDto>(
            dtos,
            entities.TotalCount,
            entities.PageIndex,
            entities.PageSize);
    }

    public virtual async Task<TKey> CreateAsync(TCreateUpdateDto createDto)
    {
        // DTO'dan entity'ye dönüştür
        var entity = Mapper.Map<TEntity>(createDto);

        // Entity'yi ekle
        var result = await Repository.AddAsync(entity);
        await UnitOfWork.SaveChangesAsync();

        return entity.Id;
    }

    public virtual async Task UpdateAsync(TCreateUpdateDto updateDto)
    {
        // Id değerini al (reflection kullanarak)
        var idProperty = typeof(TCreateUpdateDto).GetProperty("Id");
        var id = (TKey?)idProperty?.GetValue(updateDto);

        if (id == null)
            throw new ArgumentException("Id cannot be null for update operation");

        // Mevcut entity'yi getir
        var entity = await Repository.GetByIdAsync(id);

        if (entity == null)
            throw new EntityNotFoundException(typeof(TEntity).Name, id);

        // DTO'dan entity'ye değerleri eşle
        Mapper.Map(updateDto, entity);

        // Entity'yi güncelle
        await Repository.UpdateAsync(entity);
        await UnitOfWork.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(TKey id)
    {
        var entity = await Repository.GetByIdAsync(id);

        if (entity == null)
            throw new EntityNotFoundException(typeof(TEntity).Name, id);
        await Repository.DeleteAsync(entity);

        await UnitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Arama terimi için bir predicate döndürür. Alt sınıflar tarafından override edilmelidir.
    /// </summary>
    protected abstract Expression<Func<TEntity, bool>> GetSearchPredicate(string searchTerm);

    /// <summary>
    /// Varsayılan sıralama için bir fonksiyon döndürür. Alt sınıflar tarafından override edilebilir.
    /// </summary>
    protected virtual Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> GetDefaultOrdering()
    {
        // BaseEntity'den Id'ye göre sıralama varsayılan olarak kullanılır
        return query => query.OrderBy(e => e.Id);
    }
}