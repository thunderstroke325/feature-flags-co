using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.Experiments;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public interface IExperimentsService
    {
        Task<string> GetEnvironmentEvents(int envId, MetricTypeEnum metricType, string lastItem = "", string searchText = "", int pageSize = 20);
        Task<List<ExperimentResult>> GetExperimentResult(ExperimentQueryViewModel param);
    }

    public class ExperimentsService: IExperimentsService
    {
        private readonly IOptions<MySettings> _mySettings;

        public ExperimentsService(
            IOptions<MySettings> mySettings)
        {
            _mySettings = mySettings;
        }

        

        public async Task<List<ExperimentResult>> GetExperimentResult(ExperimentQueryViewModel param)
        {
            string experimentationHost = _mySettings.Value.ExperimentationServiceHost;

            using (var client = new HttpClient())
            {
                HttpContent content = new StringContent(JsonConvert.SerializeObject(param));
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                //由HttpClient发出异步Post请求
                HttpResponseMessage res = await client.PostAsync($"{experimentationHost}/api/ExperimentResults", content);
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = await res.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<ExperimentResult>>(result);
                }
                return new List<ExperimentResult>();
            }
        }

        public async Task<string> GetEnvironmentEvents(int envId, MetricTypeEnum metricType, string lastItem = "", string searchText = "", int pageSize = 20)
        {
            string esHost = _mySettings.Value.ElasticSearchHost;
            string indexTarget = "experiments";

            dynamic envIdMatch = new ExpandoObject();
            (envIdMatch as IDictionary<string, object>)["EnvironmentId.keyword"] = $"{envId}";

            dynamic boolClause = new ExpandoObject();
            boolClause.must = new List<dynamic>()
            {
                new {
                    match = new {
                        Type = "CustomEvent"
                    }
                },
                new {
                    match = envIdMatch
                },
            };

            if (!string.IsNullOrWhiteSpace(searchText)) 
            {
                boolClause.must.Add(new
                {
                    query_string = new
                    {
                        query = $"*{searchText}*",
                        default_field = "EventName"
                    }
                });
            }

            var compositeEO = new ExpandoObject();
            (compositeEO as IDictionary<string, object>)["sources"] = new List<dynamic>() {
                            new {
                                EventName = new {
                                    terms = new {
                                        field = "EventName.keyword"
                                    }
                                }
                            }
                        };


            (compositeEO as IDictionary<string, object>)["size"] = pageSize;

            if (!string.IsNullOrWhiteSpace(lastItem)) 
            {
                (compositeEO as IDictionary<string, object>)["after"] = new { 
                    EventName = lastItem
                };
            }

            var aggsEO = new
            {
                keys = new { 
                    composite = compositeEO
                }
            };

            dynamic queryEO = new ExpandoObject();
            (queryEO as IDictionary<string, object>)["bool"] = boolClause;

            dynamic sortEO = new ExpandoObject();
            (sortEO as IDictionary<string, object>)["EventName.keyword"] = new
            {
                order = "asc"
            };

            var body = new
            {
                size = 0,
                query = queryEO,
                sort = new List<dynamic> { sortEO },
                aggs = aggsEO
            };

            // to see how to create a query to fetch unique values, ref https://www.getargon.io/docs/articles/elasticsearch/unique-values.html

            using (var client = new HttpClient())
            {
                HttpContent content = new StringContent(JsonConvert.SerializeObject(body));
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                if (esHost.Contains("@")) // esHost contains username and password 
                {
                    var startIndex = esHost.LastIndexOf("/") + 1;
                    var endIndex = esHost.LastIndexOf("@");
                    var credential = esHost.Substring(startIndex, endIndex - startIndex).Split(":");
                    var userName = credential[0];
                    var password = credential[1];

                    esHost = esHost.Substring(0, startIndex) + esHost.Substring(endIndex + 1);

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                                                "Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{userName}:{password}")));
                }

                //由HttpClient发出异步Post请求
                HttpResponseMessage res = await client.PostAsync($"{esHost}/{indexTarget}/_search", content);
                if (res.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return await res.Content.ReadAsStringAsync();
                }
                return null;
            }
        }
    }
}
