using System.Collections.Generic;
using System.Linq;
using FeatureFlagsCo.MQ.ElasticSearch.DataModels;

namespace FeatureFlags.APIs.ViewModels.Analytic
{
    public class CreateAnalyticsRequest
    {
        public string Key { get; set; }

        public float Value { get; set; }

        public IEnumerable<DataDimension> Dimensions { get; set; }

        public Analytics Analytics(int envId)
        {
            var dimensions = Dimensions.Select(dimension => dimension.ToString());

            var analytics = new Analytics(envId, Key, Value, dimensions);
            return analytics;
        }
    }
}