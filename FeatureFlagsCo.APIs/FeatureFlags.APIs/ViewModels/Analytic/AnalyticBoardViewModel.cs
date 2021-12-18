using System;
using System.Collections.Generic;
using System.Linq;
using FeatureFlags.APIs.Models;
using FeatureFlagsCo.MQ.ElasticSearch;
using FeatureFlagsCo.MQ.ElasticSearch.DataModels;
using Nest;
using static Nest.Infer;

namespace FeatureFlags.APIs.ViewModels.Analytic
{
    public class DataSourceDefViewModel 
    {
        public string AnalyticBoardId { get; set; }
        public int EnvId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string KeyName { get; set; }
        public string DataType { get; set; }
    }

    public class DataGroupViewModel
    {
        public string AnalyticBoardId {get;set;}
        public int EnvId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<DataItem> Items { get; set; }
    }

    public class DataDimensionViewModel
    {
        public string AnalyticBoardId { get; set; }

        public int EnvId { get; set; }

        public string Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }

    public class CalculationParam
    {
        public int EnvId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<DataItem> Items { get; set; } = new List<DataItem>();
        public List<DataDimension> Dimensions { get; set; } = new List<DataDimension>();

        public SearchDescriptor<Analytics> SearchAggregationDescriptor()
        {
            var descriptor = new SearchDescriptor<Analytics>()
                .Query(CombinedQuery)
                .Aggregations(CombinedAggregations())
                .Index(ElasticSearchIndices.Analytics)
                .Size(0);
            
            return descriptor;
        }

        QueryContainer CombinedQuery(QueryContainerDescriptor<Analytics> query)
        {
            var dateRangeQuery = new DateRangeQuery
            {
                Field = Field<Analytics>(analytics => analytics.CreateAt),
                GreaterThanOrEqualTo = StartTime
            };
            if (EndTime.HasValue)
            {
                dateRangeQuery.LessThanOrEqualTo = EndTime.Value;
            }

            var envIdQuery = new TermQuery
            {
                Field = Field<Analytics>(analytics => analytics.EnvId),
                Value = EnvId
            };

            var queries = new List<QueryContainer> { dateRangeQuery, envIdQuery };
            if (Dimensions.Any())
            {
                var termsSetQuery = new TermsSetQuery
                {
                    Field = Field<Analytics>(analytics => analytics.Dimensions),
                    Terms = Dimensions.Select(dimension => dimension.ToString()),
                    MinimumShouldMatchScript = new InlineScript("params.num_terms")
                };

                queries.Add(termsSetQuery);
            }

            var combinedQuery = query.Bool(descriptor => descriptor.Must(queries.ToArray()));

            return combinedQuery;
        }

        AggregationBase CombinedAggregations()
        {
            var groupByKey = Items.GroupBy(item => item.DataSource.KeyName);

            var aggregations = new List<FilterAggregation>();
            foreach (var grouped in groupByKey)
            {
                var key = grouped.Key;
                
                var filterAggregation = new FilterAggregation($"{key.ToLower()}")
                {
                    Filter = new TermQuery
                    {
                        Field = Field<Analytics>(analytics => analytics.Key),
                        Value = key
                    },
                    Aggregations = new AggregationDictionary()
                };
                
                foreach (var dataItem in grouped)
                {
                    filterAggregation.Aggregations.TryAdd(
                        dataItem.AggregationName,
                        dataItem.AggregationContainer()
                    );
                }
                
                aggregations.Add(filterAggregation);
            }
            
            // combinedAggregations = aggregation1 & aggregation2 & aggregation3 ...
            var combinedAggregations = 
                aggregations.Aggregate<FilterAggregation, AggregationBase>(null, (current, agg) => current & agg);
            
            return combinedAggregations;
        }
    }

    public class CalculationItemResultViewModel 
    {
        public string Id { get; set; }
        public double Value { get; set; }
    }

    public class CalculationResultsViewModel
    {
        public IEnumerable<CalculationItemResultViewModel> Items { get; set; }
    }
}
