using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public class MongoDbEnvironmentUserPropertyService
    {
        private readonly IMongoCollection<EnvironmentUserProperty> _environmentUserProperties;
        private readonly IOptions<MySettings> _mySettings;

        public MongoDbEnvironmentUserPropertyService(IMongoDbSettings settings,
            IOptions<MySettings> mySettings)
        {
            _mySettings = mySettings;

            MongoClient client = null;
            if (mySettings.Value.HostingType == HostingTypeEnum.Azure.ToString())
            {
                MongoClientSettings s = MongoClientSettings.FromUrl(
                  new MongoUrl(settings.ConnectionString)
                );
                s.SslSettings =
                  new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
                client = new MongoClient(s);
            }
            else
            {
                client = new MongoClient(settings.ConnectionString);
            }

            var database = client.GetDatabase(settings.DatabaseName);
            _environmentUserProperties = database.GetCollection<EnvironmentUserProperty>("EnvironmentUserProperties");
        }

        public async Task UpsertItemAsync(EnvironmentUserProperty item)
        {
            string id = FeatureFlagKeyExtension.GetEnvironmentUserPropertyId(item.EnvironmentId);
            item.Id = id;

            var existingItem = await GetAsync(item.Id);
            if (existingItem != null)
            {
                item._Id = existingItem._Id;
                await UpdateAsync(item.Id, item);
            }
            else
            {
                await CreateAsync(item);
            }
        }

        public async Task<List<EnvironmentUserProperty>> GetByEnvironmentAsync(int envId)
        {
            return await _environmentUserProperties.Find(p => p.EnvironmentId == envId).ToListAsync();
        }

        public List<EnvironmentUserProperty> Get() =>
            _environmentUserProperties.Find(p => p.ObjectType == "EnvironmentUserProperties").ToList();

        public async Task<EnvironmentUserProperty> GetAsync(string id) =>
            await _environmentUserProperties.Find<EnvironmentUserProperty>(book => book.Id == id).FirstOrDefaultAsync();

        public async Task<EnvironmentUserProperty> GetByEnvironmentIdAsync(int environmentId) =>
            await _environmentUserProperties.Find<EnvironmentUserProperty>(book => book.EnvironmentId == environmentId).FirstOrDefaultAsync();

        public async Task<EnvironmentUserProperty> CreateAsync(EnvironmentUserProperty book)
        {
            await _environmentUserProperties.InsertOneAsync(book);
            return book;
        }

        public async Task UpdateAsync(string id, EnvironmentUserProperty bookIn) =>
            await _environmentUserProperties.ReplaceOneAsync(book => book.Id == id, bookIn);

        public async Task RemoveAsync(EnvironmentUserProperty bookIn) =>
            await _environmentUserProperties.DeleteOneAsync(book => book.Id == bookIn.Id);

        public async Task RemoveAsync(string id) =>
            await _environmentUserProperties.DeleteOneAsync(book => book.Id == id);
    }
}
