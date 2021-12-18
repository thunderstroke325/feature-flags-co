using System.Collections.Generic;
using System.Linq;
using FeatureFlagsCo.MQ.ElasticSearch.DataModels;

namespace FeatureFlags.APIs.ViewModels.Analytic
{
    public class CreateIntAnalyticsRequest
    {
        public string Key { get; set; }

        public int Value { get; set; }

        public IEnumerable<DataDimension> Dimensions { get; set; }

        public IntAnalytics IntAnalytics(int envId)
        {
            var dimensions = Dimensions.Select(dimension => dimension.ToString());
            
            var analytics = new IntAnalytics(envId, Key, Value, dimensions);
            return analytics;
        }
    }
}