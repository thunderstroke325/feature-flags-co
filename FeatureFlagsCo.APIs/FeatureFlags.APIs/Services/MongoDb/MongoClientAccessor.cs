using FeatureFlags.Utils.ConventionalDependencyInjection;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FeatureFlags.APIs.Services.MongoDb
{
    // see http://mongodb.github.io/mongo-csharp-driver/2.14/reference/driver/connecting/#re-use
    public class MongoClientAccessor : ISingletonDependency
    {
        public MongoClient MongoClient { get; }
        public string DatabaseName { get; }

        public MongoClientAccessor(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDbSettings:ConnectionString"];
            
            // linq provider v3 has many improvement in version 2.14.x we should use it
            var clientSettings = MongoClientSettings.FromConnectionString(connectionString);
            clientSettings.LinqProvider = LinqProvider.V3;
            
            MongoClient = new MongoClient(clientSettings);
            DatabaseName = configuration["MongoDbSettings:DatabaseName"];
        }
    }
}