using System;
using System.Collections.Generic;

namespace FeatureFlagsCo.MQ
{
    public class MqUserInfo
    {
        public string FFUserName { get; set; }
        public string FFUserEmail { get; set; }
        public string FFUserCountry { get; set; }
        public string FFUserKeyId { get; set; }
        public List<MqCustomizedProperty> FFUserCustomizedProperties { get; set; }
    }

    public class MqCustomizedProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
