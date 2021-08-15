using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Models
{
    public class EnvironmentUserProperty
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public int EnvironmentId { get; set; }
        public int ProjectId { get; set; }
        public int AccountId { get; set; }
        public string ObjectType { get { return "EnvironmentUserProperties"; } set { value = "EnvironmentUserProperties"; } }
        public List<string> Properties { get; set; }
    }
}
