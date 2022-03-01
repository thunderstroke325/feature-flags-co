using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FeatureFlags.APIs.Models
{
    [BsonIgnoreExtraElements]
    public class EnvironmentUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _Id { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        public int EnvironmentId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
        public string KeyId { get; set; }
        public List<FeatureFlagUserCustomizedProperty> CustomizedProperties { get; set; }
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
        public int EnvironmentId { get; set; }
        public string FeatureFlagId { get; set; }
        public DateTime? LastUpdatedTime { get; set; }
        public VariationOption VariationOptionResultValue { get; set; }
        public EnvironmentUser UserInfo { get; set; }
        public bool? SendToExperiment { get; set; }
        
        public CachedUserVariation CachedUserVariation()
        {
            var cachedUserVariation = new CachedUserVariation(VariationOptionResultValue, SendToExperiment);
            return cachedUserVariation;
        }
    }
}
