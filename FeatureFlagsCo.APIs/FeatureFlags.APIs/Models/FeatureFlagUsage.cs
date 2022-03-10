using FeatureFlags.APIs.Services;
using System.Collections.Generic;

namespace FeatureFlags.APIs.Models
{
    public class FeatureFlagUsage
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
        public string UserKeyId { get; set; }
        public List<CustomizedProperty> CustomizedProperties { get; set; }
        public List<InsightUserVariationParam> UserVariations { get; set; }
    }


    // TODO remove
    public class FeatureFlagUsageParam
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
        public string UserKeyId { get; set; }
        public List<CustomizedProperty> CustomizedProperties { get; set; }
        public List<InsightUserVariationParam> UserVariations { get; set; }

        public bool IsValid()
        {
            var invalid =
                string.IsNullOrWhiteSpace(UserKeyId) ||
                UserVariations == null ||
                UserVariations.Count == 0;

            return !invalid;
        }

        public EnvironmentUser AsEnvironmentUser(int envId)
        {
            var envUser = new EnvironmentUser
            {
                Id = FeatureFlagKeyExtension.GetEnvironmentUserId(envId, UserKeyId),
                EnvironmentId = envId,
                KeyId = UserKeyId,
                Name = UserName,
                Email = Email,
                Country = Country,
                CustomizedProperties = CustomizedProperties,
            };

            return envUser;
        }
    }
}