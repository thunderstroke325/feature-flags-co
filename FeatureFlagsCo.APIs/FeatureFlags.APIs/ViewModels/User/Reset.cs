using System.ComponentModel.DataAnnotations;

namespace FeatureFlags.APIs.ViewModels.User
{
    public class Reset
    {
        [Required(AllowEmptyStrings = false)]
        public string Identity { get; set; }
        
        [Required(AllowEmptyStrings = false)]
        public string Code { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string NewPassword { get; set; }
    }

    public class ResetResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }
        
        public static ResetResult Failed(string message)
        {
            var failed = new ResetResult
            {
                Success = false,
                Message = message
            };

            return failed;
        }

        public static ResetResult Ok()
        {
            var success = new ResetResult
            {
                Success = true,
                Message = string.Empty
            };

            return success;
        }
    }
}