using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace FeatureFlags.APIs.Models
{
    public class FeatureFlagTrigger
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _Id { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        public int Type { get; set; }

        public int Action { get; set; }

        public string Token { get; set; }

        public int Status { get; set; }

        public int Times { get; set; }

        public string FeatureFlagId { get; set; }

        public string Description { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime? LastTriggeredAt { get; set; }
    }
}