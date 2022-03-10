using AutoMapper;
using FeatureFlagsCo.MQ;
using FeatureFlags.APIs.Models;

namespace FeatureFlags.APIs.AutoMapperProfiles
{
    public class CustomizePropertyProfile: Profile
    {
        public CustomizePropertyProfile()
        {
            CreateMap<CustomizedProperty, MqCustomizedProperty>();
        }
    }
}
