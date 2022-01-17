using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FeatureFlags.APIs.ViewModels.Project
{
    public class ProjectViewModel
    {
        public int Id { get; set; }
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "project name cannot be empty or null")]
        public string Name { get; set; }

        public IEnumerable<EnvironmentViewModel> Environments { get; set; }
    }
}
