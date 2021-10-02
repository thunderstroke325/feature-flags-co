using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.FeatureFlagCommit
{
    public class ApproveRequestParam
    {
        public string FeatureFlagCommitId { get; set; }
        public string Comment { get; set; }
    }
}
