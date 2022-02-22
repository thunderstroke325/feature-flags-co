using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using MongoDB.Driver;

namespace FeatureFlags.APIs.Services
{
    public class EnvironmentSettingV2Service : ITransientDependency
    {
        private readonly MongoDbIntIdRepository<EnvironmentV2> _environments;

        public EnvironmentSettingV2Service(MongoDbIntIdRepository<EnvironmentV2> environments)
        {
            _environments = environments;
        }

        public async Task<List<EnvironmentSettingV2>> GetAsync(int envId, string type)
        {
            var env = await _environments.FirstOrDefaultAsync(x => x.Id == envId);
            
            var settings = env.Settings.Where(x => x.Type == type).ToList();
            return settings;
        }

        public async Task<IEnumerable<EnvironmentSettingV2>> UpsertAsync(
            int envId,
            IEnumerable<EnvironmentSettingV2> newSettings)
        {
            var env = await _environments.FirstOrDefaultAsync(x => x.Id == envId);
            if (env == null)
            {
                return null;
            }

            foreach (var newSetting in newSettings)
            {
                env.UpsertSetting(newSetting);
            }

            var updatedSettings = env.Settings;
            await _environments.UpdateOneAsync(
                x => x.Id == envId,
                Builders<EnvironmentV2>.Update.Set(x => x.Settings, updatedSettings)
            );

            return updatedSettings;
        }

        public async Task<IEnumerable<EnvironmentSettingV2>> DeleteAsync(int envId, string settingId)
        {
            var env = await _environments.FirstOrDefaultAsync(x => x.Id == envId);
            if (env == null)
            {
                return null;
            }

            env.DeleteSetting(settingId);

            var updatedSettings = env.Settings;
            await _environments.UpdateOneAsync(
                x => x.Id == envId,
                Builders<EnvironmentV2>.Update.Set(x => x.Settings, updatedSettings)
            );

            return updatedSettings;
        }
    }
}