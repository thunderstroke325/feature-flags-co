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

        public MongoDbFeatureFlagService(IMongoDbSettings settings,
            IOptions<MySettings> mySettings)
        {
            MongoClient client;
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

        public async Task<List<FeatureFlag>> GetActiveByIdsAsync(IEnumerable<string> ids)
        {
            return await _featureFlags.Find(p => !p.IsArchived && ids.Contains(p.Id)).ToListAsync();
        }

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

        public async Task<List<FeatureFlag>> GetFeatureFlagsAsync(int envId, bool isArchived, string searchText, int pageIndex, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await _featureFlags.Find((p) => p.EnvironmentId == envId && p.IsArchived == isArchived).SortByDescending(p => p.FF.LastUpdatedTime).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();
            else
                return await _featureFlags.Find((p) => p.EnvironmentId == envId && p.IsArchived == isArchived && p.FF.Name.ToLower().Contains(searchText.ToLower())).SortByDescending(p => p.FF.LastUpdatedTime).Skip(pageIndex * pageSize).Limit(pageSize).ToListAsync();
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
        }

        public async Task RemoveAsync(FeatureFlag bookIn) =>
            await _featureFlags.DeleteOneAsync(book => book.Id == bookIn.Id);

        public async Task RemoveAsync(string id) =>
            await _featureFlags.DeleteOneAsync(book => book.Id == id);

        public async Task<bool> CheckNameHasBeenUsedAsync(int envId, string name)
        {
            var featureFlag = await _featureFlags
                .Find(flag => flag.EnvironmentId == envId && flag.FF.Name == name)
                .FirstOrDefaultAsync();

            return featureFlag != null;
        }

        public async Task<List<FeatureFlag>> GetArchivedFeatureFlags(int envId)
        {
            var featureFlags = await _featureFlags
                .Find(featureFlag => featureFlag.EnvironmentId == envId && featureFlag.IsArchived)
                .SortByDescending(p => p.FF.LastUpdatedTime)
                .ToListAsync();

            return featureFlags;
        }
    }
}
