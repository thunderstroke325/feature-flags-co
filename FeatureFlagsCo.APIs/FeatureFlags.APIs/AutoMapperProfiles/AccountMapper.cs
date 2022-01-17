using AutoMapper;
using FeatureFlags.APIs.Models;

namespace FeatureFlags.APIs.AutoMapperProfiles
{
    public class AccountMapper : Profile
    {
        public AccountMapper()
        {
            CreateMap<AccountV2, AccountViewModel>();
        }
    }
}