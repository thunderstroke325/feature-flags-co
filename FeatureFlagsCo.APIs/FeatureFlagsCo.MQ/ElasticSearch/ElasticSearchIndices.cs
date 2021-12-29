using System.Linq;

namespace FeatureFlagsCo.MQ.ElasticSearch
{
    /// <summary>
    /// es search indexes [from `GET /_cat/indices`]
    /// </summary>
    public static class ElasticSearchIndices
    {
        public const string Experiment = "experiments";

        public const string Variation = "ffvariationrequestindex";
        
        public const string Analytics = "analytics";
        public const string UserBehaviorTrack = "userbehaviortrack";

        public static readonly string[] All = { Experiment, Variation, Analytics, UserBehaviorTrack };

        public static bool IsRegistered(string name) => All.Contains(name);
    }
}