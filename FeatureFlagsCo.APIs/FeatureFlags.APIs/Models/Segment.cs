using System;
using System.Collections.Generic;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.Utils.Helpers;

namespace FeatureFlags.APIs.Models
{
    public class Segment : MongoDbObjectIdEntity
    {
        public int EnvId { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }

        public IEnumerable<string> Included { get; set; }

        public IEnumerable<string> Excluded { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdateAt { get; set; }

        public Segment(
            int envId, 
            string name,
            IEnumerable<string> included,
            IEnumerable<string> excluded,
            string description)
        {
            if (envId <= 0)
            {
                throw new ArgumentException($"invalid envId {envId}");
            }
            EnvId = envId;
            
            Check.NotNullOrWhiteSpace(name, nameof(name));

            Name = name;
            Included = included ?? Array.Empty<string>();
            Excluded = excluded ?? Array.Empty<string>();
            
            Description = description ?? string.Empty;
            
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(
            string name,
            IEnumerable<string> included,
            IEnumerable<string> excluded,
            string description)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            Name = name;
            Included = included ?? Array.Empty<string>();
            Excluded = excluded ?? Array.Empty<string>();
            
            Description = description ?? string.Empty;
            
            UpdateAt = DateTime.UtcNow;
        }
    }
    
    public class FlagSegmentReference
    {
        public string FlagName { get; set; }
        
        public string FlagKeyName { get; set; }
    }
}