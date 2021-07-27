//using FFs.SDKs.CSharp;
//using Microsoft.Extensions.DependencyInjection;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Threading.Tasks;

//namespace FeatureFlags.SDKs
//{
//    public interface IFeatureFlagsCoService
//    {
//        string GetVariation(ToggleParam param, string featureFlagKey);
//    }

//    public class FeatureFlagsCoService : IFeatureFlagsCoService
//    {
//        private static readonly HttpClient _httpClient = new HttpClient();
//        private readonly string _environmentKey;
//        private readonly string _apiUrl;
//        public FeatureFlagsCoService(string environmentKey, string apiUri)
//        {
//            _environmentKey = environmentKey;
//            _apiUrl = apiUri;
//        }

//        public async Task<string> GetVariationAsync(ToggleParam param, string featureFlagKey)
//        {
//            using (var content = new StringContent(JsonConvert.SerializeObject(param), System.Text.Encoding.UTF8, "application/json"))
//            {
//                HttpResponseMessage result = _httpClient.PostAsync(_apiUrl, content).Result;
//                if (result.StatusCode == System.Net.HttpStatusCode.OK)
//                {
//                    string returnValue = await result.Content.ReadAsStringAsync();
//                    if (returnValue.ToLower() == "true")
//                        return true;
//                    else
//                        return false;
//                }
//                throw new Exception($"Failed to post data to feature-flags.co");
//            }
//            return "";
//        }
//    }

//}
