using AutoMapper;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels.Project;

namespace FeatureFlags.APIs.AutoMapperProfiles
{
    public class ProjectProfile : Profile
    {
        public ProjectProfile()
        {
            CreateMap<ProjectEnvironmentV2, ProjectViewModel>();

            CreateMap<EnvironmentV2, EnvironmentViewModel>();
        }
    }
}