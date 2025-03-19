using System.Runtime.CompilerServices;
using Backoffice.Domain.Enums;
using Backoffice.Web.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backoffice.Web.Attributes;

/// <summary>
/// Controller ve action'larda izin kontrolü yapmak için öznitelik
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class PermissionAttribute : Attribute, IFilterFactory
{
    public string Controller { get; }
    public PermissionType Type { get; }
    public string PermissionCode { get; }

    /// <summary>
    /// Controller ve izin tipi belirterek izin kontrolü yapar
    /// </summary>
    public PermissionAttribute(PermissionType type, [CallerFilePath] string sourceFilePath = "")
    {
        Type = type;
        
        // Controller adını otomatik çıkar
        if (!string.IsNullOrEmpty(sourceFilePath))
        {
            var fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
            Controller = fileName;
        }
        else
        {
            // Dosya yolu boşsa sınıf adından tahmin et
            Controller = new System.Diagnostics.StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.Name ?? "Unknown";
        }
        
        PermissionCode = PermissionHelper.GeneratePermissionCode(Controller, Type);
    }

    /// <summary>
    /// Controller adını manuel olarak belirtir (özel durumlar için)
    /// </summary>
    public PermissionAttribute(string controller, PermissionType type)
    {
        Controller = controller;
        Type = type;
        PermissionCode = PermissionHelper.GeneratePermissionCode(Controller, Type);
    }

    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new Filters.PermissionFilter(PermissionCode);
    }
}