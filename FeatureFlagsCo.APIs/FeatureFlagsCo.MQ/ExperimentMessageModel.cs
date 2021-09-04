using System;
using System.Collections.Generic;

namespace FeatureFlagsCo.MQ
{
    public class ExperimentMessageModel
    {
        public string Route { get; set; }
        public string Secret { get; set; }
        public long TimeStamp { get; set; }
        public string Type { get; set; }
        public string MethodName { get; set; }
        public ExperimentUserInfo User { get; set; }
        public string ApplicationType { get; set; }
        public List<ExperimentCustomizedProperty> CustomizedProperties { get; set; }
        public string ProjectId { get; set; }
        public string EnvironmentId { get; set; }
        public string AccountId { get; set; }
    }

    public class ExperimentUserInfo
    {
        public string FFUserName { get; set; }
        public string FFUserEmail { get; set; }
        public string FFUserCountry { get; set; }
        public string FFUserKeyId { get; set; }
        public List<ExperimentCustomizedProperty> FFUserCustomizedProperties { get; set; }
    }

    public class ExperimentCustomizedProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
