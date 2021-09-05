using FeatureFlagsCo.MQ;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FeatureFlagsCo.FeatureInsights
{
    public interface IExperimentationService
    {
        Task<Tuple<string, System.Net.HttpStatusCode>> GetListAsync(string esHost, string envId, long startUnixTimeStamp, long endUnixTimeStamp,
            int pageIndex = 0, int pageSize = 20);
    }
}
