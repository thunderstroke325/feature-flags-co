using AutoMapper;
using FeatureFlags.APIs.Models;

namespace FeatureFlags.APIs.AutoMapperProfiles
{
    public class FeatureFlagProfile : Profile
    {
        public FeatureFlagProfile()
        {
            CreateMap<FeatureFlag, ServerSdkFeatureFlag>()
                .ForMember(des => des.Timestamp, opt => opt.Ignore());
        }
    }
}