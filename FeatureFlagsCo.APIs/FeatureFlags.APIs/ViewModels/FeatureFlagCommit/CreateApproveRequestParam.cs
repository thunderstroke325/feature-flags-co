using FeatureFlags.APIs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.FeatureFlagCommit
{
    public class CreateApproveRequestParam
    {
        public FeatureFlag FeatureFlagParam { get; set; }
        public string Comment { get; set; }
    }
}
