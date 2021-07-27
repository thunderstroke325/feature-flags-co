using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FFs.SDKs.CSharp
{
    public class AgileToggle
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ToggleParam _toggle;
        private readonly string _apiUrl;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environmentSecret"></param>
        /// <param name="ffKeyName"></param>
        /// <param name="userKeyId">User identifier, User session id, etc.</param>
        public AgileToggle(string environmentSecret, string env = "Production", bool isSSL = false)
        {
            _toggle = new ToggleParam();
            _toggle.EnvironmentSecret = environmentSecret;
            _toggle.FFUserCustomizedProperties = new List<ToggleParam.ToggleParamCustomizedProperty>();

            _apiUrl = "#{protocol}#://api-#{env}#.feature-flags.co/Variation/GetUserVariationResult";
            if (env == "Production")
                _apiUrl.Replace("#{env}#", "prod");
            else
                _apiUrl.Replace("#{env}#", "dev");
            if (isSSL)
                _apiUrl.Replace("#{protocol}#", "https");
            else
                _apiUrl.Replace("#{protocol}#", "http");
        }

        public async Task<bool> IsInConditionAsync(string ffKeyName)
        {
            if (string.IsNullOrWhiteSpace(_toggle.FFUserKeyId))
                throw new Exception("Please use .AddUserKeyId() method to add a User identifier (id of user, user session id or else)");
            using (var content = new StringContent(JsonConvert.SerializeObject(_toggle), System.Text.Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage result = _httpClient.PostAsync(_apiUrl, content).Result;
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string returnValue = await result.Content.ReadAsStringAsync();
                    if (returnValue.ToLower() == "true")
                        return true;
                    else
                        return false;
                }
                throw new Exception($"Failed to post data to feature-flags.co");
            }
        }


        public AgileToggle AddCustomizedProperty(string propertyName, string propertyValue)
        {
            var existed = _toggle.FFUserCustomizedProperties.FirstOrDefault(p => p.Name == propertyName);
            if (existed == null)
                _toggle.FFUserCustomizedProperties.Add(new ToggleParam.ToggleParamCustomizedProperty { Name = propertyName, Value = propertyValue });
            else
                existed.Value = propertyValue;
            return this;
        }


        public AgileToggle AddUserKeyId(string keyId)
        {
            _toggle.FFUserKeyId = keyId;
            return this;
        }

        public AgileToggle AddName(string name)
        {
            _toggle.FFUserName = name;
            return this;
        }

        public AgileToggle AddEmail(string email)
        {
            _toggle.FFUserEmail = email;
            return this;
        }
    }


    public class ToggleParam
    {
        public string FeatureFlagKeyName { get; set; }
        public string EnvironmentSecret { get; set; }
        public string FFUserName { get; set; }
        public string FFUserEmail { get; set; }
        public string FFUserCountry { get; set; }
        public string FFUserKeyId { get; set; }
        public List<ToggleParamCustomizedProperty> FFUserCustomizedProperties { get; set; }


        public class ToggleParamCustomizedProperty
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}
