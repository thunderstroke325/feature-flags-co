using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Tests.TestBase;
using FeatureFlags.APIs.ViewModels.Analytic;
using FeatureFlagsCo.MQ.ElasticSearch;
using Shouldly;
using Xunit;

namespace FeatureFlags.APIs.Tests
{
    public class AnalyticBoardTests : IClassFixture<ElasticSearchFixture>
    {
        private readonly ElasticSearchService _elasticSearch;

        public AnalyticBoardTests(ElasticSearchFixture fixture)
        {
            _elasticSearch = fixture.ElasticSearch;
        }

        [Fact]
        public void Should_Have_Aggregation_Name()
        {
            var dataSourceDef =
                new DataSourceDef(Guid.NewGuid().ToString(), "订单", "order", "数值型");

            var dataItem = new DataItem { DataSource = dataSourceDef, CalculationType = CalculationType.Average };

            dataItem.AggregationName.ShouldNotBeNull();
            dataItem.AggregationName.ShouldBe("order_average");
        }

        [Fact]
        public async Task Should_Calculate_Result()
        {
            var param = new CalculationParam
            {
                EnvId = 122,
                StartTime = Convert.ToDateTime("2021-12-10 00:00:00"),
                EndTime = Convert.ToDateTime("2021-12-11 23:59:59"),
                Items = new List<DataItem>
                {
                    new DataItem
                    {
                        DataSource = new DataSourceDef(Guid.NewGuid().ToString(), "订单", "order", "数值型"),
                        CalculationType = CalculationType.Average,
                    },
                    new DataItem
                    {
                        DataSource = new DataSourceDef(Guid.NewGuid().ToString(), "订单", "order", "数值型"),
                        CalculationType = CalculationType.Sum
                    },
                    new DataItem
                    {
                        DataSource = new DataSourceDef(Guid.NewGuid().ToString(), "VIP", "vip", "数值型"),
                        CalculationType = CalculationType.Average
                    },
                    new DataItem
                    {
                        DataSource = new DataSourceDef(Guid.NewGuid().ToString(), "VIP", "vip", "数值型"),
                        CalculationType = CalculationType.Sum
                    },
                }
            };

            var result = await _elasticSearch.SearchDocumentAsync(param.SearchAggregationDescriptor());
            result.IsValid.ShouldBeTrue();

            // should get order & vip aggregation
            result.Aggregations.Count.ShouldBe(2);

            // assert order aggregation
            var orderAgg = result.Aggregations.Children("order");
            orderAgg.ShouldNotBeNull();
            orderAgg.Sum("order_sum").Value.ShouldBe(21.0);
            orderAgg.Average("order_average").Value.ShouldBe(3.5);

            // assert vip aggregation
            var vipAgg = result.Aggregations.Children("vip");
            vipAgg.ShouldNotBeNull();
            vipAgg.Sum("vip_sum").Value.ShouldBe(18.0);
            vipAgg.Average("vip_average").Value.ShouldBe(3.0);
        }
    }
}