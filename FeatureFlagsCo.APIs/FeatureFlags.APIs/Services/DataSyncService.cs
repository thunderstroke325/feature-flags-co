using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels.DataSync;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FeatureFlags.APIs.Services
{
    public class DataSyncService : ITransientDependency
    {
        private readonly INoSqlService _noSqlService;
        private readonly IEnvironmentService _envService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DataSyncService> _logger;

        public DataSyncService(
            INoSqlService noSqlService,
            IEnvironmentService envService, 
            IHttpClientFactory httpClientFactory, 
            ILogger<DataSyncService> logger)
        {
            _noSqlService = noSqlService;
            _envService = envService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<EnvironmentDataViewModel> GetEnvironmentDataAsync(int envId)
        {
            return new EnvironmentDataViewModel
            {
                Date = DateTime.UtcNow,
                Version = "1.0",
                FeatureFlags = await _noSqlService.GetEnvironmentDataAsync<FeatureFlag>(envId),
                EnvironmentUsers = await _noSqlService.GetEnvironmentDataAsync<EnvironmentUser>(envId),
                EnvironmentUserProperties = (await _noSqlService.GetEnvironmentDataAsync<EnvironmentUserProperty>(envId)).FirstOrDefault()
            };
        }

        public async Task SaveEnvironmentDataAsync(int envId, EnvironmentDataViewModel data)
        {
            var envSecret = await _envService.GetSecretAsync(envId);
            await _noSqlService.SaveEnvironmentDataAsync(envSecret.AccountId, envSecret.ProjectId, envId, data);
        }

        public async Task<bool> SyncToRemoteAsync(
            int envId, 
            string remoteUrl, 
            object data)
        {
            var client = _httpClientFactory.CreateClient();

            var content = JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            
            var payload = new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json);

            try
            {
                var response = await client.PostAsync(remoteUrl, payload);
                
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                var err = $"sync data to envId {envId}, remoteUrl {remoteUrl} failed";
                _logger.LogError(ex, err);

                return false;
            }
        }
    }
}
