using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.Experiments;
using FeatureFlagsCo.MQ;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public class MessagingService
    {
        private readonly IOptions<MySettings> _mySettings;
        public MessagingService(IOptions<MySettings> mySettings) 
        {
            _mySettings = mySettings;
        }

        private async Task PostData(string path, HttpContent content) 
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                    //HttpContent content = new StringContent(JsonConvert.SerializeObject(data));
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    //由HttpClient发出异步Post请求
                    //HttpResponseMessage res = await client.PutAsync($"{esHost}/{message.IndexTarget}/_create/{message.FeatureFlagId}", content);
                    HttpResponseMessage res = await client.PostAsync($"{_mySettings.Value.MessagingServiceHost}/api/{path}/", content);
                    Console.WriteLine("Code:" + res.StatusCode.ToString());
                    if (res.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        Console.WriteLine("Message Sent.");
                    }
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                }
            }
        }

        public async Task SendInsightDataAsync(MessageModel param) 
        {
            await PostData("Insights", new StringContent(JsonConvert.SerializeObject(param)));
        }

        public async Task SendFeatureFlagDataAsync(FeatureFlagMessageModel param)
        {
            await PostData("Experiments/feature-flags", new StringContent(JsonConvert.SerializeObject(param)));
        }

        public async Task SendExperimentStartEndDataAsync(ExperimentIterationMessageViewModel param)
        {
            await PostData("Experiments/experiment", new StringContent(JsonConvert.SerializeObject(param)));
        }

        public async Task SendEventDataAsync(ExperimentMessageModel param)
        {
            await PostData("Experiments/events", new StringContent(JsonConvert.SerializeObject(param)));
        }
    }
}
