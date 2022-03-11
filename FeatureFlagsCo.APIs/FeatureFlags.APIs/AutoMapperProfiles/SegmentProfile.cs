using AutoMapper;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;

namespace FeatureFlags.APIs.AutoMapperProfiles
{
    public class SegmentProfile : Profile
    {
        public SegmentProfile()
        {
            CreateMap<Segment, SegmentVm>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
            
            CreateMap<Segment, SegmentListItem>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        }
    }
}