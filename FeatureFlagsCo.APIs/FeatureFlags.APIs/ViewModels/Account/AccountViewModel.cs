using System.ComponentModel.DataAnnotations;

namespace FeatureFlags.APIs.Models
{
    public class AccountViewModel
    {
        public int Id { get; set; }
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "organization name cannot be null or empty")]
        public string OrganizationName { get; set; }
    }
}
