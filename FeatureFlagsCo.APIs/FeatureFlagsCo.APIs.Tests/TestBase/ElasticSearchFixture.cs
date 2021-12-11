using System;
using FeatureFlagsCo.MQ.ElasticSearch;
using Microsoft.Extensions.Logging.Abstractions;
using Nest;

namespace FeatureFlags.APIs.Tests.TestBase
{
    public class ElasticSearchFixture
    {
        public ElasticSearchService ElasticSearch { get; }
        
        public ElasticSearchFixture()
        {
            ElasticSearch = new ElasticSearchService(
                new ElasticClient(new Uri("http://elastic:abc123456@localhost:9200")),
                new NullLogger<ElasticSearchService>()
            );
        }
    }
}