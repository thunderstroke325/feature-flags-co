using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Models
{
    public abstract class MongoModelBase 
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public abstract string GetCollectionName();
    }
}
