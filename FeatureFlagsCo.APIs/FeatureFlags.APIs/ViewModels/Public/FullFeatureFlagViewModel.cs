using System.Collections.Generic;
using System.Text.Json.Serialization;
using FeatureFlags.APIs.Models;
using FeatureFlags.Utils.ExtensionMethods;

namespace FeatureFlags.APIs.ViewModels.Public
{
    public class FullFeatureFlagViewModel
    {
        [JsonPropertyName("_id")]
        public string _Id { get; set; }
        
        [JsonPropertyName("id")] 
        public string Id { get; set; }

        public int EnvironmentId { get; set; }

        public bool IsArchived { get; set; }

        public FeatureFlagBasicInfo FF { get; set; }

        public List<FeatureFlagPrerequisite> FFP { get; set; }

        public List<FeatureFlagTargetUsersWhoMatchTheseRuleParam> FFTUWMTR { get; set; }

        public List<TargetIndividualForVariationOption> TargetIndividuals { get; set; }

        public List<VariationOption> VariationOptions { get; set; }

        public bool? ExptIncludeAllRules { get; set; }

        public long Version => FF.LastUpdatedTime?.UnixTimestamp() ?? 0;
    }
}