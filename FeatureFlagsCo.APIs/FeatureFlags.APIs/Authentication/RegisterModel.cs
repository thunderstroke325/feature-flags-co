using System.ComponentModel.DataAnnotations;

namespace FeatureFlags.APIs.Authentication
{
    public class RegisterModel
    {
        [EmailAddress]
        [Required(ErrorMessage = "email cannot be empty", AllowEmptyStrings = false)]
        public string Email { get; set; }

        [Required(ErrorMessage = "password cannot be empty", AllowEmptyStrings = false)]
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string OrgName { get; set; }
    }
}
