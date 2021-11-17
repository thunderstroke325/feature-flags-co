using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.Project
{
    public class ProjectEnvironmentViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int EnvironmentId { get; set; }
        public string EnvironmentName { get; set; }
        public string EnvironmentDescription { get; set; }
        public string EnvironmentSecret { get; set; }
        public string EnvironmentMobileSecret { get; set; }
    }
}
