using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.FeatureFlagsViewModels;
using FeatureFlagsCo.FeatureInsights;
using FeatureFlagsCo.MQ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{

    public class ExperimentationService : IExperimentationService
    {
        private readonly IOptions<MySettings> _mySettings;

        public ExperimentationService(IOptions<MySettings> mySettings)
        {
            _mySettings = mySettings;
        }

        public async Task<Tuple<string, System.Net.HttpStatusCode>> GetListAsync(
            string esHost, string envId, long startUnixTimeStamp, long endUnixTimeStamp,
            int pageIndex = 0, int pageSize = 20)
        {
            var term1EO = new ExpandoObject();
            term1EO.TryAdd("EnvironmentId.keyword", envId);
            var mustEO = new List<dynamic>() {
                new {
                    term = term1EO
                },
                new {
                    range = new
                    {
                        TimeStamp = new
                        {
                            gte = startUnixTimeStamp,
                            lte = endUnixTimeStamp
                        }
                    }
                },
            };
            var boolEO = new
            {
                must = mustEO
            };
            var queryEO = new ExpandoObject();
            queryEO.TryAdd("bool", boolEO);
            var body = new
            {
                from = pageIndex * pageSize,
                size = pageSize,
                query = queryEO,
                sort = new List<dynamic> {
                    new
                    {
                        TimeStamp = new {
                            order = "desc"
                        }
                    }
                }
            };

            using (var client = new HttpClient())
            {
                HttpContent content = new StringContent(JsonConvert.SerializeObject(body));
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                //由HttpClient发出异步Post请求
                HttpResponseMessage res = await client.PostAsync($"{esHost}/experiments/_search", content);
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Tuple.Create<string, System.Net.HttpStatusCode>(await res.Content.ReadAsStringAsync(), res.StatusCode);
                }
                else
                {
                    return Tuple.Create<string, System.Net.HttpStatusCode>("", res.StatusCode);
                }
            }
        }
    }
}
