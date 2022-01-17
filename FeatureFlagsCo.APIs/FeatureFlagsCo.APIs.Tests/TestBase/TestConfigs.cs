using System.Collections.Generic;

namespace FeatureFlags.APIs.Tests.TestBase
{
    public class TestConfigs
    {
        public const string ElasticSearchUrl = "";

        public static readonly Dictionary<string, string> MongoConfigs = new Dictionary<string, string>
        {
            // dev
            ["MongoDbSettings:ConnectionString"] = "",
            ["MongoDbSettings:DatabaseName"] = ""
        };
    }
}