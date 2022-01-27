using AutoMapper;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels.Public;

namespace FeatureFlags.APIs.AutoMapperProfiles
{
    public class FeatureFlagProfile : Profile
    {
        public FeatureFlagProfile()
        {
            CreateMap<FeatureFlag, SdkFeatureFlag>()
                .ForMember(des => des.Version, opt => opt.Ignore());
        }
    }
}