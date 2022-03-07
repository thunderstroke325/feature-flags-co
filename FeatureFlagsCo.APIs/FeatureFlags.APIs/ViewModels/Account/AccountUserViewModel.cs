using System.ComponentModel.DataAnnotations;

namespace FeatureFlags.APIs.ViewModels.Account
{
    public class AccountUserViewModel
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string PhoneNumber { get; set; }
        
        public string Role { get; set; }
        public string InitialPassword { get; set; }
    }
}
