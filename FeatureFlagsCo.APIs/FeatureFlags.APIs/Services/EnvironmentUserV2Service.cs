using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services.MongoDb;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FeatureFlags.APIs.Services
{
    public class EnvironmentUserV2Service : ITransientDependency
    {
        private readonly IMongoCollection<EnvironmentUser> _envUsers;

        public EnvironmentUserV2Service(MongoDbPersist mongoDb)
        {
            _envUsers = mongoDb.CollectionOf<EnvironmentUser>();
        }

        public async Task UpsertAsync(EnvironmentUser user)
        {
            var existing = await _envUsers.AsQueryable()
                .FirstOrDefaultAsync(x => x.EnvironmentId == user.EnvironmentId && x.KeyId == user.KeyId);
            if (existing != null)
            {
                existing.Update(user);
                
                var filter = Builders<EnvironmentUser>.Filter.Eq(x => x._Id, existing._Id);
                await _envUsers.ReplaceOneAsync(filter, existing);
            }
            else
            {
                await _envUsers.InsertOneAsync(user);
            }
        }
    }
}