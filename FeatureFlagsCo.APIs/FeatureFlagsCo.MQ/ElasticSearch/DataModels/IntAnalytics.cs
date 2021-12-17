using System;
using System.Collections.Generic;

namespace FeatureFlagsCo.MQ.ElasticSearch.DataModels
{
    public class IntAnalytics
    {
        public DateTime CreateAt { get; }

        public int EnvId { get; }
        
        public string Key { get; }

        public int Value { get; }

        public IEnumerable<string> Dimensions { get; }

        public IntAnalytics(
            int envId, 
            string key, 
            int value, 
            IEnumerable<string> dimensions = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("int analytics key cannot be null or empty");
            }
            Key = key;
            
            Value = value;
            EnvId = envId;
            Dimensions = dimensions ?? new List<string>();
            CreateAt = DateTime.UtcNow;
        }
    }
}