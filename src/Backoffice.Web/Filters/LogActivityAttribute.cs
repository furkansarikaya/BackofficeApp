using System.Text.Json;
using Backoffice.Application.DTOs.Auditing;
using Backoffice.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backoffice.Web.Filters;

/// <summary>
/// Controller eylemlerindeki aktiviteleri otomatik olarak loglamak için kullanılan öznitelik
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class LogActivityAttribute(string category, string activityType, bool includeRequestData = false, bool includeResponseData = false)
    : ActionFilterAttribute
{
    public string Category { get; set; } = category;
    public string ActivityType { get; set; } = activityType;
    public bool IncludeRequestData { get; set; } = includeRequestData;
    public bool IncludeResponseData { get; set; } = includeResponseData;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Metod parametrelerini ve bilgilerini al
        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
        if (actionDescriptor == null)
        {
            await next();
            return;
        }

        var entityType = actionDescriptor.ControllerName;
        string? entityId = null;
        
        // Metod parametrelerinden entity ID'yi bulmaya çalış (örn: id, userId, roleId, vb.)
        if (context.ActionArguments.TryGetValue("id", out var idValue))
        {
            entityId = idValue?.ToString();
        }
        
        // Servis referansını al
        var activityLogService = context.HttpContext.RequestServices.GetService<IActivityLogService>();
        if (activityLogService == null)
        {
            await next();
            return;
        }
        
        // İsteğin detaylarını hazırla
        var details = new Dictionary<string, object>();
        
        // İstek verilerini ekle
        if (IncludeRequestData && context.ActionArguments.Count > 0)
        {
            // Hassas veriler içerebilecek parametreleri filtrele
            var filteredArgs = FilterSensitiveData(context.ActionArguments);
            details["RequestData"] = filteredArgs;
        }
        
        // Action'ı çalıştır ve sonucu al
        var result = await next();
        
        // Cevap verilerini ekle
        if (IncludeResponseData && result.Result != null)
        {
            details["ResponseStatus"] = result.Exception == null ? "Success" : "Error";
            
            if (result.Exception != null)
            {
                details["ErrorMessage"] = result.Exception.Message;
            }
            else if (result.Result is Microsoft.AspNetCore.Mvc.ObjectResult objectResult)
            {
                // Hassas verileri filtreleyerek cevap verisini ekle
                details["ResponseData"] = FilterSensitiveData(objectResult.Value);
            }
        }
        
        // Aktiviteyi logla
        await activityLogService.LogActivityAsync(new CreateActivityLogDto
        {
            Category = Category,
            ActivityType = ActivityType,
            EntityType = entityType,
            EntityId = entityId,
            Details = details.Count > 0 ? JsonSerializer.Serialize(details) : null
        });
    }
    
    // Hassas verileri filtreleyen yardımcı metot
    private object FilterSensitiveData(object data)
    {
        if (data == null)
            return null;
            
        // Dictionary tipinde veri için
        if (data is IDictionary<string, object> dictionary)
        {
            var result = new Dictionary<string, object>();
            foreach (var pair in dictionary)
            {
                // Şifre veya hassas veri içeren alanları filtrele
                if (IsSensitiveField(pair.Key))
                {
                    result[pair.Key] = "***FILTERED***";
                }
                else
                {
                    result[pair.Key] = FilterSensitiveData(pair.Value);
                }
            }
            return result;
        }
        
        // Normal nesne için (örn: ViewModel)
        if (data is not { } obj || data.GetType().IsPrimitive || data is string) return data;
        try
        {
            // Nesneyi Dictionary'e dönüştürerek hassas alanları filtrele
            var json = JsonSerializer.Serialize(obj);
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                
            if (dict != null)
            {
                return FilterSensitiveData(dict);
            }
        }
        catch
        {
            // Dönüştürme başarısız olursa, nesneyi olduğu gibi döndür
        }

        return data;
    }
    
    // Hassas alan kontrolü
    private bool IsSensitiveField(string fieldName)
    {
        var sensitiveFields = new[]
        {
            "password", "pwd", "secret", "token", "key", "pin", "credential", 
            "creditcard", "credit_card", "ccnumber", "cvv", "ssn", "socialSecurity"
        };
        
        return sensitiveFields.Any(f => fieldName.ToLower().Contains(f));
    }
}