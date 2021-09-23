using System.ComponentModel.DataAnnotations;

namespace FeatureFlags.APIs.Authentication
{
    public class RegisterModel
    {
        //[Required(ErrorMessage = "User Name is required")]
        //public string Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string OrgName { get; set; }

    }
}
