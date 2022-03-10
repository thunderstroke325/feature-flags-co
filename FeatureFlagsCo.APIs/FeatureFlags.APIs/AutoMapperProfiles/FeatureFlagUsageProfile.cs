using AutoMapper;
using FeatureFlags.APIs.Models;

namespace FeatureFlags.APIs.AutoMapperProfiles
{
    public class FeatureFlagUsageProfile : Profile
    {
        public FeatureFlagUsageProfile()
        {
            CreateMap<InsightParam, FeatureFlagUsage>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.User.Country))
                .ForMember(dest => dest.UserKeyId, opt => opt.MapFrom(src => src.User.KeyId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.CustomizedProperties, opt => opt.MapFrom(src => src.User.CustomizedProperties));
        }
    }
}
