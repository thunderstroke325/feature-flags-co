using System.Collections.Generic;
using System.Linq;

namespace FeatureFlagsCo.MQ.ElasticSearch.DataModels
{
    public class CreateIntAnalyticsRequest
    {
        public string Key { get; set; }

        public int Value { get; set; }

        public IEnumerable<AnalyticsDimension> Dimensions { get; set; }

        public IntAnalytics IntAnalytics(int envId)
        {
            var dimensions = Dimensions.Select(dimension => dimension.ToString());
            
            var analytics = new IntAnalytics(envId, Key, Value, dimensions);
            return analytics;
        }
    }
}