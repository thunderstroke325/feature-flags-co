using FeatureFlags.APIs.Models;
using System;
using System.Collections.Generic;

namespace FeatureFlags.APIs.ViewModels.DataSync
{
    public class EnvironmentDataViewModel
    {
        public string Version { get; set; }
        public DateTime Date { get; set; }

        public List<FeatureFlag> FeatureFlags { get; set; }

        public List<EnvironmentUser> EnvironmentUsers { get; set; }

        public EnvironmentUserProperty EnvironmentUserProperties { get; set; }
    }
}
