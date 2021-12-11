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
    }
}