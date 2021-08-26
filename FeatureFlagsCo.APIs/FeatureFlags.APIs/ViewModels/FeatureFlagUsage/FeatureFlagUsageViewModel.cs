using FeatureFlags.APIs.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels
{
    public class FeatureFlagUsageViewModel
    {
        //[JsonProperty("totalUsers")]
        //public int TotalUsers { get; set; }
        //[JsonProperty("hitUsers")]
        //public int HitUsers { get; set; }
        //[JsonProperty("chartData")]
        public string ChartData { get; set; }

        [JsonProperty("userDistribution")]
        public string UserByVariationValue { get; set; }

    }


    public class FeatureFlagUerDistributionViewModel
    {
        [JsonProperty("tables")]
        public List<FeatureFlagUerDistributionItemViewModel> Tables { get; set; }
    }

    public class FeatureFlagUerDistributionItemViewModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("columns")]
        public List<FeatureFlagUsageChartColumnViewModel> Columns { get; set; }
        [JsonProperty("rows")]
        public List<List<dynamic>> Rows { get; set; }
    }

    public class FeatureFlagUerDistributionItemColumnViewModel
    {
        [JsonProperty("variation")]
        public string Variation { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class FeatureFlagUsageChartDataViewModel
    {
        [JsonProperty("tables")]
        public List<FeatureFlagUsageChartItemViewModel> Tables { get; set; }
    }

    public class FeatureFlagUsageChartItemViewModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("columns")]
        public List<FeatureFlagUsageChartColumnViewModel> Columns { get; set; }
        [JsonProperty("rows")]
        public List<List<dynamic>> Rows { get; set; }
    }

    public class FeatureFlagUsageChartColumnViewModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public enum FeatureFlagUsageChartQueryTimeSpanEnum
    {
        PT30M,
        PT2H,
        P1D,
        P7D,
    }
}
