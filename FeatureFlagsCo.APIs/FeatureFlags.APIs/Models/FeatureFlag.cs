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
        public bool IsArchived { get; set; }
        public FeatureFlagBasicInfo FF { get; set; }
        public List<FeatureFlagPrerequisite> FFP { get; set; }
        public List<FeatureFlagTargetUsersWhoMatchTheseRuleParam> FFTUWMTR { get; set; }
        public List<TargetIndividualForVariationOption> TargetIndividuals { get; set; }
        public List<VariationOption> VariationOptions { get; set; }
        public string Version { get; set; }
        public DateTime? EffeciveDate { get; set; }
        public bool? ExptIncludeAllRules { get; set; }

        //public override string GetCollectionName()
        //{
        //    return "FeatureFlags";
        //}
    }



    public class FeatureFlagBasicInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public FeatureFlagType Type { get; set; }
        public string KeyName { get; set; }
        public int EnvironmentId { get; set; }
        public string CreatorUserId { get; set; }
        public string Status { get; set; }
        public bool IsDefaultRulePercentageRolloutsIncludedInExpt { get; set; }
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
        public bool isIncludedInExpt { get; set; }
        public List<FeatureFlagRuleJsonContent> RuleJsonContent { get; set; }
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

    public enum FeatureFlagType 
    {
        Classic = 1,
        Pretargeted = 2
    }

    public class CreateFeatureFlagViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public FeatureFlagType Type { get; set; }
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
        public double? ExptRollout { get; set; }
        public double[] RolloutPercentage { get; set; }
        public VariationOption ValueOption { get; set; }
    }
}
