using System.ComponentModel.DataAnnotations;

namespace FeatureFlags.APIs.ViewModels.Project
{
    public class EnvKeyViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string KeyName { get; set; }
        
        [Required(AllowEmptyStrings = false)]
        public string KeyValue { get; set; }
    }
}
