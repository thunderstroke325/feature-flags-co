using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FeatureFlagsCo.FeatureInsights.ElasticSearch
{
    public class ElasticSearchFeatureFlagsUsageService : IFeatureFlagsUsageService
    {

        public async Task<string> GetFFVariationUserCountAsync(string esHost, string indexTarget, string featureFlagId)
        {
            var term1EO = new ExpandoObject();
            term1EO.TryAdd("FeatureFlagId.keyword", featureFlagId);
            //var term2EO = new ExpandoObject();
            //term2EO.TryAdd("VariationValue.keyword", variationLocalId);
            var mustEO = new List<dynamic>() {
                new {
                    term=term1EO
                }
                //new {
                //    term=term2EO
                //},
            };
            var boolEO = new
            {
                must = mustEO
            };
            var queryEO = new ExpandoObject();
            queryEO.TryAdd("bool", boolEO);
            var body = new
            {
                query = queryEO,
                aggs = new
                {
                    types_count = new
                    {
                        value_count = new
                        {
                            field = "UserKeyId.keyword"
                        }
                    },
                    group_by_status = new
                    {
                        terms = new
                        {
                            field = "VariationValue.keyword"
                        }
                    },
                }
            };

            using (var client = new HttpClient())
            {
                HttpContent content = new StringContent(JsonConvert.SerializeObject(body));
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                //由HttpClient发出异步Post请求
                HttpResponseMessage res = await client.PostAsync($"{esHost}/{indexTarget}/_search?size=0", content);
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return await res.Content.ReadAsStringAsync();
                }
                return null;
            }
        }

        public async Task<string> GetLinearUsageCountByTimeRangeAsync(string esHost, string indexTarget, string featureFlagId, DateTime startDateTime, DateTime endDateTime, int interval)
        {
            var termEO = new ExpandoObject();
            termEO.TryAdd("FeatureFlagId.keyword", featureFlagId);
            var rangeEO = new ExpandoObject();
            var rangesEO = new List<ExpandoObject>();

            var intervalBySeconds = endDateTime.Subtract(startDateTime).TotalSeconds / interval;
            var indexDateTime = startDateTime;
            while (indexDateTime < endDateTime)
            {
                var rangeItem = new ExpandoObject();
                var from = indexDateTime.ToString("yyyy-MM-ddTHH:mm:ss");
                indexDateTime = indexDateTime.AddSeconds(intervalBySeconds);
                var to = indexDateTime.ToString("yyyy-MM-ddTHH:mm:ss");
                rangeItem.TryAdd("from", from);
                rangeItem.TryAdd("to", to);
                rangesEO.Add(rangeItem);
            }
            rangesEO.Reverse();
            rangeEO.TryAdd("date_range", new {
                field = "TimeStamp",
                format = "yyyy-MM-dd'T'HH:mm:ss",
                ranges = rangesEO
            });
            var body = new {
                query = new { 
                    term = termEO
                },
                aggs = new
                {
                    range = rangeEO
                }
            };

            using (var client = new HttpClient())
            {
                HttpContent content = new StringContent(JsonConvert.SerializeObject(body));
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                //由HttpClient发出异步Post请求
                HttpResponseMessage res = await client.PostAsync($"{esHost}/{indexTarget}/_search?size=0", content);
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return await res.Content.ReadAsStringAsync();
                }
                return null;
            }
        }
    }

    public class GetLinearUsageCountByTimeRangeBody
    {

    }
}
