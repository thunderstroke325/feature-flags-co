using AutoMapper;
using FeatureFlags.APIs.Models;

namespace FeatureFlags.APIs.AutoMapperProfiles
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<AccountV2, AccountViewModel>();
        }
    }
}