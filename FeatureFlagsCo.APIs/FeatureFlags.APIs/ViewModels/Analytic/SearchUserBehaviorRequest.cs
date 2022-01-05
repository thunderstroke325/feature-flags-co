using System;
using System.ComponentModel.DataAnnotations;
using FeatureFlagsCo.MQ.ElasticSearch;
using Microsoft.AspNetCore.Mvc;
using Nest;
using static Nest.Infer;

namespace FeatureFlags.APIs.ViewModels.Analytic
{
    public class SearchUserBehaviorRequest
    {
        [Required]
        [FromRoute(Name = "envId")]
        public int EnvId { get; set; }

        [Required]
        [FromQuery(Name = "startTimestamp")]
        public long StartTimestamp { get; set; }

        [FromQuery(Name = "endTimestamp")]
        public long? EndTimestamp { get; set; }

        [FromQuery(Name = "fromPage")]
        public int FromPage { get; set; } = 0;

        [FromQuery(Name = "takeSize")]
        [Range(0, 500)]
        public int TakeSize { get; set; } = 500;

        public void Validate()
        {
            if (EnvId <= 0)
            {
                throw new ArgumentException("invalid envId.");
            }

            if (TakeSize > 500)
            {
                throw new ArgumentException("cannot take more than 500 records.");
            }
        }

        public SearchDescriptor<TrackUserBehaviorEvent> SearchDescriptor()
        {
            var searchDescriptor = new SearchDescriptor<TrackUserBehaviorEvent>()
                .Query(CombinedQuery)
                .Sort(descriptor => descriptor.Ascending(source => source.TimeStampFromClientEnd))
                .Index(ElasticSearchIndices.UserBehaviorTrack)
                .From(FromPage)
                .Size(TakeSize);

            return searchDescriptor;
        }

        QueryContainer CombinedQuery(QueryContainerDescriptor<TrackUserBehaviorEvent> query)
        {
            var longRangeQuery = new LongRangeQuery
            {
                Field = Field<TrackUserBehaviorEvent>(source => source.TimeStampFromClientEnd),
                GreaterThanOrEqualTo = StartTimestamp
            };
            if (EndTimestamp.HasValue)
            {
                longRangeQuery.LessThanOrEqualTo = EndTimestamp.Value;
            }

            var envIdQuery = new TermQuery
            {
                Field = Field<TrackUserBehaviorEvent>(source => source.EnvironmentId),
                Value = EnvId
            };

            var combinedQuery = query.Bool(descriptor => descriptor.Must(longRangeQuery, envIdQuery));
            return combinedQuery;
        }
    }
}