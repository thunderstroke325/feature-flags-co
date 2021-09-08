using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.Project
{
    public class EnvProjectInfoViewModel
    {
        public int EnvId { get; set; }
        public int ProjectId { get; set; }
        public string EnvName { get; set; }
        public string ProjectName { get; set; }
    }
}
