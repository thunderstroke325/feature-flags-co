using System.ComponentModel.DataAnnotations;
using FeatureFlags.Utils.Helpers;

namespace FeatureFlags.APIs.ViewModels.User
{
    public class RegisterByEmail
    {
        [Required(AllowEmptyStrings = false)]
        [EmailAddress]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
    }

    public class RegisterByPhone
    {
        [Required(AllowEmptyStrings = false)]
        public string PhoneNumber { get; set; }
        
        [Required(AllowEmptyStrings = false)]
        public string Code { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(128, MinimumLength = 5)]
        public string Password { get; set; }
    }

    public class RegisterResult
    {
        public string UserId { get; set; }

        public bool Success { get; set; }

        public string Message { get; set; }

        public static RegisterResult Failed(string message)
        {
            var failed = new RegisterResult
            {
                UserId = string.Empty, 
                Success = false,
                Message = message
            };

            return failed;
        }

        public static RegisterResult Ok(string userId)
        {
            Check.NotNullOrWhiteSpace(userId, nameof(userId));
            
            var success = new RegisterResult
            {
                UserId = userId, 
                Success = true,
                Message = string.Empty
            };

            return success;
        }
    }
}