using System;
using System.Collections.Generic;

namespace FeatureFlags.APIs.Models
{
    public class FeatureFlagZeroCodeSetting : MongoModelBase
    {
        public int EnvId { get; set; }
        public bool IsArchived { get; set; }
        public bool IsActive { get; set; }
        public string FeatureFlagId { get; set; }
        public string FeatureFlagKey { get; set; }
        public string EnvSecret { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CssSelectorItem> Items { get; set; }

        public override string GetCollectionName()
        {
            return "FeatureFlagZeroCodeSetting";
        }
    }

    public class CssSelectorItem
    {
        public string Id { get; set; }
        public string CssSelector { get; set; }
        public string Description { get; set; }
        public VariationOption VariationOption { get; set; }
        public string Url { get; set; }
    }
}
