using System;
using System.Threading.Tasks;
using FeatureFlags.APIs.Tests.TestBase;
using FeatureFlagsCo.MQ.ElasticSearch;
using FeatureFlagsCo.MQ.ElasticSearch.DataModels;
using Nest;
using static Nest.Infer;
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
            var doc = new IntAnalytics(146, "order", 3, new []{ "location@guangzhou", "hotel@lavande" });
            var success = await _service.IndexDocumentAsync(doc, ElasticSearchIndices.Analytics);
            success.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Count_Document()
        {
            var valueQuery = new TermQuery
            {
                Field = Field<IntAnalytics>(analytics => analytics.Key),
                Value = "order"
            };

            var dateRangeQuery = new DateRangeQuery
            {
                Field = Field<IntAnalytics>(analytics => analytics.CreateAt),
                GreaterThanOrEqualTo = Time("2021-12-17 00:00:00"),
                LessThanOrEqualTo = Time("2021-12-17 23:59:59")
            };

            var termsSetQuery = new TermsSetQuery
            {
                Field = Field<IntAnalytics>(analytics => analytics.Dimensions),
                Terms = new []{ "hotel@Atour" },
                MinimumShouldMatchScript = new InlineScript("params.num_terms")
            };
            
            var countDescriptor = new CountDescriptor<IntAnalytics>()
                .Query(queryDescriptor => queryDescriptor.Bool(
                    boolDescriptor => boolDescriptor.Must(valueQuery, dateRangeQuery, termsSetQuery)))
                .Index(ElasticSearchIndices.Analytics);
            
            var count = await _service.CountDocumentsAsync(countDescriptor);
            
            count.ShouldBe(2);
        }

        [Fact]
        public async Task Should_Search_A_Document()
        {
            QueryContainer QueryDescriptor(QueryContainerDescriptor<IntAnalytics> query) =>
                query.Bool(descriptor => descriptor.Must(
                        valueDescriptor => valueDescriptor.Term(analytics => analytics.Key, "order"),
                        timeDescriptor => timeDescriptor.DateRange(analytics => analytics
                            .Field(item => item.CreateAt)
                            .GreaterThanOrEquals(Time("2021/12/17 00:00:00"))
                            .LessThanOrEquals(Time("2021/12/17 23:59:59"))
                        )
                    )
                );

            var searchDescriptor = new SearchDescriptor<IntAnalytics>()
                .Query(QueryDescriptor)
                .Index(ElasticSearchIndices.Analytics);

            var response = await _service.SearchDocumentAsync(searchDescriptor);
            response.IsValid.ShouldBeTrue();
            response.Hits.Count.ShouldBe(5);
        }
        
        private static DateMathExpression Time(string time)
        {
            return DateMath.Anchored(Convert.ToDateTime(time));
        }
    }
}