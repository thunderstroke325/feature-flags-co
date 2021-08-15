using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Models
{
    public class FeatureFlag
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _Id { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
        public int EnvironmentId { get; set; }
        public string ObjectType { get { return "FeatureFlag"; } set { value = "FeatureFlag"; } }
        public bool? IsArchived { get; set; }
        public FeatureFlagBasicInfo FF { get; set; }
        public List<FeatureFlagPrerequisite> FFP { get; set; }
        public List<FeatureFlagTargetUsersWhoMatchTheseRuleParam> FFTUWMTR { get; set; }

        public List<FeatureFlagTargetIndividualUser> FFTIUForFalse { get; set; }
        public List<FeatureFlagTargetIndividualUser> FFTIUForTrue { get; set; }


        public List<TargetIndividualForVariationOption> TargetIndividuals { get; set; }
        public List<VariationOption> VariationOptions { get; set; }

        public bool? IsMultiOptionMode { get; set; }
    }



    public class FeatureFlagBasicInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string KeyName { get; set; }
        public int EnvironmentId { get; set; }
        public string CreatorUserId { get; set; }
        public string Status { get; set; }
        public bool? DefaultRuleValue { get; set; }
        public double? PercentageRolloutForTrue { get; set; }
        public int PercentageRolloutForTrueNumber { get; set; }
        public double? PercentageRolloutForFalse { get; set; }
        public int PercentageRolloutForFalseNumber { get; set; }
        public string PercentageRolloutBasedProperty { get; set; }
        public bool? ValueWhenDisabled { get; set; }
        public DateTime? LastUpdatedTime { get; set; }

        public List<VariationOptionPercentageRollout> DefaultRulePercentageRollouts { get; set; }
        public VariationOption VariationOptionWhenDisabled { get; set; }
    }

    public class FeatureFlagSettings : FeatureFlagBasicInfo
    {
        public List<VariationOption> VariationOptions { get; set; }
    }

    public class FeatureFlagPrerequisite
    {
        public string PrerequisiteFeatureFlagId { get; set; }
        public bool VariationValue { get; set; }
        public VariationOption ValueOptionsVariationValue { get; set; }
    }
    public class FeatureFlagTargetIndividualUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string KeyId { get; set; }
        public string Email { get; set; }
    }
    public class FeatureFlagTargetUsersWhoMatchTheseRuleParam
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public List<FeatureFlagRuleJsonContent> RuleJsonContent { get; set; }
        public bool? VariationRuleValue { get; set; }
        public double? PercentageRolloutForTrue { get; set; }
        public int PercentageRolloutForTrueNumber { get; set; }
        public double? PercentageRolloutForFalse { get; set; }
        public int PercentageRolloutForFalseNumber { get; set; }
        public string PercentageRolloutBasedProperty { get; set; }



        public List<VariationOptionPercentageRollout> ValueOptionsVariationRuleValues { get; set; }
    }

    public class FeatureFlagRuleJsonContent
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

    public class TargetIndividualForVariationOption
    {
        public List<FeatureFlagTargetIndividualUser> Individuals { get; set; }
        public VariationOption ValueOption { get; set; }
    }

    public class VariationOptionPercentageRollout
    {
        public double[] RolloutPercentage { get; set; }
        public VariationOption ValueOption { get; set; }
    }
}
