using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.ViewModels;
using FeatureFlags.APIs.ViewModels.FeatureFlagTrigger;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FeatureFlags.APIs.Services
{
    public class MongoDbFeatureTriggerService
    {
        private readonly IMongoCollection<FeatureFlagTrigger> _triggers;

        public MongoDbFeatureTriggerService(IMongoDbSettings settings,
            IOptions<MySettings> mySettings)
        {
            MongoClient client = null;
            if (mySettings.Value.HostingType == HostingTypeEnum.Azure.ToString())
            {
                MongoClientSettings s = MongoClientSettings.FromUrl(
                    new MongoUrl(settings.ConnectionString)
                );
                s.SslSettings =
                    new SslSettings() {EnabledSslProtocols = SslProtocols.Tls12};
                client = new MongoClient(s);
            }
            else
            {
                client = new MongoClient(settings.ConnectionString);
            }

            _triggers = client
                .GetDatabase(settings.DatabaseName)
                .GetCollection<FeatureFlagTrigger>("FeatureFlagTriggers");

            // Create index on updatedAt
            var indexKeysDefinition = Builders<FeatureFlagTrigger>.IndexKeys.Descending(fft => fft.UpdatedAt);
            Task.Run(() => _triggers.Indexes.CreateOneAsync(new CreateIndexModel<FeatureFlagTrigger>(indexKeysDefinition))).Wait();
        }

        public List<FeatureFlagTrigger> Get() =>
            _triggers.Find(p => true).ToList();

        public async Task<FeatureFlagTrigger> GetAsync(string id) =>
            await _triggers
                .Find<FeatureFlagTrigger>(trigger => trigger.Id == id && trigger.Status != (int)FeatureFlagTriggerStatusEnum.Archived)
                .FirstOrDefaultAsync();

        public async Task<FeatureFlagTrigger> CreateAsync(FeatureFlagTrigger trigger)
        {
            await _triggers.InsertOneAsync(trigger);
            return trigger;
        }

        public async Task<FeatureFlagTrigger> GetByIdAndFeatureFlagIdAsync(string id, string ffid)
        {
            return await _triggers
                .Find(trigger => trigger._Id == id && trigger.FeatureFlagId == ffid && trigger.Status != (int)FeatureFlagTriggerStatusEnum.Archived)
                .FirstOrDefaultAsync();
        }

        public async Task<List<FeatureFlagTrigger>> GetByFeatureFlagIdAsync(string ffid)
        {
            return await _triggers
                .Find(trigger => trigger.FeatureFlagId == ffid && trigger.Status != (int)FeatureFlagTriggerStatusEnum.Archived)
                .SortByDescending(trigger => trigger.UpdatedAt).ToListAsync();
        }

        public async Task<FeatureFlagTrigger> GetByTokenAsync(string token)
        {
            return await _triggers
                .Find(trigger => trigger.Token == token && trigger.Status != (int)FeatureFlagTriggerStatusEnum.Archived)
                .FirstOrDefaultAsync();
        }

        public async Task UpsertAsync(FeatureFlagTrigger trigger)
        {
            if (string.IsNullOrWhiteSpace(trigger._Id))
                await this.CreateAsync(trigger);
            else
                await this.UpdateAsync(trigger._Id, trigger);
        }

        public async Task<FeatureFlagTrigger> UpdateAsync(string id, FeatureFlagTrigger trigger) =>
            await _triggers.FindOneAndReplaceAsync(p => p._Id == id, trigger);
    }
}