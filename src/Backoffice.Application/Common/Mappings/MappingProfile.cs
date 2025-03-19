using AutoMapper;
using Backoffice.Application.DTOs.Menu;
using Backoffice.Domain.Entities.Menu;

namespace Backoffice.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // MenuItem -> MenuItemDto (for nested items)
        CreateMap<MenuItem, MenuItemDto>()
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder)))
            .ForMember(dest => dest.IsVisible, opt => opt.Ignore());

        // CreateUpdateMenuItemDto -> MenuItem
        CreateMap<CreateUpdateMenuItemDto, MenuItem>();
    }
}