using System.Collections.Generic;
using FeatureFlags.APIs.Models;

namespace FeatureFlags.APIs.ViewModels.Public
{
    public class GetUserVariationsRequest
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string KeyId { get; set; }

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
                KeyId = KeyId,
                Name = Name
            };

            return envUser;
        }
    }
}