using Backoffice.Domain.Enums;

namespace Backoffice.Web.Authorization;

/// <summary>
/// İzin kodlarını oluşturmak için yardımcı sınıf
/// </summary>
public static class PermissionHelper
{
    /// <summary>
    /// Controller adı ve izin tipinden bir izin kodu oluşturur
    /// </summary>
    public static string GeneratePermissionCode(string controller, PermissionType type)
    {
        // Controller ismindeki 'Controller' son ekini kaldır
        var entity = controller.Replace("Controller", "");
        
        // Çoğul hale getir (basit yaklaşım)
        if (!entity.EndsWith("s"))
        {
            entity += "s";
        }
        
        return $"{entity}.{type}";
    }
}