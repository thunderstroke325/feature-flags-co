using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Models
{
    public class Experiment : MongoModelBase
    {
        public int EnvId { get; set; }
        public string EventName { get; set; }

        public string FlagId { get; set; }

        public string BaselineVariation { get; set; }

        public List<string> Variations { get; set; }

        public List<ExperimentIteration> Iterations { get; set; }
        public bool IsArvhived { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public override string GetCollectionName()
        {
            return "Experiments";
        }
    }

    public class ExperimentResult
    {
        public string ExperimentId { get; set; }
        public string IterationId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; } // updated time, not the end time of the iteration
        public List<IterationResult> Results { get; set; }

    }

    public class ExperimentIteration
    {
        public string Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<IterationResult> Results { get; set; }

        //public string FeatureFlagVersion { get; set; } TODO to be added feature flag version is established
    }

    public class IterationResult 
    {
        public float ChangeToBaseline { get; set; }
        public long Conversion { get; set; }
        public float ConversionRate { get; set; }
        public bool IsBaseline { get; set; }
        public bool IsInvalid { get; set; }
        public bool IsWinner { get; set; }
        public float PValue { get; set; }
        public long UniqueUsers { get; set; }
        public string Variation { get; set; }

        public List<float> ConfidenceInterval { get; set; }
    }
}
