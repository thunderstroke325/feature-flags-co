using FeatureFlags.APIs.Services.MongoDb;
using Microsoft.Extensions.Configuration;

namespace FeatureFlags.APIs.Tests.TestBase
{
    public class MongoClientAccessorFixture
    {
        public MongoClientAccessor ClientAccessor { get; }

        public MongoClientAccessorFixture()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(TestConfigs.MongoConfigs)
                .Build();
            
            ClientAccessor = new MongoClientAccessor(configuration);
        }
    }
}