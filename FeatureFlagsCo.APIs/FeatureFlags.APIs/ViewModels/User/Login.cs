using System.ComponentModel.DataAnnotations;

namespace FeatureFlags.APIs.ViewModels.User
{
    public class LoginByPassword
    {
        /// <summary>
        /// email or phoneNumber
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Identity { get; set; }
        
        /// <summary>
        /// password
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
    }

    public class LoginByPhoneCode
    {
        /// <summary>
        /// phone number
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// verification code
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Code { get; set; }
    }
    
    public class LoginResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public string Token { get; set; }

        public static LoginResult Failed(string message)
        {
            var failed = new LoginResult
            {
                Success = false,
                Message = message, 
                Token = string.Empty
            };

            return failed;
        }
        
        public static LoginResult Ok(string token)
        {
            var success = new LoginResult
            {
                Success = true,
                Message = string.Empty, 
                Token = token
            };

            return success;
        }
    }
}