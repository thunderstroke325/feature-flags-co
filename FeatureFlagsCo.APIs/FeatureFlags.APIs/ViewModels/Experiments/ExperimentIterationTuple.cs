using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.Experiments
{
    public class ExperimentIterationTuple
    {
        public string ExperimentId { get; set; }
        public string IterationId { get; set; }
    }
}
