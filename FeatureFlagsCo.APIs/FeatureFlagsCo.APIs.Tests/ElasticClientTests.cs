using System;
using System.Threading.Tasks;
using FeatureFlags.APIs.Tests.TestBase;
using Nest;
using Shouldly;
using Xunit;

namespace FeatureFlags.APIs.Tests
{
    public class ElasticClientTests
    {
        private readonly ElasticClient _client = new ElasticClient(new Uri(TestConfigs.ElasticSearchUrl));

        [Fact]
        public async Task Should_Get_Indices()
        {
            var response = await _client.Cat.IndicesAsync();
            response.IsValid.ShouldBeTrue();
        }
    }
}