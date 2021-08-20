using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Models
{
    public class EnvironmentUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _Id { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        public int EnvironmentId { get; set; }
        public string ObjectType { get { return "EnvironmentUser"; } set { value = "EnvironmentUser"; } }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
        public string KeyId { get; set; }
        public List<FeatureFlagUserCustomizedProperty> CustomizedProperties { get; set; }
        //public List<FeatureFlagUserPercentageRecord> FeatureFlagUserPercentageRecords { get; set; }

        public List<string> FeatureFlags { get; set; }
    }

    public class FeatureFlagUserCustomizedProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }



    public class EnvironmentFeatureFlagUser
    {
        [JsonProperty("id")]
        public string id { get; set; }
        public string ObjectType { get { return "EnvironmentFFUser"; } set { value = "EnvironmentFFUser"; } }
        public int EnvironmentId { get; set; }
        public string FeatureFlagId { get; set; }
        public DateTime? LastUpdatedTime { get; set; }
        public bool? ResultValue { get; set; }
        public string PercentageRolloutBasedRuleId { get; set; }

        public VariationOption VariationOptionResultValue { get; set; }
        public EnvironmentUser UserInfo { get; set; }
    }
}
