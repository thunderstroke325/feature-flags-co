using System;
using System.Threading.Tasks;
using FeatureFlags.APIs.Tests.TestBase;
using FeatureFlagsCo.MQ.ElasticSearch;
using FeatureFlagsCo.MQ.ElasticSearch.DataModels;
using Nest;
using Shouldly;
using Xunit;

namespace FeatureFlags.APIs.Tests
{
    public class ElasticSearchServiceTests : IClassFixture<ElasticSearchFixture>
    {
        private readonly ElasticSearchService _service;
        public ElasticSearchServiceTests(ElasticSearchFixture fixture)
        {
            _service = fixture.ElasticSearch;
        }

        [Fact]
        public async Task Should_Index_A_Document()
        {
            var doc = new IntAnalytics
            {
                CreateAt = DateTime.UtcNow.AddSeconds(6.8),
                EnvId = 122,
                Key = "vip",
                Value = new Random().Next(5)
            };

            var success = await _service.IndexDocumentAsync(doc, ElasticSearchIndices.Analytics);
            success.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Count_Document()
        {
            var start = DateMath.Anchored(Convert.ToDateTime("2021-12-10 00:00:00"));
            var end = DateMath.Anchored(Convert.ToDateTime("2021-12-11 23:59:59"));

            var valueQuery = new QueryContainerDescriptor<IntAnalytics>()
                .Term(analytics => analytics.Value, 5);

            var dateRangeQuery = new QueryContainerDescriptor<IntAnalytics>()
                .DateRange(descriptor => descriptor
                    .Field(item => item.CreateAt)
                    .GreaterThanOrEquals(start).LessThanOrEquals(end));

            var countDescriptor = new CountDescriptor<IntAnalytics>()
                .Query(queryDescriptor => queryDescriptor.Bool(
                    boolDescriptor => boolDescriptor.Must(valueQuery, dateRangeQuery)))
                .Index(ElasticSearchIndices.Analytics);
            
            var count = await _service.CountDocumentsAsync(countDescriptor);
            
            count.ShouldBe(1);
        }

        [Fact]
        public async Task Should_Search_A_Document()
        {
            var start = DateMath.Anchored(Convert.ToDateTime("2021-12-10 00:00:00"));
            var end = DateMath.Anchored(Convert.ToDateTime("2021-12-11 23:59:59"));

            QueryContainer QueryDescriptor(QueryContainerDescriptor<IntAnalytics> query) =>
                query.Bool(descriptor => descriptor.Must(
                        valueDescriptor => valueDescriptor.Term(analytics => analytics.Key, "vip"),
                        timeDescriptor => timeDescriptor.DateRange(analytics => analytics
                            .Field(item => item.CreateAt)
                            .GreaterThanOrEquals(start)
                            .LessThanOrEquals(end)
                        )
                    )
                );

            var searchDescriptor = new SearchDescriptor<IntAnalytics>()
                .Query(QueryDescriptor)
                .Index(ElasticSearchIndices.Analytics);

            var response = await _service.SearchDocumentAsync(searchDescriptor);
            response.IsValid.ShouldBeTrue();
            response.Hits.Count.ShouldBe(6);
        }
    }
}