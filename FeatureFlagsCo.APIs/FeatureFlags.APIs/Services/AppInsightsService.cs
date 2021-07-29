using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public interface IAppInsightsService
    {
        Task<FeatureFlagUsageChartDataViewModel> GetFFUsageChartDataAsync(string featureFlagId, string chartQueryTimeSpan);
    }

    public class AppInsightsService : IAppInsightsService
    {
        private readonly IOptions<MySettings> _mySettings;

        public AppInsightsService(IOptions<MySettings> mySettings)
        {
            _mySettings = mySettings;
        }

        public async Task<FeatureFlagUsageChartDataViewModel> GetFFUsageChartDataAsync(string featureFlagId, string chartQueryTimeSpan)
        {
            var returnModel = new FeatureFlagUsageChartDataViewModel();
            int envId = FeatureFlagKeyExtension.GetEnvIdByFeautreFlagId(featureFlagId);
            var featureFlagKey = FeatureFlagKeyExtension.GetFeautreFlagKeyById(featureFlagId);
            string binSpan = "10m";
            if (chartQueryTimeSpan == FeatureFlagUsageChartQueryTimeSpanEnum.P1D.ToString())
                binSpan = "2h";
            if (chartQueryTimeSpan == FeatureFlagUsageChartQueryTimeSpanEnum.P7D.ToString())
                binSpan = "14h";
            if (chartQueryTimeSpan == FeatureFlagUsageChartQueryTimeSpanEnum.PT2H.ToString())
                binSpan = "4h";
            if (chartQueryTimeSpan == FeatureFlagUsageChartQueryTimeSpanEnum.PT30M.ToString())
                binSpan = "2m";

            string html = string.Empty;
            string url = $"https://api.applicationinsights.azure.cn/v1/apps/{_mySettings.Value.AppInsightsApplicationId}/query?timespan={chartQueryTimeSpan}&query=traces | where (customDimensions.envId == '{envId}') and (customDimensions.featureFlagKey == '{featureFlagKey}') |  summarize totalCount = sum(itemCount) by bin(timestamp, {binSpan})";
            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Headers.Add("x-api-key", _mySettings.Value.AppInsightsApplicationApiSecret);
            request.Headers.Add("content-type", "application/json");

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                    returnModel = JsonConvert.DeserializeObject<FeatureFlagUsageChartDataViewModel>(html);
                }
            }
            return returnModel;
        }

    }
}
