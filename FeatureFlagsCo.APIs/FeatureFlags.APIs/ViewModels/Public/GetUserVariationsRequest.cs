using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FeatureFlags.APIs.Models;

namespace FeatureFlags.APIs.ViewModels.Public
{
    public class GetUserVariationsRequest
    {
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "the parameter 'userKeyId' cannot be null or empty")]
        public string UserKeyId { get; set; }
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "the parameter 'userName' cannot be null or empty")]
        public string UserName { get; set; }

        public string Email { get; set; }

        public string Country { get; set; }

        public List<FeatureFlagUserCustomizedProperty> CustomizedProperties { get; set; } 
            = new List<FeatureFlagUserCustomizedProperty>();

        public EnvironmentUser EnvironmentUser()
        {
            var envUser = new EnvironmentUser
            {
                Country = Country,
                CustomizedProperties = CustomizedProperties,
                Email = Email,
                KeyId = UserKeyId,
                Name = UserName
            };

            return envUser;
        }
    }
}