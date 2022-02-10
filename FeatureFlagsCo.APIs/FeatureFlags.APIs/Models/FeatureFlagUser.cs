using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FeatureFlags.APIs.Models
{
    public class FeatureFlagUser
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "'userKeyId' cannot be null or empty")]
        public string UserKeyId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "'userName' cannot be null or empty")]
        public string UserName { get; set; }

        public string Email { get; set; }

        public string Country { get; set; }
        
        public List<FeatureFlagUserCustomizedProperty> CustomizedProperties { get; set; } 
            = new List<FeatureFlagUserCustomizedProperty>();

        // for deserialization
        public FeatureFlagUser()
        {
        }
        
        public FeatureFlagUser(
            string userKeyId,
            string userName,
            string email,
            string country)
        {
            if (string.IsNullOrWhiteSpace(userKeyId))
            {
                throw new ArgumentException("userKeyId cannot be null or whitespace", nameof(userKeyId));
            }
            UserKeyId = userKeyId;

            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException("userName cannot be null or whitespace", nameof(userName));
            }
            UserName = userName;

            Email = email;
            Country = country;
        }
        
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