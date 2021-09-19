using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.Experiments
{
    public class ExperimentResult
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

        public List<string> ConfidenceInterval { get; set; }
    }
}
