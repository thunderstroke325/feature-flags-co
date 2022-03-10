using AutoMapper;
using FeatureFlags.APIs.Models;
using FeatureFlagsCo.MQ;

namespace FeatureFlags.APIs.AutoMapperProfiles
{
    public class InsightUserProfile : Profile
    {
        public InsightUserProfile()
        {
            CreateMap<InsightUser, MqUserInfo>()
                .ForMember(des => des.FFUserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(des => des.FFUserEmail, opt => opt.MapFrom(src => src.Email))
                .ForMember(des => des.FFUserCountry, opt => opt.MapFrom(src => src.Country))
                .ForMember(des => des.FFUserKeyId, opt => opt.MapFrom(src => src.KeyId))
                .ForMember(des => des.FFUserCustomizedProperties, opt => opt.MapFrom(src => src.CustomizedProperties));
        }
    }
}
