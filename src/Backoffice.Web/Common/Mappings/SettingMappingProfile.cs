using AutoMapper;
using Backoffice.Domain.Settings;
using Backoffice.Web.ViewModels.Settings;

namespace Backoffice.Web.Common.Mappings;

public class SettingMappingProfile : Profile
{
    public SettingMappingProfile()
    {
        CreateMap<AppSettings, AppSettingsViewModel>().ReverseMap();
    }
}