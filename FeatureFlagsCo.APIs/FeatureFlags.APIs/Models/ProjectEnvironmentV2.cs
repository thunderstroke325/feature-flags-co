using System.Collections.Generic;

namespace FeatureFlags.APIs.Models
{
    public class ProjectEnvironmentV2
    {
        public int Id { get; set; }

        public string Name { get; set; }
        
        public IEnumerable<EnvironmentV2> Environments { get; set; }
    }
}