using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FeatureFlagsCo.FeatureInsights
{
    public interface IFeatureFlagsUsageService
    {
        Task<string> GetLinearUsageCountByTimeRangeAsync(string esHost, string indexTarget, string featureFlagId, DateTime startDateTime, DateTime endDateTime, int interval);
        Task<string> GetFFVariationUserCountAsync(string esHost, string indexTarget, string featureFlagId);

    }
}
