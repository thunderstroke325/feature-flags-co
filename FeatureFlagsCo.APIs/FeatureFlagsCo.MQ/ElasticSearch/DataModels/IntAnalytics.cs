using System;

namespace FeatureFlagsCo.MQ.ElasticSearch.DataModels
{
    public class IntAnalytics
    {
        public DateTime CreateAt { get; set; }

        public int EnvId { get; set; }
        
        public string Key { get; set; }

        public int Value { get; set; }
    }
}