using System.Diagnostics;
using Backoffice.Application.Common.Interfaces;

namespace Backoffice.Web.Middleware;

/// <summary>
/// HTTP isteklerini otomatik olarak loglayan middleware
/// </summary>
public class RequestLoggingMiddleware(
    RequestDelegate next,
    IConfiguration configuration)
{
    private readonly bool _logSuccessfulRequests = configuration.GetValue("Logging:LogSuccessfulRequests", false);

    public async Task InvokeAsync(HttpContext context, IDbLoggerService dbLogger)
    {
        if (ShouldSkipLogging(context))
        {
            // Bazı istekler için loglama yapma (statik dosyalar, sağlık kontrolleri, vb.)
            await next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            // Response body'i yakalamak için bir memory stream kullan
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                // İsteği işle
                await next(context);
            }
            catch (Exception ex)
            {
                // Hata oluşursa logla
                await LogExceptionAsync(context, ex, sw.Elapsed, dbLogger);
                throw; // Hatayı yeniden fırlat
            }

            // İstek başarılı ise ve başarılı istekler loglanacaksa logla
            if (_logSuccessfulRequests)
            {
                await LogSuccessfulRequestAsync(context, sw.Elapsed, dbLogger);
            }
            else if (!IsSuccessStatusCode(context.Response.StatusCode))
            {
                // Başarısız istekler için hata logla
                await LogErrorResponseAsync(context, sw.Elapsed, dbLogger);
            }

            // Response body'i orijinal stream'e geri yaz
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
            sw.Stop();
        }
    }

    /// <summary>
    /// İstek loglanmamalı mı?
    /// </summary>
    private bool ShouldSkipLogging(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        
        // Statik dosyalar için loglama yapma
        if (path.StartsWith("/css/") || 
            path.StartsWith("/js/") || 
            path.StartsWith("/lib/") || 
            path.StartsWith("/images/") || 
            path.StartsWith("/fonts/") ||
            path.EndsWith(".ico"))
        {
            return true;
        }

        // Sağlık kontrolleri için loglama yapma
        return path.StartsWith("/health") || path.StartsWith("/ping");
    }

    /// <summary>
    /// Başarılı HTTP isteklerini loglar
    /// </summary>
    private async Task LogSuccessfulRequestAsync(HttpContext context, TimeSpan elapsed, IDbLoggerService dbLogger)
    {
        var request = context.Request;
        var response = context.Response;
        
        var logMessage = $"HTTP {request.Method} {request.Path} responded {response.StatusCode} in {elapsed.TotalMilliseconds:0.0000} ms";
        
        var additionalData = new
        {
            RequestHost = request.Host.ToString(),
            RequestScheme = request.Scheme,
            RequestQueryString = request.QueryString.ToString(),
            ResponseStatusCode = response.StatusCode,
            ElapsedMilliseconds = elapsed.TotalMilliseconds,
            Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
        };
        
        await dbLogger.LogInformationAsync(logMessage, "HttpRequest", additionalData);
    }

    /// <summary>
    /// Başarısız HTTP isteklerini loglar
    /// </summary>
    private static async Task LogErrorResponseAsync(HttpContext context, TimeSpan elapsed, IDbLoggerService dbLogger)
    {
        var request = context.Request;
        var response = context.Response;
        
        var logLevel = response.StatusCode switch
        {
            >= 500 => Domain.Enums.LogLevel.Error,
            >= 400 => Domain.Enums.LogLevel.Warning,
            _ => Domain.Enums.LogLevel.Information
        };
        
        var logMessage = $"HTTP {request.Method} {request.Path} responded {response.StatusCode} in {elapsed.TotalMilliseconds:0.0000} ms";
        
        var additionalData = new
        {
            RequestHost = request.Host.ToString(),
            RequestScheme = request.Scheme,
            RequestQueryString = request.QueryString.ToString(),
            ResponseStatusCode = response.StatusCode,
            ElapsedMilliseconds = elapsed.TotalMilliseconds,
            Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
        };
        
        await dbLogger.LogAsync(logLevel, logMessage, "HttpRequest", null, additionalData);
    }

    /// <summary>
    /// İstek sırasında oluşan hataları loglar
    /// </summary>
    private static async Task LogExceptionAsync(HttpContext context, Exception exception, TimeSpan elapsed, IDbLoggerService dbLogger)
    {
        var request = context.Request;
        
        var logMessage = $"HTTP {request.Method} {request.Path} threw an exception after {elapsed.TotalMilliseconds:0.0000} ms";
        
        var additionalData = new
        {
            RequestHost = request.Host.ToString(),
            RequestScheme = request.Scheme,
            RequestQueryString = request.QueryString.ToString(),
            ElapsedMilliseconds = elapsed.TotalMilliseconds,
            Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
        };
        
        await dbLogger.LogErrorAsync(logMessage, "HttpRequest", exception, additionalData);
    }

    /// <summary>
    /// HTTP durum kodu başarılı mı?
    /// </summary>
    private static bool IsSuccessStatusCode(int statusCode)
    {
        return statusCode is >= 200 and < 400;
    }
}

/// <summary>
/// Middleware uzantı metodu
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}