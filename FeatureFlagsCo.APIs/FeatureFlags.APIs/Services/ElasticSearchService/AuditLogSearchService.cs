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
    public interface IAuditLogSearchService
    {
        Task<Tuple<string, System.Net.HttpStatusCode>> GetFeatureFlagAuditLogAsync(
               string esHost, string featureFlagId, long startUnixTimeStamp, long endUnixTimeStamp,
               int pageIndex = 0, int pageSize = 20);
        Task<Tuple<string, System.Net.HttpStatusCode>> GetAllAuditLogAsync(
               string esHost, string environmentId, long startUnixTimeStamp, long endUnixTimeStamp,
               int pageIndex = 0, int pageSize = 20);
    }

    public class AuditLogSearchService : IAuditLogSearchService
    {
        private readonly IOptions<MySettings> _mySettings;

        public AuditLogSearchService(IOptions<MySettings> mySettings)
        {
            _mySettings = mySettings;
        }

        public async Task<Tuple<string, HttpStatusCode>> GetAllAuditLogAsync(string esHost, string environmentId, long startUnixTimeStamp, long endUnixTimeStamp, int pageIndex = 0, int pageSize = 20)
        {
            var term1EO = new ExpandoObject();
            term1EO.TryAdd("EnvironmentId.keyword", environmentId);
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
                HttpResponseMessage res = await client.PostAsync($"{esHost}/auditlog/_search", content);
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

        public async Task<Tuple<string, System.Net.HttpStatusCode>> GetFeatureFlagAuditLogAsync(
            string esHost, string featureFlagId, long startUnixTimeStamp, long endUnixTimeStamp,
            int pageIndex = 0, int pageSize = 20)
        {
            var term1EO = new ExpandoObject();
            term1EO.TryAdd("FeatureFlagId.keyword", featureFlagId);
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
                HttpResponseMessage res = await client.PostAsync($"{esHost}/auditlog/_search", content);
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
