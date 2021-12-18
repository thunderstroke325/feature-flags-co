using System;
using System.Collections.Generic;

namespace FeatureFlagsCo.MQ.ElasticSearch.DataModels
{
    public class Analytics
    {
        public DateTime CreateAt { get; }

        public int EnvId { get; }
        
        public string Key { get; }

        public float Value { get; }

        public IEnumerable<string> Dimensions { get; }

        public Analytics(
            int envId, 
            string key, 
            float value, 
            IEnumerable<string> dimensions = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("analytics key cannot be null or empty");
            }
            Key = key;
            
            Value = value;
            EnvId = envId;
            Dimensions = dimensions ?? new List<string>();
            CreateAt = DateTime.UtcNow;
        }
    }
}