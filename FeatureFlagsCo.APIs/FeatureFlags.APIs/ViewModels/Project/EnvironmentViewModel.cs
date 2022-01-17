using System.ComponentModel.DataAnnotations;

namespace FeatureFlags.APIs.ViewModels.Project
{
    public class EnvironmentViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "Environment name cannot be null or empty")]
        public string Name { get; set; }
        
        public string Description { get; set; }
        public string Secret { get; set; }
        public string MobileSecret { get; set; }
    }
}
