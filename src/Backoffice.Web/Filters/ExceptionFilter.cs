using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Backoffice.Domain.Exceptions;
using Backoffice.Web.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Backoffice.Web.Filters;

/// <summary>
/// Uygulama genelinde hata yakalama filtresi
/// </summary>
public class ExceptionFilter(
    IHostEnvironment env) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            // Farklı hata tipleri için özel işlemler
            case EntityNotFoundException notFoundException:
            {
                // 404 Not Found
                var result = new ViewResult
                {
                    ViewName = "Error",
                    StatusCode = 404,
                    ViewData = { Model = new ErrorViewModel { Message = notFoundException.Message } }
                };
            
                context.Result = result;
                context.ExceptionHandled = true;
                return;
            }
            case ValidationException validationException:
            {
                // 400 Bad Request - Validation Error
                context.ModelState.Clear();
            
                foreach (var error in validationException.Errors)
                {
                    context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
            
                // ModelState'i bir sonraki istekte göstermek için TempData'ya ekle
                var tempData = (ITempDataDictionary)context.HttpContext.Items["TempData"];
                tempData["ValidationErrors"] = validationException.Message;
            
                // Geri yönlendir veya aynı sayfayı tekrar göster
                context.Result = new RedirectResult(context.HttpContext.Request.Path.Value);
                context.ExceptionHandled = true;
                return;
            }
        }

        // Genel hata sayfası
        var errorViewModel = new ErrorViewModel
        {
            Message = env.IsDevelopment() 
                ? context.Exception.Message 
                : "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."
        };
        
        var errorResult = new ViewResult
        {
            ViewName = "Error",
            StatusCode = 500,
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = errorViewModel
            }
        };
        
        context.Result = errorResult;
        context.ExceptionHandled = true;
    }
}