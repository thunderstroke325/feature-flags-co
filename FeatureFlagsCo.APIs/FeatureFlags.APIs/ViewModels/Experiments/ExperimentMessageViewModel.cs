using FeatureFlagsCo.MQ;
using System.Collections.Generic;

namespace FeatureFlags.APIs.ViewModels.Experiments
{
    public class ExperimentMessageViewModel
    {
        public string Route { get; set; }
        public string Secret { get; set; }
        public long TimeStamp { get; set; } // This is ignored, to be removed along with SDKs
        public string Type { get; set; }
        public string EventName { get; set; }
        public float NumericValue { get; set; }
        public MqUserInfo User { get; set; }
        public string AppType { get; set; }
        public List<MqCustomizedProperty> CustomizedProperties { get; set; }
    }
}
