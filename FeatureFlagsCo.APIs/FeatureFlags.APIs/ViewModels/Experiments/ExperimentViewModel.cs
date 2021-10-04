using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.Experiments
{
    public class ExperimentViewModel
    {
        public string ExptId { get; set; }
        public int EnvId { get; set; }
        public string EventName { get; set; }
        public string StartExptTime { get; set; }
        public string EndExptTime { get; set; }
        public string FlagId { get; set; }
        public string BaselineVariation { get; set; }
        public List<string> Variations { get; set; }
    }
}
