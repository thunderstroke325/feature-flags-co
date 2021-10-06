using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.Services
{
    public class MongoDbFeatureFlagService
    {
        private readonly IMongoCollection<FeatureFlag> _featureFlags;
        private readonly IOptions<MySettings> _mySettings;

        public MongoDbFeatureFlagService(IMongoDbSettings settings,
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
            _featureFlags = database.GetCollection<FeatureFlag>("FeatureFlags");
        }

        public List<FeatureFlag> Get() =>
            _featureFlags.Find(p => true).ToList();

        public async Task<List<FeatureFlag>> GetByEnvironmentAsync(int envId)
        {
            return await _featureFlags.Find(p => p.EnvironmentId == envId).ToListAsync();
        }

        public async Task<List<FeatureFlag>> SearchAsync(string searchText, int environmentId, int pageIndex, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await _featureFlags.Find(p => p.EnvironmentId == environmentId).SortByDescending(p => p.FF.LastUpdatedTime).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();
            else
                return await _featureFlags.Find(p => p.EnvironmentId == environmentId && p.FF.Name.ToLower().Contains(searchText.ToLower())).SortByDescending(p => p.FF.LastUpdatedTime).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();
        }

        public async Task<List<FeatureFlag>> SearchActiveAsync(int envId, string searchText, int pageIndex, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await _featureFlags.Find(p => !p.IsArchived && p.EnvironmentId == envId).SortByDescending(p => p.FF.LastUpdatedTime).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();
            else
                return await _featureFlags.Find(p => !p.IsArchived && p.EnvironmentId == envId && p.FF.Name.ToLower().Contains(searchText.ToLower())).SortByDescending(p => p.FF.LastUpdatedTime).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();
        }

        public async Task<List<FeatureFlag>> GetFeatureFlagsAsync(int envId, bool isArchived, int pageIndex, int pageSize)
        {
            return await _featureFlags.Find((p) => p.EnvironmentId == envId && p.IsArchived == isArchived).SortByDescending(p => p.FF.LastUpdatedTime).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();
        }

        public async Task<FeatureFlag> GetAsync(string id) =>
            await _featureFlags.Find<FeatureFlag>(book => book.Id == id).FirstOrDefaultAsync();


        public async Task UpsertItemAsync(FeatureFlag item)
        {
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

        public async Task<FeatureFlag> CreateAsync(FeatureFlag book)
        {
            await _featureFlags.InsertOneAsync(book);
            return book;
        }

        public async Task UpdateAsync(string id, FeatureFlag featureFlag)
        {
            await _featureFlags.FindOneAndReplaceAsync(p => p.Id == id, featureFlag);
            //await _featureFlags.ReplaceOneAsync(p => p.Id == id, bookIn);
        }

        public async Task RemoveAsync(FeatureFlag bookIn) =>
            await _featureFlags.DeleteOneAsync(book => book.Id == bookIn.Id);

        public async Task RemoveAsync(string id) =>
            await _featureFlags.DeleteOneAsync(book => book.Id == id);
    }
}
