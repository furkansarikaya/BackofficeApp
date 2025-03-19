using AutoMapper;
using Backoffice.Application.DTOs.Menu;
using Backoffice.Web.ViewModels.Menu;

namespace Backoffice.Web.Common.Mappings;

public class MenuMappingProfile : Profile
{
    public MenuMappingProfile()
    {
        // Map from DTO to ViewModel
        CreateMap<MenuItemDto, MenuViewModel>()
            .ForMember(dest => dest.IsCurrentPage, opt => opt.Ignore());
        
        // Map from ViewModel to DTO
        CreateMap<MenuViewModel, MenuItemDto>();
        
        // Map from form to DTO
        CreateMap<MenuItemFormViewModel, CreateUpdateMenuItemDto>();
        CreateMap<MenuItemDto, CreateUpdateMenuItemDto>().ReverseMap();
        
        // Map from DTO to form
        CreateMap<MenuItemDto, MenuItemFormViewModel>();
    }
}