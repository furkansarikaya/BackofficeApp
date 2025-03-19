using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Backoffice.Web.ViewModels.User;

public class UserEditViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [Display(Name = "Kullanıcı Adı")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Ad")]
    public string? FirstName { get; set; }

    [Display(Name = "Soyad")]
    public string? LastName { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; }

    [Display(Name = "Roller")]
    public List<string> SelectedRoles { get; set; } = [];

    public List<SelectListItem> AvailableRoles { get; set; } = [];
}