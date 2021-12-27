using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FeatureFlagsCo.MQ.ElasticSearch.DataModels;

namespace FeatureFlags.APIs.ViewModels.Analytic
{
    public class CreateAnalyticsRequest
    {
        [Required]
        [StringLength(250)]
        public string Key { get; set; }

        [Required]
        public float Value { get; set; }

        public IEnumerable<DataDimension> Dimensions { get; set; } = new List<DataDimension>();

        public Analytics Analytics(int envId)
        {
            var dimensions = Dimensions.Select(dimension => dimension.ToString());

            var analytics = new Analytics(envId, Key, Value, dimensions);
            return analytics;
        }
    }
}