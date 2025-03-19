using Backoffice.Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace Backoffice.Infrastructure.Identity;

/// <summary>
/// Microsoft.AspNetCore.Identity.IdentityResult'ı uygulamamızın Result sınıfına dönüştürme uzantıları
/// </summary>
public static class IdentityResultExtensions
{
    /// <summary>
    /// IdentityResult nesnesini uygulama için kullanılan Result nesnesine dönüştürür
    /// </summary>
    public static Result ToApplicationResult(this IdentityResult identityResult)
    {
        return identityResult.Succeeded
            ? Result.Success()
            : Result.Failure(identityResult.Errors.Select(e => e.Description));
    }
}