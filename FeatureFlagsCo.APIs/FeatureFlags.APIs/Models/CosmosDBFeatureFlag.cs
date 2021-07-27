using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Models
{
    public class CosmosDBFeatureFlag
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public int EnvironmentId { get; set; }
        public string ObjectType { get { return "FeatureFlag"; } set { value = "FeatureFlag"; } }
        public bool? IsArchived { get; set; }
        public CosmosDBFeatureFlagBasicInfo FF { get; set; }
        public List<CosmosDBFeatureFlagPrerequisite> FFP { get; set; }
        public List<CosmosDBFeatureFlagTargetUsersWhoMatchTheseRuleParam> FFTUWMTR { get; set; }

        public List<CosmosDBFeatureFlagTargetIndividualUser> FFTIUForFalse { get; set; }
        public List<CosmosDBFeatureFlagTargetIndividualUser> FFTIUForTrue { get; set; }

        public List<VariationOption> VariationOptions { get; set; }


    }


    public class CosmosDBFeatureFlagBasicInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string KeyName { get; set; }
        public int EnvironmentId { get; set; }
        public string CreatorUserId { get; set; }
        public string Status { get; set; }
        public bool? DefaultRuleValue { get; set; }
        public VariationOption ValueOptionsDefaultRuleValue { get; set; }

        /// <summary>
        /// Also for selected value option
        /// </summary>
        public double? PercentageRolloutForTrue { get; set; }
        public int PercentageRolloutForTrueNumber { get; set; }
        /// <summary>
        /// Also for not selected value option
        /// </summary>
        public double? PercentageRolloutForFalse { get; set; }
        public int PercentageRolloutForFalseNumber { get; set; }
        public string PercentageRolloutBasedProperty { get; set; }
        public bool? ValueWhenDisabled { get; set; }
        public VariationOption ValueOptionsValueWhenDisabled { get; set; }
        public DateTime? LastUpdatedTime { get; set; }
    }
    public class CosmosDBFeatureFlagPrerequisite
    {
        public string PrerequisiteFeatureFlagId { get; set; }
        public bool VariationValue { get; set; }
        public VariationOption ValueOptionsVariationValue { get; set; }
    }
    public class CosmosDBFeatureFlagTargetIndividualUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string KeyId { get; set; }
        public string Email { get; set; }
    }
    public class CosmosDBFeatureFlagTargetUsersWhoMatchTheseRuleParam
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public List<CosmosDBFeatureFlagRuleJsonContent> RuleJsonContent { get; set; }
        public bool? VariationRuleValue { get; set; }
        public VariationOption ValueOptionsVariationRuleValue { get; set; }
        public double? PercentageRolloutForTrue { get; set; }
        public int PercentageRolloutForTrueNumber { get; set; }
        public double? PercentageRolloutForFalse { get; set; }
        public int PercentageRolloutForFalseNumber { get; set; }
        public string PercentageRolloutBasedProperty { get; set; }
    }

    public class CosmosDBFeatureFlagRuleJsonContent
    {
        [JsonProperty("property")]
        public string Property { get; set; }
        [JsonProperty("operation")]
        public string Operation { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }


    public class CreateFeatureFlagViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string KeyName { get; set; }
        public int EnvironmentId { get; set; }
        public string CreatorUserId { get; set; }
        public string Status { get; set; }
    }



    public class VariationOption
    {
        public int LocalId { get; set; }
        public int DisplayOrder { get; set; }
        public string VariationValue { get; set; }
    }
}
