using FeatureFlags.APIs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.Metrics
{
    public class MetricViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int EnvId { get; set; }
        public string Description { get; set; }
        public string MaintainerUserId { get; set; }
        public string EventName { get; set; }
        public EventType EventType { get; set; }
        public CustomEventTrackOption CustomEventTrackOption { get; set; }
    }
}
