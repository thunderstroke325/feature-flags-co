using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Models
{
    public class FeatureFlagHtmlDetectionSetting : MongoModelBase
    {
        public int EnvironmentId { get; set; }
        public bool IsArchived { get; set; }
        public bool IsActive { get; set; }
        public string FeatureFlagId { get; set; }
        public string EnvironmentKey { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public FeatureFlagHtmlDetectionSettingInput Input { get; set; }

        public override string GetCollectionName()
        {
            return "FeatureFlagHtmlDetectionSettings";
        }
    }

    public class FeatureFlagHtmlDetectionSettingInput
    {
        public string ClassName { get; set; }
        public string InnerKeyText { get; set; }
        public string NodeName { get; set; }
    }
}
